# API Gateway (Ocelot) Design Document

## 1. Tổng quan API Gateway

API Gateway là thành phần trung tâm của hệ thống TiHoMo, đóng vai trò Single Entry Point cho tất cả client requests. Được xây dựng trên Ocelot framework, gateway cung cấp routing, authentication, authorization, rate limiting, load balancing và monitoring cho toàn bộ microservices ecosystem.

### Mục tiêu chính
- Centralized routing cho tất cả microservices
- Authentication và authorization tập trung
- Cross-cutting concerns (logging, monitoring, rate limiting)
- API versioning và backward compatibility
- Load balancing và service discovery
- Request/response transformation

---

## 2. Kiến trúc Gateway

### 2.1 Ocelot Configuration Structure

```json
{
  "Routes": [
    {
      "DownstreamPathTemplate": "/api/auth/{everything}",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [
        {
          "Host": "identity-api",
          "Port": 5228
        }
      ],
      "UpstreamPathTemplate": "/api/auth/{everything}",
      "UpstreamHttpMethod": [ "GET", "POST", "PUT", "DELETE" ],
      "AuthenticationOptions": {
        "AuthenticationProviderKey": "Bearer",
        "AllowedScopes": []
      },
      "RateLimitOptions": {
        "ClientWhitelist": [],
        "EnableRateLimiting": true,
        "Period": "1m",
        "PeriodTimespan": 60,
        "Limit": 100
      }
    }
  ],
  "GlobalConfiguration": {
    "BaseUrl": "https://api.tihomo.local",
    "ServiceDiscoveryProvider": {
      "Type": "Consul",
      "Host": "consul",
      "Port": 8500
    }
  }
}
```

### 2.2 Middleware Pipeline

```csharp
public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    // Security headers
    app.UseSecurityHeaders();
    
    // CORS policy
    app.UseCors("PFMCorsPolicy");
    
    // Request logging
    app.UseRequestLogging();
    
    // Authentication
    app.UseAuthentication();
    
    // Authorization
    app.UseAuthorization();
    
    // Rate limiting
    app.UseRateLimiting();
    
    // Circuit breaker
    app.UseCircuitBreaker();
    
    // Ocelot pipeline
    app.UseOcelot().Wait();
}
```

---

## 3. Routing Configuration

### 3.1 Service Routing Matrix

| Route Pattern | Downstream Service | Port | Authentication | Rate Limit |
|---------------|-------------------|------|----------------|------------|
| `/api/auth/*` | Identity.Api | 5228 | Optional | 100/min |
| `/api/users/*` | Identity.Api | 5228 | Required (Bearer/ApiKey) | 200/min |
| `/api/apikeys/*` | Identity.Api | 5228 | Required (Bearer/ApiKey) | 100/min |
| `/api/accounts/*` | CoreFinance.Api | 5001 | Required (Bearer/ApiKey) | 300/min |
| `/api/core-finance/transaction/*` | CoreFinance.Api | 5001 | Required (Bearer/ApiKey) | 500/min |
| `/api/budgets/*` | MoneyManagement.Api | 5002 | Required (Bearer/ApiKey) | 200/min |
| `/api/goals/*` | PlanningInvestment.Api | 5003 | Required (Bearer/ApiKey) | 150/min |
| `/api/reports/*` | Reporting.Api | 5004 | Required (Bearer/ApiKey) | 50/min |
| `/api/notifications/*` | Reporting.Api | 5004 | Required (Bearer/ApiKey) | 100/min |

### 3.2 Route Configuration Details

