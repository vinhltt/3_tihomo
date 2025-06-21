using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Ocelot.Gateway.Configuration;

namespace Ocelot.Gateway.Middleware;

/// <summary>
///     API Key authentication handler for external service access
/// </summary>
public class ApiKeyAuthenticationHandler(
    IOptionsMonitor<ApiKeyAuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IOptions<ApiKeySettings> apiKeySettings)
    : AuthenticationHandler<ApiKeyAuthenticationSchemeOptions>(options, logger, encoder)
{
    private readonly ApiKeySettings _apiKeySettings = apiKeySettings.Value;
    private readonly ILogger<ApiKeyAuthenticationHandler> _logger = logger.CreateLogger<ApiKeyAuthenticationHandler>();

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        try
        {
            // Check if API key header exists
            if (!Request.Headers.ContainsKey(_apiKeySettings.HeaderName))
            {
                _logger.LogWarning("API key header '{HeaderName}' not found in request", _apiKeySettings.HeaderName);
                return Task.FromResult(AuthenticateResult.Fail("API key header not found"));
            }

            var apiKey = Request.Headers[_apiKeySettings.HeaderName].FirstOrDefault();
            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.LogWarning("API key value is empty");
                return Task.FromResult(AuthenticateResult.Fail("API key value is empty"));
            }

            // Validate API key
            if (!_apiKeySettings.ValidApiKeys.ContainsKey(apiKey))
            {
                _logger.LogWarning("Invalid API key provided: {ApiKey}", apiKey[..Math.Min(8, apiKey.Length)] + "...");
                return Task.FromResult(AuthenticateResult.Fail("Invalid API key"));
            }

            // Create claims for the authenticated API key
            var clientName = _apiKeySettings.ValidApiKeys[apiKey];
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, clientName),
                new(ClaimTypes.NameIdentifier, apiKey),
                new("client_type", "external_api"),
                new("auth_method", "api_key")
            };

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            _logger.LogInformation("API key authentication successful for client: {ClientName}", clientName);
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during API key authentication");
            return Task.FromResult(AuthenticateResult.Fail("Authentication error occurred"));
        }
    }
}

/// <summary>
///     Options for API Key authentication scheme
/// </summary>
public class ApiKeyAuthenticationSchemeOptions : AuthenticationSchemeOptions
{
    public const string DefaultScheme = "ApiKey";
}