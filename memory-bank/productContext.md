# productContext.md

## Lý do dự án tồn tại
- Nhu cầu tự động hóa các quy trình nghiệp vụ tài chính cá nhân, giảm thiểu lỗi thủ công, tăng hiệu suất và độ chính xác.
- Đáp ứng nhu cầu quản lý tài chính toàn diện, tập trung, dễ sử dụng cho người dùng phổ thông.

## Vấn đề cần giải quyết
- Khó khăn trong việc tổng hợp dữ liệu từ nhiều nguồn (ngân hàng, ví điện tử, tiền mặt, thẻ tín dụng).
- Quản lý chi tiêu, lập ngân sách, đặt mục tiêu tài chính, theo dõi nợ và đầu tư còn rời rạc, thủ công, dễ sai sót.
- Thiếu công cụ báo cáo, phân tích tài chính cá nhân trực quan, dễ hiểu.
- Cần kết nối nhiều hệ thống, dịch vụ khác nhau một cách linh hoạt, đơn giản hóa việc xây dựng và quản lý workflow.
- Thiếu khả năng dự báo các khoản chi/thu định kỳ (subscription YouTube, Google One, tiền điện, nước...), dẫn đến khó khăn trong việc lập kế hoạch và chuẩn bị tiền cho từng tháng trong năm. → ĐÃ GIẢI QUYẾT với tính năng Recurring Transactions.

## Đối tượng người dùng mục tiêu
- Cá nhân, hộ gia đình, nhóm nhỏ có nhu cầu quản lý tài chính cá nhân/chung.
- Người dùng không chuyên về công nghệ, mong muốn thao tác đơn giản, giao diện trực quan.
- Người dùng có nhu cầu tổng hợp, phân tích, lập kế hoạch tài chính cá nhân.
- Người dùng cần dự báo dòng tiền chi/thu trong tương lai để chủ động trong việc quản lý tài chính. → ĐƯỢC HỖ TRỢ bởi ExpectedTransactionService với cash flow forecast.

## Mục tiêu trải nghiệm người dùng
- Giao diện trực quan, dễ thao tác, tối ưu cho cả desktop và mobile.
- Người dùng không cần nhiều kiến thức lập trình vẫn có thể sử dụng.
- Thao tác quản lý tài khoản, giao dịch, ngân sách, mục tiêu, báo cáo nhanh chóng, không quá 3 bước cho tác vụ chính.
- Cung cấp cái nhìn tổng quan và chi tiết về các khoản thu chi định kỳ, hỗ trợ người dùng chuẩn bị tài chính cho từng tháng. → ĐÃ TRIỂN KHAI với RecurringTransactionTemplate và ExpectedTransaction.
- Giảm thiểu tình trạng thiếu hụt tiền mặt bất ngờ do không dự báo được các khoản chi định kỳ. → ĐÃ GIẢI QUYẾT với tính năng dự báo dòng tiền.

## Cách hệ thống nên hoạt động
- Cho phép kéo thả các node để xây dựng workflow tự động hóa.
- Hỗ trợ cấu hình, giám sát và quản lý workflow dễ dàng.
- Cung cấp báo cáo, biểu đồ, thông báo, nhắc nhở theo thời gian thực.
- Chuẩn hóa sử dụng FluentAssertions cho assert kết quả trong unit test, tuân thủ .NET rule.
- Tự động tạo ra các giao dịch dự kiến dựa trên mẫu giao dịch định kỳ, hỗ trợ người dùng dự báo dòng tiền và chuẩn bị tài chính trước. → ĐÃ TRIỂN KHAI với RecurringTransactionTemplateService và background job.

## Tính năng đã triển khai
- **Quản lý mẫu giao dịch định kỳ (RecurringTransactionTemplate):**
  - Tạo, sửa, xóa mẫu giao dịch với tần suất linh hoạt (daily, weekly, monthly, custom).
  - Quản lý trạng thái active/inactive của mẫu.
  - Tự động tính toán ngày thực hiện tiếp theo.
  - Hỗ trợ cron expression cho lịch trình phức tạp.

- **Quản lý giao dịch dự kiến (ExpectedTransaction):**
  - Tự động sinh giao dịch dự kiến từ mẫu định kỳ.
  - Lifecycle management: Pending → Confirmed/Cancelled/Completed.
  - Điều chỉnh số tiền dự kiến với lưu trữ lý do và số tiền gốc.
  - Liên kết với giao dịch thực tế khi xác nhận.

- **Dự báo dòng tiền:**
  - Tính toán cash flow forecast dựa trên giao dịch dự kiến.
  - Phân tích theo category để hiểu rõ cơ cấu thu chi.
  - Hỗ trợ báo cáo kế hoạch tài chính theo tháng/quý/năm.

## Tính năng cần triển khai tiếp
- **Background job service để tự động sinh giao dịch dự kiến.**
- **API Controllers cho RecurringTransactionTemplate và ExpectedTransaction.**
- **Tích hợp với NotificationService để thông báo giao dịch sắp đến hạn.**
- **Cập nhật ReportingService để kết hợp dữ liệu giao dịch thực tế và dự kiến.**
- **Giao diện người dùng cho quản lý mẫu giao dịch định kỳ.** 