**Identity Routes:**
```json
{
  "Routes": [
    {
      "DownstreamPathTemplate": "/api/auth/{everything}",
      "DownstreamScheme": "http", 
      "DownstreamHostAndPorts": [{ "Host": "identity-api", "Port": 5228 }],
      "UpstreamPathTemplate": "/api/auth/{everything}",
      "UpstreamHttpMethod": [ "GET", "POST", "PUT", "DELETE" ],
      "RateLimitOptions": {
        "EnableRateLimiting": true,
        "Period": "1m",
        "Limit": 100
      }
    },
    {
      "DownstreamPathTemplate": "/api/users/{everything}",
      "DownstreamScheme": "http", 
      "DownstreamHostAndPorts": [{ "Host": "identity-api", "Port": 5228 }],
      "UpstreamPathTemplate": "/api/users/{everything}",
      "UpstreamHttpMethod": [ "GET", "POST", "PUT", "DELETE" ],
      "AuthenticationOptions": {
        "AuthenticationProviderKey": "Bearer",
        "AllowedScopes": []
      },
      "RateLimitOptions": {
        "EnableRateLimiting": true,
        "Period": "1m",
        "Limit": 200
      }
    },
    {
      "DownstreamPathTemplate": "/api/apikeys/{everything}",
      "DownstreamScheme": "http", 
      "DownstreamHostAndPorts": [{ "Host": "identity-api", "Port": 5228 }],
      "UpstreamPathTemplate": "/api/apikeys/{everything}",
      "UpstreamHttpMethod": [ "GET", "POST", "PUT", "DELETE" ],
      "AuthenticationOptions": {
        "AuthenticationProviderKey": "Bearer",
        "AllowedScopes": []
      },
      "RateLimitOptions": {
        "EnableRateLimiting": true,
        "Period": "1m",
        "Limit": 100
      }
    }
  ]
}
```

**Core Finance Routes:**
```json
{
  "Routes": [
    {
      "DownstreamPathTemplate": "/api/accounts/{everything}",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [{ "Host": "corefinance-api", "Port": 5001 }],
      "UpstreamPathTemplate": "/api/accounts/{everything}",
      "UpstreamHttpMethod": [ "GET", "POST", "PUT", "DELETE" ],
      "AuthenticationOptions": {
        "AuthenticationProviderKey": "Bearer",
        "AllowedScopes": []
      },
      "RouteClaimsRequirement": {
        "role": "User"
      }
    }
  ]
}
```

---

## 4. Authentication & Authorization

### 4.1 Token Verification Configuration

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // Register Identity API client for token verification
    services.AddHttpClient<IIdentityApiClient, IdentityApiClient>(client =>
    {
        client.BaseAddress = new Uri("http://identity-api:5228");
    });

    services.AddAuthentication("Bearer")
        .AddScheme<BearerTokenAuthenticationSchemeOptions, BearerTokenAuthenticationHandler>("Bearer", options => { })
        .AddScheme<ApiKeyAuthenticationSchemeOptions, ApiKeyAuthenticationHandler>("ApiKey", options => { });
        
    services.AddAuthorization(options =>
    {
        options.AddPolicy("RequireUser", policy =>
            policy.RequireAuthenticatedUser());
        options.AddPolicy("RequireAdmin", policy =>
            policy.RequireClaim("role", "Admin"));
        options.AddPolicy("RequireApiKey", policy =>
            policy.RequireClaim(ClaimTypes.Role, "ApiUser"));
    });
}

// Bearer Token Authentication Handler
public class BearerTokenAuthenticationHandler : AuthenticationHandler<BearerTokenAuthenticationSchemeOptions>
{
    private readonly IIdentityApiClient _identityClient;
    
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("Authorization", out var authHeader))
            return AuthenticateResult.NoResult();
            
        var token = authHeader.FirstOrDefault()?.Replace("Bearer ", "");
        if (string.IsNullOrEmpty(token))
            return AuthenticateResult.Fail("Invalid token format");
            
        // Verify token with Identity API
        var verification = await _identityClient.VerifyTokenAsync(token);
        if (!verification.IsValid)
            return AuthenticateResult.Fail("Token verification failed");
            
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, verification.UserId),
            new Claim(ClaimTypes.Email, verification.Email),
            new Claim("provider", verification.Provider)
        };
        
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        return AuthenticateResult.Success(new AuthenticationTicket(
            new ClaimsPrincipal(identity), Scheme.Name));
    }
}
```

### 4.2 API Key Authentication

```csharp
public class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationSchemeOptions>
{
    private readonly IIdentityApiClient _identityClient;
    
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Check for API Key in Authorization header
        if (!Request.Headers.TryGetValue("Authorization", out var authHeader))
            return AuthenticateResult.NoResult();
            
        var authValue = authHeader.FirstOrDefault();
        if (string.IsNullOrEmpty(authValue) || !authValue.StartsWith("ApiKey "))
            return AuthenticateResult.NoResult();
            
        var apiKey = authValue.Replace("ApiKey ", "");
        if (string.IsNullOrEmpty(apiKey))
            return AuthenticateResult.Fail("Invalid API Key format");
        
