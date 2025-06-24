## 🎯 Mục tiêu

Thiết kế UI/UX và logic nhập liệu cho màn hình "Thêm giao dịch" và trang danh sách giao dịch trong ứng dụng quản lý chi tiêu cá nhân.

---

## 🧩 1. Giao diện mặc định (Simple Mode)

### ✅ Trường hiển thị mặc định:

| Trường                 | Loại nhập                    | Gợi ý UX                         |
| ---------------------- | ---------------------------- | -------------------------------- |
| `TransactionDirection` | Dropdown (Thu / Chi)         | Dropdown bắt buộc chọn trước     |
| `Amount`               | Số tiền                      | Hiển thị dựa theo `Direction`    |
| `TransactionDate`      | DateTime picker              | Default là hôm nay + thời gian hiện tại |
| `AccountId`            | Dropdown danh sách tài khoản | Kéo từ danh sách account đã có   |
| `CategoryType`         | Dropdown                     | Icon minh họa các loại giao dịch |
| `Description`          | Text input                   | Có autosuggest từ lịch sử        |
| `Balance`              | Số dư sau giao dịch          | Tính tự động hoặc nhập tay       |
| Nút \[Mở rộng]         | Toggle UI                    | Hiện thêm các trường nâng cao    |

---

## 🚀 2. Giao diện mở rộng (Advanced Mode)

### 📌 Nhóm "Thông tin tài chính mở rộng":

* `BalanceCompare`
* `AvailableLimit`, `AvailableLimitCompare`
* `IncreaseCreditLimit`, `UsedPercent`

### 🗂 Nhóm "Phân loại & ghi chú":

* `CategorySummary`
* `Note`
* `Group`
* `ImportFrom`

### 🛠 Nhóm "Đồng bộ & metadata":

* `TransactionCode`
* `SyncMisa`, `SyncSms`
* `Vn`

---

## 🔒 3. Ràng buộc logic

* Gộp `RevenueAmount` và `SpentAmount` thành một trường duy nhất `Amount`
* Bổ sung enum `TransactionDirection` với giá trị: `Revenue`, `Spent`
* Bắt buộc chọn `TransactionDirection` trước khi nhập `Amount`
* Khi chọn `Direction = Revenue` thì `Amount` hiểu là thu vào, ngược lại là chi ra

```csharp
public enum TransactionDirection
{
    Revenue = 1,
    Spent = 2
}
```

---

## ⏰ 4. Xử lý TransactionDate với thời gian

### Frontend (FE):
* **DateTime picker** thay vì Date picker đơn thuần
* **Default value**: Ngày hiện tại + thời gian hiện tại
* **Format hiển thị**: `dd/MM/yyyy HH:mm` (ví dụ: 15/12/2024 14:30)
* **UX**: 
  * Có thể chọn nhanh "Hôm nay", "Hôm qua", "Tuần này"
  * Có thể nhập thời gian chính xác hoặc chọn từ dropdown (00:00, 06:00, 12:00, 18:00, 23:59)
* **Validation**: Không được chọn thời gian trong tương lai

### Backend (BE):
* Lưu trữ `TransactionDate` dưới dạng `DateTime` (không phải `DateOnly`)
* Sắp xếp giao dịch theo `TransactionDate` chính xác đến phút
* API filter hỗ trợ range theo datetime

---

## 💰 5. Logic xử lý Balance tự động

### Frontend (FE):
* **Tính toán Balance tạm thời** khi người dùng nhập giao dịch:
  1. Lấy danh sách giao dịch hiện tại của cùng tài khoản
  2. Sắp xếp theo `TransactionDate` tăng dần
  3. Tìm giao dịch gần nhất có `TransactionDate` < giao dịch đang nhập
  4. Nếu tìm thấy và có `Balance` → tính Balance mới = Balance cũ ± Amount
  5. Hiển thị Balance tạm thời với màu khác (ví dụ: màu xám nhạt)
* **UX Balance field**:
  * Hiển thị giá trị tính toán tự động
  * Cho phép override bằng cách nhập tay
  * Icon "🔄" để reset về giá trị tự động
  * Tooltip giải thích cách tính

