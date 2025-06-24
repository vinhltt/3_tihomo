## 🎯 Mục tiêu

Thiết kế UI/UX và logic quản lý cho màn hình "Quản lý tài khoản" và trang danh sách tài khoản trong ứng dụng quản lý chi tiêu cá nhân.

---

## 🧩 1. Giao diện mặc định (Simple Mode)

### ✅ Trường hiển thị mặc định:

| Trường             | Loại nhập                    | Gợi ý UX                              |
| ------------------ | ---------------------------- | ------------------------------------- |
| `Name`             | Text input                   | Tên tài khoản dễ nhận biết            |
| `Type`             | Dropdown (AccountType)       | Icon minh họa cho từng loại           |
| `Currency`         | Dropdown tiền tệ             | VND mặc định, hiển thị ký hiệu        |
| `InitialBalance`   | Số tiền                      | Số dư ban đầu khi tạo tài khoản       |
| `CurrentBalance`   | Số tiền (readonly)           | Tính tự động từ giao dịch             |
| `IsActive`         | Toggle switch                | Trạng thái hoạt động của tài khoản    |
| Nút \[Mở rộng]     | Toggle UI                    | Hiện thêm các trường nâng cao         |

---

## 🚀 2. Giao diện mở rộng (Advanced Mode)

### 💳 Nhóm "Thông tin thẻ":

* `CardNumber` - Số thẻ (cho Bank, CreditCard, DebitCard)
* `AvailableLimit` - Hạn mức khả dụng (chỉ cho CreditCard)

### 🛠 Nhóm "Metadata & Audit":

* `UserId` - ID người dùng sở hữu
* `CreateAt`, `UpdateAt` - Thời gian tạo/cập nhật
* `CreateBy`, `UpdateBy` - Người tạo/cập nhật

---

## 🔒 3. Ràng buộc logic & AccountType

### AccountType enum:
```csharp
public enum AccountType
{
    Bank = 0,        // Tài khoản ngân hàng
    Wallet = 1,      // Ví điện tử  
    CreditCard = 2,  // Thẻ tín dụng
    DebitCard = 3,   // Thẻ ghi nợ
    Cash = 4         // Tiền mặt
}
```

### Logic hiển thị theo AccountType:

* **Bank, DebitCard, CreditCard**: Hiển thị trường `CardNumber`
* **CreditCard**: Thêm trường `AvailableLimit` (bắt buộc)
* **Wallet, Cash**: Ẩn `CardNumber` và `AvailableLimit`

### Validation rules:

* `Name`: Bắt buộc, tối đa 100 ký tự
* `Currency`: Bắt buộc, tối đa 10 ký tự
* `CardNumber`: Tùy chọn, tối đa 32 ký tự, format theo loại thẻ
* `InitialBalance`: Bắt buộc, decimal(18,2)
* `AvailableLimit`: Bắt buộc cho CreditCard, decimal(18,2)

---

## 💰 4. Logic xử lý CurrentBalance tự động

### Frontend (FE):
* **CurrentBalance là read-only field**:
  * Hiển thị giá trị tính toán từ backend
  * Không cho phép chỉnh sửa trực tiếp
  * Cập nhật real-time khi có giao dịch mới
* **Balance display**:
  * Format theo currency (VND: 1.000.000 ₫)
  * Màu sắc: dương (xanh), âm (đỏ), zero (xám)
  * Tooltip hiển thị công thức: InitialBalance ± ∑Transactions

### Backend (BE):
* **Tính toán CurrentBalance**:
  1. `CurrentBalance = InitialBalance + SumOfRevenue - SumOfSpent`
  2. Cập nhật tự động mỗi khi có transaction mới/sửa/xóa
  3. Cache result để tăng performance
* **Balance recalculation triggers**:
  * Khi tạo/sửa/xóa transaction thuộc account
  * Khi sửa InitialBalance
  * Background job định kỳ để đảm bảo consistency

