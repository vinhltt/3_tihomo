# API Key Authentication

## 📋 Overview

Comprehensive authentication system cho API Key Management trong TiHoMo. Hỗ trợ dual authentication: JWT Bearer tokens cho web users và API key authentication cho third-party integrations.

---

## 🔐 Authentication Methods

### 1. JWT Bearer Token Authentication
Dành cho web application users truy cập API key management endpoints.

```http
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Use Cases:**
- Web app users managing their API keys
- Admin users accessing API key management
- Internal service-to-service communication

**Token Structure:**
```json
{
  "sub": "user-uuid",
  "email": "user@example.com",
  "role": "user",
  "iat": 1640995200,
  "exp": 1640998800,
  "iss": "http://localhost:5800",
  "aud": "tihomo-api"
}
```

### 2. API Key Authentication
Dành cho third-party applications accessing TiHoMo APIs.

```http
Authorization: Bearer pfm_abc123def456ghi789jkl012mno345pqr678stu901vwx234yz
```

**Use Cases:**
- Mobile applications
- Third-party integrations
- External services
- Automated scripts

**API Key Format:**
```
pfm_{random_string}
```
- Prefix: `pfm_` (Personal Finance Management)
- Length: 50 characters total
- Encoding: Base64url safe characters

---

## 🛡️ Security Implementation

### API Key Generation
```csharp
/// <summary>
/// API Key Hasher Service - Secure key generation và hashing
/// </summary>
public class ApiKeyHasher
{
    private const string KeyPrefix = "pfm_";
    private const int KeyLength = 45; // 50 total with prefix
    
    /// <summary>
    /// Generate API Key - Tạo API key mới
    /// </summary>
    public (string ApiKey, string KeyHash, string KeyPrefix) GenerateApiKey()
    {
        // Generate random bytes
        var randomBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        
        // Create API key
        var keyBody = Convert.ToBase64String(randomBytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "")
            .Substring(0, KeyLength);
        
        var apiKey = KeyPrefix + keyBody;
        var keyPrefix = KeyPrefix + keyBody.Substring(0, 6);
        
        // Hash the key for storage
        var keyHash = HashApiKey(apiKey);
        
        return (apiKey, keyHash, keyPrefix);
    }
    
    /// <summary>
    /// Hash API Key - Hash API key để lưu trữ
    /// </summary>
    public string HashApiKey(string apiKey)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(apiKey));
        return Convert.ToBase64String(hashedBytes);
    }
    
    /// <summary>
    /// Verify API Key - Xác thực API key
    /// </summary>
    public bool VerifyApiKey(string apiKey, string storedHash)
    {
        var computedHash = HashApiKey(apiKey);
        return computedHash.Equals(storedHash, StringComparison.Ordinal);
    }
}
```

### Authentication Middleware
```csharp
/// <summary>
/// API Key Authentication Middleware - Middleware xác thực API key
/// </summary>
public class ApiKeyAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IApiKeyService _apiKeyService;
    private readonly ILogger<ApiKeyAuthenticationMiddleware> _logger;
    
    public ApiKeyAuthenticationMiddleware(
        RequestDelegate next,
        IApiKeyService apiKeyService,
        ILogger<ApiKeyAuthenticationMiddleware> logger)
    {
        _next = next;
        _apiKeyService = apiKeyService;
        _logger = logger;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        // Skip authentication for certain paths
        if (ShouldSkipAuthentication(context.Request.Path))
        {
            await _next(context);
            return;
        }
        
        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
        if (authHeader == null || !authHeader.StartsWith("Bearer "))
        {
            await HandleUnauthorized(context, "Missing or invalid Authorization header");
            return;
        }
        
        var token = authHeader.Substring("Bearer ".Length).Trim();
        
        // Check if it's an API key (starts with pfm_)
        if (token.StartsWith("pfm_"))
        {
            await HandleApiKeyAuthentication(context, token);
        }
        else
        {
            // Handle JWT authentication (existing logic)
            await HandleJwtAuthentication(context, token);
        }
    }
    
    private async Task HandleApiKeyAuthentication(HttpContext context, string apiKey)
    {
        try
        {
            var result = await _apiKeyService.VerifyApiKeyAsync(apiKey);
            if (!result.IsValid)
            {
                await HandleUnauthorized(context, result.ErrorMessage);
                return;
            }
            
            // Set user context
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, result.UserId.ToString()),
                new("api_key_id", result.ApiKeyId.ToString()),
                new("auth_type", "api_key")
            };
            
            // Add scope claims
            foreach (var scope in result.Scopes)
            {
                claims.Add(new Claim("scope", scope));
            }
            
            var identity = new ClaimsIdentity(claims, "ApiKey");
            context.User = new ClaimsPrincipal(identity);
            
            // Log API key usage
            await LogApiKeyUsage(context, result.ApiKeyId);
            
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during API key authentication");
            await HandleUnauthorized(context, "Authentication failed");
        }
    }
    
    private async Task LogApiKeyUsage(HttpContext context, Guid apiKeyId)
    {
        var usageLog = new ApiKeyUsageLog
        {
            ApiKeyId = apiKeyId,
            Timestamp = DateTime.UtcNow,
            Method = context.Request.Method,
            Endpoint = context.Request.Path,
            IpAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            UserAgent = context.Request.Headers["User-Agent"].FirstOrDefault()
        };
        
        // Log asynchronously to avoid blocking request
        _ = Task.Run(async () =>
        {
            try
            {
                await _apiKeyService.LogUsageAsync(usageLog);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log API key usage");
            }
        });
    }
}
```

---

## 🔒 Authorization & Scopes

### Scope-Based Authorization
```csharp
/// <summary>
/// API Key Scope Authorization Attribute - Kiểm tra scope permissions
/// </summary>
public class RequireScopeAttribute : Attribute, IAuthorizationFilter
{
    private readonly string[] _requiredScopes;
    
