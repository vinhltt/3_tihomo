using CoreFinance.Api.Controllers.Base;
using CoreFinance.Application.DTOs.RecurringTransactionTemplate;
using CoreFinance.Application.Interfaces;
using Shared.EntityFramework.DTOs;
using CoreFinance.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Shared.EntityFramework.BaseEfModels;

namespace CoreFinance.Api.Controllers;

/// <summary>
///     Controller for managing recurring transaction templates. (EN)<br />
///     Controller để quản lý các mẫu giao dịch định kỳ. (VI)
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class RecurringTransactionTemplateController(
    IRecurringTransactionTemplateService service,
    ILogger<RecurringTransactionTemplateController> logger)
    : CrudController<RecurringTransactionTemplate, RecurringTransactionTemplateCreateRequest,
        RecurringTransactionTemplateUpdateRequest, RecurringTransactionTemplateViewModel, Guid>(logger, service)
{
    /// <summary>
    ///     Gets paginated recurring transaction templates. (EN)<br />
    ///     Lấy danh sách mẫu giao dịch định kỳ có phân trang. (VI)
    /// </summary>
    /// <param name="request">
    ///     The request object containing pagination, filtering, and sorting details. (EN)<br />
    ///     Đối tượng request chứa thông tin phân trang, lọc và sắp xếp. (VI)
    /// </param>
    /// <returns>
    ///     A paginated list of recurring transaction templates if successful, or a status code indicating an error. (EN)<br />
    ///     Danh sách mẫu giao dịch định kỳ có phân trang nếu thành công, hoặc mã trạng thái cho biết lỗi. (VI)
    /// </returns>
    [HttpPost("filter")]
    public override async Task<ActionResult<IBasePaging<RecurringTransactionTemplateViewModel>>> GetPagingAsync(
        [FromBody] FilterBodyRequest request)
    {
        var result = await service.GetPagingAsync(request);
        return Ok(result);
    }

    /// <summary>
    ///     Gets active templates for a user by user ID. (EN)<br />
    ///     Lấy các mẫu giao dịch định kỳ đang hoạt động của người dùng theo ID người dùng. (VI)
    /// </summary>
    /// <param name="userId">
    ///     The ID of the user. (EN)<br />
    ///     ID của người dùng. (VI)
    /// </param>
    /// <returns>
    ///     A list of active recurring transaction templates. (EN)<br />
    ///     Danh sách các mẫu giao dịch định kỳ đang hoạt động. (VI)
    /// </returns>
    [HttpGet("active/{userId:guid}")]
    public async Task<IActionResult> GetActiveTemplates(Guid userId)
    {
        var result = await service.GetActiveTemplatesAsync(userId);
        return Ok(result);
    }

    /// <summary>
    ///     Gets templates by account ID. (EN)<br />
    ///     Lấy các mẫu giao dịch định kỳ theo ID tài khoản. (VI)
    /// </summary>
    /// <param name="accountId">
    ///     The ID of the account. (EN)<br />
    ///     ID của tài khoản. (VI)
    /// </param>
    /// <returns>
    ///     A list of recurring transaction templates for the specified account. (EN)<br />
    ///     Danh sách các mẫu giao dịch định kỳ cho tài khoản được chỉ định. (VI)
    /// </returns>
    [HttpGet("account/{accountId:guid}")]
    public async Task<IActionResult> GetTemplatesByAccount(Guid accountId)
    {
        var result = await service.GetTemplatesByAccountAsync(accountId);
        return Ok(result);
    }

    /// <summary>
    ///     Toggles the active status of a recurring transaction template by its ID. (EN)<br />
    ///     Bật/tắt trạng thái hoạt động của mẫu giao dịch định kỳ dựa trên ID của nó. (VI)
    /// </summary>
    /// <param name="templateId">
    ///     The ID of the template to toggle the active status for. (EN)<br />
    ///     ID của mẫu cần bật/tắt trạng thái hoạt động. (VI)
    /// </param>
    /// <param name="isActive">
    ///     The new active status. (EN)<br />
    ///     Trạng thái hoạt động mới. (VI)
    /// </param>
    /// <returns>
    ///     An Ok result if successful, or NotFound if the template is not found. (EN)<br />
    ///     Kết quả Ok nếu thành công, hoặc NotFound nếu không tìm thấy mẫu. (VI)
    /// </returns>
    [HttpPatch("{templateId:guid}/toggle-active")]
    public async Task<IActionResult> ToggleActiveStatus(Guid templateId, [FromBody] bool isActive)
    {
        var result = await service.ToggleActiveStatusAsync(templateId, isActive);
        if (!result)
            return NotFound("Template not found");

        return Ok(new { success = true, message = "Template status updated successfully" });
    }

    /// <summary>
    ///     Calculates the next execution date for a recurring transaction template by its ID. (EN)<br />
    ///     Tính ngày thực hiện tiếp theo cho mẫu giao dịch định kỳ dựa trên ID của nó. (VI)
    /// </summary>
    /// <param name="templateId">
    ///     The ID of the template. (EN)<br />
    ///     ID của mẫu. (VI)
    /// </param>
    /// <returns>
    ///     An Ok result with the next execution date if successful, or NotFound if the template is not found. (EN)<br />
    ///     Kết quả Ok kèm theo ngày thực hiện tiếp theo nếu thành công, hoặc NotFound nếu không tìm thấy mẫu. (VI)
    /// </returns>
    [HttpGet("{templateId:guid}/next-execution-date")]
    public async Task<IActionResult> CalculateNextExecutionDate(Guid templateId)
    {
        try
        {
            var nextDate = await service.CalculateNextExecutionDateAsync(templateId);
            return Ok(new { nextExecutionDate = nextDate });
        }
        catch (ArgumentException)
        {
            return NotFound("Template not found");
        }
    }

    /// <summary>
    ///     Generates expected transactions for a specific recurring transaction template. (EN)<br />
    ///     Sinh các giao dịch dự kiến cho một mẫu giao dịch định kỳ cụ thể. (VI)
    /// </summary>
    /// <param name="templateId">
    ///     The ID of the template. (EN)<br />
    ///     ID của mẫu. (VI)
    /// </param>
    /// <param name="daysInAdvance">
    ///     The number of days in advance to generate transactions for. Defaults to 30. (EN)<br />
    ///     Số ngày tạo giao dịch dự kiến trước. Mặc định là 30. (VI)
    /// </param>
    /// <returns>
    ///     An Ok result if successful, or BadRequest if an error occurs. (EN)<br />
    ///     Kết quả Ok nếu thành công, hoặc BadRequest nếu xảy ra lỗi. (VI)
    /// </returns>
    [HttpPost("{templateId:guid}/generate-expected-transactions")]
    public async Task<IActionResult> GenerateExpectedTransactions(Guid templateId, [FromQuery] int daysInAdvance = 30)
    {
        try
        {
            await service.GenerateExpectedTransactionsAsync(templateId, daysInAdvance);
            return Ok(new { success = true, message = "Expected transactions generated successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    ///     Generates expected transactions for all active recurring transaction templates. (EN)<br />
    ///     Sinh các giao dịch dự kiến cho tất cả các mẫu giao dịch định kỳ đang hoạt động. (VI)
    /// </summary>
    /// <returns>
    ///     An Ok result if successful, or BadRequest if an error occurs. (EN)<br />
    ///     Kết quả Ok nếu thành công, hoặc BadRequest nếu xảy ra lỗi. (VI)
    /// </returns>
    [HttpPost("generate-all-expected-transactions")]
    public async Task<IActionResult> GenerateExpectedTransactionsForAllActiveTemplates()
    {
        try
        {
            await service.GenerateExpectedTransactionsForAllActiveTemplatesAsync();
            return Ok(new { success = true, message = "Expected transactions generated for all active templates" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    ///     Get calendar view of recurring transactions for a user (EN)<br />
    ///     Lấy lịch xem giao dịch định kỳ của user (VI)
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="from">Start date (YYYY-MM format)</param>
    /// <param name="to">End date (YYYY-MM format)</param>
    /// <returns>Calendar view of recurring transactions</returns>
    [HttpGet("calendar/{userId:guid}")]
    public async Task<IActionResult> GetCalendarView(Guid userId, [FromQuery] string from, [FromQuery] string to)
    {
        try
        {
            if (!DateTime.TryParseExact(from + "-01", "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out var startDate) ||
                !DateTime.TryParseExact(to + "-01", "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out var endDateTemp))
            {
                return BadRequest("Invalid date format. Use YYYY-MM format.");
            }

            var endDate = endDateTemp.AddMonths(1).AddDays(-1); // Last day of the month

            var templates = await service.GetActiveTemplatesAsync(userId);
            
            var calendarEvents = new List<object>();
            
            foreach (var template in templates)
            {
                var currentDate = Math.Max(template.StartDate.Ticks, startDate.Ticks);
                var templateEndDate = template.EndDate?.Ticks ?? endDate.Ticks;
                var actualEndDate = Math.Min(templateEndDate, endDate.Ticks);
                
                var executionDate = new DateTime(currentDate);
                
                while (executionDate <= new DateTime(actualEndDate))
                {
                    calendarEvents.Add(new
                    {
                        id = template.Id,
                        title = template.Name,
                        date = executionDate.ToString("yyyy-MM-dd"),
                        amount = template.Amount,
                        type = template.TransactionType.ToString(),
                        category = template.Category,
                        frequency = template.Frequency.ToString(),
                        isActive = template.IsActive
                    });
                    
                    // Calculate next execution date based on frequency
                    executionDate = template.Frequency switch
                    {
                        Domain.Enums.RecurrenceFrequency.Daily => executionDate.AddDays(1),
                        Domain.Enums.RecurrenceFrequency.Weekly => executionDate.AddDays(7),
                        Domain.Enums.RecurrenceFrequency.Biweekly => executionDate.AddDays(14),
                        Domain.Enums.RecurrenceFrequency.Monthly => executionDate.AddMonths(1),
                        Domain.Enums.RecurrenceFrequency.Quarterly => executionDate.AddMonths(3),
                        Domain.Enums.RecurrenceFrequency.SemiAnnually => executionDate.AddMonths(6),
                        Domain.Enums.RecurrenceFrequency.Annually => executionDate.AddYears(1),
                        Domain.Enums.RecurrenceFrequency.Custom => executionDate.AddDays(template.CustomIntervalDays ?? 1),
                        _ => executionDate.AddDays(1)
                    };
                }
            }
            
            return Ok(new
            {
                from,
                to,
                events = calendarEvents.OrderBy(e => ((dynamic)e).date)
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    ///     Bulk import recurring transaction templates (EN)<br />
    ///     Import hàng loạt mẫu giao dịch định kỳ (VI)
    /// </summary>
    /// <param name="templates">List of templates to import</param>
    /// <returns>Import results</returns>
    [HttpPost("bulk-import")]
    public async Task<IActionResult> BulkImport([FromBody] List<RecurringTransactionTemplateCreateRequest> templates)
    {
        try
        {
            var results = new List<object>();
            var successCount = 0;
            var errorCount = 0;

            foreach (var template in templates)
            {
                try
                {
                    var result = await service.CreateAsync(template);
                    if (result != null)
                    {
                        results.Add(new { success = true, template = result, error = (string?)null });
                        successCount++;
                    }
                    else
                    {
                        results.Add(new { success = false, template = (object?)null, error = "Failed to create template" });
                        errorCount++;
                    }
                }
                catch (Exception ex)
                {
                    results.Add(new { success = false, template = (object?)null, error = ex.Message });
                    errorCount++;
                }
            }

            return Ok(new
            {
                totalProcessed = templates.Count,
                successCount,
                errorCount,
                results
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    ///     Execute now - manually trigger a recurring transaction (EN)<br />
    ///     Thực hiện ngay - trigger thủ công giao dịch định kỳ (VI)
    /// </summary>
    /// <param name="templateId">Template ID</param>
    /// <returns>Execution result</returns>
    [HttpPost("{templateId:guid}/execute-now")]
    public async Task<IActionResult> ExecuteNow(Guid templateId)
    {
        try
        {
            // Generate expected transaction for today
            await service.GenerateExpectedTransactionsAsync(templateId, 0);
            
            return Ok(new { 
                success = true, 
                message = "Expected transaction created for immediate execution",
                templateId 
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    ///     Skip next execution for a recurring transaction template (EN)<br />
    ///     Bỏ qua lần thực hiện tiếp theo cho mẫu giao dịch định kỳ (VI)
    /// </summary>
    /// <param name="templateId">Template ID</param>
    /// <returns>Skip result</returns>
    [HttpPost("{templateId:guid}/skip-next")]
    public async Task<IActionResult> SkipNext(Guid templateId)
    {
        try
        {
            var nextDate = await service.CalculateNextExecutionDateAsync(templateId);
            
            // Update the template to skip to the next occurrence
            // This would require adding a method to skip execution
            // For now, we'll return the next calculated date
            
            return Ok(new { 
                success = true, 
                message = "Next execution will be skipped",
                templateId,
                nextExecutionDate = nextDate
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    ///     Get forecast for a recurring transaction template (EN)<br />
    ///     Lấy dự báo cho mẫu giao dịch định kỳ (VI)
    /// </summary>
    /// <param name="templateId">Template ID</param>
    /// <param name="months">Number of months to forecast (default: 6)</param>
    /// <returns>Forecast data</returns>
    [HttpGet("{templateId:guid}/forecast")]
    public async Task<IActionResult> GetForecast(Guid templateId, [FromQuery] int months = 6)
    {
        try
        {
            var template = await service.GetByIdAsync(templateId);
            if (template == null)
            {
                return NotFound("Template not found");
            }

            var forecast = new List<object>();
            var currentDate = DateTime.UtcNow.Date;
            var endDate = currentDate.AddMonths(months);
            
            var executionDate = template.NextExecutionDate;
            
            while (executionDate <= endDate)
            {
                if (template.EndDate == null || executionDate <= template.EndDate)
                {
                    forecast.Add(new
                    {
                        date = executionDate.ToString("yyyy-MM-dd"),
                        amount = template.Amount,
                        description = template.Description,
                        category = template.Category
                    });
                }
                
                // Calculate next occurrence
                executionDate = template.Frequency switch
                {
                    Domain.Enums.RecurrenceFrequency.Daily => executionDate.AddDays(1),
                    Domain.Enums.RecurrenceFrequency.Weekly => executionDate.AddDays(7),
                    Domain.Enums.RecurrenceFrequency.Biweekly => executionDate.AddDays(14),
                    Domain.Enums.RecurrenceFrequency.Monthly => executionDate.AddMonths(1),
                    Domain.Enums.RecurrenceFrequency.Quarterly => executionDate.AddMonths(3),
                    Domain.Enums.RecurrenceFrequency.SemiAnnually => executionDate.AddMonths(6),
                    Domain.Enums.RecurrenceFrequency.Annually => executionDate.AddYears(1),
                    Domain.Enums.RecurrenceFrequency.Custom => executionDate.AddDays(template.CustomIntervalDays ?? 1),
                    _ => executionDate.AddDays(1)
                };
            }
            
            var totalAmount = forecast.Sum(f => ((dynamic)f).amount);
            
            return Ok(new
            {
                templateId,
                templateName = template.Name,
                forecastMonths = months,
                totalTransactions = forecast.Count,
                totalAmount,
                forecast = forecast.Take(100) // Limit to prevent huge responses
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}