using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Ocelot.DependencyInjection;
using Ocelot.Gateway.Configuration;
using Ocelot.Gateway.Extensions;
using Ocelot.Gateway.Middleware;
using Ocelot.Middleware;
using Serilog;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/gateway-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Add Serilog
    builder.Host.UseSerilog(); // Add configuration
    builder.Configuration
        .SetBasePath(builder.Environment.ContentRootPath)
        .AddJsonFile("appsettings.json", false, true)
        .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", true, true)        .AddEnvironmentVariables();

    // TEMPORARY: Simplified config loading for debugging
    // Just load the Development config directly
    builder.Configuration.AddJsonFile($"ocelot.{builder.Environment.EnvironmentName}.json", false, true);

    // Read service ports configuration
    // var servicePorts = builder.Configuration.GetSection("ServicePorts").Get<Dictionary<string, int>>() ??
    //                    new Dictionary<string, int>();

    // Read ocelot.json and replace environment variables with actual port values
    // var ocelotConfig = File.ReadAllText(Path.Combine(builder.Environment.ContentRootPath, "ocelot.json"));
    // foreach (var port in servicePorts) ocelotConfig = ocelotConfig.Replace($"${{{port.Key}}}", port.Value.ToString());

    // Write the processed ocelot configuration to a temporary file
    // var tempOcelotPath = Path.Combine(builder.Environment.ContentRootPath, "ocelot.processed.json");
    // File.WriteAllText(tempOcelotPath, ocelotConfig);

    // Add the processed ocelot configuration
    // builder.Configuration.AddJsonFile("ocelot.processed.json", false, true)
    //     .AddJsonFile($"ocelot.{builder.Environment.EnvironmentName}.json", true, true);

    // Bind configuration sections
    var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>() ?? new JwtSettings();
    var corsSettings = builder.Configuration.GetSection(CorsSettings.SectionName).Get<CorsSettings>() ??
                       new CorsSettings();
    var rateLimitSettings = builder.Configuration.GetSection(RateLimitSettings.SectionName).Get<RateLimitSettings>() ??
                            new RateLimitSettings();

    // Configure settings as services
    builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));
    builder.Services.Configure<ApiKeySettings>(builder.Configuration.GetSection(ApiKeySettings.SectionName));
    builder.Services.Configure<CorsSettings>(builder.Configuration.GetSection(CorsSettings.SectionName));
    builder.Services.Configure<RateLimitSettings>(builder.Configuration.GetSection(RateLimitSettings.SectionName));

    // Add services to the container
    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.JsonSerializerOptions.WriteIndented = true;
        });

    // Add authentication and authorization
    builder.Services.AddJwtAuthentication(jwtSettings);
    builder.Services.AddAuthorizationPolicies();

    // Add CORS
    builder.Services.AddCorsConfiguration(corsSettings);

    // Add health checks
    builder.Services.AddDownstreamHealthChecks(builder.Configuration);

    // Add memory cache
    builder.Services.AddMemoryCache();

    // Add Ocelot
    builder.Services.AddOcelot(builder.Configuration);

    // Add OpenAPI/Swagger
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    var app = builder.Build();

    // Configure the HTTP request pipeline
    app.UseGlobalExceptionHandling();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Ocelot Gateway API V1");
            c.RoutePrefix = "swagger";
        });
    }

    app.UseHttpsRedirection();

    app.UseRequestLogging();

    app.UseCors(corsSettings.PolicyName);

    app.UseAuthentication();
    app.UseAuthorization();

    // Handle local routes first
    app.MapControllers();

    // Map health check endpoints
    app.MapHealthChecks("/health", new HealthCheckOptions
    {
        ResponseWriter = async (context, report) =>
        {
            context.Response.ContentType = "application/json";
            var response = new
            {
                Status = report.Status.ToString(),
                TotalDuration = report.TotalDuration.TotalMilliseconds,
                Results = report.Entries.Select(entry => new
                {
                    Name = entry.Key,
                    Status = entry.Value.Status.ToString(),
                    entry.Value.Description,
                    Duration = entry.Value.Duration.TotalMilliseconds,
                    entry.Value.Data
                }),
                Timestamp = DateTime.UtcNow
            };
            await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            }));        }
    }); 
    
    // Use middleware to conditionally apply Ocelot
    app.UseWhen(context =>
        {
            var path = context.Request.Path.Value?.ToLower();
            // Only use Ocelot for specific API paths that need to be proxied
            return path != null &&
                   !path.Equals("/health") &&  // Exclude only the exact /health path, not paths that start with /health
                   !path.StartsWith("/swagger") &&
                   !path.StartsWith("/api/health") &&
                   (path.StartsWith("/identity") ||
                    path.StartsWith("/sso") ||
                    path.StartsWith("/auth") ||
                    path.StartsWith("/api/users") ||
                    path.StartsWith("/api/admin") ||
                    path.StartsWith("/api/core-finance") ||
                    path.StartsWith("/api/money-management") ||
                    path.StartsWith("/api/planning-investment") ||
                    path.StartsWith("/api/excel") ||
                    path.StartsWith("/health/"));  // Explicitly include health sub-paths for Ocelot routing
        },
        appBuilder => { appBuilder.UseOcelot().Wait(); });

    Log.Information("Ocelot Gateway started successfully on {BaseUrl}",
        builder.Configuration["GlobalConfiguration:BaseUrl"] ?? "http://localhost:5000");

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Ocelot Gateway terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}