### API endpoints mới:
```csharp
// Tính lại CurrentBalance cho account
[HttpPost("recalculate-balance/{accountId}")]
public async Task<ActionResult<decimal>> RecalculateBalance(Guid accountId)

// Lấy balance history của account
[HttpGet("balance-history/{accountId}")]
public async Task<ActionResult<BalanceHistoryResponse>> GetBalanceHistory(
    Guid accountId, 
    DateTime? fromDate, 
    DateTime? toDate)

// Lấy thống kê account summary
[HttpGet("summary/{accountId}")]
public async Task<ActionResult<AccountSummaryResponse>> GetAccountSummary(Guid accountId)
```

---

## 📋 5. Tính năng UX nâng cao đề xuất

* **Smart validation**: Kiểm tra trùng tên, số thẻ trong cùng user
* **Account templates**: Mẫu tài khoản phổ biến (VCB, Techcombank, MoMo...)
* **Currency auto-suggest**: Gợi ý currency phổ biến (VND, USD, EUR)
* **Card number formatting**: Auto-format theo pattern (4-4-4-4)
* **Balance alerts**: Cảnh báo khi balance âm hoặc gần hết limit
* **Account health**: Chỉ số đánh giá tình trạng tài khoản

---

## 🖥️ 6. Giao diện danh sách tài khoản (Account List Page)

### Layout chia đôi màn hình:

* **Khi KHÔNG có account detail mở**: Danh sách tài khoản chiếm **toàn bộ màn hình**
* **Khi CÓ account detail mở**: 
  * **Desktop**: Màn hình chia đôi:
    * **Bên trái (50%)**: Danh sách tài khoản
    * **Bên phải (50%)**: Chi tiết tài khoản
  * **Mobile**: Detail account chiếm **toàn bộ màn hình**, ẩn danh sách

### Mặc định hiển thị:

* Hiển thị **toàn bộ tài khoản** của user hiện tại
* Có thể **lọc** theo:
  * Loại tài khoản (AccountType dropdown)
  * Trạng thái: `Active` / `Inactive`
  * Đơn vị tiền tệ (Currency dropdown)
  * Tìm kiếm theo tên tài khoản
* **Sắp xếp mặc định**: `UpdateAt` giảm dần (được cập nhật gần nhất ở trên)

### Cột hiển thị theo chế độ:

#### 🔹 Chế độ đơn giản (mặc định):
Chỉ hiển thị **5 cột chính**:
1. **Tên tài khoản** (`Name`) + Icon type
2. **Loại** (`Type`) - Badge với màu sắc
3. **Số dư hiện tại** (`CurrentBalance`) - Format tiền tệ
4. **Đơn vị tiền tệ** (`Currency`)
5. **Trạng thái** (`IsActive`) - Toggle switch nhỏ

#### 🔹 Chế độ nâng cao:
Hiển thị **tất cả các cột** bao gồm:
* 5 cột chính + CardNumber, AvailableLimit, InitialBalance
* CreateAt, UpdateAt (dạng relative time)
* Actions (Edit, Delete, View Details)

#### 🔹 Nút tùy chọn cột:
* **Nút "Columns"** ở góc phải bảng
* Click vào sẽ hiện dropdown checklist các cột có thể hiển thị/ẩn
* Người dùng có thể tự chọn cột nào muốn xem thêm
* Lưu preferences của user vào localStorage

### Nút thao tác:

* `+ Tài khoản ngân hàng` → mở giao diện thêm với `Type = Bank`
* `+ Ví điện tử` → mở giao diện thêm với `Type = Wallet`
* `+ Thẻ tín dụng` → mở giao diện thêm với `Type = CreditCard`
* `+ Tiền mặt` → mở giao diện thêm với `Type = Cash`
* Khi nhấn các nút này:
  * Giao diện detail thêm account được mở ở **bên phải màn hình**
  * `Type` được chọn sẵn nhưng vẫn có thể thay đổi
  * `Currency` mặc định là "VND"
  * `IsActive` mặc định là `true`

