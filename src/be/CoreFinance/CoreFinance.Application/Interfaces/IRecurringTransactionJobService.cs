namespace CoreFinance.Application.Interfaces;

/// <summary>
///     Interface for recurring transaction background job service (EN)<br/>
///     Interface cho dịch vụ job nền giao dịch định kỳ (VI)
/// </summary>
public interface IRecurringTransactionJobService
{
    /// <summary>
    ///     Daily job to execute recurring transactions (EN)<br/>
    ///     Job hàng ngày để thực hiện giao dịch định kỳ (VI)
    /// </summary>
    /// <returns>Number of transactions processed</returns>
    Task<int> ProcessDailyRecurringTransactionsAsync();

    /// <summary>
    ///     Daily job to send notifications for upcoming transactions (EN)<br/>
    ///     Job hàng ngày để gửi thông báo cho giao dịch sắp tới (VI)
    /// </summary>
    /// <returns>Number of notifications sent</returns>
    Task<int> SendUpcomingTransactionNotificationsAsync();

    /// <summary>
    ///     Weekly job to generate analytics and insights (EN)<br/>
    ///     Job hàng tuần để tạo phân tích và insights (VI)
    /// </summary>
    /// <returns>Number of users processed</returns>
    Task<int> GenerateWeeklyAnalyticsAsync();

    /// <summary>
    ///     Clean up old expected transactions and logs (EN)<br/>
    ///     Dọn dẹp giao dịch dự kiến và logs cũ (VI)
    /// </summary>
    /// <returns>Number of records cleaned</returns>
    Task<int> CleanupOldDataAsync();
}