using PlanningInvestment.Application.DTOs.Investment;
using PlanningInvestment.Application.Services.Base;
using PlanningInvestment.Domain.Entities;

namespace PlanningInvestment.Application.Interfaces;

/// <summary>
///     Service interface for managing investment operations. (EN)<br />
///     Giao diện dịch vụ để quản lý các hoạt động đầu tư. (VI)
/// </summary>
public interface IInvestmentService : IBaseService<Investment, CreateInvestmentRequest, UpdateInvestmentRequest, InvestmentViewModel, Guid>
{
    /// <summary>
    ///     Gets all investments for a specific user. (EN)<br />
    ///     Lấy tất cả các khoản đầu tư cho một người dùng cụ thể. (VI)
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the collection of investment view models.</returns>
    Task<IEnumerable<InvestmentViewModel>?> GetUserInvestmentsAsync(Guid userId);

    /// <summary>
    ///     Updates the market price of an investment. (EN)<br />
    ///     Cập nhật giá thị trường của một khoản đầu tư. (VI)
    /// </summary>
    /// <param name="id">The investment identifier.</param>
    /// <param name="request">The market price update request.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the updated investment view model.</returns>
    Task<InvestmentViewModel?> UpdateMarketPriceAsync(Guid id, UpdateMarketPriceRequest request);

    /// <summary>
    ///     Gets portfolio summary for a specific user. (EN)<br />
    ///     Lấy tóm tắt danh mục cho một người dùng cụ thể. (VI)
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the portfolio summary.</returns>
    Task<PortfolioSummaryViewModel?> GetPortfolioSummaryAsync(Guid userId);
}