### Tương tác với account trong danh sách:

* **Click vào bất kỳ account nào** trong danh sách sẽ:
  * Mở **detail pane** ở bên phải với thông tin chi tiết account đó
  * **Chế độ xem**: Hiển thị đầy đủ thông tin account
  * **Các nút action**: `Update`, `Delete`, `Duplicate`, `View Transactions`
  * **Highlight** account được chọn trong danh sách

### Account overview trong detail pane:

* **Balance cards**: 
  * Current Balance (lớn, nổi bật)
  * Initial Balance 
  * Available Limit (nếu có)
  * Total Transactions count
* **Balance history chart**: Line chart 30 ngày gần nhất
* **Recent transactions**: 10 giao dịch gần nhất của account
* **Account health score**: Đánh giá tình trạng tài khoản

---

## 🎨 7. Giao diện chi tiết tài khoản (Detail Pane)

### Desktop layout:
* **Bên phải màn hình (50%)**
* Danh sách bên trái thu hẹp còn **50%**
* Có thể resize được ranh giới giữa 2 pane
* Scroll independent giữa list và detail

### Mobile layout:
* Chiếm **toàn bộ màn hình**
* Ẩn hoàn toàn danh sách account
* Swipe gesture để quay lại danh sách

### Cấu trúc detail pane:

#### 📊 Section 1: Account Overview
* **Header**: Account name + type icon + status badge
* **Balance Summary Cards**:
  ```
  ┌─────────────────┬─────────────────┐
  │  Current Balance │  Initial Balance │
  │   1.500.000 ₫   │   1.000.000 ₫   │
  └─────────────────┴─────────────────┘
  ┌─────────────────┬─────────────────┐
  │ Available Limit │ Used Percentage │  
  │   2.000.000 ₫   │      25%        │ (CreditCard only)
  └─────────────────┴─────────────────┘
  ```

#### 📈 Section 2: Balance Trend
* **Line chart** hiển thị biến động số dư 30 ngày gần nhất
* **Interactive tooltips** với thông tin giao dịch
* **Zoom & pan** cho period dài hơn

#### 📝 Section 3: Account Information
* **Edit form** với validation real-time
* **Dynamic fields** dựa theo AccountType
* **Auto-save** hoặc manual save

#### 🔄 Section 4: Recent Activity
* **Recent transactions** (10 items gần nhất)
* **Quick action**: Add transaction cho account này
* **Link** để xem toàn bộ transactions

#### ⚙️ Section 5: Actions
* **Primary actions**: Update, Delete
* **Secondary actions**: Duplicate, Export Data, View Full History
* **Danger zone**: Deactivate Account

### Đóng detail pane:

* **Nút X** ở góc phải header
* **Phím ESC** để đóng nhanh
* **Click outside** (chỉ trên desktop)
* **Swipe down** (mobile)

---

## 🔧 8. Logic CRUD và trạng thái

### Create Account:
* **Validation**: Real-time validation khi nhập
* **Auto-suggest**: Tên account từ type (VD: "Techcombank - ****1234")
* **Currency default**: VND cho user Việt Nam
* **Success flow**: Thêm thành công → đóng form → highlight item mới

### Update Account:
* **Dirty checking**: Chỉ gửi fields thay đổi
* **Optimistic updates**: Cập nhật UI trước, rollback nếu lỗi
* **Field restrictions**: Một số field không cho sửa sau khi tạo
* **Balance recalculation**: Tự động khi sửa InitialBalance

### Delete Account:
* **Soft delete**: Chỉ set IsActive = false
* **Dependency check**: Cảnh báo nếu có transactions liên quan
* **Confirmation modal**: "Bạn có chắc muốn xóa...?"
* **Undo option**: Cho phép khôi phục trong 30s

