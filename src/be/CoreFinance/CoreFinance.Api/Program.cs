using System.Diagnostics;
using CoreFinance.Api.Consumers;
using CoreFinance.Api.Infrastructures.Middlewares;
using CoreFinance.Api.Infrastructures.Modules;
using CoreFinance.Api.Infrastructures.ServicesExtensions;
using CoreFinance.Api.Middleware;
using CoreFinance.Infrastructure;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using CoreFinance.Api.Configuration;
using CoreFinance.Api.Extensions;
using Shared.Contracts.ConfigurationOptions;
using Shared.Contracts.Utilities;

static async Task CreateDbIfNotExistsAsync(IHost host)
{
    using var scope = host.Services.CreateScope();
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var context = services.GetRequiredService<CoreFinanceDbContext>();
        //await context.Database.EnsureCreatedAsync();
        await context.Database.MigrateAsync();
        //var dbInitializer = services.GetService<DbInitializer>();
        //if (dbInitializer == null)
        //{
        //    logger.LogError("dbInitializer is null");
        //    return;
        //}

        //await dbInitializer.Seed();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred creating the DB.");
    }
}

var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
var configuration = new ConfigurationBuilder()
    .AddEnvironmentVariables()
    .AddJsonFile("appsettings.json", false, true)
    .AddJsonFile($"appsettings.{env}.json", true, true)
    .Build();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

var corsOption = configuration.GetOptions<CorsOptions>("CorsOptions");
var policyName = corsOption!.PolicyName.Nullify("AppCorsPolicy");
builder.AddConfigurationSettings();
builder.AddGeneralConfigurations(policyName, corsOption);
builder.Services.AddInjectedServices();

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>() ?? new JwtSettings();
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));
builder.Services.AddJwtAuthentication(jwtSettings);
builder.Services.AddJwtAuthorization();

// Add MassTransit for message consumption
builder.Services.AddMassTransit(x =>
{    // Add consumers
    x.AddConsumer<TransactionDataConsumer>();
    x.AddConsumer<TransactionBatchConsumer>();

    x.SetKebabCaseEndpointNameFormatter();

    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitMqSettings = builder.Configuration.GetSection("RabbitMQ");
        var host = rabbitMqSettings["Host"] ?? "localhost";
        var username = rabbitMqSettings["Username"] ?? "tihomo";
        var password = rabbitMqSettings["Password"] ?? "tihomo123";

        cfg.Host(host, "/", h =>
        {
            h.Username(username);
            h.Password(password);
        });

        // Configure retry policy
        cfg.UseMessageRetry(r =>
            r.Exponential(5, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(60), TimeSpan.FromSeconds(5)));

        // Configure circuit breaker
        cfg.UseCircuitBreaker(cb =>
        {
            cb.TrackingPeriod = TimeSpan.FromMinutes(1);
            cb.TripThreshold = 15;
            cb.ActiveThreshold = 10;
            cb.ResetInterval = TimeSpan.FromMinutes(5);
        });

        cfg.ConfigureEndpoints(context);
    });
});

// ✅ Configure OtelSettings from appsettings.json
// Cấu hình OtelSettings từ appsettings.json
var otelSettings = builder.Configuration.GetSection("OtelSettings").Get<OtelSettings>() ?? new OtelSettings
{
    ServiceName = "TiHoMo.CoreFinance",
    ServiceVersion = "1.0.0",
    ExporterOtlpEndpoint = "http://tempo:4317",
    TracesSampler = "traceidratio",
    TracesSamplerArg = "1.0"
};

// ✅ Configure ActivitySource for distributed tracing
// Cấu hình ActivitySource cho distributed tracing
var activitySource = new ActivitySource(otelSettings.ServiceName);
builder.Services.AddSingleton(activitySource);

// ✅ Add OpenTelemetry for comprehensive observability
// Thêm OpenTelemetry cho observability toàn diện
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService(otelSettings.ServiceName, otelSettings.ServiceVersion)
        .AddAttributes(otelSettings.GetResourceAttributesDictionary())
        .AddAttributes(new Dictionary<string, object>
        {
            ["service.namespace"] = "TiHoMo",
            ["service.instance.id"] = Environment.MachineName,
            ["deployment.environment"] = builder.Environment.EnvironmentName
        }))
    .WithTracing(tracing => tracing
        .SetSampler(new TraceIdRatioBasedSampler(otelSettings.GetSamplingRatio()))
        .AddAspNetCoreInstrumentation(options =>
        {
            options.RecordException = true;
            options.Filter = httpContext => !httpContext.Request.Path.StartsWithSegments("/health");
        })
        .AddEntityFrameworkCoreInstrumentation(options =>
        {
            options.SetDbStatementForText = true;
            options.SetDbStatementForStoredProcedure = true;
        })
        .AddHttpClientInstrumentation(options => { options.RecordException = true; })
        .AddSource(otelSettings.ServiceName)
        .AddOtlpExporter(otlpOptions =>
        {
            otlpOptions.Endpoint = new Uri(otelSettings.ExporterOtlpEndpoint);
        })
        .AddConsoleExporter())
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation()
        .AddMeter(otelSettings.ServiceName)
        .AddPrometheusExporter());

// Add health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<CoreFinanceDbContext>()
    .AddCheck("rabbitmq", () => HealthCheckResult.Healthy("RabbitMQ health check placeholder"));
builder.Host.UseSerilog((hostingContext, loggerConfiguration) =>
{
    loggerConfiguration
        .ReadFrom.Configuration(hostingContext.Configuration)
        .Enrich.FromLogContext()
        .Enrich.WithMachineName();
});

var app = builder.Build();

// ✅ Add CorrelationMiddleware for correlation ID tracking and structured logging
// Thêm CorrelationMiddleware cho correlation ID tracking và structured logging
app.UseMiddleware<CorrelationMiddleware>();

// Add the performance logging middleware early in the pipeline
app.UseMiddleware<PerformanceLoggingMiddleware>();

// ✅ Add TracingMiddleware for distributed tracing
// Thêm TracingMiddleware cho distributed tracing
app.UseDistributedTracing();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "CoreFinance Api v1");
        c.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
    });
}

app.UseHttpsRedirection();

app.UseCors(policyName);

// Add authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();
app.UseRouting();

// Map health check endpoint
app.MapHealthChecks("/health");

// Note: Metrics are exported via OpenTelemetry Prometheus exporter

app.MapControllers();

await CreateDbIfNotExistsAsync(app);


try
{
    await app.RunAsync();
}
finally
{
    await Log.CloseAndFlushAsync();
}