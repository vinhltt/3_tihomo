# Identity Service - Complete Design Document

## 1. Overview

Identity Service là bounded context cốt lõi của hệ thống TiHoMo, chịu trách nhiệm xác thực, phân quyền và quản lý người dùng. Thiết kế này tập trung vào social login integration (Google, Facebook, Apple) với stateless authentication pattern thông qua API Gateway.

Service đã phát triển qua 4 phases chính để trở thành production-ready service với advanced features:

- **Phase 1**: Basic Authentication & Token Verification  
- **Phase 2**: Refresh Token Management & Security Enhancement
- **Phase 3**: Resilience Patterns & Circuit Breaker Implementation
- **Phase 4**: Monitoring & Observability System

### 1.1 Mục tiêu chính
- Cung cấp xác thực đa dạng (Social Login, API Key)
- Quản lý người dùng và phân quyền đơn giản
- Stateless authentication với high performance
- Tích hợp seamless với API Gateway
- Đảm bảo bảo mật và khả năng scale

### 1.2 Problem Statement

Hệ thống SSO truyền thống thường phức tạp với:
- Multiple authentication flows (OpenId Connect, IdentityServer4, custom sessions)
- Complex database schema với nhiều bảng không cần thiết
- Complicated redirect flows giữa multiple services
- Khó maintain và debug

### 1.3 Proposed Solution

Simplified design sử dụng pattern **stateless token verification** thông qua API Gateway, dựa trên proven patterns từ Microsoft eShop và ABP Framework.

## 2. Kiến trúc tổng quan (Primary Approach)

### 2.1 High-Level Architecture

```
┌─────────────────┐    ┌──────────────────┐    ┌─────────────────┐    ┌──────────────────┐
│   Nuxt.js SPA   │    │ Ocelot Gateway   │    │ Identity API    │    │   Other APIs     │
│                 │    │                  │    │                 │    │ (CoreFinance,    │
│                 │    │                  │    │                 │    │  MoneyMgmt, etc) │
└─────────────────┘    └──────────────────┘    └─────────────────┘    └──────────────────┘
         │                        │                        │                        │
         │ 1. Login Social        │                        │                        │
         │    Get access_token    │                        │                        │
         │                        │                        │                        │
         │ 2. API calls with      │                        │                        │
         │    Authorization:      │                        │                        │
         │    Bearer {token}      │                        │                        │
         └────────────────────────┤                        │                        │
                                  │ 3. Forward to Identity │                        │
                                  │    for token verify    │                        │
                                  └────────────────────────┤                        │
                                                           │ 4. Verify token,       │
                                                           │    get/create user     │
                                                           │                        │
                                                           │ 5. Return user claims  │
                                                           ├────────────────────────┤
                                                           │ 6. Forward to target   │
                                                           │    service with claims │
                                                           └────────────────────────┘
```

### 2.2 Core Components

#### Frontend (Nuxt.js)
```javascript
// plugins/auth.client.js
export default defineNuxtPlugin(() => {
  const { $google, $facebook, $apple } = useNuxtApp()
  
  const login = async (provider) => {
    let result
    switch (provider) {
      case 'google':
        result = await $google.signIn()
        break
      case 'facebook':
        result = await $facebook.login()
        break
      case 'apple':
        result = await $apple.signIn()
        break
    }
    
    // Store access token for API calls
    const authStore = useAuthStore()
    authStore.setToken(result.access_token)
    
    return result
  }
  
  return {
    provide: {
      auth: { login }
    }
  }
})
```

#### API Gateway (Ocelot)
```csharp
// Program.cs
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = "http://localhost:5001"; // Identity API
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

// Custom middleware for token forwarding
builder.Services.AddHttpClient("identity-verify", client =>
{
    client.BaseAddress = new Uri("http://localhost:5001");
});
```

#### Identity API
```csharp
// Models/User.cs
public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public string Name { get; set; }
    public string AvatarUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsActive { get; set; } = true;
    
    public List<UserLogin> Logins { get; set; } = new();
    public List<ApiKey> ApiKeys { get; set; } = new();
}

// Controllers/AuthController.cs
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly TokenVerificationService _tokenVerificationService;
    
    [HttpPost("verify")]
    public async Task<TokenVerificationResult> VerifyToken([FromBody] VerifyTokenRequest request)
    {
        var result = await _tokenVerificationService.VerifyAsync(request.Token, request.Provider);
        return result;
    }
}
```

### 2.3 Database Schema

