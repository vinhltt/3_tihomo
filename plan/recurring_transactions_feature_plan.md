# Kế hoạch Triển khai Tính năng Quản lý Giao dịch Định kỳ (Recurring Transactions)

## 1. Mục tiêu

Bổ sung khả năng cho người dùng theo dõi và dự báo các giao dịch thu/chi định kỳ (như subscription YouTube, Google One, tiền điện, nước...), hỗ trợ việc lập kế hoạch tài chính và chuẩn bị tiền hàng tháng/năm. Tính năng này sẽ được tích hợp vào bounded context Planning & Investment.

## 2. Phạm vi

- Thiết kế và triển khai service `RecurringTransactionService` trong bounded context Planning & Investment, sử dụng `db_planning`.
- Thiết kế và triển khai các endpoint API để quản lý mẫu giao dịch định kỳ (tạo, xem, sửa, xóa).
- Thiết kế và triển khai model `RecurringTransactionTemplate` và `ExpectedTransaction` trong database `db_planning`.
- Thiết kế và triển khai cơ chế sinh giao dịch dự kiến từ mẫu định kỳ (sử dụng Quartz.NET hoặc Hangfire).
- Cập nhật `ReportingService` để kết hợp dữ liệu từ giao dịch thực tế (db_finance) và giao dịch dự kiến (db_planning) cho báo cáo kế hoạch tiền mặt.
- Tích hợp với `NotificationService` để gửi thông báo về giao dịch định kỳ sắp đến hạn.
- Thiết kế giao diện người dùng tương ứng.

## 3. Yêu cầu Nghiệp vụ

### 3.1. Quản lý Mẫu Giao dịch Định kỳ
- **FR-RT-1:** Người dùng có thể tạo mẫu giao dịch định kỳ với: tên, mô tả, số tiền, loại (thu/chi), danh mục, tài khoản, ngày bắt đầu, tần suất lặp lại, ngày kết thúc (hoặc số lần lặp).
- **FR-RT-2:** Người dùng có thể xem danh sách các mẫu giao dịch định kỳ đã tạo.
- **FR-RT-3:** Người dùng có thể chỉnh sửa hoặc xóa mẫu giao dịch định kỳ.

### 3.2. Quản lý Giao dịch Dự kiến
- **FR-RT-4:** Hệ thống tự động sinh ra các giao dịch dự kiến dựa trên mẫu giao dịch định kỳ.
- **FR-RT-5:** Người dùng có thể liên kết giao dịch thực tế với giao dịch dự kiến.
- **FR-RT-6:** Hệ thống hiển thị thông báo khi có giao dịch định kỳ sắp đến hạn hoặc đã quá hạn.

### 3.3. Báo cáo và Dự báo
- **FR-RT-7:** Hệ thống cung cấp báo cáo kế hoạch tiền mặt kết hợp cả giao dịch thực tế và dự kiến theo tháng/năm.
- **FR-RT-8:** Người dùng có thể xem tổng số tiền cần chuẩn bị cho các giao dịch định kỳ trong tương lai.

## 4. Mô hình Dữ liệu

### 4.1. RecurringTransactionTemplate
```csharp
public enum FrequencyType
{
    Daily,
    Weekly,
    Monthly,
    Quarterly,
    Yearly
}

public class RecurringTransactionTemplate
{
    public Guid TemplateId { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Amount { get; set; }
    public CategoryType CategoryType { get; set; } // Income/Expense
    public Guid? CategoryId { get; set; }
    public Guid? AccountId { get; set; }
    public DateTime StartDate { get; set; }
    public FrequencyType FrequencyType { get; set; }
    public int FrequencyValue { get; set; } // e.g. 1 for every month, 3 for every 3 days
    public DateTime? EndDate { get; set; }
    public int? RepeatCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsActive { get; set; }
}
```

