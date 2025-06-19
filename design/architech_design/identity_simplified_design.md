# Identity Service - Simplified Social Login Design

## Overview

This document outlines the simplified design for the TiHoMo Identity service, focusing on social login integration (Google, Facebook, Apple) with stateless authentication through the API Gateway. The design is based on proven patterns from Microsoft eShop and ABP Framework.

## Current Problem

The existing SSO system is overly complex with:
- Multiple authentication flows (OpenId Connect, IdentityServer4, custom sessions)
- Complex database schema with unnecessary tables
- Complicated redirect flows between multiple services
- Difficult maintenance and debugging

## Proposed Solution

### High-Level Architecture

```
┌─────────────────┐    ┌──────────────────┐    ┌─────────────────┐    ┌──────────────────┐
│   Nuxt.js SPA   │    │ Ocelot Gateway   │    │ Identity API    │    │   Other APIs     │
│                 │    │                  │    │                 │    │ (CoreFinance,    │
│                 │    │                  │    │                 │    │  MoneyMgmt, etc) │
└─────────────────┘    └──────────────────┘    └─────────────────┘    └──────────────────┘
         │                        │                        │                        │
         │ 1. Login Google        │                        │                        │
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

## Core Components

### 1. Frontend (Nuxt.js)

#### Social Login Integration
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

#### HTTP Client with Token
```javascript
// plugins/api.client.js
export default defineNuxtPlugin(() => {
  const authStore = useAuthStore()
  
  const $fetch = $fetch.create({
    baseURL: '/api', // Proxy to Gateway
    onRequest({ options }) {
      const token = authStore.token
      if (token) {
        options.headers = {
          ...options.headers,
          Authorization: `Bearer ${token}`
        }
      }
    }
  })
  
  return {
    provide: { api: $fetch }
  }
})
```

### 2. API Gateway (Ocelot)

#### Route Configuration
```json
{
  "Routes": [
    {
      "UpstreamPathTemplate": "/api/identity/{everything}",
      "UpstreamHttpMethod": [ "GET", "POST", "PUT", "DELETE" ],
      "DownstreamPathTemplate": "/api/{everything}",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [
        {
          "Host": "localhost",
          "Port": 5001
        }  
      ],
      "ServiceName": "identity-api"
    },
    {
      "UpstreamPathTemplate": "/api/finance/{everything}",
      "UpstreamHttpMethod": [ "GET", "POST", "PUT", "DELETE" ],
      "DownstreamPathTemplate": "/api/{everything}",
      "DownstreamScheme": "http", 
      "DownstreamHostAndPorts": [
        {
          "Host": "localhost",
          "Port": 5002
        }
      ],
      "ServiceName": "finance-api",
      "AuthenticationOptions": {
        "AuthenticationProviderKey": "Bearer"
      }
    }
  ]
}
```

#### Authentication Middleware
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

### 3. Identity API

#### Core Models
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

// Models/UserLogin.cs
public class UserLogin
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Provider { get; set; } // Google, Facebook, Apple
    public string ProviderUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public User User { get; set; }
}

// Models/ApiKey.cs
public class ApiKey
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; }
    public string KeyHash { get; set; }
    public string KeyPrefix { get; set; } // First 8 chars for display
    public List<string> Scopes { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
    
    public User User { get; set; }
}
```

#### Token Verification Service
```csharp
// Services/TokenVerificationService.cs
public class TokenVerificationService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IUserService _userService;
    private readonly ILogger<TokenVerificationService> _logger;
    
    public async Task<ClaimsPrincipal> VerifyTokenAsync(string token, string provider)
    {
        UserInfo userInfo = provider switch
        {
            "google" => await VerifyGoogleTokenAsync(token),
            "facebook" => await VerifyFacebookTokenAsync(token),
            "apple" => await VerifyAppleTokenAsync(token),
            _ => throw new ArgumentException($"Unsupported provider: {provider}")
        };
        
        // Get or create user
        var user = await _userService.GetOrCreateUserAsync(userInfo, provider);
        
        // Build claims
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.Name),
            new("avatar", user.AvatarUrl ?? ""),
            new("provider", provider)
        };
        
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "Bearer"));
    }
    
    private async Task<UserInfo> VerifyGoogleTokenAsync(string token)
    {
        var client = _httpClientFactory.CreateClient();
        var response = await client.GetAsync($"https://www.googleapis.com/oauth2/v1/userinfo?access_token={token}");
        
        if (!response.IsSuccessStatusCode)
            throw new UnauthorizedAccessException("Invalid Google token");
            
        var json = await response.Content.ReadAsStringAsync();
        var userInfo = JsonSerializer.Deserialize<GoogleUserInfo>(json);
        
        return new UserInfo
        {
            Id = userInfo.Id,
            Email = userInfo.Email,
            Name = userInfo.Name,
            AvatarUrl = userInfo.Picture
        };
    }
    
    private async Task<UserInfo> VerifyFacebookTokenAsync(string token)
    {
        var client = _httpClientFactory.CreateClient();
        var response = await client.GetAsync($"https://graph.facebook.com/me?access_token={token}&fields=id,name,email,picture");
        
        if (!response.IsSuccessStatusCode)
            throw new UnauthorizedAccessException("Invalid Facebook token");
            
        var json = await response.Content.ReadAsStringAsync();
        var userInfo = JsonSerializer.Deserialize<FacebookUserInfo>(json);
        
        return new UserInfo
        {
            Id = userInfo.Id,
            Email = userInfo.Email,
            Name = userInfo.Name,
            AvatarUrl = userInfo.Picture?.Data?.Url
        };
    }
    
    private async Task<UserInfo> VerifyAppleTokenAsync(string token)
    {
        // Apple token verification requires JWT validation
        // Implementation would use Apple's public keys
        // For brevity, this is simplified
        throw new NotImplementedException("Apple token verification to be implemented");
    }
}
```

