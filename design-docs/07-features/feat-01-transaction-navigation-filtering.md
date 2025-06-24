# User Story: Transaction Navigation & Context-Aware Filtering

## 📋 Epic
**Epic #E001: Cải thiện Navigation và Filtering cho Transaction Management Screen**

## 🎯 User Story Overview

**Là một người dùng**, tôi muốn có thể:
1. **Navigate từ Account page sang Transaction page** và tự động xem các giao dịch của account đó
2. **Navigate trực tiếp từ menu** và xem tất cả giao dịch của tôi
3. **Có trải nghiệm navigation nhất quán** với breadcrumb và title phù hợp

## 🏷️ Story Details

### Story #TNF-001: Navigation từ Account Page với Auto-Selection
**Acceptance Criteria:**

**Given** tôi đang ở trang Account Management
**When** tôi click vào tên một account (ví dụ: "Techcombank")
**Then** 
- Tôi được chuyển đến Transaction page với URL: `/transactions?accountId=123&accountName=Techcombank`
- Account dropdown tự động chọn account "Techcombank"
- Danh sách transaction chỉ hiển thị giao dịch của account "Techcombank"
- Time range mặc định là 30 ngày gần nhất
- Breadcrumb hiển thị: `Dashboard > Accounts > Techcombank > Transactions`
- Page title hiển thị: "Giao dịch - Techcombank"

### Story #TNF-002: Direct Navigation từ Menu
**Acceptance Criteria:**

**Given** tôi đang ở bất kỳ page nào trong app
**When** tôi click vào menu "Transactions" hoặc navigate trực tiếp đến `/transactions`
**Then**
- Tôi được chuyển đến Transaction page với URL: `/transactions` (không có params)
- Account dropdown hiển thị "Tất cả tài khoản"
- Danh sách transaction hiển thị tất cả giao dịch của tôi
- Time range mặc định là 30 ngày gần nhất
- Breadcrumb hiển thị: `Dashboard > Transactions`
- Page title hiển thị: "Giao dịch"

### Story #TNF-003: Dynamic Account Filtering với URL Sync
**Acceptance Criteria:**

**Given** tôi đang ở Transaction page
**When** tôi thay đổi selection trong account dropdown
**Then**
- Danh sách transaction được reload với filter mới
- URL được update để reflect selection hiện tại:
  - Nếu chọn account cụ thể: `/transactions?accountId=123&accountName=Techcombank`
  - Nếu chọn "Tất cả tài khoản": `/transactions` (xóa params)
- Page title được update tương ứng
- Breadcrumb được update nếu cần

### Story #TNF-004: Transaction Detail Pane với Account Context
**Acceptance Criteria:**

**Given** tôi đang ở Transaction page với account được pre-selected
**When** tôi click vào nút "Add Transaction" (Thu hoặc Chi)
**Then**
- Detail pane mở bên phải với form thêm transaction
- Account dropdown trong form tự động chọn account hiện tại
- TransactionDirection được pre-select theo nút đã nhấn
- Sau khi thêm thành công, dropdown account trong danh sách không thay đổi

**Given** tôi đang ở Transaction page 
**When** tôi click vào một transaction trong danh sách
**Then**
- Detail pane mở bên phải với thông tin chi tiết transaction
- Transaction được highlight trong danh sách
- Desktop: Layout chia đôi 50/50
- Mobile: Detail pane fullscreen, ẩn danh sách

## 🛠️ Technical Implementation Requirements

### Frontend Changes Needed:

#### Task #TNF-F001: Route Parameter Handling
```typescript
// File: pages/transactions.vue
interface RouteQuery {
  accountId?: string
  accountName?: string
}

// Component cần handle 2 cases:
// Case 1: /transactions?accountId=123&accountName=Techcombank
// Case 2: /transactions (no params)
```