    public RequireScopeAttribute(params string[] requiredScopes)
    {
        _requiredScopes = requiredScopes;
    }
    
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;
        
        // Check if user is authenticated
        if (!user.Identity?.IsAuthenticated ?? true)
        {
            context.Result = new UnauthorizedResult();
            return;
        }
        
        // Get user scopes
        var userScopes = user.FindAll("scope").Select(c => c.Value).ToList();
        
        // Check if user has required scopes
        var hasRequiredScope = _requiredScopes.Any(scope => 
            userScopes.Contains(scope) || 
            userScopes.Contains(ApiKeyScope.Admin));
        
        if (!hasRequiredScope)
        {
            context.Result = new ForbidResult();
            return;
        }
    }
}
```

### Usage Examples
```csharp
/// <summary>
/// Transaction Controller - Sử dụng scope-based authorization
/// </summary>
[ApiController]
[Route("api/core-finance/[controller]")]
public class TransactionController : ControllerBase
{
    /// <summary>
    /// Get Transactions - Requires read scope
    /// </summary>
    [HttpGet]
    [RequireScope(ApiKeyScope.Read, ApiKeyScope.TransactionsRead)]
    public async Task<IActionResult> GetTransactions()
    {
        // Implementation
    }
    
    /// <summary>
    /// Create Transaction - Requires write scope
    /// </summary>
    [HttpPost]
    [RequireScope(ApiKeyScope.Write, ApiKeyScope.TransactionsWrite)]
    public async Task<IActionResult> CreateTransaction(CreateTransactionRequest request)
    {
        // Implementation
    }
    
