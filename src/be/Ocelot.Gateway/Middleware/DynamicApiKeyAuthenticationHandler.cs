using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Ocelot.Gateway.Configuration;

namespace Ocelot.Gateway.Middleware;

/// <summary>
///     Dynamic API Key authentication handler that validates with Identity service
/// </summary>
public class DynamicApiKeyAuthenticationHandler(
    IOptionsMonitor<ApiKeyAuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IHttpClientFactory httpClientFactory,
    IOptions<ApiKeySettings> apiKeySettings)
    : AuthenticationHandler<ApiKeyAuthenticationSchemeOptions>(options, logger, encoder)
{
    private readonly ApiKeySettings _apiKeySettings = apiKeySettings.Value;
    private readonly ILogger<DynamicApiKeyAuthenticationHandler> _logger = logger.CreateLogger<DynamicApiKeyAuthenticationHandler>();
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient("IdentityService");

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        try
        {
            // Check if API key header exists - if not, let other authentication schemes handle it
            if (!Request.Headers.TryGetValue(_apiKeySettings.HeaderName, out Microsoft.Extensions.Primitives.StringValues value))
            {
                _logger.LogDebug("API key header '{HeaderName}' not found - skipping API key authentication", _apiKeySettings.HeaderName);
                return AuthenticateResult.NoResult();
            }

            var apiKey = value.FirstOrDefault();
            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.LogWarning("API key header '{HeaderName}' found but value is empty", _apiKeySettings.HeaderName);
                return AuthenticateResult.Fail("API key value is empty");
            }

            // Log API key attempt (first 8 chars only for security)
            _logger.LogDebug("Attempting to validate API key: {ApiKeyPrefix}...", 
                apiKey[..Math.Min(8, apiKey.Length)]);

            // Validate API key with Identity service
            var validationResult = await ValidateApiKeyWithIdentityService(apiKey, GetOptions());
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Invalid API key provided: {ApiKey}... - {Reason}", 
                    apiKey[..Math.Min(8, apiKey.Length)], validationResult.ErrorMessage);
                return AuthenticateResult.Fail("Invalid API key");
            }

            // Create claims for the authenticated API key
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, validationResult.UserEmail ?? "ApiKeyUser"),
                new(ClaimTypes.NameIdentifier, validationResult.UserId ?? "unknown"),
                new("api_key_id", validationResult.ApiKeyId ?? "unknown"),
                new("client_type", "api_key"),
                new("auth_method", "api_key")
            };

            // Add scope claims
            if (validationResult.Scopes != null)
            {
                foreach (var scope in validationResult.Scopes)
                {
                    claims.Add(new Claim("scope", scope));
                }
            }

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            _logger.LogInformation("API key authentication successful for user: {UserEmail}, API Key ID: {ApiKeyId}",
                validationResult.UserEmail, validationResult.ApiKeyId);
            
            return AuthenticateResult.Success(ticket);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during API key authentication");
            return AuthenticateResult.Fail("Authentication error occurred");
        }
    }

    private JsonSerializerOptions GetOptions()
    {
        return new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    }

    private async Task<ApiKeyValidationResult> ValidateApiKeyWithIdentityService(string apiKey, JsonSerializerOptions options)
    {
        try
        {
            // Call Identity service to verify API key  
            // Send raw JSON string without extra quotes
            var requestContent = new StringContent(
                JsonSerializer.Serialize(apiKey),
                Encoding.UTF8,
                "application/json"
            );

            _logger.LogDebug("Calling Identity service to validate API key");

            var response = await _httpClient.PostAsync("api/v1/api-keys/verify", requestContent);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Identity service returned non-success status: {StatusCode}", response.StatusCode);
                return new ApiKeyValidationResult
                {
                    IsValid = false,
                    ErrorMessage = $"Identity service error: {response.StatusCode}"
                };
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var validationResponse = JsonSerializer.Deserialize<IdentityApiKeyValidationResponse>(
                responseContent,
options);

            if (validationResponse == null)
            {
                _logger.LogWarning("Invalid response from Identity service");
                return new ApiKeyValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Invalid response from Identity service"
                };
            }

            return new ApiKeyValidationResult
            {
                IsValid = validationResponse.IsValid,
                UserId = validationResponse.UserId,
                UserEmail = validationResponse.UserEmail,
                ApiKeyId = validationResponse.ApiKeyId,
                Scopes = validationResponse.Scopes,
                ErrorMessage = validationResponse.ErrorMessage
            };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error calling Identity service for API key validation");
            return new ApiKeyValidationResult
            {
                IsValid = false,
                ErrorMessage = "Identity service unavailable"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during API key validation");
            return new ApiKeyValidationResult
            {
                IsValid = false,
                ErrorMessage = "Validation error"
            };
        }
    }
}

/// <summary>
/// Response model from Identity service API key validation
/// </summary>
public class IdentityApiKeyValidationResponse
{
    public bool IsValid { get; set; }
    public string? UserId { get; set; }
    public string? UserEmail { get; set; }
    public string? ApiKeyId { get; set; }
    public string[]? Scopes { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Internal validation result model
/// </summary>
public class ApiKeyValidationResult
{
    public bool IsValid { get; set; }
    public string? UserId { get; set; }
    public string? UserEmail { get; set; }
    public string? ApiKeyId { get; set; }
    public string[]? Scopes { get; set; }
    public string? ErrorMessage { get; set; }
}