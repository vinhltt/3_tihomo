using Ocelot.Gateway.Configuration;
using Ocelot.Gateway.HealthChecks;

namespace Ocelot.Gateway.Extensions;

/// <summary>
///     Extension methods for configuring health checks
/// </summary>
public static class HealthCheckExtensions
{
    /// <summary>
    ///     Add health checks for downstream services
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Configuration</param>
    /// <returns>Service collection</returns>
    public static IServiceCollection AddDownstreamHealthChecks(this IServiceCollection services,
        IConfiguration configuration)
    {
        var healthChecksBuilder = services.AddHealthChecks();
        
        // Get environment to determine host configuration
        var environment = configuration["ASPNETCORE_ENVIRONMENT"] ?? "Development";
        
        // Configure health check URLs based on environment
        // Use Docker service names for Docker environment, localhost for others
        string identityUrl, coreFinanceUrl, excelUrl;
        
        if (environment.Equals("Docker", StringComparison.OrdinalIgnoreCase))
        {
            // Docker environment - use service names
            identityUrl = "http://identity-api:8080/health";
            coreFinanceUrl = "http://corefinance-api:8080/health";
            excelUrl = "http://excel-api:8080/health";
        }
        else
        {
            // Local development - use localhost
            identityUrl = "http://localhost:5001/health";
            coreFinanceUrl = "http://localhost:5004/health";
            excelUrl = "http://localhost:5005/health";
        }
        
        // Add health check for Identity service
        healthChecksBuilder.AddTypeActivatedCheck<DownstreamServiceHealthCheck>(
            "Identity.Api",
            ["Identity.Api", identityUrl]);

        // Add health check for Core Finance service
        healthChecksBuilder.AddTypeActivatedCheck<DownstreamServiceHealthCheck>(
            "CoreFinance.Api",
            ["CoreFinance.Api", coreFinanceUrl]);

        // Add health check for Excel service
        healthChecksBuilder.AddTypeActivatedCheck<DownstreamServiceHealthCheck>(
            "Excel.Api",
            ["Excel.Api", excelUrl]);

        // Note: MoneyManagement and PlanningInvestment services are not yet dockerized
        // They will be added when Docker support is implemented

        // Configure HttpClient for health checks
        services.AddHttpClient<DownstreamServiceHealthCheck>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(10);
            client.DefaultRequestHeaders.Add("User-Agent", "OcelotGateway-HealthCheck/1.0");
        });

        return services;
    }

    /// <summary>
    ///     Add CORS configuration
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="corsSettings">CORS settings</param>
    /// <returns>Service collection</returns>
    public static IServiceCollection AddCorsConfiguration(this IServiceCollection services, CorsSettings corsSettings)
    {
        services.AddCors(options =>
        {
            options.AddPolicy(corsSettings.PolicyName, policy =>
            {
                if (corsSettings.AllowedOrigins.Any())
                    policy.WithOrigins(corsSettings.AllowedOrigins.ToArray());
                else
                    policy.AllowAnyOrigin();

                if (corsSettings.AllowedMethods.Any())
                    policy.WithMethods(corsSettings.AllowedMethods.ToArray());
                else
                    policy.AllowAnyMethod();

                if (corsSettings.AllowedHeaders.Any() && !corsSettings.AllowedHeaders.Contains("*"))
                    policy.WithHeaders(corsSettings.AllowedHeaders.ToArray());
                else
                    policy.AllowAnyHeader();

                if (corsSettings.AllowCredentials) policy.AllowCredentials();
            });
        });

        return services;
    }
}