### Backend (BE):
* **Khi nhận transaction mới**:
  1. Tìm giao dịch gần nhất cùng `AccountId` có `TransactionDate` < transaction hiện tại
  2. Sắp xếp theo `TransactionDate` giảm dần, lấy record đầu tiên
  3. **Nếu có Balance trước đó**:
     * `Direction = Revenue` → `NewBalance = PreviousBalance + Amount`
     * `Direction = Spent` → `NewBalance = PreviousBalance - Amount`
  4. **Nếu không có Balance trước đó**:
     * Không tự động tính Balance
     * Yêu cầu nhập tay hoặc để trống
     * Log warning để admin biết cần setup Balance đầu tiên

### Logic cập nhật Balance cascade:
* **Khi cập nhật/xóa transaction**:
  1. Tìm tất cả giao dịch sau thời điểm này (cùng account)
  2. Tính lại Balance cho từng giao dịch theo thứ tự thời gian
  3. Cập nhật batch để đảm bảo tính nhất quán

### API endpoints mới:
```csharp
// Tính Balance tạm thời cho FE
[HttpPost("calculate-balance")]
public async Task<ActionResult<decimal?>> CalculateBalance(
    Guid accountId, 
    DateTime transactionDate, 
    decimal amount, 
    TransactionDirection direction)

// Lấy Balance gần nhất của account
[HttpGet("latest-balance/{accountId}")]
public async Task<ActionResult<decimal?>> GetLatestBalance(Guid accountId)
```

---

## 📋 6. Tính năng UX nâng cao đề xuất

* **Gợi ý thông minh**: mô tả, danh mục từ lịch sử giao dịch gần đây
* **Quick-add**: thêm nhanh giao dịch mẫu
* **Auto-calculation**: tính toán gợi ý số dư dựa vào lịch sử
* **Cảnh báo logic**: khi dữ liệu không khớp hoặc thiếu thông tin
* **Balance validation**: cảnh báo khi Balance âm hoặc không hợp lý
* **Time shortcuts**: "5 phút trước", "1 giờ trước", "Sáng nay", "Chiều nay"

---

## 🖥️ 7. Giao diện danh sách giao dịch (Transaction List Page)

### Layout chia đôi màn hình:

* **Khi KHÔNG có transaction detail mở**: Danh sách giao dịch chiếm **toàn bộ màn hình**
* **Khi CÓ transaction detail mở**: 
  * **Desktop**: Màn hình chia đôi:
    * **Bên trái (50%)**: Danh sách giao dịch 
    * **Bên phải (50%)**: Chi tiết giao dịch
  * **Mobile**: Detail transaction chiếm **toàn bộ màn hình**, ẩn danh sách

### Mặc định hiển thị:

#### 🔀 Navigation Context - 2 trường hợp:

**Trường hợp 1: Navigate từ Account page (có URL params)**
* URL: `/transactions?accountId=123&accountName=Techcombank`
* **Auto-select account** trong dropdown filter
* **Load transactions** của account đó trong **30 ngày gần nhất**
* **Breadcrumb**: `Accounts > [Account Name] > Transactions`
* **Title**: "Giao dịch - [Account Name]"

**Trường hợp 2: Navigate từ Menu (direct access)**
* URL: `/transactions` (không có params)
* **Dropdown account** để ở **"Tất cả tài khoản"**
* **Load toàn bộ transactions** của user trong **30 ngày gần nhất**
* **Breadcrumb**: `Dashboard > Transactions`
* **Title**: "Giao dịch"

#### 📊 Default Behavior:
* **Time range**: 30 ngày gần nhất (mặc định cho cả 2 trường hợp)
* **Account filter**:
  * **Có accountId**: Auto-select account cụ thể
  * **Không có accountId**: "Tất cả tài khoản" (All Accounts)
* **Sorting**: `TransactionDate` giảm dần (mới nhất ở trên)

#### 🎛 Filter Options:
* **Account dropdown**:
  * Option đầu tiên: "Tất cả tài khoản" (value = null/empty)
  * Danh sách accounts của user (active accounts only)
  * **Pre-selected** account nếu có accountId trong URL
* **Transaction type**: `All` / `Revenue` / `Spent`
* **Date range**: Custom date picker (default 30 ngày gần nhất)

### Cột hiển thị theo chế độ:

