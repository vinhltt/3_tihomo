# Feature Design: Investment Portfolio Tracking - Basic Implementation

## 1. Feature Overview & Business Requirements

### 1.1 Tổng quan tính năng (Feature Overview)
TiHoMo cần một module quản lý đầu tư cơ bản để users có thể track portfolio, tính toán lời/lỗ, và monitor investment performance. Đây là tính năng foundational cho Planning & Investment bounded context, sẽ được extend với advanced features trong future releases.

### 1.2 Business Requirements
- **BR-001**: User có thể nhập thông tin investment (symbol, type, purchase price, quantity, purchase date)
- **BR-002**: User có thể update current market price manually để track real-time performance
- **BR-003**: System tự động calculate profit/loss dựa trên (current_price - purchase_price) * quantity
- **BR-004**: User có thể view portfolio overview với total invested capital và current value
- **BR-005**: Basic CRUD operations cho investment records với proper data validation

### 1.3 Success Criteria
- User có thể add new investment trong < 60 giây
- Profit/loss calculation hiển thị real-time khi update market price
- Portfolio overview cung cấp quick insights về investment performance
- System maintain data integrity với proper error handling
- Foundation sẵn sàng cho advanced features (charts, analytics, alerts)

## 2. Current State Analysis

### 2.1 Existing Implementation ✅
**PlanningInvestment Service đã có:**
- Clean Architecture structure (Domain, Application, Infrastructure, API, Contracts)
- `PlanningInvestmentDbContext` với PostgreSQL integration
- EF Core migrations setup với snake_case naming convention
- `Debt` entity như reference pattern cho new entities
- API structure với swagger documentation
- Integration với existing TiHoMo authentication system

### 2.2 Architecture Strengths
- Follows established Clean Architecture patterns
- Consistent với other TiHoMo services (CoreFinance, MoneyManagement)
- PostgreSQL database với proper migration management
- Bilingual XML documentation standard
- xUnit + FluentAssertions testing framework

### 2.3 Current Limitations
- Chỉ có `Debt` entity, chưa có `Investment` entity
- Chưa có business logic services cho investment management
- Chưa có API endpoints cho investment operations
- Chưa có DTOs cho investment data transfer
- Frontend UI chưa được implement

## 3. Gap Analysis & Enhancement Requirements

### 3.1 Backend Enhancements Required
| Component | Current Status | Required Enhancement |
|-----------|----------------|---------------------|
| Domain Entity | ❌ Missing | Create `Investment` entity với proper properties |
| Business Service | ❌ Missing | Create `InvestmentService` với CRUD operations |
| API Controllers | ❌ Missing | Create `InvestmentController` với REST endpoints |
| DTOs | ❌ Missing | Create request/response DTOs cho API |
| Validators | ❌ Missing | FluentValidation cho investment data |
| Unit Tests | ❌ Missing | Comprehensive test coverage |
| Database Migration | ❌ Missing | EF Core migration cho Investment table |

### 3.2 Frontend Requirements (Future Story)
- Investment management UI components
- Portfolio dashboard với charts
- Real-time profit/loss display
- Integration với backend APIs

## 4. Technical Design

### 4.1 Domain Model Design

