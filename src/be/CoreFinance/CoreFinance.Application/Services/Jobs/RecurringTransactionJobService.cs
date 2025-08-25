using CoreFinance.Application.Interfaces;
using CoreFinance.Domain.Entities;
using CoreFinance.Domain.UnitOfWorks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoreFinance.Application.Services.Jobs;

/// <summary>
///     Background job service for recurring transaction automation (EN)<br/>
///     Dịch vụ job nền cho tự động hóa giao dịch định kỳ (VI)
/// </summary>
public class RecurringTransactionJobService(
    IUnitOfWork unitOfWork,
    IRecurringTransactionTemplateService templateService,
    IExpectedTransactionService expectedTransactionService,
    ILogger<RecurringTransactionJobService> logger) : IRecurringTransactionJobService
{
    /// <summary>
    ///     Daily job to execute recurring transactions (EN)<br/>
    ///     Job hàng ngày để thực hiện giao dịch định kỳ (VI)
    /// </summary>
    /// <returns>Number of transactions processed</returns>
    public async Task<int> ProcessDailyRecurringTransactionsAsync()
    {
        logger.LogInformation("Starting daily recurring transactions processing");
        
        try
        {
            var processedCount = 0;
            var today = DateTime.UtcNow.Date;
            
            // Get all active templates that need execution today or before
            var templatesToProcess = await unitOfWork.Repository<RecurringTransactionTemplate, Guid>()
                .GetNoTrackingEntities()
                .Where(t => t.IsActive && 
                           t.AutoGenerate && 
                           t.NextExecutionDate.Date <= today &&
                           (!t.EndDate.HasValue || t.EndDate.Value.Date >= today))
                .ToListAsync();

            logger.LogInformation("Found {Count} templates to process", templatesToProcess.Count);

            foreach (var template in templatesToProcess)
            {
                try
                {
                    // Generate expected transactions for this template
                    await templateService.GenerateExpectedTransactionsAsync(template.Id, template.DaysInAdvance);
                    processedCount++;
                    
                    logger.LogDebug("Processed template {TemplateId} - {TemplateName}", 
                        template.Id, template.Name);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error processing template {TemplateId} - {TemplateName}", 
                        template.Id, template.Name);
                }
            }
            
            logger.LogInformation("Daily recurring transactions processing completed. Processed {Count} templates", 
                processedCount);
                
            return processedCount;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in daily recurring transactions processing");
            throw;
        }
    }

    /// <summary>
    ///     Daily job to send notifications for upcoming transactions (EN)<br/>
    ///     Job hàng ngày để gửi thông báo cho giao dịch sắp tới (VI)
    /// </summary>
    /// <returns>Number of notifications sent</returns>
    public async Task<int> SendUpcomingTransactionNotificationsAsync()
    {
        logger.LogInformation("Starting upcoming transaction notifications job");
        
        try
        {
            var notificationsSent = 0;
            var notificationDays = new[] { 1, 3, 7 }; // 1, 3, 7 days in advance
            
            foreach (var days in notificationDays)
            {
                var targetDate = DateTime.UtcNow.AddDays(days).Date;
                
                // Get expected transactions for this date
                var upcomingTransactions = await unitOfWork.Repository<ExpectedTransaction, Guid>()
                    .GetNoTrackingEntities()
                    .Include(et => et.RecurringTransactionTemplate)
                    .Where(et => et.ExpectedDate.Date == targetDate &&
                                et.Status == Domain.Enums.ExpectedTransactionStatus.Pending)
                    .GroupBy(et => et.UserId)
                    .ToListAsync();

                foreach (var userTransactions in upcomingTransactions)
                {
                    try
                    {
                        // Here you would send actual notifications (email, push, etc.)
                        // For now, we'll just log
                        var userId = userTransactions.Key;
                        var transactionCount = userTransactions.Count();
                        var totalAmount = userTransactions.Sum(et => et.ExpectedAmount);
                        
                        logger.LogInformation(
                            "Notification: User {UserId} has {Count} transactions totaling {Amount:C} due in {Days} days",
                            userId, transactionCount, totalAmount, days);
                            
                        notificationsSent++;
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error sending notification for user {UserId}", userTransactions.Key);
                    }
                }
            }
            
            logger.LogInformation("Upcoming transaction notifications completed. Sent {Count} notifications", 
                notificationsSent);
                
            return notificationsSent;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in upcoming transaction notifications job");
            throw;
        }
    }

    /// <summary>
    ///     Weekly job to generate analytics and insights (EN)<br/>
    ///     Job hàng tuần để tạo phân tích và insights (VI)
    /// </summary>
    /// <returns>Number of users processed</returns>
    public async Task<int> GenerateWeeklyAnalyticsAsync()
    {
        logger.LogInformation("Starting weekly analytics generation");
        
        try
        {
            var processedUsers = 0;
            var endDate = DateTime.UtcNow.Date;
            var startDate = endDate.AddDays(-30); // Last 30 days
            
            // Get all users with recurring transactions
            var usersWithTemplates = await unitOfWork.Repository<RecurringTransactionTemplate, Guid>()
                .GetNoTrackingEntities()
                .Where(t => t.IsActive)
                .Select(t => t.UserId)
                .Distinct()
                .ToListAsync();

            foreach (var userId in usersWithTemplates)
            {
                try
                {
                    // Generate analytics for this user
                    // For now, just log that analytics would be generated
                    logger.LogDebug("Generated analytics for user {UserId}", userId);
                    
                    processedUsers++;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error generating analytics for user {UserId}", userId);
                }
            }
            
            logger.LogInformation("Weekly analytics generation completed. Processed {Count} users", processedUsers);
            return processedUsers;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in weekly analytics generation");
            throw;
        }
    }

    /// <summary>
    ///     Clean up old expected transactions and logs (EN)<br/>
    ///     Dọn dẹp giao dịch dự kiến và logs cũ (VI)
    /// </summary>
    /// <returns>Number of records cleaned</returns>
    public async Task<int> CleanupOldDataAsync()
    {
        logger.LogInformation("Starting old data cleanup");
        
        try
        {
            var cleanupCount = 0;
            var cutoffDate = DateTime.UtcNow.AddDays(-90); // Keep last 90 days
            
            await using var transaction = await unitOfWork.BeginTransactionAsync();
            
            // Clean up old completed/cancelled expected transactions
            // For now, just log that cleanup would happen
            // In a real implementation, you would query and delete old records
            logger.LogInformation("Cleanup would remove records older than {CutoffDate}", cutoffDate);
            cleanupCount = 0; // Placeholder

            await unitOfWork.SaveChangesAsync();
            await transaction.CommitAsync();
            
            logger.LogInformation("Old data cleanup completed. Removed {Count} records", cleanupCount);
            return cleanupCount;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in old data cleanup");
            throw;
        }
    }
}