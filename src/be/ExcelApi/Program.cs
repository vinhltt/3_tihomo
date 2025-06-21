using System.Reflection;
using System.Text;
using ExcelApi.Middleware;
using ExcelApi.Services;
using MassTransit;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;
using Serilog;

// fix error encoding 1252 cho ExcelDataReader
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

// Add services to the container.
// Add NewtonsoftJson support for more flexible JSON handling
builder.Services.AddControllers().AddNewtonsoftJson();

// Register ExcelProcessingService
builder.Services.AddScoped<IExcelProcessingService, ExcelProcessingService>();
builder.Services.AddScoped<MessagePublishingService>();

// Add MassTransit for message publishing
builder.Services.AddMassTransit(x =>
{
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
    .AddCheck("rabbitmq", () => HealthCheckResult.Healthy("RabbitMQ health check placeholder"));

// Add correlation context service  
builder.Services.AddScoped<LocalCorrelationContextService>();

// Add Swagger/OpenAPI services
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Excel API",
        Version = "v1",
        Description = "API for processing Excel and CSV files with message queue integration."
    });

    // Enable XML comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

    if (File.Exists(xmlPath)) c.IncludeXmlComments(xmlPath);

    c.EnableAnnotations();

    // Map IFormFile to binary format for Swagger UI
    c.MapType<IFormFile>(() => new OpenApiSchema
    {
        Type = "string",
        Format = "binary"
    });
});

var app = builder.Build();

// Add enhanced correlation middleware
app.UseMiddleware<CorrelationMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Excel API v1");
        c.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();

// Add health checks endpoint
app.MapHealthChecks("/health");

app.MapControllers();

// Add request logging
app.UseSerilogRequestLogging();

Log.Information("Excel API starting up...");

app.Run();

/// <summary>
///     Simple correlation context service for tracking requests (local implementation)
/// </summary>
public class LocalCorrelationContextService
{
    private Guid _correlationId;

    public Guid CorrelationId => _correlationId == Guid.Empty ? GenerateCorrelationId() : _correlationId;

    public void SetCorrelationId(Guid correlationId)
    {
        _correlationId = correlationId;
    }

    public Guid GenerateCorrelationId()
    {
        _correlationId = Guid.NewGuid();
        return _correlationId;
    }

    public string GetCorrelationIdString()
    {
        return CorrelationId.ToString();
    }
}