#### Task #TNF-F002: Store State Management 
```typescript
// File: stores/transactionFilter.ts
interface TransactionFilterState {
  selectedAccountId: string | null
  selectedAccountName: string
  dateFrom: Date
  dateTo: Date
  transactionType: 'All' | 'Revenue' | 'Spent'
}

// Actions needed:
// - setAccountFilter(accountId, accountName)
// - setDateRange(from, to)
// - resetToDefault()
// - loadFromUrlParams(query)
```

#### Task #TNF-F003: Component Updates

**Subtask #TNF-F003.1: AccountDropdown Component:**
- Thêm option "Tất cả tài khoản" làm item đầu tiên
- Support pre-selection từ props
- Emit change events để parent component handle

**Subtask #TNF-F003.2: TransactionList Component:**
- Handle URL params trong onMounted
- Implement loadTransactions với dynamic filters
- Support account pre-selection

**Subtask #TNF-F003.3: NavigationBreadcrumb Component:**
- Dynamic breadcrumb generation dựa trên context
- Support back navigation về Account page với highlighting

#### Task #TNF-F004: Account Management Integration
```typescript
// File: pages/accounts.vue
// Thêm click handler cho account name:
function navigateToTransactions(account) {
  router.push({
    path: '/transactions',
    query: {
      accountId: account.id,
      accountName: account.name
    }
  })
}
```

### Backend Changes Needed:

#### Task #TNF-B001: API Enhancement
```csharp
// File: TransactionController.cs
[HttpGet]
public async Task<ActionResult<PagedResult<TransactionDto>>> GetTransactions(
    [FromQuery] GetTransactionsQuery query)
{
    // Support filtering by accountId (nullable)
    // Support date range filtering
    // Return proper pagination
}

public class GetTransactionsQuery
{
    public Guid? AccountId { get; set; }  // Nullable for "all accounts"
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public TransactionDirection? Direction { get; set; }
    public int Page { get; set; } = 1
    public int PageSize { get; set; } = 50
}
```

#### Task #TNF-B002: Query Optimization
- Tối ưu query khi filter theo accountId
- Index optimization cho (AccountId, TransactionDate)
- Proper pagination để handle large datasets

### Database Changes:

**Task #TNF-DB001: Index Optimization**

**Không cần thay đổi schema**, chỉ cần tối ưu indexes:
```sql
-- Optimize queries for account-based filtering
CREATE INDEX IF NOT EXISTS idx_transactions_account_date 
ON transactions (account_id, transaction_date DESC);

-- Support general date-based queries
CREATE INDEX IF NOT EXISTS idx_transactions_date 
ON transactions (transaction_date DESC);
```

## 🧪 Testing Requirements

### Task #TNF-T001: Unit Tests:
- [ ] TransactionFilter store actions và mutations
- [ ] Route parameter parsing logic
- [ ] Account dropdown component với pre-selection
- [ ] Breadcrumb generation logic

### Task #TNF-T002: Integration Tests:
- [ ] Navigation flow từ Account page → Transaction page
- [ ] URL parameter handling và sync
- [ ] Filter state persistence across navigation
- [ ] API integration với dynamic filters

### Task #TNF-T003: E2E Tests:
- [ ] Complete user journey: Account → Transactions → Back
- [ ] Account filtering và URL updates
- [ ] Mobile responsive behavior
- [ ] Detail pane interactions với context

### Task #TNF-T004: Manual Testing Scenarios:
1. **Happy Path**: Navigate từ account "Techcombank" → Transaction page → Verify auto-selection
2. **Direct Access**: Access `/transactions` directly → Verify "All accounts" behavior  
3. **Account Switching**: Switch account trong dropdown → Verify URL update và data reload
4. **Mobile Experience**: Test responsive behavior và detail pane fullscreen
5. **Edge Cases**: Invalid accountId trong URL, empty transaction list, etc.

## 📊 Definition of Done

### Functional Requirements:
- [ ] Navigation từ Account page hoạt động với URL parameters
- [ ] Direct navigation từ menu hoạt động với default state
- [ ] Account dropdown filtering hoạt động với URL sync
- [ ] Breadcrumb và page title update đúng theo context
- [ ] Detail pane integration với account context
- [ ] Responsive behavior cho mobile/desktop

