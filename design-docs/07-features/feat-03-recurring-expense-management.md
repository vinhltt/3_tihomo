# FEAT-03: QUẢN LÝ CHI TIÊU ĐỊNH KỲ

**Mã tính năng:** feat-03  
**Tên tính năng:** Quản lý Chi tiêu Định kỳ & Lập lịch Job  
**Trạng thái:** Đang Thiết kế  
**Ưu tiên:** Cao  
**Epic:** Tự động hóa Tài chính Cốt lõi  

## 📋 TÓM TẮT YÊU CẦU

### 🎯 Tổng quan Tính năng
**Tính năng**: Quản lý Chi tiêu Định kỳ & Lập lịch Job  
**Mục đích**: Quản lý chi tiêu định kỳ và tự động tạo kế hoạch chi tiêu hàng tháng

### 🗂️ Yêu cầu Cơ sở Dữ liệu

#### Bảng: RecurringTransactions (Giao dịch Định kỳ)
```sql
RecurringTransactions:
- Id (PK, UUID)
- Code (Khóa Nghiệp vụ, duy nhất)
- UserId (FK) -- Hỗ trợ đa người dùng
- AccountId (FK) -- Liên kết với Tài khoản
- CategoryId (FK) -- Danh mục chi tiêu
- TransactionType (Thu nhập/Chi tiêu/Chuyển khoản)
- Amount (Decimal) -- Số tiền
- Currency (String, mặc định VND)
- Frequency (Enum: Hàng ngày/Hàng tuần/Hàng tháng/Hàng quý/Hàng năm)
- FrequencyInterval (Int) -- Mỗi X ngày/tuần/tháng
- StartDate (DateTime) -- Ngày bắt đầu
- EndDate (DateTime, nullable) -- Ngày kết thúc
- NextExecutionDate (DateTime) -- Ngày thực hiện tiếp theo
- LastExecutionDate (DateTime, nullable) -- Ngày thực hiện cuối
- Title (String) -- Tiêu đề
- Description (Text) -- Mô tả
- IsActive (Boolean) -- Có hoạt động
- IsAutoExecute (Boolean) -- Tự động thực hiện
- CreatedAt/UpdatedAt -- Ngày tạo/cập nhật
- CreatedBy/UpdatedBy -- Người tạo/cập nhật
```

#### Bảng Hỗ trợ
```sql
RecurringTransactionExecutions (Lịch sử Thực hiện):
- Id (PK)
- RecurringTransactionId (FK)
- ExecutionDate (DateTime) -- Ngày thực hiện
- PlannedAmount vs ActualAmount -- Số tiền dự kiến vs thực tế
- Status (Pending/Executed/Failed/Skipped) -- Trạng thái
- TransactionId (FK, nullable) -- Liên kết với giao dịch thực tế
- Notes (Text) -- Ghi chú

RecurringTransactionTemplates (Mẫu Giao dịch):
- Id (PK)
- Name (String) -- "Tiền thuê nhà hàng tháng", "Tiền ăn hàng tuần"
- DefaultCategory -- Danh mục mặc định
- DefaultAmount -- Số tiền mặc định
- DefaultFrequency -- Tần suất mặc định
- IsSystemTemplate/IsUserTemplate -- Mẫu hệ thống/người dùng
```

### 🔧 Yêu cầu Kỹ thuật
- **Backend**: .NET 9 microservices (dịch vụ CoreFinance)
- **Cơ sở dữ liệu**: PostgreSQL với Entity Framework Core
- **Lập lịch Job**: Background jobs cho tự động thực hiện
- **API**: RESTful endpoints với JWT authentication
- **Frontend**: Tích hợp Nuxt 3

### ⚙️ Yêu cầu Lập lịch Job

**Background Jobs cần có:**

1. **🔄 Job Thực hiện** (Cron hàng ngày):
   - Quét NextExecutionDate <= HÔM NAY
   - Tạo giao dịch chờ xử lý
   - Cập nhật NextExecutionDate dựa trên tần suất

2. **📬 Job Thông báo** (Hàng ngày):
   - Thông báo cho người dùng về các khoản thanh toán sắp tới (1-3 ngày trước)
   - Nhắc nhở về các phê duyệt thủ công quá hạn

3. **📊 Job Phân tích** (Hàng tuần):
   - Tính toán mô hình chi tiêu
   - Cập nhật so sánh ngân sách vs thực tế
   - Tạo insights/khuyến nghị

### 🎯 Thiết kế API Endpoints

