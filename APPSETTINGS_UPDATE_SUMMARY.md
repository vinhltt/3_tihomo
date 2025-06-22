# TiHoMo Development Configuration Update Summary

## Mục đích
Cập nhật tất cả file `appsettings.Development.json` để đồng bộ với tài nguyên đã tạo trong `docker-compose.dev.yml`.

## Thay đổi đã thực hiện

### 1. Identity Service (`Identity.Api/appsettings.Development.json`)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=identity;Username=identity_user;Password=identity_pass;Port=5431"
  },
  // ✅ Added Redis configuration
  "Redis": {
    "ConnectionString": "localhost:6379",
    "Password": "redis123"
  },
  // ✅ Added RabbitMQ configuration  
  "RabbitMQ": {
    "Host": "localhost",
    "Username": "tihomo",
    "Password": "tihomo123"
  },
  // ✅ Added Prometheus configuration
  "Prometheus": {
    "Enabled": true,
    "MetricsPath": "/metrics"
  }
}
```

**Thay đổi:**
- ✅ **Database**: Sửa port từ 5433 → 5431, database name từ `identity_dev` → `identity`
- ✅ **Redis**: Thêm config Redis với password
- ✅ **RabbitMQ**: Thêm config message queue
- ✅ **Prometheus**: Thêm config metrics

### 2. CoreFinance Service (`CoreFinance.Api/appsettings.Development.json`)
```json
{
  "ConnectionStrings": {
    "CoreFinanceDb": "Host=localhost;Database=corefinance;Username=corefinance_user;Password=corefinance_pass;Port=5432"
  }
  // ✅ Already has RabbitMQ, Loki configs
}
```

**Thay đổi:**
- ✅ **Database**: Sửa port từ 5433 → 5432
- ✅ **Giữ nguyên**: RabbitMQ, Loki configs đã có sẵn

### 3. MoneyManagement Service (`MoneyManagement.Api/appsettings.Development.json`)
```json
{
  "ConnectionStrings": {
    "MoneyManagementDb": "Host=localhost;Database=db_money;Username=money_user;Password=money_pass;Port=5435"
  },
  "UseInMemoryDatabase": false, // ✅ Changed from true
  // ✅ Added Redis configuration
  "Redis": {
    "ConnectionString": "localhost:6379",
    "Password": "redis123"
  },
  // ✅ Added RabbitMQ configuration
  "RabbitMQ": {
    "Host": "localhost",
    "Username": "tihomo",
    "Password": "tihomo123"
  },
  // ✅ Added Prometheus configuration
  "Prometheus": {
    "Enabled": true,
    "MetricsPath": "/metrics"
  }
}
```

**Thay đổi:**
- ✅ **Database**: Sửa username/password từ postgres → money_user/money_pass, thêm port 5435
- ✅ **InMemory**: Tắt InMemory database, dùng PostgreSQL thật
- ✅ **Redis**: Thêm config cache
- ✅ **RabbitMQ**: Thêm config message queue
- ✅ **Prometheus**: Thêm config metrics

### 4. PlanningInvestment Service (`PlanningInvestment.Api/appsettings.Development.json`)
```json
{
  // ✅ Added database configuration
  "ConnectionStrings": {
    "PlanningInvestmentDb": "Host=localhost;Database=db_planning;Username=planning_user;Password=planning_pass;Port=5436"
  },
  // ✅ Added Redis configuration
  "Redis": {
    "ConnectionString": "localhost:6379",
    "Password": "redis123"
  },
  // ✅ Added RabbitMQ configuration
  "RabbitMQ": {
    "Host": "localhost",
    "Username": "tihomo",
    "Password": "tihomo123"
  },
  // ✅ Added Prometheus configuration
  "Prometheus": {
    "Enabled": true,
    "MetricsPath": "/metrics"
  }
}
```

**Thay đổi:**
- ✅ **Database**: Thêm connection string hoàn chỉnh (trước đó chỉ có logging)
- ✅ **Redis**: Thêm config cache
- ✅ **RabbitMQ**: Thêm config message queue
- ✅ **Prometheus**: Thêm config metrics

### 5. ExcelApi Service (`ExcelApi/appsettings.Development.json`)
```json
{
  // ✅ Already has RabbitMQ, Loki configs
  "RabbitMQ": {
    "Host": "localhost",
    "Username": "tihomo", 
    "Password": "tihomo123"
  }
}
```

**Thay đổi:**
- ✅ **Giữ nguyên**: RabbitMQ, Loki configs đã đúng

### 6. Ocelot Gateway (`Ocelot.Gateway/appsettings.Development.json`)
```json
{
  "ServicePorts": {
    "IdentitySsoPort": 5001,
    "CoreFinanceApiPort": 5002,    // ✅ Fixed order
    "MoneyManagementApiPort": 5003,
    "PlanningInvestmentApiPort": 5004,
    "ExcelApiPort": 5005,
    "GatewayPort": 5000
  },
  // ✅ Added Redis configuration
  "Redis": {
    "ConnectionString": "localhost:6379",
    "Password": "redis123"
  },
  // ✅ Added Prometheus configuration
  "Prometheus": {
    "Enabled": true,
    "MetricsPath": "/metrics"
  }
}
```

**Thay đổi:**
- ✅ **Service Ports**: Sửa thứ tự port mapping
- ✅ **Redis**: Thêm config cache cho session
- ✅ **Prometheus**: Thêm config metrics

## Port Mapping Summary

| Service | Database Port | API Port | Database Name | Username |
|---------|---------------|----------|---------------|----------|
| **Identity** | 5431 | 5001 | identity | identity_user |
| **CoreFinance** | 5432 | 5002 | corefinance | corefinance_user |
| **MoneyManagement** | 5435 | 5003 | db_money | money_user |
| **PlanningInvestment** | 5436 | 5004 | db_planning | planning_user |
| **ExcelApi** | - | 5005 | - | - |
| **Gateway** | - | 5000 | - | - |

## Infrastructure Services

| Service | Port | Credentials |
|---------|------|-------------|
| **Redis** | 6379 | Password: redis123 |
| **RabbitMQ** | 5672/15672 | tihomo/tihomo123 |
| **Prometheus** | 9090 | - |
| **Grafana** | 3000 | admin/admin123 |
| **Loki** | 3100 | - |

## Validation Steps

### 1. Kiểm tra kết nối Database
```bash
# Test từng database connection
psql -h localhost -p 5431 -U identity_user -d identity
psql -h localhost -p 5432 -U corefinance_user -d corefinance  
psql -h localhost -p 5435 -U money_user -d db_money
psql -h localhost -p 5436 -U planning_user -d db_planning
```

### 2. Kiểm tra Redis
```bash
redis-cli -h localhost -p 6379
AUTH redis123
PING
```

### 3. Kiểm tra RabbitMQ
- **Management UI**: http://localhost:15672
- **Credentials**: tihomo/tihomo123

### 4. Kiểm tra Prometheus Targets
- **URL**: http://localhost:9090/targets
- **Expected**: Tất cả API services (5000-5005) hiển thị UP

## Next Steps

1. **Start Infrastructure**: `docker-compose -f docker-compose.dev.yml up -d`
2. **Run Setup Script**: `./setup-config.sh` hoặc `setup-config.bat`
3. **Start API Services**: Sử dụng Visual Studio hoặc dotnet CLI
4. **Verify Connections**: Kiểm tra tất cả services kết nối thành công
5. **Test Monitoring**: Xem metrics trong Grafana dashboards

## Benefits

✅ **Consistency**: Tất cả services dùng chung infrastructure  
✅ **Monitoring**: Prometheus metrics từ tất cả APIs  
✅ **Caching**: Redis cho session và performance  
✅ **Messaging**: RabbitMQ cho async processing  
✅ **Logging**: Loki aggregation (CoreFinance, ExcelApi)  
✅ **Development**: Easy local debugging và testing  

Tất cả configs đã được đồng bộ với docker-compose infrastructure! 🎉