### Technical Requirements:
- [ ] Code tuân theo design patterns của project
- [ ] API endpoints hỗ trợ filtering parameters
- [ ] Frontend state management với Pinia stores
- [ ] Proper error handling cho edge cases
- [ ] Performance optimization cho large datasets

### Quality Requirements:
- [ ] Unit tests coverage >= 80%
- [ ] Integration tests cho main flows
- [ ] E2E tests cho critical paths
- [ ] Code review approved
- [ ] Documentation updated (component docs, API docs)
- [ ] No breaking changes cho existing functionality

## 🔗 Dependencies & Constraints

### Dependencies:
- Account Management page đã implement navigation links
- Transaction API đã support accountId filtering
- Design system components (dropdown, breadcrumb) sẵn sàng

### Technical Constraints:
- Maintain backward compatibility với existing URLs
- Support cả mobile và desktop layouts
- Performance requirement: Load transactions < 2 seconds
- URL params phải readable và bookmarkable

### Business Constraints:
- User chỉ xem được transactions của accounts họ own
- Date range mặc định không quá 30 ngày để tránh performance issues
- Account dropdown chỉ hiển thị active accounts

## 📅 Estimated Timeline

**Epic #E001 Total Estimation: 5-7 days (1 developer)**

### Sprint Breakdown:
- **Day 1-2**: Task #TNF-F001, #TNF-F002 (Frontend route handling và store setup)
- **Day 3**: Task #TNF-F003, #TNF-F004 (Account dropdown enhancement và URL sync)  
- **Day 4**: Task #TNF-B001, #TNF-B002, #TNF-DB001 (Backend API filtering enhancement)
- **Day 5**: Task #TNF-T002 (Integration testing và bug fixes)
- **Day 6-7**: Task #TNF-T003, #TNF-T004 (E2E testing, documentation, và deployment)

### Risk Mitigation:
- **Risk**: Complex state management giữa components
  - **Mitigation**: Use Pinia stores để centralize state
- **Risk**: Performance issues với large transaction datasets  
  - **Mitigation**: Implement proper pagination và indexing
- **Risk**: Mobile responsive issues với detail pane
  - **Mitigation**: Early mobile testing và progressive enhancement

---

## 📝 Ticket Breakdown Summary

### Epic: E001 - Transaction Navigation & Filtering Enhancement

#### User Stories:
- **TNF-001**: Navigation từ Account Page với Auto-Selection  
- **TNF-002**: Direct Navigation từ Menu
- **TNF-003**: Dynamic Account Filtering với URL Sync
- **TNF-004**: Transaction Detail Pane với Account Context

#### Frontend Tasks:
- **TNF-F001**: Route Parameter Handling (pages/transactions.vue)
- **TNF-F002**: Store State Management (stores/transactionFilter.ts)
- **TNF-F003**: Component Updates
  - **TNF-F003.1**: AccountDropdown Component
  - **TNF-F003.2**: TransactionList Component  
  - **TNF-F003.3**: NavigationBreadcrumb Component
- **TNF-F004**: Account Management Integration (pages/accounts.vue)

#### Backend Tasks:
- **TNF-B001**: API Enhancement (TransactionController.cs)
- **TNF-B002**: Query Optimization (Repository layer)

#### Database Tasks:
- **TNF-DB001**: Index Optimization (SQL scripts)

#### Testing Tasks:
- **TNF-T001**: Unit Tests (Components, Store, Logic)
- **TNF-T002**: Integration Tests (API flows, Navigation)
- **TNF-T003**: E2E Tests (User journeys)
- **TNF-T004**: Manual Testing Scenarios (Edge cases)

### Dependencies:
- TNF-F001 → TNF-F002 → TNF-F003 → TNF-F004
- TNF-B001 → TNF-B002
- TNF-F003 requires TNF-B001 completion
- All Testing tasks require implementation completion

### Critical Path: 
TNF-F001 → TNF-F002 → TNF-B001 → TNF-F003 → TNF-T002 → TNF-T003