#### Investment Entity
```csharp
/// <summary>
/// Represents an investment record in user's portfolio (EN)<br/>
/// Đại diện cho một bản ghi đầu tư trong danh mục của người dùng (VI)
/// </summary>
public class Investment : BaseEntity
{
    /// <summary>
    /// Stock symbol or investment identifier (EN)<br/>
    /// Mã chứng khoán hoặc định danh đầu tư (VI)
    /// </summary>
    public string Symbol { get; set; } = string.Empty;
    
    /// <summary>
    /// Type of investment (Stock, Bond, Fund, etc.) (EN)<br/>
    /// Loại đầu tư (Cổ phiếu, Trái phiếu, Quỹ, v.v.) (VI)
    /// </summary>
    public InvestmentType InvestmentType { get; set; }
    
    /// <summary>
    /// Price paid when purchasing the investment (EN)<br/>
    /// Giá đã trả khi mua đầu tư (VI)
    /// </summary>
    public decimal PurchasePrice { get; set; }
    
    /// <summary>
    /// Number of shares/units purchased (EN)<br/>
    /// Số lượng cổ phiếu/đơn vị đã mua (VI)
    /// </summary>
    public decimal Quantity { get; set; }
    
    /// <summary>
    /// Current market price (manually updated) (EN)<br/>
    /// Giá thị trường hiện tại (cập nhật thủ công) (VI)
    /// </summary>
    public decimal? CurrentMarketPrice { get; set; }
    
    /// <summary>
    /// Date when investment was purchased (EN)<br/>
    /// Ngày mua đầu tư (VI)
    /// </summary>
    public DateTime PurchaseDate { get; set; }
    
    /// <summary>
    /// User ID who owns this investment (EN)<br/>
    /// ID người dùng sở hữu khoản đầu tư này (VI)
    /// </summary>
    public string UserId { get; set; } = string.Empty;
    
    /// <summary>
    /// Optional notes about the investment (EN)<br/>
    /// Ghi chú tùy chọn về khoản đầu tư (VI)
    /// </summary>
    public string? Notes { get; set; }
    
    // Calculated Properties
    /// <summary>
    /// Total invested amount (PurchasePrice * Quantity) (EN)<br/>
    /// Tổng số tiền đầu tư (Giá mua * Số lượng) (VI)
    /// </summary>
    public decimal TotalInvestedAmount => PurchasePrice * Quantity;
    
    /// <summary>
    /// Current total value if market price is available (EN)<br/>
    /// Tổng giá trị hiện tại nếu có giá thị trường (VI)
    /// </summary>
    public decimal? CurrentTotalValue => CurrentMarketPrice * Quantity;
    
    /// <summary>
    /// Profit or loss amount (EN)<br/>
    /// Số tiền lời hoặc lỗ (VI)
    /// </summary>
    public decimal? ProfitLoss => CurrentTotalValue - TotalInvestedAmount;
    
    /// <summary>
    /// Profit or loss percentage (EN)<br/>
    /// Phần trăm lời hoặc lỗ (VI)
    /// </summary>
    public decimal? ProfitLossPercentage => 
        CurrentMarketPrice.HasValue ? 
        ((CurrentMarketPrice.Value - PurchasePrice) / PurchasePrice) * 100 : 
        null;
}
```

#### InvestmentType Enum
```csharp
/// <summary>
/// Types of investments supported by the system (EN)<br/>
/// Các loại đầu tư được hệ thống hỗ trợ (VI)
/// </summary>
public enum InvestmentType
{
    /// <summary>Stock/Equity (EN)<br/>Cổ phiếu (VI)</summary>
    Stock = 1,
    
    /// <summary>Government Bond (EN)<br/>Trái phiếu chính phủ (VI)</summary>
    GovernmentBond = 2,
    
    /// <summary>Corporate Bond (EN)<br/>Trái phiếu doanh nghiệp (VI)</summary>
    CorporateBond = 3,
    
    /// <summary>Mutual Fund (EN)<br/>Quỹ tương hỗ (VI)</summary>
    MutualFund = 4,
    
    /// <summary>ETF (Exchange Traded Fund) (EN)<br/>Quỹ ETF (VI)</summary>
    ETF = 5,
    
    /// <summary>Real Estate Investment Trust (EN)<br/>Quỹ đầu tư bất động sản (VI)</summary>
    REIT = 6,
    
    /// <summary>Other investment types (EN)<br/>Các loại đầu tư khác (VI)</summary>
    Other = 99
}
```

### 4.2 Service Layer Design

