using Identity.Application.Services.ApiKeys;
using System.Security.Claims;

namespace Identity.Sso.Middleware;

public class ApiKeyAuthenticationMiddleware(RequestDelegate next)
{
    private const string ApiKeyHeaderName = "X-API-Key";

    public async Task InvokeAsync(HttpContext context, IApiKeyService apiKeyService)
    {
        // Only process requests that don't already have authentication
        if (!context.User.Identity?.IsAuthenticated == true && context.Request.Headers.ContainsKey(ApiKeyHeaderName))
        {
            var apiKey = context.Request.Headers[ApiKeyHeaderName].FirstOrDefault();
              if (!string.IsNullOrEmpty(apiKey))
            {
                var validationResult = await apiKeyService.VerifyApiKeyAsync(apiKey);
                  if (validationResult != null && validationResult.IsValid && validationResult.UserId.HasValue)
                {
                    // Create claims for the API key user
                    var claims = new List<Claim>
                    {
                        new(ClaimTypes.NameIdentifier, validationResult.UserId.Value.ToString()),
                        new("sub", validationResult.UserId.Value.ToString()),
                        new("auth_type", "api_key")
                    };// Add scope claims
                    foreach (var scope in validationResult.Scopes)
                    {
                        claims.Add(new Claim("scope", scope));
                    }

                    var identity = new ClaimsIdentity(claims, "ApiKey");
                    context.User = new ClaimsPrincipal(identity);
                }
            }
        }

        await next(context);
    }
}