        // Verify API Key with Identity API
        var verification = await _identityClient.VerifyApiKeyAsync(apiKey);
        if (!verification.IsValid)
            return AuthenticateResult.Fail("API Key verification failed");
        
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, verification.UserId),
            new Claim("api_key_id", verification.ApiKeyId),
            new Claim(ClaimTypes.Role, "ApiUser"),
            new Claim("scope", verification.Scope ?? "read")
        };
        
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        return AuthenticateResult.Success(new AuthenticationTicket(
            new ClaimsPrincipal(identity), Scheme.Name));
    }
}

// Identity API Client Interface
public interface IIdentityApiClient
{
    Task<TokenVerificationResult> VerifyTokenAsync(string token);
    Task<ApiKeyVerificationResult> VerifyApiKeyAsync(string apiKey);
}

public class TokenVerificationResult
{
    public bool IsValid { get; set; }
    public string UserId { get; set; }
    public string Email { get; set; }
    public string Provider { get; set; }
}

public class ApiKeyVerificationResult
{
    public bool IsValid { get; set; }
    public string UserId { get; set; }
    public string ApiKeyId { get; set; }
    public string Scope { get; set; }
}
```
        
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        
        return AuthenticateResult.Success(new AuthenticationTicket(principal, Scheme.Name));
    }
}
```

---

## 5. Cross-Cutting Concerns

### 5.1 Rate Limiting

**Global Rate Limiting Configuration:**
```json
{
  "GlobalConfiguration": {
    "RateLimitOptions": {
      "DisableRateLimitHeaders": false,
      "QuotaExceededMessage": "API rate limit exceeded. Please try again later.",
      "HttpStatusCode": 429,
      "ClientIdHeader": "X-Client-Id"
    }
  }
}
```

**Per-Route Rate Limiting:**
```csharp
public class CustomRateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IRateLimitService _rateLimitService;
    
    public async Task InvokeAsync(HttpContext context)
    {
        var clientId = GetClientIdentifier(context);
        var route = context.Request.Path.ToString();
        
        var rateLimitCheck = await _rateLimitService.CheckRateLimitAsync(clientId, route);
        
        if (!rateLimitCheck.IsAllowed)
        {
            context.Response.StatusCode = 429;
            context.Response.Headers.Add("X-RateLimit-Limit", rateLimitCheck.Limit.ToString());
            context.Response.Headers.Add("X-RateLimit-Remaining", "0");
            context.Response.Headers.Add("X-RateLimit-Reset", rateLimitCheck.ResetTime.ToString());
            
            await context.Response.WriteAsync("Rate limit exceeded");
            return;
        }
        
        // Add rate limit headers
        context.Response.Headers.Add("X-RateLimit-Limit", rateLimitCheck.Limit.ToString());
        context.Response.Headers.Add("X-RateLimit-Remaining", rateLimitCheck.Remaining.ToString());
        
        await _next(context);
    }
}
```

### 5.2 Circuit Breaker Pattern

```csharp
public class CircuitBreakerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ICircuitBreakerService _circuitBreakerService;
    
    public async Task InvokeAsync(HttpContext context)
    {
        var serviceName = GetDownstreamServiceName(context.Request.Path);
        var circuitBreaker = _circuitBreakerService.GetCircuitBreaker(serviceName);
        
        if (circuitBreaker.State == CircuitBreakerState.Open)
        {
            context.Response.StatusCode = 503;
            await context.Response.WriteAsync($"Service {serviceName} is currently unavailable");
            return;
        }
        
        try
        {
            await _next(context);
            circuitBreaker.RecordSuccess();
        }
        catch (HttpRequestException ex) when (IsServerError(ex))
        {
            circuitBreaker.RecordFailure();
            throw;
        }
    }
}
```

### 5.3 Request/Response Logging

```csharp
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;
    
    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = Guid.NewGuid().ToString();
        context.Items["CorrelationId"] = correlationId;
        context.Response.Headers.Add("X-Correlation-ID", correlationId);
        
        var stopwatch = Stopwatch.StartNew();
        
        // Log request
        _logger.LogInformation("Request {Method} {Path} started. CorrelationId: {CorrelationId}",
            context.Request.Method,
            context.Request.Path,
            correlationId);
        
        await _next(context);
        
        stopwatch.Stop();
        
        // Log response
        _logger.LogInformation("Request {Method} {Path} completed in {ElapsedMs}ms with status {StatusCode}. CorrelationId: {CorrelationId}",
            context.Request.Method,
            context.Request.Path,
            stopwatch.ElapsedMilliseconds,
            context.Response.StatusCode,
            correlationId);
    }
}
```