**CRUD Cơ bản:**
```
GET    /api/v1/recurring-transactions        # Lấy danh sách
POST   /api/v1/recurring-transactions        # Tạo mới
GET    /api/v1/recurring-transactions/{id}   # Lấy chi tiết
PUT    /api/v1/recurring-transactions/{id}   # Cập nhật
DELETE /api/v1/recurring-transactions/{id}   # Xóa
```

**Thao tác Nâng cao:**
```
POST   /api/v1/recurring-transactions/{id}/execute-now    # Thực hiện ngay
POST   /api/v1/recurring-transactions/{id}/skip-next     # Bỏ qua lần tiếp theo
GET    /api/v1/recurring-transactions/{id}/forecast?months=6  # Dự báo
GET    /api/v1/recurring-transactions/calendar?from=2024-01&to=2024-12  # Lịch
POST   /api/v1/recurring-transactions/bulk-import        # Import hàng loạt
```

### ✨ Tính năng Cốt lõi Đã xác định
1. **📅 Quản lý Tần suất** - Hàng ngày, Hàng tuần, Hàng tháng, Hàng quý, Hàng năm
2. **🔄 Tự động thực hiện** - Job tự động tạo giao dịch
3. **📊 Theo dõi Ngân sách** - So sánh thực tế vs dự kiến
4. **⚠️ Thông báo** - Cảnh báo khi gần ngày thanh toán
5. **📈 Dự báo** - Dự báo dòng tiền
6. **🏷️ Danh mục** - Phân loại chi tiêu
7. **💰 Số tiền Linh hoạt** - Hỗ trợ điều chỉnh

### 👥 Đối tượng Người dùng
- **Người dùng Cá nhân**: Quản lý tài chính cá nhân
- **Quản lý Gia đình**: Lập kế hoạch chi tiêu hộ gia đình  
- **Người dùng Doanh nghiệp**: Chi phí định kỳ doanh nghiệp nhỏ

### 🎯 Chỉ số Thành công
- Tỷ lệ áp dụng của người dùng
- Độ chính xác giao dịch tự động
- Thời gian tiết kiệm khi nhập liệu thủ công
- Độ chính xác dự báo ngân sách

---

## 📖 EPIC 1: QUẢN LÝ GIAO DỊCH ĐỊNH KỲ CƠ BẢN

### User Story 1.1: Tạo Mẫu Giao dịch Định kỳ

**Với tư cách là người dùng cá nhân quản lý tài chính cá nhân**  
**Tôi muốn tạo một mẫu giao dịch định kỳ cho các chi phí hàng tháng**  
**Để tôi có thể tự động hóa việc lập kế hoạch tài chính lặp đi lặp lại và không bao giờ bỏ lỡ các khoản thanh toán**

#### Tiêu chí Chấp nhận:
- **Cho trước** Tôi đã đăng nhập vào TiHoMo và đang ở trang giao dịch định kỳ
- **Khi** Tôi nhấp "Tạo Giao dịch Định kỳ"
- **Thì** Tôi sẽ thấy một form với các trường bắt buộc:
  - Tên/Mô tả Giao dịch
  - Số tiền (VND)
  - Loại Giao dịch (Thu nhập/Chi tiêu/Chuyển khoản)
  - Tài khoản (dropdown từ các tài khoản của tôi)
  - Danh mục (dropdown của danh mục chi tiêu)
  - Tần suất (Hàng tháng/Hàng tuần/Hàng ngày/Hàng quý/Hàng năm)
  - Ngày Bắt đầu
  - Ngày Kết thúc (tùy chọn)
  - Tự động thực hiện (checkbox)

- **Khi** Tôi điền đầy đủ các trường bắt buộc và nhấp "Lưu"
- **Thì** Hệ thống sẽ:
  - Tạo một Code duy nhất cho giao dịch
  - Tính toán NextExecutionDate dựa trên tần suất
  - Lưu mẫu vào cơ sở dữ liệu
  - Hiển thị xác nhận thành công
  - Chuyển hướng đến danh sách giao dịch định kỳ

- **Khi** Tôi để trống các trường bắt buộc
- **Thì** Tôi sẽ thấy lỗi validation cho mỗi trường thiếu

**Giá trị Kinh doanh:** Nền tảng cho tất cả tự động hóa giao dịch định kỳ - giảm 80% công việc thủ công  
**Ưu tiên:** Phải có (MVP)  
**Story Points:** 5  
**Phụ thuộc:** Hệ thống quản lý tài khoản, Hệ thống danh mục