```sql
-- Users table
CREATE TABLE Users (
    Id uniqueidentifier PRIMARY KEY DEFAULT NEWID(),
    Email nvarchar(256) NOT NULL,
    Name nvarchar(256) NOT NULL,
    AvatarUrl nvarchar(512),
    CreatedAt datetime2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt datetime2 NOT NULL DEFAULT GETUTCDATE(),
    IsActive bit NOT NULL DEFAULT 1
);

-- User logins (external providers)
CREATE TABLE UserLogins (
    Id uniqueidentifier PRIMARY KEY DEFAULT NEWID(),
    UserId uniqueidentifier NOT NULL,
    Provider nvarchar(50) NOT NULL, -- Google, Facebook, Apple
    ProviderUserId nvarchar(256) NOT NULL,
    CreatedAt datetime2 NOT NULL DEFAULT GETUTCDATE(),
    
    FOREIGN KEY (UserId) REFERENCES Users(Id),
    UNIQUE(Provider, ProviderUserId)
);

-- API Keys for third-party integration
CREATE TABLE ApiKeys (
    Id uniqueidentifier PRIMARY KEY DEFAULT NEWID(),
    UserId uniqueidentifier NOT NULL,
    Name nvarchar(256) NOT NULL,
    HashedKey nvarchar(512) NOT NULL,
    KeyPrefix nvarchar(16) NOT NULL, -- First 8 chars for identification
    Scopes nvarchar(max), -- JSON array of scopes
    Status nvarchar(20) NOT NULL DEFAULT 'Active', -- Active, Revoked, Expired
    CreatedAt datetime2 NOT NULL DEFAULT GETUTCDATE(),
    ExpiresAt datetime2,
    LastUsedAt datetime2,
    UsageCount int NOT NULL DEFAULT 0,
    
    FOREIGN KEY (UserId) REFERENCES Users(Id)
);

-- Indexes
CREATE INDEX IX_Users_Email ON Users(Email);
CREATE INDEX IX_UserLogins_UserId ON UserLogins(UserId);
CREATE INDEX IX_UserLogins_Provider_ProviderUserId ON UserLogins(Provider, ProviderUserId);
CREATE INDEX IX_ApiKeys_UserId ON ApiKeys(UserId);
CREATE INDEX IX_ApiKeys_KeyPrefix ON ApiKeys(KeyPrefix);
```

### 2.4 API Endpoints

**Authentication:**
- `POST /api/auth/verify` - Token verification (internal)
- `POST /api/auth/login/google` - Google login
- `POST /api/auth/login/facebook` - Facebook login  
- `POST /api/auth/login/apple` - Apple login

**User Management:**
- `GET /api/users/me` - Get current user profile
- `GET /api/users/{id}` - Get user by ID
- `PUT /api/users/{id}` - Update user profile

**API Key Management:**
- `POST /api/apikeys` - Create API key
- `GET /api/apikeys` - List user's API keys
- `DELETE /api/apikeys/{id}` - Revoke API key

## 3. Request Flow Analysis

### 3.1 Overall System Flow

```mermaid
sequenceDiagram
    participant Frontend as Nuxt.js SPA
    participant Gateway as Ocelot Gateway
    participant Identity as Identity API
    participant Google as Google API
    participant Target as Target Service

    %% Phase 1: Authentication
    Frontend->>Google: 1. Login with Google SDK
    Google-->>Frontend: 2. Return access token
    Frontend->>Frontend: 3. Store token locally

    %% Phase 2: API Request
    Frontend->>Gateway: 4. API request with Bearer token
    Gateway->>Identity: 5. Verify token
    Identity->>Google: 6. Validate with Google API
    Google-->>Identity: 7. Return user info
    Identity->>Identity: 8. Get/Create user in DB
    Identity-->>Gateway: 9. Return user claims
    Gateway->>Target: 10. Forward request with user claims
    Target-->>Gateway: 11. Return response
    Gateway-->>Frontend: 12. Return final response
```

### 3.2 Detailed Flow Breakdown

#### Phase 1: User Authentication (Social Login)
```javascript
// Frontend (Nuxt.js)
const loginWithGoogle = async () => {
  // 1. Initiate Google OAuth flow
  const credential = await google.accounts.id.prompt()
  
  // 2. Store token for subsequent API calls
  const authStore = useAuthStore()
  authStore.setToken(credential)
  
  return credential
}
```

#### Phase 2: API Request Flow
```csharp
// Gateway - Authentication Middleware
public class BearerTokenAuthenticationHandler : AuthenticationHandler
{
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Extract token from header
        var token = GetTokenFromHeader();
        
        // Verify with Identity service
        var verificationResult = await _identityClient.VerifyTokenAsync(token);
        
        if (verificationResult.IsValid)
        {
            var claims = CreateClaimsFromResult(verificationResult);
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "Bearer"));
            return AuthenticateResult.Success(new AuthenticationTicket(principal, "Bearer"));
        }
        
        return AuthenticateResult.Fail("Invalid token");
    }
}
```

#### Phase 3: Target Service Processing  
```csharp
// CoreFinance.Api - TransactionController
[HttpGet]
[Authorize] // Gateway đã verify token và inject claims
public async Task<IActionResult> GetTransactions()
{
    // Gateway đã inject user claims vào headers:
    // X-User-Id: {userId}
    // X-User-Email: {email}
    var userId = HttpContext.Request.Headers["X-User-Id"];
    
    var transactions = await _transactionService.GetUserTransactionsAsync(userId);
    return Ok(transactions);
}
```

### 3.3 Alternative Flow: API Key Authentication

```mermaid
sequenceDiagram
    participant ThirdParty as Third-Party Client
    participant Gateway as Ocelot Gateway  
    participant Identity as Identity API
    participant Target as Target Service

    ThirdParty->>Gateway: Request with ApiKey header
    Gateway->>Identity: Verify API key
    Identity->>Identity: Validate key & get user
    Identity-->>Gateway: Return user claims
    Gateway->>Target: Forward with user context
    Target-->>Gateway: Return response
    Gateway-->>ThirdParty: Return response
```

## 4. Key Advantages