---

## 6. Service Discovery & Load Balancing

### 6.1 Consul Integration

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddOcelot()
        .AddConsul()
        .AddPolly();
        
    services.Configure<ConsulOptions>(options =>
    {
        options.Host = "consul";
        options.Port = 8500;
        options.HealthCheckInterval = TimeSpan.FromSeconds(30);
        options.DeregisterCriticalServiceAfter = TimeSpan.FromMinutes(5);
    });
}
```

**Service Registration:**
```csharp
public class ServiceRegistrar
{
    private readonly IConsulClient _consulClient;
    
    public async Task RegisterServiceAsync(ServiceRegistration registration)
    {
        var consulRegistration = new AgentServiceRegistration
        {
            ID = registration.Id,
            Name = registration.ServiceName,
            Address = registration.Address,
            Port = registration.Port,
            Tags = registration.Tags,
            Check = new AgentServiceCheck
            {
                HTTP = $"http://{registration.Address}:{registration.Port}/health",
                Interval = TimeSpan.FromSeconds(30),
                DeregisterCriticalServiceAfter = TimeSpan.FromMinutes(5)
            }
        };
        
        await _consulClient.Agent.ServiceRegister(consulRegistration);
    }
}
```

### 6.2 Load Balancing Configuration

```json
{
  "Routes": [
    {
      "DownstreamPathTemplate": "/api/core-finance/transaction/{everything}",
      "DownstreamScheme": "http",
      "ServiceName": "corefinance-api",
      "LoadBalancerOptions": {
        "Type": "RoundRobin"
      },
      "HealthCheck": {
        "Path": "/health",
        "Timeout": 5000
      }
    }
  ]
}
```

---

## 7. Security Headers & CORS

### 7.1 Security Headers Middleware

```csharp
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;
    
    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Add("X-Frame-Options", "DENY");
        context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
        context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
        context.Response.Headers.Add("Content-Security-Policy", 
            "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline'");
        
        // Remove server header
        context.Response.Headers.Remove("Server");
        
        await _next(context);
    }
}
```

### 7.2 CORS Configuration

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddCors(options =>
    {
        options.AddPolicy("PFMCorsPolicy", builder =>
        {
            builder
                .WithOrigins(
                    "https://tihomo.local",
                    "https://admin.tihomo.local",
                    "http://localhost:3333",
                    "http://localhost:3001"
                )
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials()
                .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
        });
        
        // Separate policy for mobile apps
        options.AddPolicy("MobileCorsPolicy", builder =>
        {
            builder
                .WithOrigins("capacitor://localhost", "ionic://localhost")
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        });
    });
}
```

---

## 8. Request/Response Transformation

### 8.1 Request Transformation

```json
{
  "Routes": [
    {
      "DownstreamPathTemplate": "/api/v2/accounts/{everything}",
      "UpstreamPathTemplate": "/api/accounts/{everything}",
      "RequestIdKey": "X-Request-ID",
      "AddHeadersToRequest": {
        "X-Gateway-Version": "1.0",
        "X-Forwarded-For": "{RemoteIpAddress}"
      },
      "AddClaimsToRequest": {
        "X-User-Id": "sub",
        "X-User-Role": "role"
      }
    }
  ]
}
```

### 8.2 Response Transformation

```csharp
public class ResponseTransformationMiddleware
{
    private readonly RequestDelegate _next;
    
    public async Task InvokeAsync(HttpContext context)
    {
        var originalBodyStream = context.Response.Body;
        
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;
        
        await _next(context);
        
        if (ShouldTransformResponse(context))
        {
            var response = await ReadResponseBody(responseBody);
            var transformedResponse = TransformResponse(response, context);
            
            await WriteResponseBody(transformedResponse, originalBodyStream);
        }
        else
        {
            responseBody.Seek(0, SeekOrigin.Begin);
            await responseBody.CopyToAsync(originalBodyStream);
        }
    }
    
    private string TransformResponse(string response, HttpContext context)
    {
        var apiResponse = new ApiResponseWrapper
        {
            Success = context.Response.StatusCode < 400,
            Data = JsonSerializer.Deserialize<object>(response),
            Timestamp = DateTime.UtcNow,
            CorrelationId = context.Items["CorrelationId"]?.ToString(),
            Version = "1.0"
        };
        
        return JsonSerializer.Serialize(apiResponse);
    }
}
```

---

