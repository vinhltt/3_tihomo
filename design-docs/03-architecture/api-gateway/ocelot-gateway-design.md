# API Gateway (Ocelot) Design Document

## 1. Overview

API Gateway serves as the central entry point for the TiHoMo system, built on Ocelot framework. It provides routing, authentication, authorization, rate limiting, load balancing, and monitoring for the entire microservices ecosystem.

### Primary Objectives
- Centralized routing for all microservices
- Unified authentication and authorization
- Cross-cutting concerns (logging, monitoring, rate limiting)
- API versioning and backward compatibility
- Load balancing and service discovery
- Request/response transformation

---

## 2. Gateway Architecture

### 2.1 Ocelot Configuration Structure

```json
{
  "Routes": [
    {
      "DownstreamPathTemplate": "/api/auth/{everything}",
      "DownstreamScheme": "https",
      "DownstreamHostAndPorts": [
        {
          "Host": "localhost",
          "Port": 5001
        }
      ],
      "UpstreamPathTemplate": "/identity/auth/{everything}",
      "UpstreamHttpMethod": [ "GET", "POST", "PUT", "DELETE" ],
      "RateLimitOptions": {
        "EnableRateLimiting": true,
        "Period": "1m",
        "PeriodTimespan": 60,
        "Limit": 100
      }
    }
  ],
  "GlobalConfiguration": {
    "BaseUrl": "https://localhost:5000"
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
    app.UseCors("TiHomoCorsPolicy");
    
    // Request logging
    app.UseRequestLogging();
    
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
| `/identity/auth/*` | Identity.Api | 5001 | Optional | 100/min |
| `/api/core-finance/*` | CoreFinance.Api | 5002 | Required | 200/min |
| `/api/money-management/*` | MoneyManagement.Api | 5003 | Required | 200/min |
| `/api/planning-investment/*` | PlanningInvestment.Api | 5004 | Required | 200/min |
| `/api/reporting/*` | Reporting.Api | 5005 | Required | 150/min |

### 3.2 Route Definitions

#### Identity Service Routes
```json
{
  "DownstreamPathTemplate": "/api/auth/{everything}",
  "DownstreamScheme": "https",
  "DownstreamHostAndPorts": [
    {
      "Host": "localhost",
      "Port": 5001
    }
  ],
  "UpstreamPathTemplate": "/identity/auth/{everything}",
  "UpstreamHttpMethod": [ "GET", "POST", "PUT", "DELETE" ]
}
```

#### Core Finance Service Routes
```json
{
  "DownstreamPathTemplate": "/api/{everything}",
  "DownstreamScheme": "https", 
  "DownstreamHostAndPorts": [
    {
      "Host": "localhost",
      "Port": 5002
    }
  ],
  "UpstreamPathTemplate": "/api/core-finance/{everything}",
  "UpstreamHttpMethod": [ "GET", "POST", "PUT", "DELETE" ]
}
```

---

## 4. Authentication & Authorization

### 4.1 Authentication Strategies
- **Social Login**: Google, Facebook, Apple token verification
- **JWT Bearer**: Token-based authentication for API access
- **API Keys**: Service-to-service authentication

### 4.2 Authorization Patterns
- Route-based authorization
- Resource-based access control
- Role-based permissions
- Scope-based API access

---

## 5. Cross-Cutting Concerns

### 5.1 Rate Limiting
```json
{
  "RateLimitOptions": {
    "EnableRateLimiting": true,
    "Period": "1m",
    "PeriodTimespan": 60,
    "Limit": 100,
    "QuotaExceededMessage": "Rate limit exceeded. Please try again later."
  }
}
```

### 5.2 Request/Response Logging
- Correlation ID tracking
- Request/response payload logging
- Performance metrics
- Error tracking

### 5.3 Circuit Breaker Pattern
- Automatic failure detection
- Service degradation handling
- Health check integration
- Fallback mechanisms

---

## 6. Service Discovery & Load Balancing

### 6.1 Service Registration
Services register themselves with health check endpoints:
```csharp
services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database")
    .AddCheck<ExternalServiceHealthCheck>("external_api");
```

### 6.2 Load Balancing Strategies
- **Round Robin**: Default distribution
- **Least Response Time**: Performance-based routing
- **Weighted Round Robin**: Capacity-based distribution

---

## 7. Security Implementation

### 7.1 HTTPS Configuration
```csharp
services.Configure<HttpsRedirectionOptions>(options =>
{
    options.RedirectStatusCode = StatusCodes.Status308PermanentRedirect;
    options.HttpsPort = 5000;
});
```

### 7.2 CORS Policy
```csharp
services.AddCors(options =>
{
    options.AddPolicy("TiHomoCorsPolicy", builder =>
    {
        builder.WithOrigins("https://localhost:3333")
               .AllowAnyHeader()
               .AllowAnyMethod()
               .AllowCredentials();
    });
});
```

### 7.3 Security Headers
- Content Security Policy (CSP)
- X-Frame-Options
- X-Content-Type-Options
- Strict-Transport-Security

---

## 8. Monitoring & Observability

### 8.1 Health Checks
```csharp
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
```

### 8.2 Metrics Collection
- Request count and duration
- Error rates by endpoint
- Service availability metrics
- Custom business metrics

### 8.3 Distributed Tracing
- Correlation ID propagation
- Request flow tracking
- Performance bottleneck identification
- Cross-service dependency mapping

---

## 9. Error Handling & Resilience

### 9.1 Global Exception Handling
```csharp
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var errorFeature = context.Features.Get<IExceptionHandlerFeature>();
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        
        logger.LogError(errorFeature.Error, "Unhandled exception occurred");
        
        context.Response.StatusCode = 500;
        await context.Response.WriteAsync("An error occurred");
    });
});
```

### 9.2 Retry Policies
- Exponential backoff
- Circuit breaker integration
- Timeout handling
- Dead letter queue for failed requests

---

## 10. Performance Optimization

### 10.1 Caching Strategies
- Response caching for static content
- Distributed caching for shared data
- Cache invalidation patterns

### 10.2 Compression
```csharp
services.AddResponseCompression(options =>
{
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});
```

### 10.3 Connection Pooling
- HTTP connection reuse
- Database connection pooling
- Resource optimization

---

## 11. Configuration Management

### 11.1 Environment-Specific Configurations
- Development: `ocelot.Development.json`
- Staging: `ocelot.Staging.json`
- Production: `ocelot.Production.json`

### 11.2 Configuration Sources
- JSON configuration files
- Environment variables
- Azure Key Vault (for production)
- Configuration reload without restart

---

## 12. Deployment & DevOps

### 12.1 Docker Configuration
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
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

### 12.2 Health Check Integration
- Kubernetes readiness/liveness probes
- Load balancer health checks
- Monitoring system integration

---

## 13. Security Best Practices

### 13.1 Input Validation
- Request size limits
- Content type validation
- Parameter sanitization
- SQL injection prevention

### 13.2 API Security
- Rate limiting per client
- Request throttling
- IP whitelisting/blacklisting
- DDoS protection

---

## 14. Testing Strategy

### 14.1 Unit Testing
- Route configuration testing
- Middleware behavior testing
- Authentication/authorization testing

### 14.2 Integration Testing
- End-to-end route testing
- Service integration testing
- Performance testing
- Security testing

---

## 15. Future Enhancements

### 15.1 Planned Features
- GraphQL federation support
- Advanced caching strategies
- AI-powered traffic analysis
- Enhanced monitoring dashboards

### 15.2 Scalability Considerations
- Horizontal scaling strategies
- Performance optimization
- Resource utilization monitoring
- Capacity planning

---

This document serves as the comprehensive guide for the TiHoMo API Gateway implementation and should be updated as the system evolves.
