using CoreFinance.Api.Consumers;
using CoreFinance.Api.Infrastructures.Middlewares;
using CoreFinance.Api.Infrastructures.Modules;
using CoreFinance.Api.Infrastructures.ServicesExtensions;
using CoreFinance.Infrastructure;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;
using Shared.Contracts.ConfigurationOptions;
using Shared.Contracts.Utilities;

async Task CreateDbIfNotExistsAsync(IHost host)
{
    using var scope = host.Services.CreateScope();
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var context = services.GetRequiredService<CoreFinanceDbContext>();
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

// Add the performance logging middleware early in the pipeline
app.UseMiddleware<PerformanceLoggingMiddleware>();

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

// Add the authorization middleware
//app.UseAuthorization();
app.UseRouting();

// Map health check endpoint
app.MapHealthChecks("/health");

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