### Account Status:
* **Active/Inactive toggle**: Trong danh sách và detail
* **Cascade effects**: Inactive account không hiện trong transaction dropdown
* **Visual indicators**: Màu xám cho inactive accounts

---

## ⌨️ 9. Keyboard shortcuts

* **ESC**: Đóng detail pane
* **Ctrl/Cmd + N**: Tạo account mới
* **Enter**: Submit form trong detail pane
* **Tab**: Di chuyển giữa các trường nhập liệu
* **↑/↓**: Di chuyển giữa các account trong danh sách
* **Ctrl/Cmd + S**: Save changes trong detail form
* **Delete**: Xóa account đang chọn (với confirmation)

---

## 🎨 10. UI/UX Design System

### Account Type Icons:
* **Bank**: 🏦 (hoặc bank icon)
* **Wallet**: 👛 (hoặc wallet icon)
* **CreditCard**: 💳 (hoặc credit card icon)
* **DebitCard**: 💳 (hoặc debit card icon, màu khác)
* **Cash**: 💰 (hoặc cash icon)

### Color Coding:
* **Positive Balance**: `text-success` (xanh lá)
* **Negative Balance**: `text-danger` (đỏ)
* **Zero Balance**: `text-muted` (xám)
* **Active Account**: `text-primary` (xanh dương)
* **Inactive Account**: `text-muted` (xám)

### Card Number Display:
* **Security masking**: `**** **** **** 1234`
* **Full display**: Chỉ trong edit mode
* **Auto-formatting**: Thêm space mỗi 4 số

### Balance Formatting:
```typescript
// VND: 1.000.000 ₫
// USD: $1,000.00
// EUR: €1.000,00
```

---

## 🔧 11. Technical Implementation Notes

### Database Considerations:
* Index on `(UserId, IsActive)` for user accounts filtering
* Index on `(Type, IsActive)` for type-based queries
* Soft delete pattern với `IsActive` field

### Performance Optimization:
* **Lazy loading**: Balance history và transactions
* **Pagination**: Cho danh sách account và transactions
* **Caching**: CurrentBalance và summary data
* **Debounced search**: Tránh quá nhiều API calls

### Security:
* **Card number encryption**: Lưu trữ an toàn số thẻ
* **Audit logging**: Track tất cả thay đổi quan trọng
* **Access control**: User chỉ thấy account của mình

### Error Handling:
* **Network errors**: Retry mechanism với exponential backoff
* **Validation errors**: Real-time feedback
* **Server errors**: Graceful degradation
* **Offline support**: Cache data cho read operations

### Responsive Design:
* **Desktop (≥1024px)**: Full split-pane layout
* **Tablet (768px-1023px)**: Condensed layout, detail 60% width
* **Mobile (<768px)**: Stack layout, full-screen detail

---

## 📊 12. Analytics & Monitoring

### User Behavior Tracking:
* Account creation patterns (most used types)
* Feature usage (simple vs advanced mode)
* Error rates per form field
* Time to complete account setup

### Performance Metrics:
* Page load time
* API response time
* Balance calculation duration
* Chart rendering performance

### Business Metrics:
* Average accounts per user
* Most popular account types
* Currency distribution
* Active vs inactive ratio

---

## 🚀 13. Future Enhancements

### Phase 2 Features:
* **Account linking**: Liên kết multiple cards/accounts
* **Import from banks**: Tự động sync từ ngân hàng
* **Account categorization**: Nhóm accounts theo mục đích
* **Spending limits**: Đặt hạn mức chi tiêu theo account

### Phase 3 Features:
* **Account sharing**: Chia sẻ account với family members
* **Investment tracking**: Track investment accounts
* **Multi-currency**: Real-time exchange rates
* **Account insights**: AI-powered financial advice

### Advanced Analytics:
* **Cash flow analysis**: Per account cash flow patterns
* **Account performance**: ROI, growth trends
* **Spending behavior**: Per account spending patterns
* **Predictive analytics**: Account balance forecasting
