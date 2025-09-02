using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PlanningInvestment.Application.DTOs.Investment;
using PlanningInvestment.Application.Interfaces;
using PlanningInvestment.Application.Services.Base;
using PlanningInvestment.Domain.Entities;
using PlanningInvestment.Domain.Enums;
using PlanningInvestment.Domain.UnitOfWorks;

namespace PlanningInvestment.Application.Services;

/// <summary>
///     Service for managing investment operations. (EN)<br />
///     Dịch vụ để quản lý các hoạt động đầu tư. (VI)
/// </summary>
public class InvestmentService : BaseService<Investment, CreateInvestmentRequest, UpdateInvestmentRequest, InvestmentViewModel, Guid>, IInvestmentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<InvestmentService> _logger;

    /// <summary>
    ///     Initializes a new instance of the InvestmentService class. (EN)<br />
    ///     Khởi tạo một phiên bản mới của lớp InvestmentService. (VI)
    /// </summary>
    /// <param name="mapper">The AutoMapper instance.</param>
    /// <param name="unitOfWork">The unit of work instance.</param>
    /// <param name="logger">The logger instance.</param>
    public InvestmentService(IMapper mapper, IUnitOfWork unitOfWork, ILogger<InvestmentService> logger) 
        : base(mapper, unitOfWork, logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    ///     Gets all investments for a specific user. (EN)<br />
    ///     Lấy tất cả các khoản đầu tư cho một người dùng cụ thể. (VI)
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the collection of investment view models.</returns>
    public async Task<IEnumerable<InvestmentViewModel>?> GetUserInvestmentsAsync(Guid userId)
    {
        try
        {
            _logger.LogTrace("{GetUserInvestmentsAsync} request: {userId}", nameof(GetUserInvestmentsAsync), userId);

            var investments = await _unitOfWork.Repository<Investment, Guid>()
                .GetAllAsync();

            // Filter by userId in service layer since repository filters by current user automatically
            var userInvestments = investments.Where(x => x.UserId == userId).ToList();

            if (userInvestments == null || !userInvestments.Any())
            {
                _logger.LogInformation("No investments found for user {userId}", userId);
                return Enumerable.Empty<InvestmentViewModel>();
            }

            var investmentViewModels = userInvestments.Select(MapToViewModel).ToList();
            
            _logger.LogTrace("{GetUserInvestmentsAsync} response: {count} investments found", nameof(GetUserInvestmentsAsync), investmentViewModels.Count);
            return investmentViewModels;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting investments for user {userId}", userId);
            throw;
        }
    }

    /// <summary>
    ///     Updates the market price of an investment. (EN)<br />
    ///     Cập nhật giá thị trường của một khoản đầu tư. (VI)
    /// </summary>
    /// <param name="id">The investment identifier.</param>
    /// <param name="request">The market price update request.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the updated investment view model.</returns>
    public async Task<InvestmentViewModel?> UpdateMarketPriceAsync(Guid id, UpdateMarketPriceRequest request)
    {
        try
        {
            _logger.LogTrace("{UpdateMarketPriceAsync} request: {id}, price: {price}", 
                nameof(UpdateMarketPriceAsync), id, request.CurrentMarketPrice);

            var investment = await _unitOfWork.Repository<Investment, Guid>().GetByIdAsync(id);
            if (investment == null)
            {
                _logger.LogWarning("Investment with id {id} not found", id);
                return null;
            }

            investment.CurrentMarketPrice = request.CurrentMarketPrice;
            investment.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Repository<Investment, Guid>().UpdateAsync(investment);
            await _unitOfWork.SaveChangesAsync();

            var result = MapToViewModel(investment);
            _logger.LogTrace("{UpdateMarketPriceAsync} response: Investment {id} market price updated to {price}", 
                nameof(UpdateMarketPriceAsync), id, request.CurrentMarketPrice);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating market price for investment {id}", id);
            throw;
        }
    }

    /// <summary>
    ///     Gets portfolio summary for a specific user. (EN)<br />
    ///     Lấy tóm tắt danh mục cho một người dùng cụ thể. (VI)
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the portfolio summary.</returns>
    public async Task<PortfolioSummaryViewModel?> GetPortfolioSummaryAsync(Guid userId)
    {
        try
        {
            _logger.LogTrace("{GetPortfolioSummaryAsync} request: {userId}", nameof(GetPortfolioSummaryAsync), userId);

            var investments = await _unitOfWork.Repository<Investment, Guid>()
                .GetAllAsync();

            // Filter by userId in service layer since repository filters by current user automatically
            var userInvestments = investments.Where(x => x.UserId == userId).ToList();

            if (userInvestments == null || !userInvestments.Any())
            {
                _logger.LogInformation("No investments found for user {userId}", userId);
                return new PortfolioSummaryViewModel
                {
                    TotalInvestedAmount = 0,
                    CurrentTotalValue = 0,
                    TotalProfitLoss = 0,
                    TotalProfitLossPercentage = 0,
                    InvestmentCount = 0,
                    InvestmentTypeBreakdown = new List<InvestmentTypeBreakdown>(),
                    LastUpdated = DateTime.UtcNow
                };
            }

            var totalInvestedAmount = userInvestments.Sum(x => x.TotalInvestedAmount);
            var currentTotalValue = userInvestments.Where(x => x.CurrentMarketPrice.HasValue)
                .Sum(x => x.CurrentTotalValue);
            var totalProfitLoss = currentTotalValue.HasValue ? currentTotalValue.Value - totalInvestedAmount : (decimal?)null;
            var totalProfitLossPercentage = totalProfitLoss.HasValue && totalInvestedAmount > 0 
                ? (totalProfitLoss.Value / totalInvestedAmount) * 100 
                : (decimal?)null;

            var typeBreakdown = userInvestments
                .GroupBy(x => x.InvestmentType)
                .Select(g => new InvestmentTypeBreakdown
                {
                    InvestmentType = g.Key,
                    Count = g.Count(),
                    TotalInvestedAmount = g.Sum(x => x.TotalInvestedAmount),
                    CurrentTotalValue = g.Where(x => x.CurrentMarketPrice.HasValue).Sum(x => x.CurrentTotalValue),
                    ProfitLoss = g.Where(x => x.CurrentMarketPrice.HasValue).Sum(x => x.ProfitLoss)
                })
                .ToList();

            var result = new PortfolioSummaryViewModel
            {
                TotalInvestedAmount = totalInvestedAmount,
                CurrentTotalValue = currentTotalValue,
                TotalProfitLoss = totalProfitLoss,
                TotalProfitLossPercentage = totalProfitLossPercentage,
                InvestmentCount = userInvestments.Count(),
                InvestmentTypeBreakdown = typeBreakdown,
                LastUpdated = DateTime.UtcNow
            };

            _logger.LogTrace("{GetPortfolioSummaryAsync} response: Portfolio summary calculated for {count} investments", 
                nameof(GetPortfolioSummaryAsync), userInvestments.Count());
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while calculating portfolio summary for user {userId}", userId);
            throw;
        }
    }

    /// <summary>
    ///     Maps an Investment entity to InvestmentViewModel. (EN)<br />
    ///     Ánh xạ thực thể Investment sang InvestmentViewModel. (VI)
    /// </summary>
    /// <param name="investment">The investment entity.</param>
    /// <returns>The investment view model.</returns>
    private InvestmentViewModel MapToViewModel(Investment investment)
    {
        return new InvestmentViewModel
        {
            Id = investment.Id,
            UserId = investment.UserId,
            Symbol = investment.Symbol,
            InvestmentType = investment.InvestmentType,
            PurchasePrice = investment.PurchasePrice,
            Quantity = investment.Quantity,
            CurrentMarketPrice = investment.CurrentMarketPrice,
            PurchaseDate = investment.PurchaseDate,
            Notes = investment.Notes,
            TotalInvestedAmount = investment.TotalInvestedAmount,
            CurrentTotalValue = investment.CurrentTotalValue,
            ProfitLoss = investment.ProfitLoss,
            ProfitLossPercentage = investment.ProfitLossPercentage,
            IsProfitable = investment.IsProfitable,
            CreatedAt = investment.CreatedAt,
            UpdatedAt = investment.UpdatedAt,
            CreateBy = investment.CreateBy,
            UpdateBy = investment.UpdateBy
        };
    }
}