#### 🔹 Chế độ đơn giản (mặc định):
Chỉ hiển thị **4 cột chính**:
1. **Ngày giờ giao dịch** (`TransactionDate`) - Format: `dd/MM HH:mm`
2. **Mô tả** (`Description`) 
3. **Số tiền** (Amount với format màu)
4. **Số dư** (`Balance`) - hiển thị nếu có

#### 🔹 Chế độ nâng cao:
Hiển thị **tất cả các cột** bao gồm:
* Ngày giờ, Mô tả, Số tiền, Số dư (4 cột chính)
* Account, CategoryType, Note, v.v.

#### 🔹 Nút tùy chọn cột:
* **Nút "Columns"** ở góc phải bảng
* Click vào sẽ hiện dropdown checklist các cột có thể hiển thị/ẩn
* Người dùng có thể tự chọn cột nào muốn xem thêm
* Lưu preferences của user

### Nút thao tác:

* `+ Giao dịch Thu` → mở giao diện thêm với `TransactionDirection = Revenue`
* `+ Giao dịch Chi` → mở giao diện thêm với `TransactionDirection = Spent`
* Khi nhấn các nút này:
  * Giao diện detail thêm transaction được mở ở **bên phải màn hình**
  * `Direction` được chọn sẵn nhưng vẫn có thể thay đổi
  * **Account selection behavior**:
    * **Nếu có account filter**: Pre-select account đó
    * **Nếu "Tất cả tài khoản"**: Dropdown để trống, user phải chọn
  * `TransactionDate` mặc định là thời gian hiện tại

### Account Dropdown UI/UX:

#### Visual Design:
```vue
<!-- Account Dropdown Component -->
<template>
  <div class="account-filter-container">
    <label class="filter-label">Tài khoản</label>
    <select 
      v-model="selectedAccountId" 
      @change="onAccountChange"
      class="account-dropdown"
      :class="{
        'all-accounts': selectedAccountId === null,
        'specific-account': selectedAccountId !== null
      }"
    >
      <option value="" class="all-accounts-option">
        🏦 Tất cả tài khoản
      </option>
      <option 
        v-for="account in userAccounts" 
        :key="account.id"
        :value="account.id"
        class="account-option"
      >
        {{ getAccountIcon(account.type) }} {{ account.name }}
        <span class="account-balance">
          ({{ formatCurrency(account.currentBalance) }})
        </span>
      </option>
    </select>
    
    <!-- Transaction count indicator -->
    <div class="transaction-count-indicator">
      {{ transactionCount }} giao dịch trong 30 ngày gần nhất
    </div>
  </div>
</template>

<style scoped>
.account-dropdown.all-accounts {
  @apply border-blue-300 bg-blue-50 text-blue-700 font-medium;
}

.account-dropdown.specific-account {
  @apply border-green-300 bg-green-50 text-green-700 font-medium;
}

.transaction-count-indicator {
  @apply text-sm text-gray-500 mt-1;
}

.account-balance {
  @apply text-gray-400 font-normal;
}
</style>
```

#### Account Icons Mapping:
```typescript
function getAccountIcon(accountType: AccountType): string {
  switch (accountType) {
    case AccountType.Bank: return '🏦'
    case AccountType.Wallet: return '👛'
    case AccountType.CreditCard: return '💳'
    case AccountType.DebitCard: return '💳'
    case AccountType.Cash: return '💰'
    default: return '📊'
  }
}
```

### Tương tác với transaction trong danh sách:

* **Click vào bất kỳ transaction nào** trong danh sách sẽ:
  * Mở **detail pane** ở bên phải với thông tin chi tiết transaction đó
  * **Chế độ xem**: Hiển thị đầy đủ thông tin transaction
  * **Các nút action**: `Update`, `Delete`, `Duplicate`
  * **Highlight** transaction được chọn trong danh sách

### Giao diện chi tiết giao dịch (Detail Pane):

* **Desktop**: 
  * Hiện ở **bên phải màn hình (50%)**
  * Danh sách bên trái thu hẹp còn **50%**
  * Có thể resize được ranh giới giữa 2 pane
* **Mobile**: 
  * Chiếm **toàn bộ màn hình**
  * Ẩn hoàn toàn danh sách transaction
* **Đóng detail pane**: 
  * **Nút X** ở góc phải detail pane
  * **Phím ESC** để đóng nhanh
  * Click ra ngoài vùng detail (chỉ trên desktop)

