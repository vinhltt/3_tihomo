using Identity.Api.Services;
using System.Security.Claims;

namespace Identity.Api.Configuration;

public class ApiKeyAuthenticationMiddleware(RequestDelegate next, ILogger<ApiKeyAuthenticationMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context, IApiKeyService apiKeyService)
    {
        // Check if API key is provided in the request
        var apiKey = ExtractApiKey(context.Request);
        
        if (!string.IsNullOrEmpty(apiKey))
        {
            var user = await apiKeyService.ValidateApiKeyAsync(apiKey);
            
            if (user != null)
            {
                // Create claims for the user
                var claims = new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new(ClaimTypes.Email, user.Email),
                    new(ClaimTypes.Name, user.Name),
                    new("user_id", user.Id.ToString()),
                    new("email", user.Email),
                    new("name", user.Name),
                    new("auth_type", "apikey")
                };

                var identity = new ClaimsIdentity(claims, "ApiKey");
                var principal = new ClaimsPrincipal(identity);
                
                context.User = principal;
                
                logger.LogDebug("API key authentication successful for user {UserId}", user.Id);
            }
            else
            {
                logger.LogWarning("Invalid API key provided");
            }
        }

        await next(context);
    }

    private static string? ExtractApiKey(HttpRequest request)
    {
        // Check Authorization header (Bearer token)
        if (request.Headers.TryGetValue("Authorization", out var authHeader))
        {
            var authValue = authHeader.ToString();
            if (authValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return authValue["Bearer ".Length..].Trim();
            }
            else if (authValue.StartsWith("ApiKey ", StringComparison.OrdinalIgnoreCase))
            {
                return authValue["ApiKey ".Length..].Trim();
            }
        }

        // Check X-API-Key header
        if (request.Headers.TryGetValue("X-API-Key", out var apiKeyHeader))
        {
            return apiKeyHeader.ToString();
        }

        // Check query parameter
        if (request.Query.TryGetValue("api_key", out var apiKeyQuery))
        {
            return apiKeyQuery.ToString();
        }

        return null;
    }
}
