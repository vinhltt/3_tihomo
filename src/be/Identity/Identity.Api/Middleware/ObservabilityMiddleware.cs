using Identity.Api.Services;
using System.Diagnostics;

namespace Identity.Api.Middleware;

/// <summary>
/// Middleware for request correlation and observability
/// Middleware cho correlation và observability của request
/// </summary>
public class ObservabilityMiddleware
{
    private readonly RequestDelegate _next;
    private readonly TelemetryService _telemetryService;
    private readonly ILogger<ObservabilityMiddleware> _logger;

    public ObservabilityMiddleware(
        RequestDelegate next,
        TelemetryService telemetryService,
        ILogger<ObservabilityMiddleware> logger)
    {
        _next = next;
        _telemetryService = telemetryService;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Generate or extract correlation ID
        // Tạo hoặc extract correlation ID
        var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault() 
                          ?? Guid.NewGuid().ToString();
        
        // Add correlation ID to response headers
        // Thêm correlation ID vào response headers
        context.Response.Headers.Append("X-Correlation-ID", correlationId);
        
        // Add correlation ID to HttpContext for logging
        // Thêm correlation ID vào HttpContext cho logging
        context.Items["CorrelationId"] = correlationId;

        // Start activity for distributed tracing
        // Bắt đầu activity cho distributed tracing
        using var activity = TelemetryService.ActivitySource.StartActivity("http_request");
        activity?.SetTag("http.method", context.Request.Method);
        activity?.SetTag("http.url", context.Request.Path);
        activity?.SetTag("correlation_id", correlationId);

        // Track active requests
        // Theo dõi active requests
        _telemetryService.IncrementActiveRequests();
        
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            // Continue to next middleware
            // Tiếp tục với middleware tiếp theo
            await _next(context);
            
            // Record successful request
            // Ghi lại request thành công
            activity?.SetTag("http.status_code", context.Response.StatusCode);
            activity?.SetStatus(
                context.Response.StatusCode >= 400 
                    ? ActivityStatusCode.Error 
                    : ActivityStatusCode.Ok);
        }
        catch (Exception ex)
        {
            // Record failed request
            // Ghi lại request thất bại
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.SetTag("error.type", ex.GetType().Name);
            activity?.SetTag("error.message", ex.Message);
            
            _logger.LogError(ex, 
                "Request failed for {Method} {Path} with CorrelationId {CorrelationId}",
                context.Request.Method, 
                context.Request.Path, 
                correlationId);
                
            throw;
        }
        finally
        {
            // Always decrement active requests
            // Luôn giảm active requests
            _telemetryService.DecrementActiveRequests();
            
            stopwatch.Stop();
            
            // Log request completion with timing
            // Log hoàn thành request với timing
            _logger.LogInformation(
                "Request {Method} {Path} completed in {Duration}ms with status {StatusCode} [CorrelationId: {CorrelationId}]",
                context.Request.Method,
                context.Request.Path,
                stopwatch.ElapsedMilliseconds,
                context.Response.StatusCode,
                correlationId);
        }
    }
}