#### InvestmentService Interface
```csharp
/// <summary>
/// Service interface for investment management operations (EN)<br/>
/// Giao diện dịch vụ cho các thao tác quản lý đầu tư (VI)
/// </summary>
public interface IInvestmentService
{
    /// <summary>Create a new investment record (EN)<br/>Tạo bản ghi đầu tư mới (VI)</summary>
    Task<InvestmentViewModel> CreateInvestmentAsync(CreateInvestmentRequest request);
    
    /// <summary>Get investment by ID (EN)<br/>Lấy đầu tư theo ID (VI)</summary>
    Task<InvestmentViewModel?> GetInvestmentByIdAsync(Guid id);
    
    /// <summary>Get all investments for a user (EN)<br/>Lấy tất cả đầu tư của người dùng (VI)</summary>
    Task<List<InvestmentViewModel>> GetUserInvestmentsAsync(string userId);
    
    /// <summary>Update investment information (EN)<br/>Cập nhật thông tin đầu tư (VI)</summary>
    Task<InvestmentViewModel> UpdateInvestmentAsync(Guid id, UpdateInvestmentRequest request);
    
    /// <summary>Update market price for an investment (EN)<br/>Cập nhật giá thị trường cho đầu tư (VI)</summary>
    Task<InvestmentViewModel> UpdateMarketPriceAsync(Guid id, UpdateMarketPriceRequest request);
    
    /// <summary>Delete an investment record (EN)<br/>Xóa bản ghi đầu tư (VI)</summary>
    Task DeleteInvestmentAsync(Guid id);
    
    /// <summary>Get portfolio summary for a user (EN)<br/>Lấy tóm tắt danh mục cho người dùng (VI)</summary>
    Task<PortfolioSummaryViewModel> GetPortfolioSummaryAsync(string userId);
}
```

### 4.3 API Endpoints Design

#### REST API Specification
```
Base URL: /api/investments

GET    /api/investments                    # Get user's all investments
GET    /api/investments/{id}              # Get specific investment
POST   /api/investments                   # Create new investment
PUT    /api/investments/{id}              # Update investment
PATCH  /api/investments/{id}/market-price # Update market price only
DELETE /api/investments/{id}              # Delete investment
GET    /api/investments/portfolio/summary # Get portfolio summary
```

### 4.4 Data Transfer Objects

#### Request DTOs
```csharp
public class CreateInvestmentRequest
{
    public string Symbol { get; set; } = string.Empty;
    public InvestmentType InvestmentType { get; set; }
    public decimal PurchasePrice { get; set; }
    public decimal Quantity { get; set; }
    public DateTime PurchaseDate { get; set; }
    public decimal? CurrentMarketPrice { get; set; }
    public string? Notes { get; set; }
}

public class UpdateInvestmentRequest
{
    public string Symbol { get; set; } = string.Empty;
    public InvestmentType InvestmentType { get; set; }
    public decimal PurchasePrice { get; set; }
    public decimal Quantity { get; set; }
    public DateTime PurchaseDate { get; set; }
    public decimal? CurrentMarketPrice { get; set; }
    public string? Notes { get; set; }
}

public class UpdateMarketPriceRequest
{
    public decimal CurrentMarketPrice { get; set; }
}
```

#### Response DTOs
```csharp
public class InvestmentViewModel
{
    public Guid Id { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public InvestmentType InvestmentType { get; set; }
    public string InvestmentTypeName { get; set; } = string.Empty;
    public decimal PurchasePrice { get; set; }
    public decimal Quantity { get; set; }
    public decimal? CurrentMarketPrice { get; set; }
    public DateTime PurchaseDate { get; set; }
    public string? Notes { get; set; }
    
    // Calculated fields
    public decimal TotalInvestedAmount { get; set; }
    public decimal? CurrentTotalValue { get; set; }
    public decimal? ProfitLoss { get; set; }
    public decimal? ProfitLossPercentage { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class PortfolioSummaryViewModel
{
    public decimal TotalInvestedAmount { get; set; }
    public decimal? TotalCurrentValue { get; set; }
    public decimal? TotalProfitLoss { get; set; }
    public decimal? TotalProfitLossPercentage { get; set; }
    public int TotalInvestments { get; set; }
    public List<InvestmentTypeSummary> ByInvestmentType { get; set; } = new();
}

public class InvestmentTypeSummary
{
    public InvestmentType InvestmentType { get; set; }
    public string InvestmentTypeName { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal TotalInvestedAmount { get; set; }
    public decimal? TotalCurrentValue { get; set; }
    public decimal? ProfitLoss { get; set; }
}
```

