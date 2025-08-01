using Shared.EntityFramework.DTOs;
using CoreFinance.Domain.Enums;

namespace CoreFinance.Application.DTOs.RecurringTransactionTemplate;

/// <summary>
///     Represents a view model for recurring transaction template data. (EN)<br />
///     Đại diện cho view model dữ liệu mẫu giao dịch định kỳ. (VI)
/// </summary>
public class RecurringTransactionTemplateViewModel : BaseViewModel<Guid>
{
    /// <summary>
    ///     The ID of the user who owns the template (optional). (EN)<br />
    ///     ID của người dùng sở hữu mẫu (tùy chọn). (VI)
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    ///     The ID of the account associated with the template. (EN)<br />
    ///     ID của tài khoản liên quan đến mẫu. (VI)
    /// </summary>
    public Guid AccountId { get; set; }

    /// <summary>
    ///     The name of the template. (EN)<br />
    ///     Tên của mẫu. (VI)
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    ///     A description of the template (optional). (EN)<br />
    ///     Mô tả về mẫu (tùy chọn). (VI)
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    ///     The amount of the recurring transaction. (EN)<br />
    ///     Số tiền của giao dịch định kỳ. (VI)
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    ///     The type of recurring transaction (e.g., income, expense). (EN)<br />
    ///     Loại giao dịch định kỳ (ví dụ: thu nhập, chi tiêu). (VI)
    /// </summary>
    public RecurringTransactionType TransactionType { get; set; }

    /// <summary>
    ///     The category of the recurring transaction (optional). (EN)<br />
    ///     Danh mục của giao dịch định kỳ (tùy chọn). (VI)
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    ///     The frequency of the recurrence (e.g., daily, weekly, monthly). (EN)<br />
    ///     Tần suất lặp lại (ví dụ: hàng ngày, hàng tuần, hàng tháng). (VI)
    /// </summary>
    public RecurrenceFrequency Frequency { get; set; }

    /// <summary>
    ///     The custom interval in days for recurring transactions with custom frequency (optional). (EN)<br />
    ///     Khoảng thời gian tùy chỉnh theo ngày cho giao dịch định kỳ với tần suất tùy chỉnh (tùy chọn). (VI)
    /// </summary>
    public int? CustomIntervalDays { get; set; }

    /// <summary>
    ///     The next scheduled execution date for the recurring transaction. (EN)<br />
    ///     Ngày thực hiện tiếp theo đã lên lịch cho giao dịch định kỳ. (VI)
    /// </summary>
    public DateTime NextExecutionDate { get; set; }

    /// <summary>
    ///     The start date for the recurring transaction. (EN)<br />
    ///     Ngày bắt đầu cho giao dịch định kỳ. (VI)
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    ///     The end date for the recurring transaction (optional). (EN)<br />
    ///     Ngày kết thúc cho giao dịch định kỳ (tùy chọn). (VI)
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    ///     The Cron expression for complex recurrence patterns (optional). (EN)<br />
    ///     Cron expression cho các mẫu lặp lại phức tạp (tùy chọn). (VI)
    /// </summary>
    public string? CronExpression { get; set; }

    /// <summary>
    ///     Indicates if the template is currently active. (EN)<br />
    ///     Cho biết mẫu hiện đang hoạt động hay không. (VI)
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    ///     Indicates if expected transactions should be automatically generated from this template. (EN)<br />
    ///     Cho biết giao dịch dự kiến có nên được tự động sinh ra từ mẫu này hay không. (VI)
    /// </summary>
    public bool AutoGenerate { get; set; }

    /// <summary>
    ///     The number of days in advance to generate expected transactions for. (EN)<br />
    ///     Số ngày tạo giao dịch dự kiến trước. (VI)
    /// </summary>
    public int DaysInAdvance { get; set; }

    /// <summary>
    ///     Additional notes about the template (optional). (EN)<br />
    ///     Ghi chú bổ sung về mẫu (tùy chọn). (VI)
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    ///     The creation date and time of the template. (EN)<br />
    ///     Ngày và giờ tạo mẫu. (VI)
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    ///     The last update date and time of the template. (EN)<br />
    ///     Ngày và giờ cập nhật cuối cùng của mẫu. (VI)
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    // Navigation properties

    /// <summary>
    ///     The name of the associated account. (EN)<br />
    ///     Tên của tài khoản liên kết. (VI)
    /// </summary>
    public string? AccountName { get; set; }

    /// <summary>
    ///     The type of the associated account. (EN)<br />
    ///     Loại của tài khoản liên kết. (VI)
    /// </summary>
    public AccountType? AccountType { get; set; }

    /// <summary>
    ///     The number of expected transactions generated from this template. (EN)<br />
    ///     Số lượng giao dịch dự kiến được sinh ra từ mẫu này. (VI)
    /// </summary>
    public int ExpectedTransactionCount { get; set; }
}