### ✅ Stateless & Scalable
- Không có server-side sessions
- Gateway có thể scale horizontally
- Identity service chỉ cần verify token, không maintain state

### ✅ Security
- Token verification với social provider mỗi request (hoặc với caching TTL ngắn)
- API key được hash trong database
- Claims được inject an toàn qua headers

### ✅ Performance
- Token verification có thể được cache (5 phút TTL)  
- Single hop tới Identity service
- Target services không cần gọi Identity service

### ✅ Simplicity
- Loại bỏ OAuth2/OIDC server complexity
- Straight-forward social login flow
- Clear separation of concerns

## 5. Error Handling & Edge Cases

### 5.1 Token Refresh Flow
```mermaid
sequenceDiagram
    participant Frontend as Nuxt.js SPA
    participant Google as Google API
    
    Frontend->>Google: Check token expiry
    alt Token expired
        Frontend->>Google: Request new token
        Google-->>Frontend: Return fresh token
        Frontend->>Frontend: Update stored tokens
    end
```

### 5.2 Rate Limiting & Abuse Prevention
- Gateway implements rate limiting per IP/user
- Identity service tracks failed verification attempts  
- API keys có usage quotas và rate limits

## 6. Alternative Architecture (Complex Approach - Reference)

### 6.1 Dual Service Architecture

**Identity.Sso (Port 5217) - SSO Server:**
- Single Sign-On server cho OAuth2/OIDC flows
- Login/Register/Consent pages với Razor Views
- Cookie-based authentication cho UI sessions
- Target Users: End users thông qua browser interface

**Identity.Api (Port 5228) - Management API:**
- REST API để quản lý users, roles, API keys  
- JWT Bearer tokens và API Key authentication
- Target Users: Applications, admins, third-party integrations

### 6.2 OAuth2/OIDC Flows
```mermaid
sequenceDiagram
    participant Client as Web App
    participant SSO as Identity.Sso
    participant User as End User
    participant DB as db_identity

    Client->>SSO: GET /connect/authorize
    SSO->>User: Show login page
    User->>SSO: Submit credentials
    SSO->>DB: Validate user
    SSO->>Client: Redirect with auth code
    Client->>SSO: POST /connect/token {code}
    SSO->>DB: Verify code
    SSO->>Client: Return access_token + refresh_token
```

## 7. Production-Ready Improvements ⚠️ **CRITICAL FIXES NEEDED**

### 7.1 Enhanced Token Verification Strategy 🔥 **HIGH PRIORITY**

**Issue**: Current design verifies Google token on every request - MAJOR performance and security risk.

**Root Cause**: 
```csharp
// ❌ PROBLEMATIC: Current implementation
var googleResponse = await _httpClient.GetAsync(
    $"https://oauth2.googleapis.com/tokeninfo?id_token={request.Token}");
// This calls Google API on EVERY request!
```

**Solution**: Multi-layer validation with local verification and intelligent caching.

```csharp
// ✅ PRODUCTION-READY: Enhanced Token Validation
public class EnhancedTokenVerificationService : ITokenVerificationService
{
    private readonly IMemoryCache _cache;
    private readonly IDistributedCache _distributedCache;
    private readonly HttpClient _httpClient;
    private readonly ILogger<EnhancedTokenVerificationService> _logger;
    
    public async Task<TokenVerificationResult> VerifyTokenAsync(string token, string provider)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            // Step 1: Parse JWT locally first (validation without API call)
            var jwt = ParseJwtToken(token);
            if (jwt == null || jwt.ValidTo < DateTime.UtcNow)
            {
                return TokenVerificationResult.Invalid("Token expired or malformed");
            }
            
            // Step 2: Check L1 cache (Memory) - 2 minute TTL
            var cacheKey = $"token_verify:{provider}:{ComputeHash(token)}";
            if (_cache.TryGetValue(cacheKey, out TokenVerificationResult cachedResult))
            {
                _logger.LogDebug("Token verification from L1 cache in {Duration}ms", stopwatch.ElapsedMilliseconds);
                return cachedResult;
            }
            
            // Step 3: Check L2 cache (Redis) - 5 minute TTL
            var distributedResult = await GetFromDistributedCache(cacheKey);
            if (distributedResult != null)
            {
                _cache.Set(cacheKey, distributedResult, TimeSpan.FromMinutes(2));
                _logger.LogDebug("Token verification from L2 cache in {Duration}ms", stopwatch.ElapsedMilliseconds);
                return distributedResult;
            }
            
            // Step 4: Only verify with Google when necessary
            var verificationResult = await VerifyWithGoogleApi(token);
            
            // Cache results if valid
            if (verificationResult.IsValid)
            {
                var cacheDuration = TimeSpan.FromMinutes(2);
                _cache.Set(cacheKey, verificationResult, cacheDuration);
                await _distributedCache.SetStringAsync(cacheKey, 
                    JsonSerializer.Serialize(verificationResult),
                    new DistributedCacheEntryOptions 
                    { 
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) 
                    });
            }
            
            _logger.LogInformation("Token verification completed in {Duration}ms, Valid: {IsValid}", 
                stopwatch.ElapsedMilliseconds, verificationResult.IsValid);
                
            return verificationResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Token verification failed in {Duration}ms", stopwatch.ElapsedMilliseconds);
            return TokenVerificationResult.Invalid("Verification failed");
        }
    }
    
    private JwtSecurityToken ParseJwtToken(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            return handler.ReadJwtToken(token);
        }
        catch
        {
            return null;
        }
    }
    
    private string ComputeHash(string input)
    {
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(hash);
    }
    
    private async Task<TokenVerificationResult> VerifyWithGoogleApi(string token)
    {
        var response = await _httpClient.GetAsync(
            $"https://oauth2.googleapis.com/tokeninfo?id_token={token}");
            
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var tokenInfo = JsonSerializer.Deserialize<GoogleTokenInfo>(content);
            
            return new TokenVerificationResult
            {
                IsValid = true,
                UserId = tokenInfo.Sub,
                Email = tokenInfo.Email,
                Name = tokenInfo.Name,
                Provider = "Google",
                ExpiresAt = DateTimeOffset.FromUnixTimeSeconds(long.Parse(tokenInfo.Exp)).DateTime
            };
        }
        
        return TokenVerificationResult.Invalid("Google API validation failed");
    }
}

// Enhanced result model
public class TokenVerificationResult
{
    public bool IsValid { get; set; }
    public string UserId { get; set; }
    public string Email { get; set; }
    public string Name { get; set; }
    public string Provider { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string ErrorCode { get; set; }
    public string ErrorMessage { get; set; }
    
    public static TokenVerificationResult Invalid(string error) 
        => new() { IsValid = false, ErrorMessage = error };
}
```

