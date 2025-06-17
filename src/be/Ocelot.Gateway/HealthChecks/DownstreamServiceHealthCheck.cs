using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Ocelot.Gateway.HealthChecks;

/// <summary>
/// Health check for downstream services
/// </summary>
public class DownstreamServiceHealthCheck : IHealthCheck
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<DownstreamServiceHealthCheck> _logger;
    private readonly string _serviceName;
    private readonly string _healthCheckUrl;

    public DownstreamServiceHealthCheck(
        HttpClient httpClient,
        ILogger<DownstreamServiceHealthCheck> logger,
        string serviceName,
        string healthCheckUrl)
    {
        _httpClient = httpClient;
        _logger = logger;
        _serviceName = serviceName;
        _healthCheckUrl = healthCheckUrl;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Checking health for service: {ServiceName} at {Url}", _serviceName, _healthCheckUrl);

            using var response = await _httpClient.GetAsync(_healthCheckUrl, cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogDebug("Health check successful for {ServiceName}: {StatusCode}", _serviceName, response.StatusCode);
                
                return HealthCheckResult.Healthy($"{_serviceName} is healthy", 
                    new Dictionary<string, object>
                    {
                        ["service"] = _serviceName,
                        ["status_code"] = (int)response.StatusCode,
                        ["response_time"] = DateTime.UtcNow,
                        ["url"] = _healthCheckUrl
                    });
            }

            _logger.LogWarning("Health check failed for {ServiceName}: {StatusCode}", _serviceName, response.StatusCode);
            return HealthCheckResult.Unhealthy($"{_serviceName} returned {response.StatusCode}",
                data: new Dictionary<string, object>
                {
                    ["service"] = _serviceName,
                    ["status_code"] = (int)response.StatusCode,
                    ["url"] = _healthCheckUrl
                });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error during health check for {ServiceName}", _serviceName);
            return HealthCheckResult.Unhealthy($"{_serviceName} is unreachable: {ex.Message}",
                exception: ex,
                data: new Dictionary<string, object>
                {
                    ["service"] = _serviceName,
                    ["error"] = ex.Message,
                    ["url"] = _healthCheckUrl
                });
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger.LogError(ex, "Timeout during health check for {ServiceName}", _serviceName);
            return HealthCheckResult.Unhealthy($"{_serviceName} health check timed out",
                exception: ex,
                data: new Dictionary<string, object>
                {
                    ["service"] = _serviceName,
                    ["error"] = "Timeout",
                    ["url"] = _healthCheckUrl
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during health check for {ServiceName}", _serviceName);
            return HealthCheckResult.Unhealthy($"{_serviceName} health check failed: {ex.Message}",
                exception: ex,
                data: new Dictionary<string, object>
                {
                    ["service"] = _serviceName,
                    ["error"] = ex.Message,
                    ["url"] = _healthCheckUrl
                });
        }
    }
}