#### Authentication Controller
```csharp
// Controllers/AuthController.cs
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly TokenVerificationService _tokenVerificationService;
    
    [HttpPost("verify")]
    public async Task<IActionResult> VerifyToken([FromBody] VerifyTokenRequest request)
    {
        try
        {
            var principal = await _tokenVerificationService.VerifyTokenAsync(request.Token, request.Provider);
            
            var claims = principal.Claims.Select(c => new { c.Type, c.Value }).ToList();
            
            return Ok(new
            {
                Success = true,
                Claims = claims,
                UserId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                Email = principal.FindFirst(ClaimTypes.Email)?.Value,
                Name = principal.FindFirst(ClaimTypes.Name)?.Value
            });
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new { Success = false, Message = "Invalid token" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Success = false, Message = ex.Message });
        }
    }
}

public class VerifyTokenRequest
{
    public string Token { get; set; }
    public string Provider { get; set; }
}
```

#### API Key Management
```csharp
// Controllers/ApiKeyController.cs
[ApiController]
[Route("api/apikeys")]
[Authorize]
public class ApiKeyController : ControllerBase
{
    private readonly IApiKeyService _apiKeyService;
    
    [HttpPost]
    public async Task<IActionResult> CreateApiKey([FromBody] CreateApiKeyRequest request)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
        var apiKey = await _apiKeyService.CreateApiKeyAsync(userId, request.Name, request.Scopes);
        
        return Ok(new
        {
            Id = apiKey.Id,
            Name = apiKey.Name,
            Key = apiKey.Key, // Only returned on creation
            Prefix = apiKey.KeyPrefix,
            Scopes = apiKey.Scopes,
            CreatedAt = apiKey.CreatedAt
        });
    }
    
    [HttpGet]
    public async Task<IActionResult> GetApiKeys()
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
        var apiKeys = await _apiKeyService.GetApiKeysAsync(userId);
        
        return Ok(apiKeys.Select(k => new
        {
            Id = k.Id,
            Name = k.Name,
            Prefix = k.KeyPrefix,
            Scopes = k.Scopes,
            CreatedAt = k.CreatedAt,
            ExpiresAt = k.ExpiresAt,
            IsRevoked = k.IsRevoked
        }));
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> RevokeApiKey(Guid id)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
        await _apiKeyService.RevokeApiKeyAsync(id, userId);
        return NoContent();
    }
}
```

## Database Schema