## 5. Database Design

### 5.1 Investment Table Schema
```sql
CREATE TABLE investments (
    id                   UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id             VARCHAR(100) NOT NULL,
    symbol              VARCHAR(20) NOT NULL,
    investment_type     INTEGER NOT NULL,
    purchase_price      DECIMAL(18,4) NOT NULL,
    quantity            DECIMAL(18,6) NOT NULL,
    current_market_price DECIMAL(18,4) NULL,
    purchase_date       DATE NOT NULL,
    notes               TEXT NULL,
    created_at          TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at          TIMESTAMP WITH TIME ZONE NULL,
    created_by          VARCHAR(100) NULL,
    updated_by          VARCHAR(100) NULL,
    
    CONSTRAINT ck_investments_purchase_price_positive CHECK (purchase_price > 0),
    CONSTRAINT ck_investments_quantity_positive CHECK (quantity > 0),
    CONSTRAINT ck_investments_current_price_positive CHECK (current_market_price IS NULL OR current_market_price > 0)
);

CREATE INDEX ix_investments_user_id ON investments (user_id);
CREATE INDEX ix_investments_symbol ON investments (symbol);
CREATE INDEX ix_investments_investment_type ON investments (investment_type);
CREATE INDEX ix_investments_purchase_date ON investments (purchase_date);
```

## 6. Validation Rules

### 6.1 Business Validation
- **Symbol**: Required, max 20 characters, alphanumeric + dash/dot allowed
- **PurchasePrice**: Required, must be > 0, max 4 decimal places
- **Quantity**: Required, must be > 0, max 6 decimal places
- **CurrentMarketPrice**: Optional, must be > 0 if provided, max 4 decimal places
- **PurchaseDate**: Required, cannot be future date, cannot be before 1900-01-01
- **InvestmentType**: Required, must be valid enum value
- **UserId**: Required, must match authenticated user
- **Notes**: Optional, max 1000 characters

### 6.2 FluentValidation Implementation
```csharp
public class CreateInvestmentRequestValidator : AbstractValidator<CreateInvestmentRequest>
{
    public CreateInvestmentRequestValidator()
    {
        RuleFor(x => x.Symbol)
            .NotEmpty().WithMessage("Symbol is required")
            .MaximumLength(20).WithMessage("Symbol must not exceed 20 characters")
            .Matches(@"^[A-Za-z0-9\-\.]+$").WithMessage("Symbol can only contain alphanumeric characters, dashes, and dots");
            
        RuleFor(x => x.PurchasePrice)
            .GreaterThan(0).WithMessage("Purchase price must be greater than 0")
            .ScalePrecision(4, 18).WithMessage("Purchase price cannot have more than 4 decimal places");
            
        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0")
            .ScalePrecision(6, 18).WithMessage("Quantity cannot have more than 6 decimal places");
            
        RuleFor(x => x.CurrentMarketPrice)
            .GreaterThan(0).WithMessage("Market price must be greater than 0")
            .ScalePrecision(4, 18).WithMessage("Market price cannot have more than 4 decimal places")
            .When(x => x.CurrentMarketPrice.HasValue);
            
        RuleFor(x => x.PurchaseDate)
            .LessThanOrEqualTo(DateTime.Today).WithMessage("Purchase date cannot be in the future")
            .GreaterThan(new DateTime(1900, 1, 1)).WithMessage("Purchase date cannot be before 1900");
            
        RuleFor(x => x.InvestmentType)
            .IsInEnum().WithMessage("Invalid investment type");
            
        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes cannot exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}
```

