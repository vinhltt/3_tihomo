using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Net;

namespace Ocelot.Gateway.Controllers;

/// <summary>
/// Health check controller for monitoring gateway and downstream services
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class HealthController(HealthCheckService healthCheckService, ILogger<HealthController> logger)
    : ControllerBase
{
    /// <summary>
    /// Get overall health status of the gateway and all downstream services
    /// </summary>
    /// <returns>Health check report</returns>
    [HttpGet]
    public async Task<IActionResult> GetHealth()
    {
        try
        {
            var healthReport = await healthCheckService.CheckHealthAsync();
            
            var response = new
            {
                Status = healthReport.Status.ToString(),
                TotalDuration = healthReport.TotalDuration.TotalMilliseconds,
                Results = healthReport.Entries.Select(entry => new
                {
                    Name = entry.Key,
                    Status = entry.Value.Status.ToString(),
                    Description = entry.Value.Description,
                    Duration = entry.Value.Duration.TotalMilliseconds,
                    Data = entry.Value.Data,
                    Exception = entry.Value.Exception?.Message
                }),
                Timestamp = DateTime.UtcNow
            };

            var statusCode = healthReport.Status switch
            {
                HealthStatus.Healthy => HttpStatusCode.OK,
                HealthStatus.Degraded => HttpStatusCode.OK,
                HealthStatus.Unhealthy => HttpStatusCode.ServiceUnavailable,
                _ => HttpStatusCode.ServiceUnavailable
            };

            return StatusCode((int)statusCode, response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while checking health");
            return StatusCode(500, new { Error = "Health check failed", Message = ex.Message });
        }
    }

    /// <summary>
    /// Get simplified health status (ready/not ready)
    /// </summary>
    /// <returns>Simple health status</returns>
    [HttpGet("ready")]
    public async Task<IActionResult> GetReadiness()
    {
        try
        {
            var healthReport = await healthCheckService.CheckHealthAsync();
            
            if (healthReport.Status == HealthStatus.Healthy)
            {
                return Ok(new { Status = "Ready", Timestamp = DateTime.UtcNow });
            }

            return ServiceUnavailable(new { Status = "Not Ready", Timestamp = DateTime.UtcNow });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while checking readiness");
            return ServiceUnavailable(new { Status = "Not Ready", Error = ex.Message, Timestamp = DateTime.UtcNow });
        }
    }

    /// <summary>
    /// Get liveness status (alive/dead)
    /// </summary>
    /// <returns>Simple liveness status</returns>
    [HttpGet("live")]
    public IActionResult GetLiveness()
    {
        // Gateway is alive if this endpoint responds
        return Ok(new { Status = "Alive", Timestamp = DateTime.UtcNow });
    }

    /// <summary>
    /// Get health status of a specific downstream service
    /// </summary>
    /// <param name="serviceName">Name of the service to check</param>
    /// <returns>Service-specific health status</returns>
    [HttpGet("services/{serviceName}")]
    public async Task<IActionResult> GetServiceHealth(string serviceName)
    {
        try
        {
            var healthReport = await healthCheckService.CheckHealthAsync();
            
            var serviceEntry = healthReport.Entries.FirstOrDefault(e => 
                e.Key.Equals(serviceName, StringComparison.OrdinalIgnoreCase));

            if (serviceEntry.Key == null)
            {
                return NotFound(new { Error = $"Service '{serviceName}' not found" });
            }

            var response = new
            {
                Name = serviceEntry.Key,
                Status = serviceEntry.Value.Status.ToString(),
                Description = serviceEntry.Value.Description,
                Duration = serviceEntry.Value.Duration.TotalMilliseconds,
                Data = serviceEntry.Value.Data,
                Exception = serviceEntry.Value.Exception?.Message,
                Timestamp = DateTime.UtcNow
            };

            var statusCode = serviceEntry.Value.Status switch
            {
                HealthStatus.Healthy => HttpStatusCode.OK,
                HealthStatus.Degraded => HttpStatusCode.OK,
                HealthStatus.Unhealthy => HttpStatusCode.ServiceUnavailable,
                _ => HttpStatusCode.ServiceUnavailable
            };

            return StatusCode((int)statusCode, response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while checking health for service {ServiceName}", serviceName);
            return StatusCode(500, new { Error = "Service health check failed", Message = ex.Message });
        }
    }

    private ObjectResult ServiceUnavailable(object value)
    {
        return StatusCode((int)HttpStatusCode.ServiceUnavailable, value);
    }
}
