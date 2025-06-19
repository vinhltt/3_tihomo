using Google.Apis.Auth;
using Identity.Api.Models;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Identity.Api.Services;

/// <summary>
/// Enhanced token verification service with multi-layer caching and performance optimization
/// Service xác thực token nâng cao với caching đa tầng và tối ưu hiệu suất
/// </summary>
public class EnhancedTokenVerificationService : ITokenVerificationService
{
    private readonly IMemoryCache _memoryCache;
    private readonly IDistributedCache _distributedCache;
    private readonly HttpClient _httpClient;
    private readonly ILogger<EnhancedTokenVerificationService> _logger;
    private readonly IConfiguration _configuration;

    public EnhancedTokenVerificationService(
        IMemoryCache memoryCache,
        IDistributedCache distributedCache,
        HttpClient httpClient,
        ILogger<EnhancedTokenVerificationService> logger,
        IConfiguration configuration)
    {
        _memoryCache = memoryCache;
        _distributedCache = distributedCache;
        _httpClient = httpClient;
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// Verify token with provider using multi-layer caching strategy
    /// Xác thực token với provider sử dụng chiến lược caching đa tầng
    /// </summary>
    public async Task<SocialUserInfo?> VerifyTokenAsync(string provider, string token)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            // Step 1: Parse JWT locally first (validation without external API call)
            // Bước 1: Parse JWT locally trước (validation mà không gọi API ngoài)
            var jwt = ParseJwtToken(token);
            if (jwt == null || jwt.ValidTo < DateTime.UtcNow)
            {
                _logger.LogWarning("Token expired or malformed for provider {Provider}", provider);
                return null;
            }

            // Step 2: Check L1 cache (Memory) - 2 minute TTL for fast access
            // Bước 2: Kiểm tra L1 cache (Memory) - TTL 2 phút cho truy cập nhanh
            var cacheKey = $"token_verify:{provider}:{ComputeHash(token)}";
            if (_memoryCache.TryGetValue(cacheKey, out SocialUserInfo? cachedResult))
            {
                _logger.LogDebug("Token verification from L1 cache in {Duration}ms", stopwatch.ElapsedMilliseconds);
                return cachedResult;
            }

            // Step 3: Check L2 cache (Redis) - 5 minute TTL for persistence
            // Bước 3: Kiểm tra L2 cache (Redis) - TTL 5 phút cho persistence
            var distributedResult = await GetFromDistributedCacheAsync(cacheKey);
            if (distributedResult != null)
            {
                // Cache back to L1 for faster subsequent access
                _memoryCache.Set(cacheKey, distributedResult, TimeSpan.FromMinutes(2));
                _logger.LogDebug("Token verification from L2 cache in {Duration}ms", stopwatch.ElapsedMilliseconds);
                return distributedResult;
            }

            // Step 4: Verify with external provider API (last resort)
            // Bước 4: Xác thực với API provider bên ngoài (phương án cuối)
            var verificationResult = await VerifyWithProviderApiAsync(provider, token);
            
            if (verificationResult != null)
            {
                // Cache results in both layers for future requests
                // Cache kết quả ở cả 2 tầng cho các request sau
                await CacheVerificationResultAsync(cacheKey, verificationResult);
                _logger.LogInformation("Token verification completed via API in {Duration}ms", stopwatch.ElapsedMilliseconds);
            }
            else
            {
                _logger.LogWarning("Token verification failed for provider {Provider}", provider);
            }

            return verificationResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Token verification error for provider {Provider} after {Duration}ms", 
                provider, stopwatch.ElapsedMilliseconds);
            return null;
        }
    }

    /// <summary>
    /// Verify Google token using enhanced strategy
    /// Xác thực Google token sử dụng chiến lược nâng cao
    /// </summary>
    public async Task<SocialUserInfo?> VerifyGoogleTokenAsync(string token)
    {
        return await VerifyTokenAsync("google", token);
    }

    /// <summary>
    /// Verify Facebook token using enhanced strategy
    /// Xác thực Facebook token sử dụng chiến lược nâng cao  
    /// </summary>
    public async Task<SocialUserInfo?> VerifyFacebookTokenAsync(string token)
    {
        return await VerifyTokenAsync("facebook", token);
    }

    #region Private Helper Methods

    /// <summary>
    /// Parse JWT token locally without external API call
    /// Parse JWT token locally mà không gọi API ngoài
    /// </summary>
    private JwtSecurityToken? ParseJwtToken(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            return handler.ReadJwtToken(token);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to parse JWT token locally");
            return null;
        }
    }

    /// <summary>
    /// Compute secure hash for cache key from token
    /// Tính toán hash bảo mật cho cache key từ token
    /// </summary>
    private static string ComputeHash(string input)
    {
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(hash);
    }

    /// <summary>
    /// Get verification result from distributed cache (Redis)
    /// Lấy kết quả xác thực từ distributed cache (Redis)
    /// </summary>
    private async Task<SocialUserInfo?> GetFromDistributedCacheAsync(string key)
    {
        try
        {
            var cachedData = await _distributedCache.GetStringAsync(key);
            if (!string.IsNullOrEmpty(cachedData))
            {
                return JsonSerializer.Deserialize<SocialUserInfo>(cachedData);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get from distributed cache");
        }
        return null;
    }

    /// <summary>
    /// Cache verification result in both L1 and L2 cache
    /// Cache kết quả xác thực ở cả L1 và L2 cache
    /// </summary>
    private async Task CacheVerificationResultAsync(string cacheKey, SocialUserInfo result)
    {
        try
        {
            // L1 Cache (Memory) - 2 minute TTL
            _memoryCache.Set(cacheKey, result, TimeSpan.FromMinutes(2));

            // L2 Cache (Redis) - 5 minute TTL  
            var jsonData = JsonSerializer.Serialize(result);
            await _distributedCache.SetStringAsync(cacheKey, jsonData, 
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to cache verification result");
        }
    }

    /// <summary>
    /// Verify token with external provider API (Google/Facebook)
    /// Xác thực token với API provider bên ngoài (Google/Facebook)
    /// </summary>
    private async Task<SocialUserInfo?> VerifyWithProviderApiAsync(string provider, string token)
    {
        return provider.ToLower() switch
        {
            "google" => await VerifyGoogleTokenWithApiAsync(token),
            "facebook" => await VerifyFacebookTokenWithApiAsync(token),
            _ => null
        };
    }

    /// <summary>
    /// Verify Google token with Google API (fallback method)
    /// Xác thực Google token với Google API (phương thức fallback)
    /// </summary>
    private async Task<SocialUserInfo?> VerifyGoogleTokenWithApiAsync(string token)
    {
        try
        {
            var clientId = _configuration["GoogleAuth:ClientId"];
            if (string.IsNullOrEmpty(clientId))
            {
                _logger.LogError("Google Client ID not configured");
                return null;
            }

            var payload = await GoogleJsonWebSignature.ValidateAsync(token, 
                new GoogleJsonWebSignature.ValidationSettings
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
            _logger.LogError(ex, "Failed to verify Google token with API");
            return null;
        }
    }

    /// <summary>
    /// Verify Facebook token with Facebook API (fallback method)
    /// Xác thực Facebook token với Facebook API (phương thức fallback)
    /// </summary>
    private async Task<SocialUserInfo?> VerifyFacebookTokenWithApiAsync(string token)
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
            using var document = JsonDocument.Parse(userJson);
            var root = document.RootElement;
            
            return new SocialUserInfo
            {
                Id = root.GetProperty("id").GetString() ?? "",
                Email = root.TryGetProperty("email", out var emailProp) ? emailProp.GetString() ?? "" : "",
                Name = root.GetProperty("name").GetString() ?? "",
                PictureUrl = root.TryGetProperty("picture", out var pictureProp) 
                    ? pictureProp.GetProperty("data").GetProperty("url").GetString() 
                    : null,
                Provider = "Facebook"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to verify Facebook token with API");
            return null;
        }
    }

    #endregion
}