## 7. Implementation Plan

### 7.1 Phase 1: Domain Layer (1 hour)
- [ ] Create `Investment` entity trong `PlanningInvestment.Domain/Entities/`
- [ ] Create `InvestmentType` enum trong `PlanningInvestment.Domain/Enums/`
- [ ] Update domain model relationships nếu cần

### 7.2 Phase 2: Infrastructure Layer (1 hour)
- [ ] Add `Investment` DbSet trong `PlanningInvestmentDbContext`
- [ ] Configure entity mapping với proper constraints
- [ ] Create EF Core migration cho Investment table
- [ ] Test database operations

### 7.3 Phase 3: Application Layer (1 hour)
- [ ] Create `IInvestmentService` interface
- [ ] Implement `InvestmentService` với all business logic
- [ ] Create DTOs cho requests và responses
- [ ] Add FluentValidation validators
- [ ] Configure AutoMapper profiles

### 7.4 Phase 4: API Layer (1 hour)
- [ ] Create `InvestmentController` với REST endpoints
- [ ] Add proper error handling và validation
- [ ] Configure Swagger documentation
- [ ] Test API endpoints với Postman/Swagger

### 7.5 Phase 5: Testing & Documentation
- [ ] Write unit tests cho InvestmentService
- [ ] Write integration tests cho API endpoints
- [ ] Update API documentation
- [ ] Create sample data cho testing

## 8. Risk Assessment & Mitigation

### 8.1 Technical Risks
| Risk | Impact | Probability | Mitigation |
|------|--------|-------------|------------|
| Database migration conflicts | Medium | Low | Use separate migration, test thoroughly |
| Performance impact with large portfolios | Medium | Medium | Add proper indexing, pagination for future |
| Decimal precision errors | High | Low | Use proper decimal types, validation |
| Integration with existing auth | Low | Low | Follow existing patterns |

### 8.2 Business Risks
| Risk | Impact | Probability | Mitigation |
|------|--------|-------------|------------|
| User data loss | High | Low | Proper validation, backup strategy |
| Incorrect profit/loss calculation | Medium | Low | Comprehensive testing, clear formula documentation |
| Security vulnerabilities | High | Low | Follow existing security patterns, input validation |

## 9. Future Enhancement Opportunities

### 9.1 Immediate Next Stories (After INV-001)
- **INV-002**: Real-time market data integration (VietStock API, Yahoo Finance)
- **INV-003**: Portfolio performance charts và analytics
- **INV-004**: Investment alerts và notifications
- **INV-005**: Tax calculation support cho capital gains

### 9.2 Advanced Features (Later)
- **INV-006**: Portfolio diversification analysis
- **INV-007**: Investment recommendations based on goals
- **INV-008**: Automated rebalancing suggestions
- **INV-009**: Social trading features
- **INV-010**: Integration với external brokerage accounts

## 10. Success Metrics & KPIs

### 10.1 Technical Metrics
- [ ] API response time < 200ms for CRUD operations
- [ ] Database queries optimized với proper indexing
- [ ] Unit test coverage > 90%
- [ ] Zero critical security vulnerabilities
- [ ] API documentation completeness = 100%

### 10.2 User Experience Metrics
- [ ] Time to create investment record < 60 seconds
- [ ] Profit/loss calculation accuracy = 100%
- [ ] Portfolio loading time < 2 seconds
- [ ] Error rate < 1% for valid operations
- [ ] User satisfaction với basic investment tracking

### 10.3 Business Metrics
- [ ] Foundation ready cho advanced investment features
- [ ] Integration points defined cho future enhancements
- [ ] Scalable architecture để support large portfolios
- [ ] Clear upgrade path cho real-time data integration

---

**Document Version**: 1.0  
**Created**: August 26, 2025  
**Last Updated**: August 26, 2025  
**Status**: Ready for Implementation  
**Related Story**: [INV-001 - Investment Portfolio Tracking](/docs/stories/investment-portfolio-tracking.md)