### Liên kết dropdown:

* Khi đổi `dropdown account` ở danh sách → dropdown ở phần thêm sẽ cập nhật theo
* Ngược lại, đổi dropdown ở phần thêm **không ảnh hưởng** đến danh sách

### Logic nhập và reset:

* **Thêm mới**: reset `Amount` và `Description` sau khi thêm thành công, giữ `TransactionDate` hiện tại
* **Cập nhật**: giữ nguyên dữ liệu vừa cập nhật
* **Xóa**: đóng detail pane và refresh danh sách

### Trạng thái loading:

* CRUD transaction → hiển thị loading overlay **chỉ trong detail pane**, không chặn thao tác ở danh sách
* Load danh sách → hiển thị skeleton loading trong bảng
* **Balance calculation** → hiển thị spinner nhỏ bên cạnh Balance field

### Cột Số tiền:

* Hiển thị thêm cột `Số tiền` ở danh sách:
  * Nếu là giao dịch `Revenue`: hiển thị `+RevenueAmount` (màu xanh)
  * Nếu là giao dịch `Spent`: hiển thị `-SpentAmount` (màu đỏ)
* Vẫn giữ dữ liệu phía sau gồm 2 trường riêng biệt `RevenueAmount` và `SpentAmount`
* Format: `+1,000,000 ₫` hoặc `-250,000 ₫`

### Responsive behavior:

* **Desktop (≥1024px)**: Layout chia đôi như mô tả
* **Tablet (768px-1023px)**: Detail pane chiếm 60% màn hình
* **Mobile (<768px)**: Detail fullscreen, ẩn danh sách

---

## ⌨️ 8. Keyboard shortcuts

* **ESC**: Đóng detail pane
* **Ctrl/Cmd + N**: Tạo giao dịch mới 
* **Enter**: Submit form trong detail pane
* **Tab**: Di chuyển giữa các trường nhập liệu
* **↑/↓**: Di chuyển giữa các transaction trong danh sách
* **Ctrl/Cmd + T**: Focus vào TransactionDate picker
* **Ctrl/Cmd + B**: Focus vào Balance field

---

## 🔧 9. Technical Implementation Notes

### Database Changes:
* `TransactionDate` column: `TIMESTAMP` instead of `DATE`
* Index on `(AccountId, TransactionDate)` for balance calculation performance
* Consider adding `CalculatedBalance` flag to distinguish auto vs manual balance

### Performance Considerations:
* Cache latest balance per account for quick calculation
* Batch balance recalculation when needed
* Use database triggers for balance consistency (optional)

### Error Handling:
* Handle timezone conversion properly
* Validate balance calculation logic
* Graceful fallback when balance calculation fails

### Navigation Integration:
* Support URL parameters for account pre-selection: `/transactions?accountId=123&accountName=Techcombank`
* Auto-select account trong dropdown khi có URL parameters
* Load transactions filtered by selected account ngay lập tức
* Display breadcrumb navigation khi đến từ Account page
* Maintain navigation context để quay lại Account page với highlighting

### Navigation Context Handling:
```typescript
// Component initialization logic
onMounted(async () => {
  const { accountId, accountName } = route.query
  
  if (accountId && accountName) {
    // Case 1: From Account page - có URL parameters
    await handleAccountNavigation(accountId, accountName)
  } else {
    // Case 2: From Menu - direct access
    await handleDirectNavigation()
  }
})

// Handle navigation from Account page
async function handleAccountNavigation(accountId: string, accountName: string) {
  // Set filter state
  filterStore.setAccountFilter(accountId, accountName)
  
  // Setup breadcrumb
  breadcrumbStore.setBreadcrumb([
    { name: 'Dashboard', path: '/' },
    { name: 'Accounts', path: '/accounts' },
    { name: accountName, path: `/accounts?highlight=${accountId}` },
    { name: 'Transactions', path: '' }
  ])
  
  // Set page title
  document.title = `Giao dịch - ${accountName}`
  
  // Load transactions for specific account (30 days)
  await loadTransactions({
    accountId: accountId,
    dateFrom: subDays(new Date(), 30),
    dateTo: new Date()
  })
}

// Handle direct navigation from menu
async function handleDirectNavigation() {
  // Set default filter state
  filterStore.setAccountFilter(null, 'Tất cả tài khoản')
  
  // Setup breadcrumb
  breadcrumbStore.setBreadcrumb([
    { name: 'Dashboard', path: '/' },
    { name: 'Transactions', path: '' }
  ])
  
  // Set page title
  document.title = 'Giao dịch'
  
  // Load all transactions (30 days)
  await loadTransactions({
    accountId: null, // All accounts
    dateFrom: subDays(new Date(), 30),
    dateTo: new Date()
  })
}
```