---

### User Story 1.2: Xem Danh sách Giao dịch Định kỳ

**Với tư cách là người dùng có giao dịch định kỳ**  
**Tôi muốn xem danh sách toàn diện tất cả giao dịch định kỳ của mình**  
**Để tôi có thể giám sát và quản lý các cam kết tài chính tự động**

#### Tiêu chí Chấp nhận:
- **Cho trước** Tôi đã tạo các giao dịch định kỳ
- **Khi** Tôi điều hướng đến trang giao dịch định kỳ
- **Thì** Tôi sẽ thấy một bảng/danh sách hiển thị:
  - Tên Giao dịch
  - Số tiền với đơn vị tiền tệ (VND)
  - Loại (biểu tượng Thu nhập/Chi tiêu với mã màu)
  - Tần suất (văn bản như "Hàng tháng", "Hàng tuần")
  - Ngày Thanh toán Tiếp theo
  - Tên Tài khoản
  - Trạng thái (Hoạt động/Vô hiệu hóa với toggle)
  - Hành động (Sửa, Xóa, Thực hiện Ngay)

- **Và** danh sách sẽ:
  - Có thể sắp xếp theo Tên, Số tiền, Ngày Thanh toán Tiếp theo
  - Có thể lọc theo Loại, Tài khoản, Trạng thái
  - Phân trang nếu có hơn 20 mục
  - Hiển thị trạng thái trống nếu không có giao dịch nào

- **Khi** Tôi nhấp vào một hàng giao dịch
- **Thì** Tôi sẽ thấy chi tiết đầy đủ của giao dịch

**Giá trị Kinh doanh:** Khả năng hiển thị thiết yếu vào các cam kết định kỳ - cho phép người dùng kiểm soát  
**Ưu tiên:** Phải có (MVP)  
**Story Points:** 3  
**Phụ thuộc:** Không có

---

### User Story 1.3: Chỉnh sửa Giao dịch Định kỳ

**Với tư cách là người dùng có giao dịch định kỳ hiện tại**  
**Tôi muốn sửa đổi chi tiết giao dịch như số tiền hoặc tần suất**  
**Để tôi có thể thích ứng với hoàn cảnh tài chính thay đổi**

#### Tiêu chí Chấp nhận:
- **Cho trước** Tôi đang xem danh sách giao dịch định kỳ của mình
- **Khi** Tôi nhấp "Sửa" trên bất kỳ giao dịch nào
- **Thì** Tôi sẽ thấy form chỉnh sửa được điền sẵn với các giá trị hiện tại
  - Tất cả các trường có thể chỉnh sửa trừ Code
  - Tôi có thể sửa đổi: Số tiền, Mô tả, Tần suất, Tài khoản, Danh mục, Ngày Kết thúc
  - Cài đặt tự động thực hiện có thể bật/tắt

- **Khi** Tôi lưu các thay đổi hợp lệ
- **Thì** Hệ thống sẽ:
  - Cập nhật bản ghi giao dịch
  - Tính lại NextExecutionDate nếu tần suất thay đổi
  - Hiển thị xác nhận thành công
  - Cập nhật chế độ xem danh sách ngay lập tức

- **Khi** Tôi thay đổi tần suất
- **Thì** NextExecutionDate sẽ được tính lại dựa trên:
  - Cài đặt tần suất mới
  - Ngày thực hiện cuối (nếu có) hoặc ngày bắt đầu
  - Không tạo ra các lần thực hiện trùng lặp

- **Khi** Tôi cố gắng đặt ngày kết thúc trước ngày bắt đầu
- **Thì** Tôi sẽ thấy lỗi validation

**Giá trị Kinh doanh:** Tính linh hoạt để thích ứng với thay đổi cuộc sống - tăng khả năng giữ chân người dùng lâu dài  
**Ưu tiên:** Phải có (MVP)  
**Story Points:** 4  
**Phụ thuộc:** Logic validation, Dịch vụ tính toán ngày

---

### User Story 1.4: Xóa Giao dịch Định kỳ

**Với tư cách là người dùng không còn cần giao dịch định kỳ**  
**Tôi muốn xóa an toàn nó khỏi hệ thống**  
**Để nó không tiếp tục tạo ra các giao dịch không mong muốn**