### 4.2. ExpectedTransaction
```csharp
public enum ExpectedTransactionStatus
{
    Scheduled,
    Completed,
    Missed,
    Cancelled
}

public class ExpectedTransaction
{
    public Guid ExpectedTransactionId { get; set; }
    public Guid UserId { get; set; }
    public Guid TemplateId { get; set; }
    public Guid? AccountId { get; set; }
    public decimal Amount { get; set; }
    public CategoryType CategoryType { get; set; } // Income/Expense
    public Guid? CategoryId { get; set; }
    public DateTime ScheduledDate { get; set; }
    public string Description { get; set; }
    public string Note { get; set; }
    public ExpectedTransactionStatus Status { get; set; }
    public Guid? ActualTransactionId { get; set; } // Link to actual transaction if completed
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

## 5. API Endpoints

### 5.1. RecurringTransactionTemplate API
- `GET /api/recurring-templates` - Liệt kê mẫu giao dịch định kỳ
- `GET /api/recurring-templates/{id}` - Chi tiết mẫu
- `POST /api/recurring-templates` - Tạo mẫu mới
- `PUT /api/recurring-templates/{id}` - Cập nhật mẫu
- `DELETE /api/recurring-templates/{id}` - Xóa mẫu

### 5.2. ExpectedTransaction API
- `GET /api/expected-transactions` - Liệt kê giao dịch dự kiến (filter by date range, status)
- `GET /api/expected-transactions/{id}` - Chi tiết giao dịch dự kiến
- `PUT /api/expected-transactions/{id}/status` - Cập nhật trạng thái (e.g., mark as completed)
- `PUT /api/expected-transactions/{id}/link` - Liên kết với giao dịch thực tế

### 5.3. Report API
- `GET /api/reports/cash-flow-plan` - Báo cáo kế hoạch tiền mặt (kết hợp thực tế và dự kiến)

## 6. Các Bước Triển khai

### 6.1. Phase 1: Thiết kế và Chuẩn bị (1 tuần)
- Chi tiết hóa yêu cầu nghiệp vụ
- Thiết kế database schema
- Thiết kế API endpoints và DTOs
- Thiết kế UI/UX cho quản lý mẫu giao dịch định kỳ
- Chọn giải pháp và thư viện xử lý lịch trình (Quartz.NET, Hangfire, NCrontab)

### 6.2. Phase 2: Triển khai Backend (2 tuần)
- Set up RecurringTransactionService project
- Triển khai model và repository
- Triển khai API endpoints
- Triển khai background worker để sinh giao dịch dự kiến
- Triển khai cơ chế publish events (ExpectedTransactionCreated, DueDateApproaching)
- Viết unit tests và integration tests

### 6.3. Phase 3: Tích hợp (1 tuần)
- Cập nhật ReportingService để lấy dữ liệu từ ExpectedTransaction
- Cập nhật NotificationService để gửi thông báo
- Triển khai cơ chế liên kết giao dịch thực tế và dự kiến

### 6.4. Phase 4: UI và Testing (1 tuần)
- Triển khai UI cho quản lý mẫu giao dịch định kỳ
- Triển khai UI cho báo cáo kế hoạch tiền mặt
- End-to-end testing
- Performance testing

## 7. Rủi ro và Giảm thiểu

| Rủi ro | Mức độ | Giảm thiểu |
|--------|--------|------------|
| Xử lý phức tạp các quy tắc lặp | Cao | Sử dụng thư viện chuyên dụng (Quartz.NET, NCrontab) |
| Đồng bộ giữa db_planning và db_finance | Cao | Sử dụng Message Bus, EventSourcing, hoặc ReadModel cho báo cáo |
| Hiệu năng khi sinh nhiều giao dịch dự kiến | Trung bình | Tối ưu hóa truy vấn, phân trang, caching |
| Trải nghiệm người dùng phức tạp | Trung bình | Thiết kế UI đơn giản, hướng dẫn, gợi ý |

## 8. Tiêu chí Hoàn thành

- API cho quản lý mẫu giao dịch định kỳ hoạt động ổn định
- Cơ chế sinh giao dịch dự kiến hoạt động đúng theo lịch trình
- Báo cáo kế hoạch tiền mặt hiển thị chính xác dữ liệu kết hợp
- Thông báo gửi đúng thời điểm cho các giao dịch sắp đến hạn
- Unit tests và integration tests đạt coverage >80%
- UI trực quan, dễ sử dụng

## 9. Tài nguyên và Phân công

- Backend developers: 2 người (RecurringTransactionService, Integration)
- Frontend developers: 1 người (UI/UX)
- QA: 1 người (Testing)
- BA: 1 người (Requirements, User stories)
- Thời gian dự kiến: 5 tuần

---

*Tài liệu kế hoạch cho tính năng Recurring Transactions. Phiên bản 1.0.* 