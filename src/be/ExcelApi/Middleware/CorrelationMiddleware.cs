using System.Diagnostics;
using Serilog.Context;

namespace ExcelApi.Middleware;

/// <summary>
///     Middleware to handle correlation ID tracking and structured logging
///     Middleware để xử lý correlation ID tracking và structured logging
/// </summary>
public class CorrelationMiddleware(RequestDelegate next, ILogger<CorrelationMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = GetOrCreateCorrelationId(context);

        // Add to Serilog context for all logs in this request
        using (LogContext.PushProperty("CorrelationId", correlationId))
        using (LogContext.PushProperty("RequestPath", context.Request.Path))
        using (LogContext.PushProperty("RequestMethod", context.Request.Method))
        using (LogContext.PushProperty("UserAgent", context.Request.Headers.UserAgent.ToString()))
        {
            var stopwatch = Stopwatch.StartNew();

            logger.LogInformation("Request started: {Method} {Path}",
                context.Request.Method, context.Request.Path);

            try
            {
                await next(context);

                stopwatch.Stop();
                logger.LogInformation(
                    "Request completed: {Method} {Path} - Status: {StatusCode} - Duration: {ElapsedMs}ms",
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode,
                    stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                logger.LogError(ex, "Request failed: {Method} {Path} - Duration: {ElapsedMs}ms - Error: {ErrorMessage}",
                    context.Request.Method,
                    context.Request.Path,
                    stopwatch.ElapsedMilliseconds,
                    ex.Message);
                throw;
            }
        }
    }

    private string GetOrCreateCorrelationId(HttpContext context)
    {
        // Try to get correlation ID from header
        if (context.Request.Headers.TryGetValue("X-Correlation-ID", out var headerValue))
        {
            var correlationId = headerValue.FirstOrDefault();
            if (!string.IsNullOrEmpty(correlationId))
            {
                // Add to response header
                context.Response.Headers["X-Correlation-ID"] = correlationId;
                return correlationId;
            }
        }

        // Generate new correlation ID
        var newCorrelationId = Guid.NewGuid().ToString();
        context.Response.Headers["X-Correlation-ID"] = newCorrelationId;
        context.Items["CorrelationId"] = newCorrelationId;

        return newCorrelationId;
    }
}