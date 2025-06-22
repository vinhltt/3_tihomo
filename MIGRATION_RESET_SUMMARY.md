# TiHoMo Migration Reset & Recreation Summary

## Mục đích
Xóa tất cả migrations hiện tại và tạo lại sau khi có thay đổi trong Shared components để đảm bảo tính nhất quán của database schema.

## Thay đổi đã thực hiện

### ✅ 1. Xóa tất cả migrations cũ
```bash
# Đã xóa các thư mục migrations sau:
- Identity/Identity.Infrastructure/Migrations
- Identity/Identity.Api/Migrations  
- CoreFinance/CoreFinance.Infrastructure/Migrations
```

### ✅ 2. Tạo lại migrations cho các service hiện có

#### **Identity Service**
```bash
cd Identity/Identity.Infrastructure
dotnet ef migrations add InitialCreate --startup-project ../Identity.Api --context Identity.Infrastructure.Data.IdentityDbContext
```
- ✅ **Status**: Success
- ✅ **Migration**: `InitialCreate` created
- ✅ **Context**: `Identity.Infrastructure.Data.IdentityDbContext`

#### **CoreFinance Service**
```bash
cd CoreFinance/CoreFinance.Infrastructure
dotnet ef migrations add InitialCreate --startup-project ../CoreFinance.Api
```
- ✅ **Status**: Success
- ✅ **Migration**: `InitialCreate` created
- ✅ **Context**: `CoreFinanceDbContext`

#### **MoneyManagement Service**
```bash
cd MoneyManagement/MoneyManagement.Infrastructure
dotnet ef migrations add InitialCreate --startup-project ../MoneyManagement.Api
```
- ✅ **Status**: Success
- ✅ **Migration**: `InitialCreate` created
- ✅ **Context**: `MoneyManagementDbContext`

### ✅ 3. Thiết lập PlanningInvestment Service (mới)

#### **Created DbContext**
- ✅ **File**: `PlanningInvestment.Infrastructure/PlanningInvestmentDbContext.cs`
- ✅ **Features**:
  - Inherits from `DbContext`
  - Uses PostgreSQL with snake_case naming convention
  - Configured for `Debt` entity
  - Proper column mapping for BaseEntity properties

#### **Updated Project References**
- ✅ **Infrastructure.csproj**: Added EF Core packages
- ✅ **Api.csproj**: Added Infrastructure reference and EF Tools
- ✅ **Program.cs**: Added DbContext configuration and health checks

#### **Migration Created**
```bash
cd PlanningInvestment/PlanningInvestment.Infrastructure
dotnet ef migrations add InitialCreate --startup-project ../PlanningInvestment.Api
```
- ✅ **Status**: Success
- ✅ **Migration**: `InitialCreate` created
- ✅ **Context**: `PlanningInvestmentDbContext`

## File Changes Summary

### ✅ Created Files
```
PlanningInvestment/PlanningInvestment.Infrastructure/
├── PlanningInvestmentDbContext.cs
└── Migrations/
    ├── {timestamp}_InitialCreate.cs
    ├── {timestamp}_InitialCreate.Designer.cs
    └── PlanningInvestmentDbContextModelSnapshot.cs
```

### ✅ Updated Files
```
PlanningInvestment/PlanningInvestment.Infrastructure/PlanningInvestment.Infrastructure.csproj
PlanningInvestment/PlanningInvestment.Api/PlanningInvestment.Api.csproj
PlanningInvestment/PlanningInvestment.Api/Program.cs
```

### ✅ Recreated Migration Files
```
Identity/Identity.Infrastructure/Migrations/
├── {timestamp}_InitialCreate.cs
├── {timestamp}_InitialCreate.Designer.cs
└── IdentityDbContextModelSnapshot.cs

CoreFinance/CoreFinance.Infrastructure/Migrations/
├── {timestamp}_InitialCreate.cs
├── {timestamp}_InitialCreate.Designer.cs
└── CoreFinanceDbContextModelSnapshot.cs

MoneyManagement/MoneyManagement.Infrastructure/Migrations/
├── {timestamp}_InitialCreate.cs
├── {timestamp}_InitialCreate.Designer.cs
└── MoneyManagementDbContextModelSnapshot.cs
```

## Database Schema Alignment

### ✅ BaseEntity Properties Mapping
Tất cả entities bây giờ đều tuân theo cùng BaseEntity schema:

```csharp
// Common base properties với snake_case mapping
Id -> id
CreatedAt -> created_at
UpdatedAt -> updated_at
CreateBy -> create_by
UpdateBy -> update_by
IsDeleted -> is_deleted
```

### ✅ Service-Specific Entities
| Service | Entities | Database | Port |
|---------|----------|----------|------|
| **Identity** | User, Role, ApiKey, RefreshToken | identity | 5431 |
| **CoreFinance** | Account, Transaction, Category, etc. | corefinance | 5432 |
| **MoneyManagement** | Budget, BudgetCategory, Expense, etc. | db_money | 5435 |
| **PlanningInvestment** | Debt | db_planning | 5436 |

## Next Steps

### 1. Update Database
Sau khi start infrastructure containers, apply migrations:

```bash
# Start infrastructure first
docker-compose -f docker-compose.dev.yml up -d

# Apply migrations for each service
cd Identity/Identity.Infrastructure
dotnet ef database update --startup-project ../Identity.Api --context Identity.Infrastructure.Data.IdentityDbContext

cd ../../CoreFinance/CoreFinance.Infrastructure  
dotnet ef database update --startup-project ../CoreFinance.Api

cd ../../MoneyManagement/MoneyManagement.Infrastructure
dotnet ef database update --startup-project ../MoneyManagement.Api

cd ../../PlanningInvestment/PlanningInvestment.Infrastructure
dotnet ef database update --startup-project ../PlanningInvestment.Api
```

### 2. Verify Database Schema
Kiểm tra các bảng được tạo đúng với schema:

```sql
-- Connect to each database and verify tables
\c identity
\dt

\c corefinance  
\dt

\c db_money
\dt

\c db_planning
\dt
```

### 3. Test API Services
Start tất cả API services và verify kết nối database:
- Identity API: http://localhost:5001/health
- CoreFinance API: http://localhost:5002/health
- MoneyManagement API: http://localhost:5003/health  
- PlanningInvestment API: http://localhost:5004/health

## Lợi ích đạt được

✅ **Consistency**: Tất cả services dùng chung BaseEntity schema  
✅ **Clean Slate**: Migrations mới phản ánh đúng current state  
✅ **Complete Coverage**: Cả 4 services đều có migrations  
✅ **Infrastructure Ready**: PlanningInvestment đã setup đầy đủ  
✅ **Version Alignment**: EF Core packages đồng bộ  

Tất cả migrations đã được reset và tạo lại thành công! 🎉