#### Tiêu chí Chấp nhận:
- **Cho trước** Tôi đang xem danh sách giao dịch định kỳ của mình
- **Khi** Tôi nhấp "Xóa" trên bất kỳ giao dịch nào
- **Thì** Tôi sẽ thấy hộp thoại xác nhận nêu:
  - "Bạn có chắc chắn muốn xóa '[Tên Giao dịch]'?"
  - "Điều này sẽ dừng tất cả các lần thực hiện tự động trong tương lai"
  - "Lịch sử giao dịch trong quá khứ sẽ được bảo tồn"
  - Nút Hủy và Xác nhận Xóa

- **Khi** Tôi nhấp "Xác nhận Xóa"
- **Thì** Hệ thống sẽ:
  - Đánh dấu giao dịch định kỳ là đã xóa (soft delete)
  - Hủy bỏ các lần thực hiện đang chờ
  - Loại bỏ nó khỏi danh sách hoạt động
  - Hiển thị thông báo thành công "Giao dịch định kỳ đã được xóa thành công"

- **Khi** Tôi nhấp "Hủy"
- **Thì** Hộp thoại sẽ đóng mà không có thay đổi nào

- **Khi** Giao dịch có các lần thực hiện đang chờ
- **Thì** Xác nhận sẽ cảnh báo: "Giao dịch này có X lần thực hiện đang chờ sẽ bị hủy"

**Giá trị Kinh doanh:** Chức năng dọn dẹp an toàn - ngăn chặn các khoản phí không mong muốn  
**Ưu tiên:** Phải có (MVP)  
**Story Points:** 2  
**Phụ thuộc:** Triển khai soft delete, Quản lý thực hiện đang chờ

---

### User Story 1.5: Thực hiện Thủ công Giao dịch Định kỳ

**Với tư cách là người dùng muốn kiểm soát thời gian giao dịch**  
**Tôi muốn thực hiện thủ công một giao dịch định kỳ trước ngày dự kiến**  
**Để tôi có thể xử lý các khoản thanh toán sớm hoặc bù đắp các giao dịch bị bỏ lỡ**

#### Tiêu chí Chấp nhận:
- **Cho trước** Tôi có một giao dịch định kỳ đang hoạt động
- **Khi** Tôi nhấp "Thực hiện Ngay" từ menu hành động
- **Thì** Tôi sẽ thấy hộp thoại xác nhận hiển thị:
  - Chi tiết giao dịch (Tên, Số tiền, Tài khoản, Mô tả)
  - Số dư tài khoản hiện tại
  - Số dư sau giao dịch
  - Xác nhận "Thực hiện Giao dịch Ngay?"

- **Khi** Tài khoản của tôi có đủ số dư và tôi xác nhận
- **Thì** Hệ thống sẽ:
  - Tạo bản ghi giao dịch thực tế
  - Trừ/cộng số tiền từ/vào tài khoản
  - Cập nhật LastExecutionDate thành hôm nay
  - Tính toán NextExecutionDate tiếp theo dựa trên tần suất
  - Hiển thị thông báo thành công với mã tham chiếu giao dịch

- **Khi** Tài khoản của tôi không đủ số dư
- **Thì** Tôi sẽ thấy lỗi: "Số dư không đủ. Hiện tại: X VND, Cần: Y VND"

- **Khi** Việc thực hiện thất bại vì bất kỳ lý do nào
- **Thì** Tôi sẽ thấy thông báo lỗi phù hợp và giao dịch định kỳ sẽ không thay đổi

**Giá trị Kinh doanh:** Kiểm soát và tính linh hoạt của người dùng - xử lý các trường hợp đặc biệt  
**Ưu tiên:** Nên có  
**Story Points:** 5  
**Phụ thuộc:** Dịch vụ tạo giao dịch, Validation số dư tài khoản

---

### User Story 1.6: Vô hiệu hóa/Kích hoạt Giao dịch Định kỳ

**Với tư cách là người dùng muốn tạm dừng giao dịch định kỳ**  
**Tôi muốn vô hiệu hóa nó mà không mất cấu hình**  
**Để tôi có thể tiếp tục sau này khi cần**

#### Tiêu chí Chấp nhận:
- **Cho trước** Tôi đang xem danh sách giao dịch định kỳ của mình
- **Khi** Tôi chuyển công tắc Trạng thái từ Hoạt động sang Vô hiệu hóa
- **Thì** Hệ thống sẽ:
  - Cập nhật trường IsActive thành false
  - Hủy bỏ các lần thực hiện đang chờ
  - Thay đổi chỉ báo hình ảnh để hiển thị trạng thái "Vô hiệu hóa"
  - Làm mờ hàng giao dịch
  - Hiển thị xác nhận "Giao dịch đã được vô hiệu hóa thành công"