### Error Handling & Validation:

#### Navigation Validation:
```typescript
// Validate account access when coming from URL params
async function validateAccountAccess(accountId: string): Promise<boolean> {
  try {
    const account = await $fetch(`/api/accounts/${accountId}`)
    
    // Check if account belongs to current user
    if (!account || account.userId !== currentUser.id) {
      console.warn(`Account ${accountId} not found or access denied`)
      return false
    }
    
    // Check if account is active
    if (!account.isActive) {
      console.warn(`Account ${accountId} is inactive`)
      return false
    }
    
    return true
  } catch (error) {
    console.error('Account validation failed:', error)
    return false
  }
}

// Handle invalid account ID in URL
async function handleInvalidAccount(accountId: string) {
  // Show error notification
  notificationStore.addNotification({
    type: 'warning',
    title: 'Tài khoản không tồn tại',
    message: 'Tài khoản được chọn không tồn tại hoặc đã bị xóa. Hiển thị tất cả giao dịch.',
    duration: 5000
  })
  
  // Fallback to show all transactions
  await handleDirectNavigation()
  
  // Clean up URL
  router.replace('/transactions')
}
```

#### Loading States:
```typescript
const loadingState = reactive({
  accounts: false,
  transactions: false,
  validation: false
})

// Show appropriate loading indicators
const isLoading = computed(() => 
  loadingState.accounts || loadingState.transactions || loadingState.validation
)

// Loading skeleton for different contexts
const loadingMessage = computed(() => {
  if (loadingState.validation) return 'Đang kiểm tra quyền truy cập...'
  if (loadingState.accounts) return 'Đang tải danh sách tài khoản...'
  if (loadingState.transactions) return 'Đang tải giao dịch...'
  return 'Đang tải...'
})
```

#### Edge Cases:
```typescript
// Handle edge cases in navigation
async function handleNavigationEdgeCases() {
  // Case 1: User has no accounts
  if (accounts.value.length === 0) {
    showEmptyAccountsState()
    return
  }
  
  // Case 2: Account ID exists but account is deleted/inactive
  if (route.query.accountId) {
    const isValid = await validateAccountAccess(route.query.accountId as string)
    if (!isValid) {
      await handleInvalidAccount(route.query.accountId as string)
      return
    }
  }
  
  // Case 3: No transactions found
  if (transactions.value.length === 0) {
    showEmptyTransactionsState()
  }
}

function showEmptyAccountsState() {
  // Show empty state with "Create Account" CTA
  emptyStateStore.setEmptyState({
    type: 'no-accounts',
    title: 'Chưa có tài khoản nào',
    message: 'Tạo tài khoản đầu tiên để bắt đầu quản lý giao dịch',
    actionLabel: 'Tạo tài khoản',
    actionHandler: () => router.push('/accounts?action=create')
  })
}

function showEmptyTransactionsState() {
  const isFiltered = !!filterStore.selectedAccountId
  
  emptyStateStore.setEmptyState({
    type: 'no-transactions',
    title: isFiltered 
      ? `Không có giao dịch cho ${filterStore.selectedAccountName}`
      : 'Chưa có giao dịch nào',
    message: 'Thêm giao dịch đầu tiên để bắt đầu theo dõi tài chính',
    actionLabel: 'Thêm giao dịch',
    actionHandler: () => openTransactionForm()
  })
}
```

---

## 🔌 10. API Integration Patterns

### Transaction Loading Logic:

