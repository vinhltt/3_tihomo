# TiHoMo Deployment System Usage Guide

## Tổng quan

Hệ thống deployment mới của TiHoMo được thiết kế để hỗ trợ deployment linh hoạt với các tùy chọn:
- **Service-specific deployment**: Deploy từng service riêng lẻ
- **Dependency management**: Tự động stop/restart dependent services
- **Environment-aware**: Hỗ trợ development và production configuration
- **Manual trigger only**: Chỉ deploy khi được trigger thủ công

## Workflow Structure

### 1. Deploy Orchestrator (Main Workflow)
**File**: `.github/workflows/deploy-orchestrator.yml`

Workflow chính để điều phối tất cả deployment:

```bash
# Trigger via GitHub Actions UI với các tùy chọn:
- deployment_type: full-system | infrastructure-only | backend-only | frontend-only | specific-service
- environment: development | staging | production  
- specific_services: comma-separated list (khi chọn specific-service)
- force_rebuild: true/false
- skip_health_checks: true/false
```

### 2. Deploy Infrastructure
**File**: `.github/workflows/deploy-infrastructure.yml`

Deploy cơ sở hạ tầng (databases, Redis, RabbitMQ):

```bash
# Services deployed:
- PostgreSQL databases (identity, corefinance, moneymanagement, planninginvestment, reporting)
- Redis (with authentication)
- RabbitMQ (with management UI)
```

### 3. Deploy Backend Services
**File**: `.github/workflows/deploy-backend-services.yml`

Deploy API services với dependency management:

```bash
# Services available:
- identity-api (depends on: identity-postgres, redis)
- corefinance-api (depends on: corefinance-postgres, identity-api, redis)
- excel-api (depends on: corefinance-api)
- ocelot-gateway (depends on: identity-api, corefinance-api)
```

### 4. Deploy Frontend
**File**: `.github/workflows/deploy-frontend.yml`

Deploy Nuxt frontend application:

```bash
# Frontend deployment:
- Graceful shutdown of existing instance
- Build with environment-specific configuration
- Health check với backend connectivity test
```

## Dependency Management

Hệ thống tự động quản lý dependencies:

### Backend Dependencies
```
identity-api → corefinance-api → excel-api
     ↓              ↓              ↓
ocelot-gateway ← ← ← ← ← ← ← ← ← ← ←
     ↓
frontend-nuxt
```

### Dependency Rules
1. **Khi deploy identity-api**: Stop tất cả dependent services (corefinance-api, excel-api, ocelot-gateway, frontend-nuxt)
2. **Khi deploy corefinance-api**: Stop excel-api, ocelot-gateway, frontend-nuxt
3. **Khi deploy ocelot-gateway**: Stop frontend-nuxt
4. **Sau khi deploy**: Tự động restart các services đã bị stop

## Environment Configuration

Hệ thống sử dụng **single docker-compose.yml** với environment variables để control behavior:

### Development (Default)
```bash
# Environment variables for development:
NODE_ENV=development
NUXT_DEV_SSR=true
NUXT_DEV_TOOLS=true
API_LOG_LEVEL=Information
ENABLE_DEV_TOOLS=development
FRONTEND_SOURCE_MOUNT=./src/fe/nuxt
```

### Production
```bash  
# Environment variables for production:
NODE_ENV=production
NUXT_DEV_SSR=false
NUXT_DEV_TOOLS=false
API_LOG_LEVEL=Warning
ENABLE_DEV_TOOLS=none
FRONTEND_SOURCE_MOUNT=/dev/null
REDIS_PERSISTENCE_CONFIG=--save 900 1 --save 300 10 --save 60 10000
PROMETHEUS_RETENTION=30d
GRAFANA_ANALYTICS_ENABLED=false
```

### Key Differences
- **Development**: Source code mount, debug tools enabled, detailed logging
- **Production**: Built app only, minimal logging, persistence enabled, analytics disabled

## Usage Examples

### 1. Full System Deployment
```bash
# Deploy all infrastructure + backend + frontend
Deployment Type: full-system
Environment: development
Force Rebuild: false
```

### 2. Infrastructure Only
```bash
# Deploy only databases, Redis, RabbitMQ
Deployment Type: infrastructure-only
Environment: production
Force Rebuild: false
```

### 3. Backend Services Only
```bash
# Deploy all API services
Deployment Type: backend-only
Environment: development
Force Rebuild: true
```

### 4. Single Service Deployment
```bash
# Deploy specific service(s)
Deployment Type: specific-service
Specific Services: identity-api,corefinance-api
Environment: production
Force Rebuild: false
```

