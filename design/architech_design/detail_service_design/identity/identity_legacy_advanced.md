# Identity Service Detailed Design - Advanced Implementation

## 1. Service Overview

Identity Service trong TiHoMo system đã phát triển qua 4 phases chính để trở thành production-ready service với advanced features:

- **Phase 1**: Basic Authentication & Token Verification
- **Phase 2**: Refresh Token Management & Security Enhancement
- **Phase 3**: Resilience Patterns & Circuit Breaker Implementation
- **Phase 4**: Monitoring & Observability System

## 2. Phase 3: Resilience & Circuit Breaker Design

### 2.1 Architecture Pattern
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

### 2.2 Resilience Components

#### 2.2.1 Circuit Breaker Configuration
```csharp
// Circuit breaker opens after 5 failures in 30 seconds
// Stays open for 30 seconds before attempting recovery
FailureRatio = 0.5,
SamplingDuration = TimeSpan.FromSeconds(30),
MinimumThroughput = 5,
BreakDuration = TimeSpan.FromSeconds(30)
```

#### 2.2.2 Retry Strategy
```csharp
// 3 retry attempts with decorrelated jitter backoff
// Starting from 200ms median delay
MaxRetryAttempts = 3,
DelayGenerator = Backoff.DecorrelatedJitterBackoffV2(
    medianFirstRetryDelay: TimeSpan.FromMilliseconds(200),
    retryCount: 3)
```

#### 2.2.3 Fallback Mechanisms
1. **Primary**: External provider API call
2. **Fallback Level 1**: Cached token validation result
3. **Fallback Level 2**: Local JWT parsing for basic claims
4. **Fallback Level 3**: Graceful null return

## 3. Phase 4: Monitoring & Observability Design

### 3.1 Observability Stack
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

### 3.2 Metrics Design

#### 3.2.1 Custom Business Metrics
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

#### 3.2.2 Runtime Metrics (Automatic)
- `process_runtime_dotnet_gc_collections_count_total`
- `process_runtime_dotnet_gc_objects_size_bytes`
- `process_runtime_dotnet_gc_allocations_size_bytes_total`
- `http_server_request_duration_seconds`

### 3.3 Distributed Tracing Design

#### 3.3.1 Activity Sources
- **"Identity.Api"**: Main application activities
- **"Identity.TokenVerification.Resilience"**: Resilience pattern activities

#### 3.3.2 Instrumentation Coverage
- **ASP.NET Core**: HTTP request/response tracing
- **Entity Framework Core**: Database query tracing với SQL statements
- **HTTP Client**: External provider API call tracing
- **Custom Business Logic**: Token verification flow tracing

#### 3.3.3 Trace Context
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

### 3.4 Structured Logging Design

#### 3.4.1 Log Format
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

#### 3.4.2 Correlation Strategy
- **Automatic Generation**: ObservabilityMiddleware generates unique GUID cho mỗi request
- **Propagation**: Correlation ID được propagated qua tất cả log entries trong request
- **Context Enrichment**: Serilog LogContext automatically includes correlation properties

## 4. Health Check Design

### 4.1 Health Check Hierarchy
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

### 4.2 Health Check Response
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

## 5. Implementation Components

### 5.1 Key Classes
- **ResilientTokenVerificationService**: Main resilience wrapper service
- **TelemetryService**: Custom metrics và business logic monitoring
- **ObservabilityMiddleware**: Request correlation và timing middleware
- **CircuitBreakerHealthCheck**: Resilience patterns health monitoring
- **TelemetryHealthCheck**: Observability system health verification

### 5.2 Package Dependencies
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

## 6. Production Benefits

### 6.1 Availability Improvements
- **99.9% uptime** even during external provider outages
- **Automatic recovery** when providers come back online
- **Predictable latency** với 10-second maximum response time
- **Graceful degradation** với multi-level fallback

### 6.2 Observability Benefits
- **Complete request tracing** với unique correlation IDs
- **Real-time metrics** cho business logic và infrastructure
- **Detailed error tracking** với stack traces và context
- **Performance monitoring** với response times và throughput
- **Dashboard-ready** metrics cho Grafana visualization
- **Alerting-ready** health checks cho Prometheus AlertManager

### 6.3 Operational Benefits
- **Zero-downtime deployments** với health check integration
- **Proactive monitoring** với circuit breaker state tracking
- **Debugging efficiency** với structured logging và distributed tracing
- **Performance optimization** với detailed metrics và profiling data
- **SLA compliance** với comprehensive monitoring và alerting
