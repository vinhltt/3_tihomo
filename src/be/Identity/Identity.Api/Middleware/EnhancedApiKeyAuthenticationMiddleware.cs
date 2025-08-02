using Identity.Application.Common.Interfaces;
using Identity.Application.Services.Security;
using Identity.Domain.Entities;
using Identity.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace Identity.Api.Middleware;

/// <summary>
/// Enhanced API Key Authentication Middleware - Middleware xác thực API key nâng cao (EN)<br/>
/// Middleware xác thực khóa API nâng cao (VI)
/// </summary>
public class EnhancedApiKeyAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<EnhancedApiKeyAuthenticationMiddleware> _logger;
    private const string ApiKeyHeaderName = "X-API-Key";
    private const string ApiKeyQueryParam = "api_key";

    /// <summary>
    /// Constructor for Enhanced API Key Authentication Middleware (EN)<br/>
    /// Constructor cho Middleware xác thực khóa API nâng cao (VI)
    /// </summary>
    public EnhancedApiKeyAuthenticationMiddleware(
        RequestDelegate next,
        ILogger<EnhancedApiKeyAuthenticationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Process HTTP request with enhanced API key validation (EN)<br/>
    /// Xử lý HTTP request với xác thực khóa API nâng cao (VI)
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        // Skip authentication for certain paths
        if (ShouldSkipAuthentication(context))
        {
            await _next(context);
            return;
        }

        try
        {
            var apiKey = ExtractApiKey(context);
            if (string.IsNullOrEmpty(apiKey))
            {
                await HandleUnauthorized(context, "API key is required");
                return;
            }

            var clientIpAddress = GetClientIpAddress(context);
            var enhancedApiKeyService = context.RequestServices.GetRequiredService<IEnhancedApiKeyService>();
            
            // Verify API key with enhanced security validation
            var verificationResult = await enhancedApiKeyService.VerifyApiKeyAsync(
                apiKey, clientIpAddress, context.RequestAborted);

            if (!verificationResult.IsValid)
            {
                await HandleUnauthorized(context, verificationResult.ErrorMessage ?? "Invalid API key");
                return;
            }

            // Additional security checks
            if (!await PerformAdditionalSecurityChecks(context, verificationResult))
            {
                return; // Response already handled in the method
            }

            // Add authenticated user information to context
            SetAuthenticationContext(context, verificationResult);

            // Store start time for response time calculation
            context.Items["StartTime"] = DateTime.UtcNow;

            // Log successful authentication
            _logger.LogInformation("API key authenticated successfully for user {UserId} from IP {ClientIp}", 
                verificationResult.UserId, clientIpAddress);

            // Continue to next middleware
            await _next(context);
            
            // Log API key usage after request completion
            await LogApiKeyUsageAsync(context, verificationResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during API key authentication");
            await HandleInternalServerError(context, "Authentication service unavailable");
        }
    }

    /// <summary>
    /// Extract API key from request headers or query parameters (EN)<br/>
    /// Trích xuất khóa API từ headers hoặc query parameters (VI)
    /// </summary>
    private string? ExtractApiKey(HttpContext context)
    {
        // Try header first
        if (context.Request.Headers.TryGetValue(ApiKeyHeaderName, out var headerValue))
        {
            return headerValue.FirstOrDefault();
        }

        // Try query parameter
        if (context.Request.Query.TryGetValue(ApiKeyQueryParam, out var queryValue))
        {
            return queryValue.FirstOrDefault();
        }

        // Try Authorization header with Bearer scheme
        var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return authHeader.Substring("Bearer ".Length).Trim();
        }

        return null;
    }

    /// <summary>
    /// Get client IP address from request (EN)<br/>
    /// Lấy địa chỉ IP client từ request (VI)
    /// </summary>
    private string GetClientIpAddress(HttpContext context)
    {
        // Check for forwarded IP (behind proxy/load balancer)
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            // Take the first IP in the chain
            var firstIp = forwardedFor.Split(',').FirstOrDefault()?.Trim();
            if (!string.IsNullOrEmpty(firstIp))
                return firstIp;
        }

        // Check for real IP header
        var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
            return realIp;

        // Check for client IP header
        var clientIp = context.Request.Headers["X-Client-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(clientIp))
            return clientIp;

        // Fall back to connection remote IP
        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    /// <summary>
    /// Perform additional security checks (EN)<br/>
    /// Thực hiện các kiểm tra bảo mật bổ sung (VI)
    /// </summary>
    private async Task<bool> PerformAdditionalSecurityChecks(HttpContext context, dynamic verificationResult)
    {
        // Check HTTPS requirement if specified in security settings
        if (verificationResult.SecuritySettings?.RequireHttps == true && !context.Request.IsHttps)
        {
            await HandleForbidden(context, "HTTPS is required for this API key");
            return false;
        }

        // Check CORS settings if applicable
        var origin = context.Request.Headers.Origin.FirstOrDefault();
        if (!string.IsNullOrEmpty(origin) && verificationResult.SecuritySettings?.AllowCorsRequests == true)
        {
            var allowedOrigins = verificationResult.SecuritySettings?.AllowedOrigins as List<string>;
            if (allowedOrigins?.Any() == true && !string.IsNullOrEmpty(origin) && !allowedOrigins.Contains(origin) && !allowedOrigins.Contains("*"))
            {
                await HandleForbidden(context, "Origin not allowed");
                return false;
            }
        }

        // Add additional custom security checks here as needed

        return true;
    }

    /// <summary>
    /// Set authentication context for downstream middleware (EN)<br/>
    /// Thiết lập context xác thực cho middleware downstream (VI)
    /// </summary>
    private void SetAuthenticationContext(HttpContext context, dynamic verificationResult)
    {
        // Set user ID in context
        context.Items["UserId"] = verificationResult.UserId;
        context.Items["ApiKeyId"] = verificationResult.ApiKeyId;
        context.Items["ApiKeyScopes"] = verificationResult.Scopes;
        context.Items["IsApiKeyAuthenticated"] = true;

        // Set custom headers for downstream services
        context.Request.Headers["X-User-Id"] = verificationResult.UserId?.ToString();
        context.Request.Headers["X-API-Key-Id"] = verificationResult.ApiKeyId?.ToString();
    }

    /// <summary>
    /// Log API key usage for analytics and monitoring (EN)<br/>
    /// Ghi log sử dụng API key cho phân tích và giám sát (VI)
    /// </summary>
    private async Task LogApiKeyUsageAsync(HttpContext context, dynamic verificationResult)
    {
        try
        {
            // Get required services
            var serviceProvider = context.RequestServices;
            var dbContext = serviceProvider.GetService<IdentityDbContext>();
            
            if (dbContext == null || verificationResult.ApiKeyId == null)
                return;

            var startTime = context.Items["StartTime"] as DateTime? ?? DateTime.UtcNow;
            var responseTime = (int)(DateTime.UtcNow - startTime).TotalMilliseconds;

            // Create usage log entry
            var usageLog = new ApiKeyUsageLog
            {
                Id = Guid.NewGuid(),
                ApiKeyId = (Guid)verificationResult.ApiKeyId,
                Timestamp = DateTime.UtcNow,
                Method = context.Request.Method,
                Endpoint = context.Request.Path.Value ?? string.Empty,
                StatusCode = context.Response.StatusCode,
                ResponseTime = responseTime,
                IpAddress = GetClientIpAddress(context),
                UserAgent = context.Request.Headers.UserAgent.FirstOrDefault(),
                RequestSize = context.Request.ContentLength ?? 0,
                ResponseSize = 0, // Will be updated in response middleware if needed
                RequestId = context.TraceIdentifier,
                ScopesUsed = verificationResult.Scopes ?? new List<string>(),
                CreatedAt = DateTime.UtcNow
            };

            // Add to database
            dbContext.ApiKeyUsageLogs.Add(usageLog);
            await dbContext.SaveChangesAsync();

            // Log successful usage tracking
            var apiKeyId = (Guid?)verificationResult.ApiKeyId;
            _logger.LogDebug("API key usage logged for key {ApiKeyId} - {Method} {Endpoint}", 
                apiKeyId?.ToString() ?? "unknown", usageLog.Method, usageLog.Endpoint);
        }
        catch (Exception ex)
        {
            // Don't fail the request if logging fails
            var apiKeyId = (Guid?)verificationResult.ApiKeyId;
            _logger.LogWarning(ex, "Failed to log API key usage for key {ApiKeyId}", 
                apiKeyId?.ToString() ?? "unknown");
        }
    }

    /// <summary>
    /// Check if authentication should be skipped for this request (EN)<br/>
    /// Kiểm tra có nên bỏ qua xác thực cho request này không (VI)
    /// </summary>
    private bool ShouldSkipAuthentication(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLowerInvariant();
        
        // Debug logging to understand path issues
        _logger.LogDebug("API Key middleware checking path: {Path}", path);
        
        // Skip authentication for public endpoints
        var publicPaths = new[]
        {
            "/health",
            "/swagger",
            "/api/auth/login",
            "/api/auth/register", 
            "/api/auth/refresh",
            "/api/public/",
            "/metrics",
            // Handle duplicate API path from Gateway routing
            "/api/api/auth/login",
            "/api/api/auth/register",
            "/api/api/auth/refresh"
        };

        var shouldSkip = publicPaths.Any(publicPath => path?.StartsWith(publicPath) == true);
        _logger.LogDebug("Should skip authentication for path {Path}: {ShouldSkip}", path, shouldSkip);
        
        return shouldSkip;
    }

    /// <summary>
    /// Handle unauthorized response (EN)<br/>
    /// Xử lý phản hồi unauthorized (VI)
    /// </summary>
    private async Task HandleUnauthorized(HttpContext context, string message)
    {
        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
        context.Response.ContentType = "application/json";

        var response = new
        {
            error = "Unauthorized",
            message = message,
            timestamp = DateTime.UtcNow.ToString("O"),
            path = context.Request.Path.Value
        };

        var json = JsonSerializer.Serialize(response);
        await context.Response.WriteAsync(json);

        _logger.LogWarning("Unauthorized API request: {Message} - Path: {Path} - IP: {ClientIp}", 
            message, context.Request.Path, GetClientIpAddress(context));
    }

    /// <summary>
    /// Handle forbidden response (EN)<br/>
    /// Xử lý phản hồi forbidden (VI)
    /// </summary>
    private async Task HandleForbidden(HttpContext context, string message)
    {
        context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
        context.Response.ContentType = "application/json";

        var response = new
        {
            error = "Forbidden",
            message = message,
            timestamp = DateTime.UtcNow.ToString("O"),
            path = context.Request.Path.Value
        };

        var json = JsonSerializer.Serialize(response);
        await context.Response.WriteAsync(json);

        _logger.LogWarning("Forbidden API request: {Message} - Path: {Path} - IP: {ClientIp}", 
            message, context.Request.Path, GetClientIpAddress(context));
    }

    /// <summary>
    /// Handle internal server error response (EN)<br/>
    /// Xử lý phản hồi internal server error (VI)
    /// </summary>
    private async Task HandleInternalServerError(HttpContext context, string message)
    {
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        context.Response.ContentType = "application/json";

        var response = new
        {
            error = "Internal Server Error",
            message = message,
            timestamp = DateTime.UtcNow.ToString("O"),
            path = context.Request.Path.Value
        };

        var json = JsonSerializer.Serialize(response);
        await context.Response.WriteAsync(json);
    }
}

/// <summary>
/// Extension methods for registering the enhanced API key authentication middleware (EN)<br/>
/// Extension methods để đăng ký middleware xác thực khóa API nâng cao (VI)
/// </summary>
public static class EnhancedApiKeyAuthenticationMiddlewareExtensions
{
    /// <summary>
    /// Add enhanced API key authentication middleware to the pipeline (EN)<br/>
    /// Thêm middleware xác thực khóa API nâng cao vào pipeline (VI)
    /// </summary>
    public static IApplicationBuilder UseEnhancedApiKeyAuthentication(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<EnhancedApiKeyAuthenticationMiddleware>();
    }
} 