### 5. Frontend Only
```bash
# Deploy frontend with production build
Deployment Type: frontend-only
Environment: production
Force Rebuild: true
```

## Health Checks

### Infrastructure Health Check
- PostgreSQL databases connectivity
- Redis authentication và connectivity
- RabbitMQ management interface

### Backend Health Check
- API endpoints `/health` status
- Service dependencies verification
- Container status monitoring

### Frontend Health Check
- HTTP response từ Nuxt application
- Backend connectivity từ frontend
- Production build verification

### Health Check Behavior
- **Development**: Warnings only, deployment continues
- **Production**: Warnings logged, manual verification recommended
- **Critical failures**: Only hard dependency failures stop deployment

## Troubleshooting

### Common Issues

1. **Environment Variables Missing**
```bash
# Check .env file in deployment directory
ssh truenas-cf-tunnel
cd /path/to/deploy_branch
cat .env | grep MISSING_VAR
```

2. **Service Startup Failures**
```bash
# Check service logs
docker compose logs --tail=50 service-name
```

3. **Dependency Chain Issues**
```bash
# Check service status
docker compose ps --format "table {{.Name}}\t{{.Status}}"
```

4. **Network Configuration**
```bash
# Check network subnet configuration
grep DOCKER_NETWORK_SUBNET .env
docker network ls | grep tihomo
```

### Manual Recovery

#### Restart Dependent Services
```bash
# If deployment partially fails, manually restart:
docker compose start corefinance-api
sleep 10
docker compose start excel-api
sleep 10
docker compose start ocelot-gateway
sleep 10
docker compose start frontend-nuxt
```

#### Reset Deployment
```bash
# Complete reset (use with caution)
docker compose down
docker system prune -f
# Re-run deployment
```

## Best Practices

### 1. Deployment Order
1. Infrastructure → Backend → Frontend
2. Always deploy dependencies before dependents
3. Use force rebuild after major changes

### 2. Environment Strategy
- **Development**: Quick iteration, debug enabled
- **Staging**: Production-like testing
- **Production**: Optimized, monitoring enabled

### 3. Monitoring
- Check Discord notifications for deployment status
- Review GitHub Actions logs for detailed information
- Monitor Grafana dashboards after deployment

### 4. Rollback Strategy
- Keep previous deployments in separate directories
- Use Git tags for version tracking
- Quick rollback: redeploy previous working commit

## Access Points After Deployment

### Development Environment
```
Frontend: http://truenas-ip:3500
API Gateway: http://truenas-ip:5000
Grafana: http://truenas-ip:3002
RabbitMQ: http://truenas-ip:15672
pgAdmin: http://truenas-ip:8080
```

### Production Environment
```
Frontend: http://truenas-ip:3500 (behind reverse proxy)
API Gateway: http://truenas-ip:5000 (internal)
Monitoring: http://truenas-ip:3002
```

## Notifications

Tất cả deployment results sẽ được gửi đến Discord channel với thông tin:
- Deployment status (Success/Failed)
- Services deployed
- Environment và branch
- Access URLs
- Link đến detailed logs

## Simplified Architecture

### Single Docker Compose File
Hệ thống đã được **simplified** để sử dụng **single docker-compose.yml** file với:
- **Environment variables** để control behavior
- **Conditional configuration** dựa trên `NODE_ENV` và deployment environment
- **No more docker-compose.prod.yml** - all configuration via environment variables

### Environment Variables Auto-Setting
Infrastructure workflow tự động set environment variables based on deployment environment:

```bash
# Development environment tự động set:
NUXT_DEV_SSR=true
NUXT_DEV_TOOLS=true  
API_LOG_LEVEL=Information
ENABLE_DEV_TOOLS=development
FRONTEND_SOURCE_MOUNT=./src/fe/nuxt

# Production environment tự động set:
NUXT_DEV_SSR=false
NUXT_DEV_TOOLS=false
API_LOG_LEVEL=Warning
ENABLE_DEV_TOOLS=none
FRONTEND_SOURCE_MOUNT=/dev/null
REDIS_PERSISTENCE_CONFIG=--save 900 1 --save 300 10 --save 60 10000
```

### Benefits of Single File Approach
1. **Simpler maintenance**: Chỉ 1 file compose để maintain
2. **Environment parity**: Đảm bảo development và production dùng same services
3. **Clearer configuration**: Tất cả config trong environment variables
4. **Easier debugging**: Không cần switch giữa multiple compose files

---

**Note**: Hệ thống này chỉ hỗ trợ manual deployment triggers. Không có auto-deployment để đảm bảo kiểm soát chặt chẽ việc deploy lên production.