### 7.2 Optimized User Management Service 🔥 **HIGH PRIORITY**

**Issue**: Database hit on every request to check/create user causes performance bottleneck.

**Solution**: Multi-level caching with upsert pattern instead of check-then-create.

```csharp
// ✅ PRODUCTION-READY: Cached User Service
public class CachedUserService : IUserService
{
    private readonly IMemoryCache _memoryCache;
    private readonly IDistributedCache _distributedCache;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<CachedUserService> _logger;
    
    public async Task<User> GetOrCreateUserAsync(string email, string provider, string providerId)
    {
        // L1 Cache (Memory) - 5 minute TTL for active users
        var userCacheKey = $"user:{provider}:{providerId}";
        if (_memoryCache.TryGetValue(userCacheKey, out User cachedUser))
        {
            return cachedUser;
        }
        
        // L2 Cache (Redis) - 15 minute TTL
        var distributedUser = await GetUserFromDistributedCache(userCacheKey);
        if (distributedUser != null)
        {
            _memoryCache.Set(userCacheKey, distributedUser, TimeSpan.FromMinutes(5));
            return distributedUser;
        }
        
        // Database with UPSERT pattern (atomic operation)
        var user = await _userRepository.UpsertUserAsync(new UserUpsertRequest
        {
            Email = email,
            Provider = provider,
            ProviderId = providerId
        });
        
        // Cache in both layers
        _memoryCache.Set(userCacheKey, user, TimeSpan.FromMinutes(5));
        await CacheUserInDistributedCache(userCacheKey, user, TimeSpan.FromMinutes(15));
        
        _logger.LogInformation("User retrieved/created: {UserId}, Email: {Email}, Provider: {Provider}", 
            user.Id, user.Email, provider);
            
        return user;
    }
    
    private async Task<User> GetUserFromDistributedCache(string key)
    {
        var cachedData = await _distributedCache.GetStringAsync(key);
        if (!string.IsNullOrEmpty(cachedData))
        {
            return JsonSerializer.Deserialize<User>(cachedData);
        }
        return null;
    }
    
    private async Task CacheUserInDistributedCache(string key, User user, TimeSpan expiry)
    {
        await _distributedCache.SetStringAsync(key, 
            JsonSerializer.Serialize(user),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiry
            });
    }
}

// Database Repository with UPSERT
public class UserRepository : IUserRepository
{
    private readonly IdentityDbContext _context;
    
    public async Task<User> UpsertUserAsync(UserUpsertRequest request)
    {
        // Try to find existing user first
        var existingUser = await _context.Users
            .Include(u => u.Logins)
            .FirstOrDefaultAsync(u => u.Logins.Any(l => l.Provider == request.Provider && l.ProviderUserId == request.ProviderId));
            
        if (existingUser != null)
        {
            // Update existing user if needed
            existingUser.Email = request.Email;
            existingUser.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return existingUser;
        }
        
        // Create new user with atomic transaction
        var newUser = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            Name = request.Email.Split('@')[0], // Default name from email
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true,
            Logins = new List<UserLogin>
            {
                new UserLogin
                {
                    Provider = request.Provider,
                    ProviderUserId = request.ProviderId,
                    CreatedAt = DateTime.UtcNow
                }
            }
        };
        
        _context.Users.Add(newUser);
        await _context.SaveChangesAsync();
        
        return newUser;
    }
}
```

### 7.3 Refresh Token Management 🔥 **MEDIUM PRIORITY**

**Issue**: Google ID tokens expire in 1 hour, causing users to logout frequently.

**Solution**: Implement automatic token refresh with secure storage and proactive refresh.