    /// <summary>
    /// Delete Transaction - Requires admin scope
    /// </summary>
    [HttpDelete("{id}")]
    [RequireScope(ApiKeyScope.Admin)]
    public async Task<IActionResult> DeleteTransaction(Guid id)
    {
        // Implementation
    }
}
```

---

## 🚦 Rate Limiting

### Per-API-Key Rate Limiting
```csharp
/// <summary>
/// API Key Rate Limiting Middleware - Rate limiting per API key
/// </summary>
public class ApiKeyRateLimitMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IMemoryCache _cache;
    private readonly ILogger<ApiKeyRateLimitMiddleware> _logger;
    
    public ApiKeyRateLimitMiddleware(
        RequestDelegate next,
        IMemoryCache cache,
        ILogger<ApiKeyRateLimitMiddleware> logger)
    {
        _next = next;
        _cache = cache;
        _logger = logger;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        // Skip if not API key authentication
        if (!context.User.HasClaim("auth_type", "api_key"))
        {
            await _next(context);
            return;
        }
        
        var apiKeyId = context.User.FindFirst("api_key_id")?.Value;
        if (string.IsNullOrEmpty(apiKeyId))
        {
            await _next(context);
            return;
        }
        
        // Check rate limit
        var rateLimitResult = await CheckRateLimit(apiKeyId);
        if (!rateLimitResult.IsAllowed)
        {
            await HandleRateLimitExceeded(context, rateLimitResult);
            return;
        }
        
        await _next(context);
    }
    
    private async Task<RateLimitResult> CheckRateLimit(string apiKeyId)
    {
        var cacheKey = $"rate_limit:{apiKeyId}";
        var now = DateTime.UtcNow;
        var windowStart = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0);
        
        var rateLimitInfo = _cache.Get<RateLimitInfo>(cacheKey);
        if (rateLimitInfo == null || rateLimitInfo.WindowStart != windowStart)
        {
            rateLimitInfo = new RateLimitInfo
            {
                WindowStart = windowStart,
                RequestCount = 0,
                Limit = 100 // Default limit, should be from API key settings
            };
        }
        
        rateLimitInfo.RequestCount++;
        
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpiration = windowStart.AddMinutes(1)
        };
        _cache.Set(cacheKey, rateLimitInfo, cacheOptions);
        
        return new RateLimitResult
        {
            IsAllowed = rateLimitInfo.RequestCount <= rateLimitInfo.Limit,
            RequestCount = rateLimitInfo.RequestCount,
            Limit = rateLimitInfo.Limit,
            ResetTime = windowStart.AddMinutes(1)
        };
    }
}
```

---

## 🌐 IP Whitelisting

### IP Address Validation
```csharp
/// <summary>
/// IP Whitelist Middleware - Validate IP addresses
/// </summary>
public class IpWhitelistMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IApiKeyService _apiKeyService;
    private readonly ILogger<IpWhitelistMiddleware> _logger;
    
    public IpWhitelistMiddleware(
        RequestDelegate next,
        IApiKeyService apiKeyService,
        ILogger<IpWhitelistMiddleware> logger)
    {
        _next = next;
        _apiKeyService = apiKeyService;
        _logger = logger;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        // Skip if not API key authentication
        if (!context.User.HasClaim("auth_type", "api_key"))
        {
            await _next(context);
            return;
        }
        
        var apiKeyId = context.User.FindFirst("api_key_id")?.Value;
        if (string.IsNullOrEmpty(apiKeyId) || !Guid.TryParse(apiKeyId, out var keyId))
        {
            await _next(context);
            return;
        }
        
        // Get client IP
        var clientIp = GetClientIpAddress(context);
        if (string.IsNullOrEmpty(clientIp))
        {
            await HandleForbidden(context, "Unable to determine client IP address");
            return;
        }
        
        // Check IP whitelist
        var isAllowed = await _apiKeyService.IsIpAllowedAsync(keyId, clientIp);
        if (!isAllowed)
        {
            _logger.LogWarning("IP address {ClientIp} not allowed for API key {ApiKeyId}", 
                clientIp, keyId);
            await HandleForbidden(context, "IP address not allowed");
            return;
        }
        
        await _next(context);
    }
    
    private string? GetClientIpAddress(HttpContext context)
    {
        // Check X-Forwarded-For header (for load balancers/proxies)
        var xForwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(xForwardedFor))
        {
            return xForwardedFor.Split(',').First().Trim();
        }
        
        // Check X-Real-IP header
        var xRealIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(xRealIp))
        {
            return xRealIp;
        }
        
        // Use connection remote IP
        return context.Connection.RemoteIpAddress?.ToString();
    }
}
```

---

## 🔧 Configuration

### Authentication Configuration
```csharp
/// <summary>
/// Authentication Configuration - Cấu hình authentication
/// </summary>
public class AuthenticationConfiguration
{
    public JwtSettings Jwt { get; set; } = new();
    public ApiKeySettings ApiKey { get; set; } = new();
}

public class JwtSettings
{
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public int ExpirationMinutes { get; set; } = 60;
}

public class ApiKeySettings
{
    public string Prefix { get; set; } = "pfm_";
    public int DefaultRateLimitPerMinute { get; set; } = 100;
    public int DefaultDailyUsageQuota { get; set; } = 10000;
    public bool RequireHttps { get; set; } = true;
    public int MaxApiKeysPerUser { get; set; } = 10;
}
```

### Startup Configuration
```csharp
/// <summary>
/// Startup Configuration - Cấu hình authentication services
/// </summary>
public void ConfigureServices(IServiceCollection services)
{
    // JWT Authentication
    services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = Configuration["Jwt:Issuer"],
                ValidAudience = Configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(Configuration["Jwt:SecretKey"]))
            };
        });
    
    // API Key Services
    services.AddScoped<IApiKeyService, ApiKeyService>();
    services.AddScoped<ApiKeyHasher>();
    services.AddMemoryCache(); // For rate limiting
    
    // Authorization
    services.AddAuthorization(options =>
    {
        options.AddPolicy("ApiKeyOrJwt", policy =>
        {
            policy.RequireAuthenticatedUser();
            policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
        });
    });
}

