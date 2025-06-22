# TiHoMo Development Environment Setup

## Overview
File `docker-compose.dev.yml` cung cấp toàn bộ infrastructure cần thiết cho development và debugging của hệ thống TiHoMo.

## Services Included

### Databases (PostgreSQL)
- **identity-postgres** (Port 5431): Database cho Identity service
- **corefinance-postgres** (Port 5432): Database cho CoreFinance service  
- **moneymanagement-postgres** (Port 5435): Database cho MoneyManagement service
- **planninginvestment-postgres** (Port 5436): Database cho PlanningInvestment service
- **reporting-postgres** (Port 5437): Database cho Reporting service

### Caching & Session
- **redis** (Port 6379): Cache và session storage

### Message Queue
- **rabbitmq** (Port 5672/15672): Message queue với management UI

### Monitoring & Logging
- **prometheus** (Port 9090): Metrics collection
- **grafana** (Port 3000): Dashboards và visualization
- **loki** (Port 3100): Log aggregation

### Development Tools
- **pgadmin** (Port 8080): Database management interface
- **mailhog** (Port 8025): Email testing tool
- **nginx** (Port 80/443): Reverse proxy

## Usage

### 1. Start All Services
```bash
docker-compose -f docker-compose.dev.yml up -d
```

### 2. Start Specific Services
```bash
# Chỉ start databases
docker-compose -f docker-compose.dev.yml up -d identity-postgres corefinance-postgres moneymanagement-postgres planninginvestment-postgres

# Chỉ start monitoring stack
docker-compose -f docker-compose.dev.yml up -d prometheus grafana loki

# Chỉ start message queue
docker-compose -f docker-compose.dev.yml up -d rabbitmq redis
```

### 3. Stop All Services
```bash
docker-compose -f docker-compose.dev.yml down
```

### 4. Stop and Remove Volumes (⚠️ Data Loss)
```bash
docker-compose -f docker-compose.dev.yml down -v
```

## Service URLs

| Service | URL | Credentials |
|---------|-----|------------|
| Grafana Dashboard | http://localhost:3000 | admin/admin123 |
| RabbitMQ Management | http://localhost:15672 | tihomo/tihomo123 |
| pgAdmin | http://localhost:8080 | admin@tihomo.local/admin123 |
| Mailhog UI | http://localhost:8025 | - |
| Prometheus | http://localhost:9090 | - |

## Database Connections

### Connection Strings
```
# Identity Service
Host=localhost;Port=5431;Database=identity;Username=identity_user;Password=identity_pass

# CoreFinance Service  
Host=localhost;Port=5432;Database=corefinance;Username=corefinance_user;Password=corefinance_pass

# MoneyManagement Service
Host=localhost;Port=5435;Database=db_money;Username=money_user;Password=money_pass

# PlanningInvestment Service
Host=localhost;Port=5436;Database=db_planning;Username=planning_user;Password=planning_pass

# Reporting Service
Host=localhost;Port=5437;Database=db_reporting;Username=reporting_user;Password=reporting_pass
```

## Development Workflow

### 1. Start Infrastructure
```bash
# Start tất cả tài nguyên cần thiết
docker-compose -f docker-compose.dev.yml up -d

# Kiểm tra trạng thái
docker-compose -f docker-compose.dev.yml ps
```

### 2. Run API Services
Sau khi infrastructure đã chạy, bạn có thể start các API service bằng Visual Studio hoặc dotnet CLI:

```bash
# API Gateway (Port 5000)
cd src/be/Ocelot.Gateway
dotnet run

# Identity Service (Port 5001)  
cd src/be/Identity/Identity.Api
dotnet run

# CoreFinance Service (Port 5002)
cd src/be/CoreFinance/CoreFinance.Api  
dotnet run

# MoneyManagement Service (Port 5003)
cd src/be/MoneyManagement/MoneyManagement.Api
dotnet run

# PlanningInvestment Service (Port 5004)
cd src/be/PlanningInvestment/PlanningInvestment.Api
dotnet run

# ExcelApi Service (Port 5005)
cd src/be/ExcelApi
dotnet run
```

### 3. Run Frontend
```bash
cd src/fe/nuxt
npm install
npm run dev
# Frontend sẽ chạy trên port 3333
```

## Architecture Flow

```
[Frontend:3333] 
    ↓ 
[API Gateway:5000] 
    ↓ 
[Microservices:5001-5005]
    ↓ 
[Databases:5431,5432,5435-5437]
```

Frontend gọi API Gateway, Gateway routing đến các microservice tương ứng, mỗi service connect đến database riêng.

## Troubleshooting

### Port Conflicts
Nếu gặp lỗi port đã được sử dụng:
```bash
# Kiểm tra process đang dùng port
netstat -ano | findstr :5432

# Kill process (thay <PID> bằng process ID)
taskkill /PID <PID> /F
```

### Database Connection Issues
```bash
# Kiểm tra database đã start chưa
docker-compose -f docker-compose.dev.yml ps

# Xem logs của database service
docker-compose -f docker-compose.dev.yml logs identity-postgres
```

### Reset Data
```bash
# Stop services và xóa volumes
docker-compose -f docker-compose.dev.yml down -v

# Start lại
docker-compose -f docker-compose.dev.yml up -d
```

## Notes

- Tất cả services đều chạy trong network `tihomo-network` (172.20.0.0/16)
- Data được persist trong Docker volumes
- Config files được mount từ thư mục `config/` (cần tạo nếu chưa có)
- Development tools (pgAdmin, Mailhog) giúp debug và test
- Monitoring stack (Prometheus, Grafana, Loki) để theo dõi hệ thống