#### API Call for Account-Specific Transactions:
```typescript
// From Account page navigation
async function loadAccountTransactions(accountId: string) {
  const dateFrom = subDays(new Date(), 30) // 30 ngày gần nhất
  const dateTo = new Date()
  
  const response = await $fetch('/api/core-finance/transaction', {
    query: {
      accountId: accountId,
      dateFrom: dateFrom.toISOString(),
      dateTo: dateTo.toISOString(),
      sortBy: 'transactionDate',
      sortOrder: 'desc',
      pageSize: 50 // Pagination
    }
  })
  
  return response
}
```

#### API Call for All Transactions:
```typescript
// From Menu direct access
async function loadAllTransactions() {
  const dateFrom = subDays(new Date(), 30) // 30 ngày gần nhất
  const dateTo = new Date()
  
  const response = await $fetch('/api/core-finance/transaction', {
    query: {
      // accountId: null/undefined - load all accounts
      dateFrom: dateFrom.toISOString(),
      dateTo: dateTo.toISOString(),
      sortBy: 'transactionDate',
      sortOrder: 'desc',
      pageSize: 50
    }
  })
  
  return response
}
```

### Backend API Response Structure:

#### Endpoint: GET /api/core-finance/transaction
```csharp
[HttpGet]
public async Task<ActionResult<PaginatedResponse<TransactionDto>>> GetTransactions(
    [FromQuery] TransactionFilterRequest request)
{
    // Default to last 30 days if no date range provided
    if (!request.DateFrom.HasValue)
        request.DateFrom = DateTime.Now.AddDays(-30);
    
    if (!request.DateTo.HasValue)
        request.DateTo = DateTime.Now;
    
    var query = _context.Transactions
        .Where(t => t.UserId == CurrentUserId)
        .Where(t => t.TransactionDate >= request.DateFrom 
                 && t.TransactionDate <= request.DateTo);
    
    // Filter by account if specified
    if (!string.IsNullOrEmpty(request.AccountId))
    {
        query = query.Where(t => t.AccountId == Guid.Parse(request.AccountId));
    }
    
    // Apply sorting
    query = request.SortBy?.ToLower() switch
    {
        "transactiondate" => request.SortOrder?.ToLower() == "asc" 
            ? query.OrderBy(t => t.TransactionDate)
            : query.OrderByDescending(t => t.TransactionDate),
        _ => query.OrderByDescending(t => t.TransactionDate)
    };
    
    var totalCount = await query.CountAsync();
    var transactions = await query
        .Skip((request.Page - 1) * request.PageSize)
        .Take(request.PageSize)
        .Include(t => t.Account)
        .Include(t => t.Category)
        .Select(t => new TransactionDto
        {
            Id = t.Id,
            AccountId = t.AccountId,
            AccountName = t.Account.Name,
            Amount = t.Amount,
            Direction = t.Direction,
            TransactionDate = t.TransactionDate,
            Description = t.Description,
            Balance = t.Balance,
            CategoryName = t.Category.Name
        })
        .ToListAsync();
    
    return Ok(new PaginatedResponse<TransactionDto>
    {
        Data = transactions,
        TotalCount = totalCount,
        Page = request.Page,
        PageSize = request.PageSize,
        TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize)
    });
}
```

### Filter Request Model:
```csharp
public class TransactionFilterRequest
{
    public string? AccountId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string? Direction { get; set; } // "revenue", "spent", or null for all
    public string SortBy { get; set; } = "transactionDate";
    public string SortOrder { get; set; } = "desc";
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}
```

### Performance Optimization:

#### Caching Strategy:
```typescript
// Cache transaction data based on filter criteria
const cacheKey = computed(() => {
  const accountKey = selectedAccountId.value || 'all'
  const dateKey = `${filterStore.dateFrom}_${filterStore.dateTo}`
  return `transactions_${accountKey}_${dateKey}`
})

// Cache với expiry time
const { data: transactions, refresh } = await useCachedAsyncData(
  cacheKey.value,
  () => loadTransactions(filterStore.currentFilter),
  {
    default: () => [],
    expires: 5 * 60 * 1000 // 5 minutes cache
  }
)
```

#### Intelligent Loading:
```typescript
// Chỉ reload khi filter thay đổi thật sự
watch(
  () => filterStore.currentFilter,
  async (newFilter, oldFilter) => {
    // So sánh deep để tránh unnecessary API calls
    if (!isEqual(newFilter, oldFilter)) {
      await refresh()
    }
  },
  { deep: true }
)
```

---