## 9. Monitoring & Observability

### 9.1 Health Checks

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddHealthChecks()
        .AddCheck<GatewayHealthCheck>("gateway-health")
        .AddConsul(options =>
        {
            options.HostName = "consul";
            options.Port = 8500;
        })
        .AddUrlGroup(new Uri("http://identity-api:5228/health"), "identity-api")
        .AddUrlGroup(new Uri("http://corefinance-api:5001/health"), "corefinance-api")
        .AddUrlGroup(new Uri("http://moneymanagement-api:5002/health"), "moneymanagement-api");
}

public class GatewayHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        // Check gateway-specific health metrics
        var isHealthy = CheckGatewayHealth();
        
        return Task.FromResult(isHealthy 
            ? HealthCheckResult.Healthy("Gateway is healthy")
            : HealthCheckResult.Unhealthy("Gateway is unhealthy"));
    }
}
```

### 9.2 Metrics Collection

```csharp
public class MetricsMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IMetrics _metrics;
    
    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var route = GetRouteName(context.Request.Path);
        
        try
        {
            await _next(context);
            
            // Record success metrics
            _metrics.Measure.Counter.Increment("gateway_requests_total", 
                new MetricTags("route", route, "status", context.Response.StatusCode.ToString()));
            
            _metrics.Measure.Timer.Time("gateway_request_duration", stopwatch.Elapsed,
                new MetricTags("route", route));
        }
        catch (Exception)
        {
            // Record error metrics
            _metrics.Measure.Counter.Increment("gateway_requests_total",
                new MetricTags("route", route, "status", "error"));
            throw;
        }
    }
}
```

---

## 10. Error Handling & Resilience

### 10.1 Global Error Handler

```csharp
public class GlobalErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalErrorHandlingMiddleware> _logger;
    
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred. CorrelationId: {CorrelationId}",
                context.Items["CorrelationId"]);
                
            await HandleExceptionAsync(context, ex);
        }
    }
    
    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;
        response.ContentType = "application/json";
        
        var errorResponse = exception switch
        {
            TimeoutException => new ErrorResponse("Gateway timeout", 504),
            HttpRequestException => new ErrorResponse("Downstream service error", 502),
            UnauthorizedAccessException => new ErrorResponse("Unauthorized", 401),
            _ => new ErrorResponse("Internal server error", 500)
        };
        
        response.StatusCode = errorResponse.StatusCode;
        await response.WriteAsync(JsonSerializer.Serialize(errorResponse));
    }
}
```

### 10.2 Retry Policy

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddOcelot()
        .AddPolly();
        
    // Configure retry policies
    services.Configure<QoSOptions>(options =>
    {
        options.ExceptionsAllowedBeforeBreaking = 3;
        options.DurationOfBreak = 5000;
        options.TimeoutValue = 10000;
    });
}
```

**Polly Configuration:**
```json
{
  "Routes": [
    {
      "QoSOptions": {
        "ExceptionsAllowedBeforeBreaking": 3,
        "DurationOfBreak": 5000,
        "TimeoutValue": 10000
      },
      "HttpHandlerOptions": {
        "AllowAutoRedirect": false,
        "UseCookieContainer": false,
        "UseTracing": false
      }
    }
  ]
}
```

---

## 11. Deployment & Configuration

### 11.1 Docker Configuration

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Ocelot.Gateway/Ocelot.Gateway.csproj", "Ocelot.Gateway/"]
RUN dotnet restore "Ocelot.Gateway/Ocelot.Gateway.csproj"
COPY . .
WORKDIR "/src/Ocelot.Gateway"
RUN dotnet build "Ocelot.Gateway.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Ocelot.Gateway.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Ocelot.Gateway.dll"]
```

### 11.2 Environment Configuration

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Ocelot": "Debug"
    }
  },
  "IdentityServer": {
    "Authority": "http://identity-sso:5217",
    "ApiName": "tihomo-gateway",
    "RequireHttpsMetadata": false
  },
  "Consul": {
    "Host": "consul",
    "Port": 8500
  },
  "RateLimiting": {
    "EnableRateLimiting": true,
    "Storage": "Redis",
    "ConnectionString": "redis:6379"
  }
}
```

---

*API Gateway là backbone của hệ thống TiHoMo microservices, đảm bảo security, performance, reliability và observability cho toàn bộ ecosystem. Thiết kế này cung cấp foundation mạnh mẽ cho scalable và maintainable architecture.*
