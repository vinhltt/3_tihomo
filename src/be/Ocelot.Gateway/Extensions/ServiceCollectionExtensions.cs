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
        var healthChecksBuilder = services.AddHealthChecks(); // Add health check for Identity SSO service
        healthChecksBuilder.AddTypeActivatedCheck<DownstreamServiceHealthCheck>(
            "Identity.Sso",
            ["Identity.Sso", "https://localhost:5001/health"]);

        // Add health check for Core Finance service
        healthChecksBuilder.AddTypeActivatedCheck<DownstreamServiceHealthCheck>(
            "CoreFinance.Api",
            ["CoreFinance.Api", "https://localhost:5004/health"]);

        // Add health check for Money Management service
        healthChecksBuilder.AddTypeActivatedCheck<DownstreamServiceHealthCheck>(
            "MoneyManagement.Api",
            [
                "MoneyManagement.Api", "https://localhost:5002/health"
            ]); // Add health check for Planning Investment service
        healthChecksBuilder.AddTypeActivatedCheck<DownstreamServiceHealthCheck>(
            "PlanningInvestment.Api",
            ["PlanningInvestment.Api", "https://localhost:5206/health"]);

        // Add health check for Reporting service (when available)
        healthChecksBuilder.AddTypeActivatedCheck<DownstreamServiceHealthCheck>(
            "Reporting.Api",
            ["Reporting.Api", "https://localhost:5004/health"]);

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