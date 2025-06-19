using Google.Apis.Auth;
using Identity.Api.Models;
using System.IdentityModel.Tokens.Jwt;

namespace Identity.Api.Services;

public interface ITokenVerificationService
{
    Task<SocialUserInfo?> VerifyGoogleTokenAsync(string token);
    Task<SocialUserInfo?> VerifyFacebookTokenAsync(string token);
    Task<SocialUserInfo?> VerifyTokenAsync(string provider, string token);
}

public class SocialUserInfo
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? PictureUrl { get; set; }
    public string Provider { get; set; } = string.Empty;
}

public class TokenVerificationService : ITokenVerificationService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<TokenVerificationService> _logger;
    private readonly IConfiguration _configuration;

    public TokenVerificationService(
        HttpClient httpClient, 
        ILogger<TokenVerificationService> logger,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<SocialUserInfo?> VerifyTokenAsync(string provider, string token)
    {
        return provider.ToLower() switch
        {
            "google" => await VerifyGoogleTokenAsync(token),
            "facebook" => await VerifyFacebookTokenAsync(token),
            _ => null
        };
    }

    public async Task<SocialUserInfo?> VerifyGoogleTokenAsync(string token)
    {
        try
        {
            var clientId = _configuration["GoogleAuth:ClientId"];
            if (string.IsNullOrEmpty(clientId))
            {
                _logger.LogError("Google Client ID not configured");
                return null;
            }

            var payload = await GoogleJsonWebSignature.ValidateAsync(token, new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { clientId }
            });

            return new SocialUserInfo
            {
                Id = payload.Subject,
                Email = payload.Email,
                Name = payload.Name,
                PictureUrl = payload.Picture,
                Provider = "Google"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to verify Google token");
            return null;
        }
    }

    public async Task<SocialUserInfo?> VerifyFacebookTokenAsync(string token)
    {
        try
        {
            var appId = _configuration["FacebookAuth:AppId"];
            var appSecret = _configuration["FacebookAuth:AppSecret"];
            
            if (string.IsNullOrEmpty(appId) || string.IsNullOrEmpty(appSecret))
            {
                _logger.LogError("Facebook App ID or Secret not configured");
                return null;
            }

            // Verify token with Facebook Graph API
            var verifyUrl = $"https://graph.facebook.com/debug_token?input_token={token}&access_token={appId}|{appSecret}";
            var verifyResponse = await _httpClient.GetAsync(verifyUrl);
            
            if (!verifyResponse.IsSuccessStatusCode)
            {
                _logger.LogError("Facebook token verification failed");
                return null;
            }

            // Get user info
            var userUrl = $"https://graph.facebook.com/me?fields=id,name,email,picture&access_token={token}";
            var userResponse = await _httpClient.GetAsync(userUrl);
            
            if (!userResponse.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to get Facebook user info");
                return null;
            }

            var userJson = await userResponse.Content.ReadAsStringAsync();
            var userInfo = System.Text.Json.JsonSerializer.Deserialize<dynamic>(userJson);
            
            // Note: This is a simplified implementation. In production, you'd want better JSON handling
            return new SocialUserInfo
            {
                Id = userInfo?.GetProperty("id").GetString() ?? "",
                Email = userInfo?.GetProperty("email").GetString() ?? "",
                Name = userInfo?.GetProperty("name").GetString() ?? "",
                PictureUrl = userInfo?.GetProperty("picture")?.GetProperty("data")?.GetProperty("url")?.GetString(),
                Provider = "Facebook"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to verify Facebook token");
            return null;
        }
    }
}