```csharp
// ✅ FRONTEND: Auto-refresh token logic
// composables/useAutoRefresh.ts
export const useAutoRefresh = () => {
  const authStore = useAuthStore()
  const refreshInterval = ref<NodeJS.Timeout>()
  
  const startAutoRefresh = () => {
    // Check token expiry every 5 minutes
    refreshInterval.value = setInterval(async () => {
      const token = authStore.token
      if (!token) return
      
      const tokenInfo = parseJWT(token)
      const expiresAt = new Date(tokenInfo.exp * 1000)
      const now = new Date()
      const timeUntilExpiry = expiresAt.getTime() - now.getTime()
      
      // Refresh if token expires in less than 10 minutes
      if (timeUntilExpiry < 10 * 60 * 1000) {
        await refreshTokenIfNeeded()
      }
    }, 5 * 60 * 1000) // Check every 5 minutes
  }
  
  const refreshTokenIfNeeded = async () => {
    try {
      // Use Google's token refresh API
      const result = await $fetch('/api/auth/refresh', {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${authStore.token}`
        }
      })
      
      if (result.success && result.token) {
        authStore.setToken(result.token)
        console.log('Token refreshed successfully')
      }
    } catch (error) {
      console.error('Token refresh failed:', error)
      // Redirect to login if refresh fails
      await navigateTo('/auth/login')
    }
  }
  
  const stopAutoRefresh = () => {
    if (refreshInterval.value) {
      clearInterval(refreshInterval.value)
    }
  }
  
  return {
    startAutoRefresh,
    stopAutoRefresh,
    refreshTokenIfNeeded
  }
}

