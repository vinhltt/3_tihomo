using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Identity.Api.Configuration;
using Identity.Api.Services;
using Identity.Api.HealthChecks;
using Identity.Api.Middleware;
using Identity.Application.Services.RefreshTokens;
using Identity.Infrastructure.Services;
using Identity.Infrastructure.Data;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using Serilog;
using Serilog.Context;

var builder = WebApplication.CreateBuilder(args);

// ✅ Configure Serilog for structured logging với correlation ID support
// Cấu hình Serilog cho structured logging với hỗ trợ correlation ID
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", Serilog.Events.LogEventLevel.Information)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "TiHoMo.Identity")
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {CorrelationId} {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "TiHoMo Identity API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new()
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityDefinition("ApiKey", new()
    {
        Description = "API Key Authorization header. Enter your API key in the text input below.",
        Name = "X-API-Key",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey
    });
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Add Entity Framework - register both contexts for different layers
// Thêm Entity Framework - đăng ký cả hai contexts cho các layer khác nhau
builder.Services.AddDbContext<Identity.Api.Configuration.IdentityDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDbContext<Identity.Infrastructure.Data.IdentityDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ✅ Add caching services for performance optimization
// Thêm caching services để tối ưu hiệu suất
builder.Services.AddMemoryCache(options =>
{
    options.SizeLimit = 1000; // Limit cache size to prevent memory issues
});

// Add Redis distributed cache (if configured)
// Thêm Redis distributed cache (nếu được cấu hình)
var redisConnectionString = builder.Configuration.GetConnectionString("Redis");
if (!string.IsNullOrEmpty(redisConnectionString))
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConnectionString;
        options.InstanceName = "TiHoMo.Identity";
    });
}
else
{
    // Fallback to in-memory distributed cache for development
    // Fallback về in-memory distributed cache cho development
    builder.Services.AddDistributedMemoryCache();
}

// Add JWT Authentication
var jwtSecretKey = builder.Configuration["JWT:SecretKey"];
if (string.IsNullOrEmpty(jwtSecretKey))
{
    throw new InvalidOperationException("JWT SecretKey not configured");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSecretKey)),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["JWT:Issuer"] ?? "TiHoMo.Identity",
            ValidateAudience = true,
            ValidAudience = builder.Configuration["JWT:Audience"] ?? "TiHoMo.Clients",
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// Add HttpClient for external API calls
builder.Services.AddHttpClient();

// ✅ Add OpenTelemetry for comprehensive observability
// Thêm OpenTelemetry cho observability toàn diện
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService("TiHoMo.Identity", "1.0.0")
        .AddAttributes(new Dictionary<string, object>
        {
            ["service.namespace"] = "TiHoMo",
            ["service.instance.id"] = Environment.MachineName,
            ["deployment.environment"] = builder.Environment.EnvironmentName
        }))
    .WithTracing(tracing => tracing
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
        .AddHttpClientInstrumentation(options =>
        {
            options.RecordException = true;
        })
        .AddSource("TiHoMo.Identity.Telemetry")
        .AddConsoleExporter()
        .AddJaegerExporter())
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation()
        .AddProcessInstrumentation()
        .AddMeter("TiHoMo.Identity.Telemetry")
        .AddPrometheusExporter());

// ✅ Register TelemetryService for custom metrics and tracing
// Đăng ký TelemetryService cho custom metrics và tracing
builder.Services.AddSingleton<TelemetryService>();

// ✅ Add enhanced application services with resilience patterns
// Thêm enhanced application services với resilience patterns
builder.Services.AddScoped<ITokenVerificationService>(provider =>
{
    // Register the enhanced service first
    var enhancedService = ActivatorUtilities.CreateInstance<EnhancedTokenVerificationService>(provider);
    
    // Then wrap it with resilience patterns
    return ActivatorUtilities.CreateInstance<ResilientTokenVerificationService>(
        provider, 
        enhancedService);
});
builder.Services.AddScoped<IUserService, EnhancedUserService>();
builder.Services.AddScoped<IApiKeyService, ApiKeyService>();
builder.Services.AddScoped<IJwtService, JwtService>();

// ✅ Add refresh token service for Phase 2 implementation
// Thêm refresh token service cho triển khai Phase 2
builder.Services.AddScoped<IRefreshTokenService, EfRefreshTokenService>();

// ✅ Add background service for token cleanup
// Thêm background service để dọn dẹp token
builder.Services.AddHostedService<RefreshTokenCleanupService>();

// Add health checks for monitoring (including resilience patterns)
// Thêm health checks để monitoring (bao gồm resilience patterns)
builder.Services.AddHealthChecks()
    .AddDbContextCheck<Identity.Api.Configuration.IdentityDbContext>("database")
    .AddCheck("memory_cache", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("Memory cache is healthy"))
    .AddCheck<CircuitBreakerHealthCheck>("circuit_breaker")
    .AddCheck<TelemetryHealthCheck>("telemetry");

// Add logging configuration for performance monitoring
// Thêm cấu hình logging để monitoring hiệu suất
builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
if (builder.Environment.IsDevelopment())
{
    builder.Logging.AddConsole();
    builder.Logging.AddDebug();
}

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "TiHoMo Identity API v1");
    });
}

app.UseHttpsRedirection();

app.UseCors();

// ✅ Add ObservabilityMiddleware for correlation ID and request metrics
// Thêm ObservabilityMiddleware cho correlation ID và request metrics
app.UseMiddleware<ObservabilityMiddleware>();

// Add API Key authentication middleware before JWT authentication
app.UseMiddleware<ApiKeyAuthenticationMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ✅ Map Prometheus metrics endpoint for scraping
// Map Prometheus metrics endpoint cho scraping
app.MapPrometheusScrapingEndpoint("/metrics");

// Health check endpoint with detailed information
// Health check endpoint với thông tin chi tiết
app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        var result = new
        {
            Status = report.Status.ToString(),
            Timestamp = DateTime.UtcNow,
            Duration = report.TotalDuration,
            Services = report.Entries.Select(e => new
            {
                Service = e.Key,
                Status = e.Value.Status.ToString(),
                Duration = e.Value.Duration,
                Description = e.Value.Description,
                Data = e.Value.Data.Count > 0 ? e.Value.Data : null
            })
        };
        
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(result));
    }
});

app.Run();
