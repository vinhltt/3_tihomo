# TiHoMo Port Mapping & Architecture Summary

## Port Allocation

### API Services
| Service | Port | Description |
|---------|------|-------------|
| **API Gateway** | 5000 | Ocelot Gateway - Entry point cho tất cả API calls |
| **Identity Service** | 5001 | Authentication & Authorization |
| **CoreFinance Service** | 5002 | Core financial transactions |
| **MoneyManagement Service** | 5003 | Budget & expense management |
| **PlanningInvestment Service** | 5004 | Investment planning & portfolio |
| **ExcelApi Service** | 5005 | Excel import/export functionality |

### Frontend
| Service | Port | Description |
|---------|------|-------------|
| **Nuxt Frontend** | 3333 | Main frontend application |

### Databases (PostgreSQL)
| Database | Port | Credentials |
|----------|------|-------------|
| **Identity DB** | 5431 | identity_user/identity_pass |
| **CoreFinance DB** | 5432 | corefinance_user/corefinance_pass |
| **MoneyManagement DB** | 5435 | money_user/money_pass |
| **PlanningInvestment DB** | 5436 | planning_user/planning_pass |
| **Reporting DB** | 5437 | reporting_user/reporting_pass |

### Infrastructure Services
| Service | Port | Description | Credentials |
|---------|------|-------------|-------------|
| **Redis** | 6379 | Cache & session store | - |
| **RabbitMQ** | 5672/15672 | Message queue | tihomo/tihomo123 |
| **Prometheus** | 9090 | Metrics collection | - |
| **Grafana** | 3000 | Dashboards | admin/admin123 |
| **Loki** | 3100 | Log aggregation | - |
| **pgAdmin** | 8080 | Database management | admin@tihomo.local/admin123 |
| **Mailhog** | 1025/8025 | Email testing | - |
| **Nginx** | 80/443 | Reverse proxy | - |

## Architecture Flow

```
┌─────────────────┐
│  Frontend:3333  │
└─────────┬───────┘
          │ All API calls
          ▼
┌─────────────────┐
│ API Gateway:5000│ ◄─── Single entry point
└─────────┬───────┘
          │ Routes to specific services
          ▼
┌─────────────────────────────────────────┐
│         Microservices                   │
│  ┌─────────┐ ┌─────────┐ ┌─────────┐   │
│  │Identity │ │CoreFin  │ │MoneyMgmt│   │
│  │  :5001  │ │  :5002  │ │  :5003  │   │
│  └─────────┘ └─────────┘ └─────────┘   │
│  ┌─────────┐ ┌─────────┐               │
│  │Planning │ │ExcelApi │               │
│  │  :5004  │ │  :5005  │               │
│  └─────────┘ └─────────┘               │
└─────────┬───────────────────────────────┘
          │ Each service connects to own DB
          ▼
┌─────────────────────────────────────────┐
│              Databases                  │
│  ┌─────────┐ ┌─────────┐ ┌─────────┐   │
│  │Identity │ │CoreFin  │ │MoneyMgmt│   │
│  │  :5431  │ │  :5432  │ │  :5435  │   │
│  └─────────┘ └─────────┘ └─────────┘   │
│  ┌─────────┐ ┌─────────┐               │
│  │Planning │ │Reporting│               │
│  │  :5436  │ │  :5437  │               │
│  └─────────┘ └─────────┘               │
└─────────────────────────────────────────┘
```

## Gateway Routing Rules

### Ocelot.json Configuration
```json
{
  "Routes": [
    {
      "UpstreamPathTemplate": "/api/identity/{everything}",
      "DownstreamPathTemplate": "/api/{everything}",
      "DownstreamHostAndPorts": [{"Host": "localhost", "Port": 5001}]
    },
    {
      "UpstreamPathTemplate": "/api/corefinance/{everything}",
      "DownstreamPathTemplate": "/api/{everything}",
      "DownstreamHostAndPorts": [{"Host": "localhost", "Port": 5002}]
    },
    {
      "UpstreamPathTemplate": "/api/moneymanagement/{everything}",
      "DownstreamPathTemplate": "/api/{everything}",
      "DownstreamHostAndPorts": [{"Host": "localhost", "Port": 5003}]
    },
    {
      "UpstreamPathTemplate": "/api/planninginvestment/{everything}",
      "DownstreamPathTemplate": "/api/{everything}",
      "DownstreamHostAndPorts": [{"Host": "localhost", "Port": 5004}]
    },
    {
      "UpstreamPathTemplate": "/api/excel/{everything}",
      "DownstreamPathTemplate": "/api/{everything}",
      "DownstreamHostAndPorts": [{"Host": "localhost", "Port": 5005}]
    }
  ]
}
```

## Frontend Configuration

### Nuxt.config.ts
```typescript
export default defineNuxtConfig({
  runtimeConfig: {
    public: {
      apiBase: 'http://localhost:5000' // Gateway URL
    }
  },
  devServer: {
    port: 3333
  }
})
```

### API Call Examples
```typescript
// ❌ WRONG - Direct service call
const response = await $fetch('http://localhost:5001/api/auth/login')

// ✅ CORRECT - Through Gateway
const response = await $fetch('/api/identity/auth/login', {
  baseURL: useRuntimeConfig().public.apiBase
})
```

## Development Workflow

### 1. Start Infrastructure
```bash
# Windows
.\start-dev-env.bat

# Linux/WSL
./start-dev-env.sh
```

### 2. Start API Services
```bash
# Gateway (must start first)
cd src/be/Ocelot.Gateway && dotnet run

# Services (can start in parallel)
cd src/be/Identity/Identity.Api && dotnet run
cd src/be/CoreFinance/CoreFinance.Api && dotnet run
cd src/be/MoneyManagement/MoneyManagement.Api && dotnet run
cd src/be/PlanningInvestment/PlanningInvestment.Api && dotnet run
cd src/be/ExcelApi && dotnet run
```

### 3. Start Frontend
```bash
cd src/fe/nuxt && npm run dev
```

## Key Changes Made

### 1. Port Standardization
- ✅ Identity service: 5001 (fixed from 5228)
- ✅ Gateway: 5000 (confirmed)
- ✅ Frontend: 3333 (standardized)

### 2. Gateway Routing
- ✅ Added ExcelApi route: `/api/excel/{everything}`
- ✅ All routes properly configured with correct ports

### 3. Frontend Cleanup
- ✅ Removed all SSO code
- ✅ Updated apiBase to use Gateway (5000)
- ✅ All API calls now go through Gateway

### 4. Infrastructure
- ✅ Complete docker-compose.dev.yml with all services
- ✅ Proper database separation per service
- ✅ Monitoring & logging stack
- ✅ Development tools (pgAdmin, Mailhog)

## Benefits of Gateway Pattern

### 1. Security
- Single entry point for authentication
- Centralized authorization policies
- API key management
- Rate limiting

### 2. Scalability  
- Load balancing across service instances
- Circuit breaker patterns
- Retry policies
- Request/response transformation

### 3. Maintainability
- Service discovery
- Centralized logging & monitoring
- API versioning
- CORS handling

### 4. Development Experience
- Single URL for frontend
- Consistent error handling
- API documentation aggregation
- Testing & debugging ease

## Monitoring URLs

- **Application Metrics**: http://localhost:9090 (Prometheus)
- **System Dashboards**: http://localhost:3000 (Grafana)
- **Message Queue**: http://localhost:15672 (RabbitMQ)
- **Database Management**: http://localhost:8080 (pgAdmin)
- **Email Testing**: http://localhost:8025 (Mailhog)

All services are now properly configured and ready for development!
