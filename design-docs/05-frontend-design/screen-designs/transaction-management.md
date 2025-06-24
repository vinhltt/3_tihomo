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

* Hiển thị **toàn bộ giao dịch trong 30 ngày gần nhất** của **tất cả tài khoản**
* Có thể **lọc** theo:
  * Tài khoản (Account dropdown)
  * Loại giao dịch: `Revenue` / `Spent`
  * Khoảng thời gian (theo `TransactionDate` với thời gian)
* **Sắp xếp mặc định**: `TransactionDate` giảm dần (mới nhất ở trên)

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
  * Tài khoản được chọn sẵn theo dropdown account hiện tại trong danh sách
  * `TransactionDate` mặc định là thời gian hiện tại

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