- **Khi** Tôi chuyển từ Vô hiệu hóa sang Hoạt động
- **Thì** Hệ thống sẽ:
  - Cập nhật trường IsActive thành true
  - Tính lại NextExecutionDate dựa trên ngày hiện tại và tần suất
  - Khôi phục giao diện bình thường
  - Hiển thị xác nhận "Giao dịch đã được kích hoạt thành công"

- **Khi** Một giao dịch bị vô hiệu hóa
- **Thì** Nó sẽ:
  - Không xuất hiện trong các lần thực hiện sắp tới
  - Vẫn hiển thị trong danh sách với trạng thái vô hiệu hóa
  - Bị loại trừ khỏi tính toán dự báo

**Giá trị Kinh doanh:** Kiểm soát tạm thời mà không mất dữ liệu - xử lý thay đổi cuộc sống  
**Ưu tiên:** Nên có  
**Story Points:** 3  
**Phụ thuộc:** Quản lý trạng thái, Lập lịch thực hiện

---

## 📊 TÓM TẮT EPIC 1

### Phân tích Story Points:
- **Stories Phải có:** 4 stories (14 điểm)
- **Stories Nên có:** 2 stories (8 điểm)
- **Tổng Epic 1:** 22 story points

### Phân bổ Ưu tiên:
1. **Story 1.1 - Tạo Mẫu:** Phải có (5 pts)
2. **Story 1.2 - Xem Danh sách:** Phải có (3 pts)
3. **Story 1.3 - Chỉnh sửa Giao dịch:** Phải có (4 pts)
4. **Story 1.4 - Xóa Giao dịch:** Phải có (2 pts)
5. **Story 1.5 - Thực hiện Thủ công:** Nên có (5 pts)
6. **Story 1.6 - Vô hiệu hóa/Kích hoạt:** Nên có (3 pts)

### Ghi chú Triển khai Kỹ thuật:
- Schema cơ sở dữ liệu hỗ trợ tất cả yêu cầu
- API endpoints: Thao tác CRUD với validation
- Frontend: Validation form và cập nhật real-time
- Điểm tích hợp background job đã được xác định

---

## 🔍 DANH SÁCH KIỂM TRA REVIEW

### Review Yêu cầu:
- [ ] Schema cơ sở dữ liệu có đầy đủ các trường cần thiết?
- [ ] API endpoints bao phủ hết các use cases?
- [ ] Yêu cầu lập lịch job rõ ràng?
- [ ] Công nghệ phù hợp với kiến trúc TiHoMo?

### Review User Stories:
- [ ] User stories có đầy đủ tiêu chí chấp nhận?
- [ ] Giá trị kinh doanh được định nghĩa rõ ràng?
- [ ] Mức độ ưu tiên phù hợp?
- [ ] Ước tính story points hợp lý?
- [ ] Các phụ thuộc được xác định đúng?

### Các Yếu tố Thiếu:
- [ ] Có thiếu user scenarios nào không?
- [ ] Các edge cases có được bao phủ đủ?
- [ ] Scenarios xử lý lỗi đầy đủ?
- [ ] Các cân nhắc bảo mật có được đề cập?

---

## 📋 CÁC GIAI ĐOẠN PHÁT TRIỂN

### Giai đoạn 1: Nền tảng (Epic 1)
- Triển khai schema cơ sở dữ liệu
- APIs CRUD cốt lõi
- Giao diện frontend cơ bản
- Chức năng MVP

### Giai đoạn 2: Tự động hóa (Epic 2)
- Hệ thống lập lịch job
- Logic tự động thực hiện
- Hệ thống thông báo
- Xử lý tần suất nâng cao

### Giai đoạn 3: Thông minh hóa (Epic 3)
- Thuật toán dự báo
- Dashboard phân tích
- Tạo insights
- Tối ưu hóa hiệu suất

---

## 📋 CÁC BƯỚC TIẾP THEO

**REVIEW CHECKLIST:**
1. ✅ Bạn review requirements summary
2. ✅ Bạn review database schema  
3. ✅ Bạn review từng user story chi tiết
4. ✅ Bạn check acceptance criteria
5. ✅ Bạn verify business value
6. ✅ Bạn confirm story points estimation

**SAU KHI REVIEW:**
- Có thiếu requirements nào không?
- User stories có cover đủ use cases?
- Cần adjust priority hoặc story points?
- Ready cho technical design phase?

**Vị trí File:** `design-docs/07-features/feat-03-recurring-expense-management.md`