### Simplified Schema
```sql
-- Users table
CREATE TABLE Users (
    Id uniqueidentifier PRIMARY KEY DEFAULT NEWID(),
    Email nvarchar(256) NOT NULL UNIQUE,
    Name nvarchar(256) NOT NULL,
    AvatarUrl nvarchar(512),
    CreatedAt datetime2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt datetime2 NOT NULL DEFAULT GETUTCDATE(),
    IsActive bit NOT NULL DEFAULT 1
);

-- User logins (external providers)
CREATE TABLE UserLogins (
    Id uniqueidentifier PRIMARY KEY DEFAULT NEWID(),
    UserId uniqueidentifier NOT NULL FOREIGN KEY REFERENCES Users(Id),
    Provider nvarchar(50) NOT NULL, -- Google, Facebook, Apple
    ProviderUserId nvarchar(256) NOT NULL,
    CreatedAt datetime2 NOT NULL DEFAULT GETUTCDATE(),
    UNIQUE(Provider, ProviderUserId)
);

-- API Keys for third-party integration
CREATE TABLE ApiKeys (
    Id uniqueidentifier PRIMARY KEY DEFAULT NEWID(),
    UserId uniqueidentifier NOT NULL FOREIGN KEY REFERENCES Users(Id),
    Name nvarchar(256) NOT NULL,
    KeyHash nvarchar(256) NOT NULL, -- SHA256 hash
    KeyPrefix nvarchar(8) NOT NULL, -- First 8 chars for display
    Scopes nvarchar(1000), -- JSON array of scopes
    CreatedAt datetime2 NOT NULL DEFAULT GETUTCDATE(),
    ExpiresAt datetime2,
    IsRevoked bit NOT NULL DEFAULT 0
);

-- Indexes
CREATE INDEX IX_UserLogins_UserId ON UserLogins(UserId);
CREATE INDEX IX_UserLogins_Provider_ProviderUserId ON UserLogins(Provider, ProviderUserId);
CREATE INDEX IX_ApiKeys_UserId ON ApiKeys(UserId);
CREATE INDEX IX_ApiKeys_KeyPrefix ON ApiKeys(KeyPrefix);
```

## Authentication Flow

### 1. User Login Flow
1. User clicks "Login with Google/Facebook/Apple" on Nuxt frontend
2. Frontend uses respective SDK to authenticate with provider
3. Frontend receives access token from provider
4. Frontend stores token and uses it for subsequent API calls

### 2. API Request Flow
1. Frontend makes API call to Gateway with `Authorization: Bearer {token}`
2. Gateway receives request and checks if route requires authentication
3. If authenticated route, Gateway calls Identity API to verify token:
   ```
   POST /api/auth/verify
   {
     "token": "user_access_token",
     "provider": "google"
   }
   ```
4. Identity API verifies token with provider and returns user claims
5. Gateway forwards request to target service with user claims in headers
6. Target service processes request with user context

### 3. API Key Authentication Flow
1. Third-party client includes API key in request: `Authorization: ApiKey {key}`
2. Gateway recognizes API key format and calls Identity API for verification
3. Identity API validates key and returns associated user claims
4. Request proceeds with user context

## Security Considerations

### Token Validation
- Verify tokens with provider APIs on every request
- Implement token caching with short TTL (5 minutes)
- Handle token expiration gracefully

### API Key Security
- Generate cryptographically secure random keys (32 bytes)
- Store only SHA256 hash in database
- Support key rotation and expiration
- Implement rate limiting per API key

### CORS Configuration
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3333", "https://yourdomain.com")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});
```

## Configuration

### appsettings.json
```json
{
  "Authentication": {
    "Google": {
      "ClientId": "your-google-client-id",
      "ClientSecret": "your-google-client-secret"
    },
    "Facebook": {
      "AppId": "your-facebook-app-id",
      "AppSecret": "your-facebook-app-secret"
    },
    "Apple": {
      "ClientId": "your-apple-client-id",
      "TeamId": "your-apple-team-id",
      "KeyId": "your-apple-key-id",
      "PrivateKey": "your-apple-private-key"
    }
  },
  "TokenValidation": {
    "CacheDurationMinutes": 5,
    "ClockSkewMinutes": 5
  },
  "ApiKeys": {
    "DefaultExpirationDays": 365,
    "MaxKeysPerUser": 10
  }
}
```

## Advantages of This Design

1. **Simplicity**: Minimal components and clear separation of concerns
2. **Stateless**: No server-side sessions or complex state management
3. **Scalable**: Each component can be scaled independently
4. **Secure**: Token verification with providers, secure API key management
5. **Flexible**: Easy to add new social providers
6. **Maintainable**: Clear architecture with well-defined interfaces
7. **Standards-based**: Uses OAuth 2.0 and JWT standards

## Migration Strategy

1. **Phase 1**: Implement new Identity API alongside existing system
2. **Phase 2**: Update Gateway to use new Identity API for verification
3. **Phase 3**: Update frontend to use social login SDKs
4. **Phase 4**: Migrate existing users to new system
5. **Phase 5**: Remove old SSO components

## Testing Strategy

### Unit Tests
- Token verification logic
- User creation/retrieval
- API key generation and validation

### Integration Tests
- End-to-end authentication flows
- Gateway integration
- Provider token validation

### Load Tests
- Token verification performance
- Concurrent user authentication
- API key authentication throughput

This simplified design eliminates the complexity of the current SSO system while providing robust social login capabilities and maintaining security standards.