public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    // Order matters!
    app.UseAuthentication();
    app.UseMiddleware<ApiKeyAuthenticationMiddleware>();
    app.UseMiddleware<ApiKeyRateLimitMiddleware>();
    app.UseMiddleware<IpWhitelistMiddleware>();
    app.UseAuthorization();
}
```

---

## 📊 Monitoring & Logging

### Authentication Events
```csharp
/// <summary>
/// Authentication Events - Log authentication events
/// </summary>
public class AuthenticationEvents
{
    public static class EventIds
    {
        public static readonly EventId ApiKeyAuthenticated = new(1001, "ApiKeyAuthenticated");
        public static readonly EventId ApiKeyAuthenticationFailed = new(1002, "ApiKeyAuthenticationFailed");
        public static readonly EventId RateLimitExceeded = new(1003, "RateLimitExceeded");
        public static readonly EventId IpAddressBlocked = new(1004, "IpAddressBlocked");
        public static readonly EventId SuspiciousActivity = new(1005, "SuspiciousActivity");
    }
}

/// <summary>
/// Security Event Logger - Log security events
/// </summary>
public class SecurityEventLogger
{
    private readonly ILogger<SecurityEventLogger> _logger;
    
    public SecurityEventLogger(ILogger<SecurityEventLogger> logger)
    {
        _logger = logger;
    }
    
    public void LogApiKeyAuthenticated(Guid apiKeyId, string ipAddress, string userAgent)
    {
        _logger.LogInformation(AuthenticationEvents.EventIds.ApiKeyAuthenticated,
            "API key {ApiKeyId} authenticated from IP {IpAddress} with user agent {UserAgent}",
            apiKeyId, ipAddress, userAgent);
    }
    
    public void LogAuthenticationFailed(string apiKey, string reason, string ipAddress)
    {
        _logger.LogWarning(AuthenticationEvents.EventIds.ApiKeyAuthenticationFailed,
            "API key authentication failed for key {ApiKeyPrefix} from IP {IpAddress}. Reason: {Reason}",
            apiKey.Substring(0, Math.Min(10, apiKey.Length)), ipAddress, reason);
    }
    
    public void LogRateLimitExceeded(Guid apiKeyId, string ipAddress, int requestCount, int limit)
    {
        _logger.LogWarning(AuthenticationEvents.EventIds.RateLimitExceeded,
            "Rate limit exceeded for API key {ApiKeyId} from IP {IpAddress}. Requests: {RequestCount}, Limit: {Limit}",
            apiKeyId, ipAddress, requestCount, limit);
    }
}
```

---

## 🧪 Testing

### Authentication Tests
```csharp
/// <summary>
/// API Key Authentication Tests - Test authentication logic
/// </summary>
public class ApiKeyAuthenticationTests
{
    [Test]
    public async Task ValidApiKey_ShouldAuthenticate()
    {
        // Arrange
        var apiKey = "pfm_validkey123";
        var mockService = new Mock<IApiKeyService>();
        mockService.Setup(s => s.VerifyApiKeyAsync(apiKey))
            .ReturnsAsync(new ApiKeyVerificationResult
            {
                IsValid = true,
                UserId = Guid.NewGuid(),
                ApiKeyId = Guid.NewGuid(),
                Scopes = new[] { "read", "write" }
            });
        
        // Act & Assert
        // Test implementation
    }
    
    [Test]
    public async Task InvalidApiKey_ShouldRejectAuthentication()
    {
        // Arrange
        var apiKey = "pfm_invalidkey123";
        var mockService = new Mock<IApiKeyService>();
        mockService.Setup(s => s.VerifyApiKeyAsync(apiKey))
            .ReturnsAsync(new ApiKeyVerificationResult
            {
                IsValid = false,
                ErrorMessage = "Invalid API key"
            });
        
        // Act & Assert
        // Test implementation
    }
    
    [Test]
    public async Task RateLimitExceeded_ShouldReturnTooManyRequests()
    {
        // Test rate limiting logic
    }
    
    [Test]
    public async Task IpNotWhitelisted_ShouldReturnForbidden()
    {
        // Test IP whitelisting logic
    }
}
```

---

## 🔐 Security Best Practices

### 1. Key Management
- **Never log full API keys** - Only log prefixes
- **Use secure random generation** - Cryptographically secure RNG
- **Hash keys for storage** - Never store plain text keys
- **Implement key rotation** - Allow users to regenerate keys

### 2. Rate Limiting
- **Per-key limits** - Individual limits for each API key
- **Burst protection** - Handle traffic spikes gracefully
- **Sliding window** - More accurate than fixed window
- **Different limits per scope** - Higher limits for read-only operations

### 3. IP Whitelisting
- **Support CIDR notation** - Allow IP ranges
- **Handle proxy headers** - X-Forwarded-For, X-Real-IP
- **Log blocked attempts** - Monitor for suspicious activity
- **Flexible configuration** - Optional per-key setting

### 4. Monitoring
- **Comprehensive logging** - All authentication events
- **Anomaly detection** - Unusual usage patterns
- **Real-time alerts** - Immediate notification of issues
- **Usage analytics** - Track API key usage patterns

---

*Last updated: December 28, 2024*
*Status: Design Complete, Implementation Ready* 