// ✅ BACKEND: Refresh token endpoint
[HttpPost("refresh")]
public async Task<ActionResult<RefreshTokenResponse>> RefreshToken()
{
    try
    {
        var currentToken = GetTokenFromHeader();
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(currentToken);
        
        // Extract user info from current token
        var userId = jwt.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
        var email = jwt.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
        
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(email))
        {
            return BadRequest("Invalid token structure");
        }
        
        // Generate new token with fresh expiry
        var newToken = await _tokenService.GenerateTokenAsync(userId, email);
        
        return Ok(new RefreshTokenResponse
        {
            Success = true,
            Token = newToken,
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Token refresh failed");
        return StatusCode(500, new RefreshTokenResponse
        {
            Success = false,
            Error = "Token refresh failed"
        });
    }
}

// Token service to generate internal JWT
public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    
    public async Task<string> GenerateTokenAsync(string userId, string email)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        
        var claims = new[]
        {
            new Claim("sub", userId),
            new Claim("email", email),
            new Claim("iss", "identity-service"),
            new Claim("aud", "tihomo-services"),
            new Claim("iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new Claim("exp", DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };
        
        var token = new JwtSecurityToken(
            issuer: "identity-service",
            audience: "tihomo-services",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials
        );
        
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
```

### 7.4 Circuit Breaker & Resilience Patterns 🔥 **MEDIUM PRIORITY**

**Issue**: Single point of failure - if Identity service is down, entire system fails.

**Solution**: Implement circuit breaker with fallback mechanisms and graceful degradation.

```csharp
// ✅ PRODUCTION-READY: Resilient Authentication Service
public class ResilientAuthenticationService : IAuthenticationService
{
    private readonly ICircuitBreaker _identityCircuitBreaker;
    private readonly ITokenVerificationService _primaryService;
    private readonly ILocalJwtValidator _fallbackValidator;
    private readonly ILogger<ResilientAuthenticationService> _logger;
    
    public async Task<AuthenticationResult> AuthenticateAsync(string token)
    {
        try
        {
            // Try primary Identity service with circuit breaker protection
            return await _identityCircuitBreaker.ExecuteAsync(async () =>
            {
                var result = await _primaryService.VerifyTokenAsync(token, "Google");
                
                if (!result.IsValid)
                {
                    throw new AuthenticationException("Token validation failed");
                }
                
                return new AuthenticationResult
                {
                    IsSuccess = true,
                    UserId = result.UserId,
                    Email = result.Email,
                    Claims = CreateClaims(result)
                };
            });
        }
        catch (CircuitBreakerOpenException)
        {
            // Circuit breaker is open - use fallback local validation
            _logger.LogWarning("Identity service circuit breaker OPEN - using fallback validation");
            return await FallbackAuthentication(token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Primary authentication failed - attempting fallback");
            return await FallbackAuthentication(token);
        }
    }
    
    private async Task<AuthenticationResult> FallbackAuthentication(string token)
    {
        try
        {
            // Fallback 1: Local JWT validation (if we have local tokens)
            var localResult = await _fallbackValidator.ValidateLocalJwtAsync(token);
            if (localResult.IsValid)
            {
                _logger.LogInformation("Fallback authentication successful via local JWT");
                return new AuthenticationResult
                {
                    IsSuccess = true,
                    UserId = localResult.UserId,
                    Email = localResult.Email,
                    Claims = CreateClaims(localResult),
                    IsFallback = true
                };
            }
            
            // Fallback 2: Cached validation result
            var cachedResult = await GetCachedAuthenticationResult(token);
            if (cachedResult != null)
            {
                _logger.LogInformation("Fallback authentication successful via cache");
                return cachedResult;
            }
            
            // All fallbacks failed
            _logger.LogError("All authentication methods failed");
            return AuthenticationResult.Failed("Authentication service unavailable");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fallback authentication failed");
            return AuthenticationResult.Failed("Authentication failed");
        }
    }
}

// Circuit Breaker Configuration
public class CircuitBreakerOptions
{
    public string ServiceName { get; set; } = "IdentityService";
    public int FailureThreshold { get; set; } = 5; // Open after 5 consecutive failures
    public TimeSpan OpenTimeout { get; set; } = TimeSpan.FromSeconds(30); // Stay open for 30 seconds
    public TimeSpan SamplingDuration { get; set; } = TimeSpan.FromMinutes(2); // Sample window
    public int MinimumThroughput { get; set; } = 10; // Minimum requests before considering failure rate
}

// API Gateway middleware
public class ResilientAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ResilientAuthenticationService _authService;
    
    public async Task InvokeAsync(HttpContext context)
    {
        var token = ExtractToken(context);
        if (string.IsNullOrEmpty(token))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Missing authentication token");
            return;
        }
        
        var authResult = await _authService.AuthenticateAsync(token);
        
        if (authResult.IsSuccess)
        {
            // Inject user claims into headers for downstream services
            context.Request.Headers.Add("X-User-Id", authResult.UserId);
            context.Request.Headers.Add("X-User-Email", authResult.Email);
            
            if (authResult.IsFallback)
            {
                context.Request.Headers.Add("X-Auth-Mode", "fallback");
            }
            
            await _next(context);
        }
        else
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync($"Authentication failed: {authResult.ErrorMessage}");
        }
    }
}
```

## 9. Advanced Implementation (Phase 3 & 4)

### 9.1 Phase 3: Resilience & Circuit Breaker Design

#### 9.1.1 Architecture Pattern
```
┌─────────────────────────────────────────────────────────────┐
│                    AuthController                           │
└─────────────────────┬───────────────────────────────────────┘
                      │
┌─────────────────────▼───────────────────────────────────────┐
│           ResilientTokenVerificationService                 │
│  ┌─────────────────────────────────────────────────────┐   │
│  │              Polly Resilience Pipeline             │   │
│  │  • Circuit Breaker (5 failures, 30s break)        │   │
│  │  • Retry (3 attempts with exponential backoff)     │   │
│  │  • Timeout (10 seconds)                            │   │
│  │  • Fallback (cache → local parsing → null)         │   │
│  └─────────────────────────────────────────────────────┘   │
└─────────────────────┬───────────────────────────────────────┘
                      │
┌─────────────────────▼───────────────────────────────────────┐
│           EnhancedTokenVerificationService                  │
│  ┌─────────────────────────────────────────────────────┐   │
│  │                Multi-Layer Cache                    │   │
│  │  • Memory Cache (L1) - 100ms TTL                   │   │
│  │  • Redis Cache (L2) - 300s TTL                     │   │
│  │  • Local JWT parsing for basic validation          │   │
│  └─────────────────────────────────────────────────────┘   │
└─────────────────────┬───────────────────────────────────────┘
                      │
┌─────────────────────▼───────────────────────────────────────┐
│              External Provider APIs                         │
│                (Google, Facebook)                           │
└─────────────────────────────────────────────────────────────┘
```

#### 9.1.2 Resilience Components

##### Circuit Breaker Configuration
```csharp
// Circuit breaker opens after 5 failures in 30 seconds
// Stays open for 30 seconds before attempting recovery
FailureRatio = 0.5,
SamplingDuration = TimeSpan.FromSeconds(30),
MinimumThroughput = 5,
BreakDuration = TimeSpan.FromSeconds(30)
```

##### Retry Strategy
```csharp
// 3 retry attempts with decorrelated jitter backoff
// Starting from 200ms median delay
MaxRetryAttempts = 3,
DelayGenerator = Backoff.DecorrelatedJitterBackoffV2(
    medianFirstRetryDelay: TimeSpan.FromMilliseconds(200),
    retryCount: 3)
```

##### Fallback Mechanisms
1. **Primary**: External provider API call
2. **Fallback Level 1**: Cached token validation result
3. **Fallback Level 2**: Local JWT parsing for basic claims
4. **Fallback Level 3**: Graceful null return

### 9.2 Phase 4: Monitoring & Observability Design

#### 9.2.1 Observability Stack
```
┌─────────────────────────────────────────────────────────────┐
│                    HTTP Requests                            │
└─────────────────────┬───────────────────────────────────────┘
                      │
┌─────────────────────▼───────────────────────────────────────┐
│              ObservabilityMiddleware                        │
│  • Generate Correlation ID                                 │
│  • Start OpenTelemetry Activity                            │
│  • Record Request Timing                                   │
│  • Track Active Request Count                              │
└─────────────────────┬───────────────────────────────────────┘
                      │
┌─────────────────────▼───────────────────────────────────────┐
│                Application Layer                            │
│  • TelemetryService Integration                             │
│  • Business Logic Metrics                                  │
│  • Circuit Breaker Telemetry                               │
└─────────────────────┬───────────────────────────────────────┘
                      │
┌─────────────────────▼───────────────────────────────────────┐
│              Telemetry Exporters                            │
│  ┌─────────────────┬─────────────────┬───────────────────┐ │
│  │   Prometheus    │    Serilog      │   OpenTelemetry   │ │
│  │   /metrics      │  Structured     │   Distributed     │ │
│  │   Endpoint      │    Logging      │     Tracing       │ │
│  └─────────────────┴─────────────────┴───────────────────┘ │
└─────────────────────────────────────────────────────────────┘
```

#### 9.2.2 Metrics Design

##### Custom Business Metrics
- **Counters**: 
  - `identity_token_verification_attempts_total`
  - `identity_token_verification_successes_total`
  - `identity_token_verification_failures_total`
  - `identity_circuit_breaker_opened_total`
  - `identity_cache_hits_total`
  - `identity_cache_misses_total`

- **Histograms**:
  - `identity_token_verification_duration_seconds`
  - `identity_external_provider_response_time_seconds`
  - `identity_cache_operation_duration_seconds`

- **Gauges**:
  - `identity_circuit_breaker_state` (0=Closed, 1=Open, 2=HalfOpen)
  - `identity_active_requests_current`

##### Runtime Metrics (Automatic)
- `process_runtime_dotnet_gc_collections_count_total`
- `process_runtime_dotnet_gc_objects_size_bytes`
- `process_runtime_dotnet_gc_allocations_size_bytes_total`
- `http_server_request_duration_seconds`

#### 9.2.3 Distributed Tracing Design

##### Activity Sources
- **"Identity.Api"**: Main application activities
- **"Identity.TokenVerification.Resilience"**: Resilience pattern activities

##### Instrumentation Coverage
- **ASP.NET Core**: HTTP request/response tracing
- **Entity Framework Core**: Database query tracing với SQL statements
- **HTTP Client**: External provider API call tracing
- **Custom Business Logic**: Token verification flow tracing

##### Trace Context
```json
{
  "TraceId": "89751302e522d19a7489b0f4f23ceda8",
  "SpanId": "d26587f5dc54f120",
  "Resource": {
    "service.name": "TiHoMo.Identity",
    "service.namespace": "TiHoMo",
    "deployment.environment": "Development",
    "service.instance.id": "MACHINE_NAME"
  }
}
```

#### 9.2.4 Structured Logging Design

##### Log Format
```json
{
  "Timestamp": "2025-06-19T12:01:22.6871611Z",
  "Level": "Information",
  "CorrelationId": "c6b6a18a-a54d-46db-8015-de3f284825ad",
  "Application": "TiHoMo.Identity",
  "Message": "Request GET /metrics completed in 85ms with status 200",
  "Properties": {
    "RequestPath": "/metrics",
    "StatusCode": 200,
    "Duration": 85
  }
}
```

##### Correlation Strategy
- **Automatic Generation**: ObservabilityMiddleware generates unique GUID cho mỗi request
- **Propagation**: Correlation ID được propagated qua tất cả log entries trong request
- **Context Enrichment**: Serilog LogContext automatically includes correlation properties

### 9.3 Health Check Design

#### 9.3.1 Health Check Hierarchy
```
┌─────────────────────────────────────────────────────────────┐
│                  /health Endpoint                           │
└─────────────────────┬───────────────────────────────────────┘
                      │
           ┌──────────┼──────────┐
           │          │          │
    ┌──────▼───┐ ┌────▼────┐ ┌───▼─────┐
    │Database  │ │Circuit  │ │Telemetry│
    │Health    │ │Breaker  │ │Health   │
    │Check     │ │Health   │ │Check    │
    └──────────┘ └─────────┘ └─────────┘
```

#### 9.3.2 Health Check Response
```json
{
  "Status": "Healthy",
  "Timestamp": "2025-06-19T12:01:15.2875877Z",
  "Duration": "00:00:00.0503784",
  "Services": [
    {
      "Service": "database",
      "Status": "Healthy",
      "Duration": "00:00:00.0497418",
      "Description": null,
      "Data": null
    },
    {
      "Service": "circuit_breaker",
      "Status": "Healthy",
      "Duration": "00:00:00.0000354",
      "Description": "Circuit breaker and resilience patterns are properly configured and operational",
      "Data": {
        "service_type": "ResilientTokenVerificationService",
        "timestamp": "2025-06-19T12:01:15.23911788Z",
        "resilience_enabled": true
      }
    },
    {
      "Service": "telemetry",
      "Status": "Healthy",
      "Duration": "00:00:00.0000232",
      "Description": "Telemetry system is operational",
      "Data": {
        "tracing": "inactive",
        "metrics_counter": "available",
        "metrics_histogram": "available",
        "metrics_gauge": "available"
      }
    }
  ]
}
```

### 9.4 Implementation Components

#### 9.4.1 Key Classes
- **ResilientTokenVerificationService**: Main resilience wrapper service
- **TelemetryService**: Custom metrics và business logic monitoring
- **ObservabilityMiddleware**: Request correlation và timing middleware
- **CircuitBreakerHealthCheck**: Resilience patterns health monitoring
- **TelemetryHealthCheck**: Observability system health verification

#### 9.4.2 Package Dependencies
```xml
<!-- Resilience -->
<PackageReference Include="Polly" Version="8.2.1" />
<PackageReference Include="Polly.Extensions.Http" Version="3.0.0" />
<PackageReference Include="Polly.Contrib.WaitAndRetry" Version="1.1.1" />

<!-- Observability -->
<PackageReference Include="OpenTelemetry" Version="1.9.0" />
<PackageReference Include="OpenTelemetry.Extensions.Hosting" Version="1.9.0" />
<PackageReference Include="OpenTelemetry.Instrumentation.AspNetCore" Version="1.9.0" />
<PackageReference Include="OpenTelemetry.Instrumentation.Http" Version="1.9.0" />
<PackageReference Include="OpenTelemetry.Instrumentation.EntityFrameworkCore" Version="1.0.0-beta.12" />
<PackageReference Include="OpenTelemetry.Exporter.Prometheus.AspNetCore" Version="1.8.0-rc.1" />

<!-- Structured Logging -->
<PackageReference Include="Serilog" Version="4.0.1" />
<PackageReference Include="Serilog.AspNetCore" Version="8.0.2" />
<PackageReference Include="Serilog.Sinks.Console" Version="6.0.0" />
```

### 9.5 Production Benefits

#### 9.5.1 Availability Improvements
- **99.9% uptime** even during external provider outages
- **Automatic recovery** when providers come back online
- **Predictable latency** với 10-second maximum response time
- **Graceful degradation** với multi-level fallback

#### 9.5.2 Observability Benefits
- **Complete request tracing** với unique correlation IDs
- **Real-time metrics** cho business logic và infrastructure
- **Detailed error tracking** với stack traces và context
- **Performance monitoring** với response times và throughput
- **Dashboard-ready** metrics cho Grafana visualization
- **Alerting-ready** health checks cho Prometheus AlertManager

#### 9.5.3 Operational Benefits
- **Zero-downtime deployments** với health check integration
- **Proactive monitoring** với circuit breaker state tracking
- **Debugging efficiency** với structured logging và distributed tracing
- **Performance optimization** với detailed metrics và profiling data
- **SLA compliance** với comprehensive monitoring và alerting

## 10. Production Readiness Checklist 🎯 **IMPLEMENTATION ROADMAP**
### Phase 1: Critical Performance Fixes (Week 1) 🔥
- [ ] **Enhanced Token Verification Service**
  - [ ] Implement JWT local parsing for structure validation
  - [ ] Add L1 (Memory) cache with 2-minute TTL
  - [ ] Add L2 (Redis) cache with 5-minute TTL  
  - [ ] Hash tokens for secure cache keys
  - [ ] Only call Google API on cache miss

- [ ] **Optimized User Service**
  - [ ] Implement multi-level user caching (5min/15min TTL)
  - [ ] Replace check-then-create with atomic upsert pattern
  - [ ] Add proper database indexing on Provider + ProviderUserId
  - [ ] Implement cache invalidation strategies

### Phase 2: User Experience (Week 2) 🔥
- [ ] **Refresh Token Flow**
  - [ ] Frontend auto-refresh logic (check every 5 minutes)
  - [ ] Proactive refresh when token expires in <10 minutes
  - [ ] Backend refresh endpoint with new JWT generation
  - [ ] Graceful handling of refresh failures → redirect to login

### Phase 3: Resilience & Reliability (Week 3) 🔥
- [ ] **Circuit Breaker Implementation**
  - [ ] Circuit breaker for Google API calls (5 failures → 30s open)
  - [ ] Fallback to local JWT validation when available
  - [ ] Fallback to cached authentication results
  - [ ] Graceful degradation with clear error messages

- [ ] **Error Handling & Rate Limiting**
  - [ ] Implement rate limiting per user/IP (100 requests/minute)
  - [ ] Add retry policies with exponential backoff
  - [ ] Proper error codes and user-friendly messages
  - [ ] Session invalidation on security events

### Phase 4: Monitoring & Operations (Week 4) 📊
- [ ] **Comprehensive Monitoring**
  - [ ] Token verification metrics (success rate, duration, cache hit rate)
  - [ ] Database operation metrics (query time, cache hit rate)  
  - [ ] Circuit breaker state monitoring
  - [ ] Social provider API health checks

- [ ] **Observability & Alerting**
  - [ ] Structured logging with correlation IDs
  - [ ] Health check endpoints for all dependencies
  - [ ] Alerts for high error rates (>5%), slow responses (>2s)
  - [ ] Dashboard for authentication metrics

### Phase 5: Security Hardening (Week 5) 🔒
- [ ] **Enhanced Security**
  - [ ] API key rate limiting and usage quotas
  - [ ] CORS policies for production domains
  - [ ] Input validation and sanitization
  - [ ] Secure token storage (HttpOnly cookies for web)
  - [ ] Regular security audits and penetration testing

### Current Status Assessment ✅
Based on memory bank, current implementation has:
- ✅ **Basic Google login working** (Frontend + Backend)
- ✅ **Identity.Api service running** on port 5214
- ✅ **JWT token flow implemented**
- ✅ **Database user storage working**

**CRITICAL GAPS** that need immediate attention:
- ❌ **No token verification caching** → Google API called on every request
- ❌ **No refresh token flow** → Users logout every hour  
- ❌ **No fallback mechanisms** → Single point of failure
- ❌ **No monitoring/observability** → Blind to performance issues

### Implementation Priority 🎯
1. **Week 1**: Token caching + User caching (CRITICAL for performance)
2. **Week 2**: Refresh token flow (CRITICAL for UX)
3. **Week 3**: Circuit breaker + Resilience (CRITICAL for reliability)
4. **Week 4**: Monitoring + Health checks (CRITICAL for operations)
5. **Week 5**: Security hardening (IMPORTANT for production)
