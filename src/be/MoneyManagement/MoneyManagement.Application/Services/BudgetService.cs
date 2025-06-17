using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MoneyManagement.Application.DTOs.Budget;
using MoneyManagement.Application.Interfaces;
using MoneyManagement.Domain.Entities;
using MoneyManagement.Domain.Enums;
using MoneyManagement.Domain.UnitOfWorks;

namespace MoneyManagement.Application.Services;

/// <summary>
/// Budget management service implementation (EN)<br/>
/// Triển khai dịch vụ quản lý ngân sách (VI)
/// </summary>
public class BudgetService : IBudgetService
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<BudgetService> _logger;

    public BudgetService(IMapper mapper, IUnitOfWork unitOfWork, ILogger<BudgetService> logger)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<BudgetViewModel> CreateBudgetAsync(CreateBudgetRequest request, Guid userId)
    {
        try
        {
            _logger.LogInformation("Creating new budget for user {UserId}", userId);

            var budget = _mapper.Map<Budget>(request);
            budget.UserId = userId;
            budget.SetDefaultValue(userId.ToString());

            await _unitOfWork.Repository<Budget, Guid>().CreateAsync(budget);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Budget created successfully with ID {BudgetId}", budget.Id);
            return _mapper.Map<BudgetViewModel>(budget);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating budget for user {UserId}", userId);
            throw;
        }
    }

    public async Task<BudgetViewModel> UpdateBudgetAsync(UpdateBudgetRequest request, Guid userId)
    {
        try
        {
            var existingBudget = await _unitOfWork.Repository<Budget, Guid>().GetByIdAsync(request.Id);
            if (existingBudget == null || existingBudget.UserId != userId)
                throw new InvalidOperationException($"Budget with ID {request.Id} not found or access denied");

            var originalSpentAmount = existingBudget.SpentAmount;
            _mapper.Map(request, existingBudget);
            existingBudget.SpentAmount = originalSpentAmount;
            existingBudget.SetValueUpdate(userId.ToString());

            await _unitOfWork.Repository<Budget, Guid>().UpdateAsync(existingBudget);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<BudgetViewModel>(existingBudget);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating budget {BudgetId}", request.Id);
            throw;
        }
    }

    public async Task<bool> DeleteBudgetAsync(Guid budgetId, Guid userId)
    {
        try
        {
            var budget = await _unitOfWork.Repository<Budget, Guid>().GetByIdAsync(budgetId);
            if (budget == null || budget.UserId != userId) return false;

            await _unitOfWork.Repository<Budget, Guid>().DeleteSoftAsync(budget);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting budget {BudgetId}", budgetId);
            throw;
        }
    }

    public async Task<BudgetViewModel?> GetBudgetByIdAsync(Guid budgetId, Guid userId)
    {
        var budget = await _unitOfWork.Repository<Budget, Guid>()
            .GetNoTrackingEntities()
            .Where(b => b.Id == budgetId && b.UserId == userId && string.IsNullOrEmpty(b.Deleted))
            .FirstOrDefaultAsync();

        return budget == null ? null : _mapper.Map<BudgetViewModel>(budget);
    }

    public async Task<List<BudgetViewModel>> GetBudgetsByUserIdAsync(Guid userId)
    {
        var budgets = await _unitOfWork.Repository<Budget, Guid>()
            .GetNoTrackingEntities()
            .Where(b => b.UserId == userId && string.IsNullOrEmpty(b.Deleted))
            .OrderByDescending(b => b.CreateAt)
            .ToListAsync();

        return _mapper.Map<List<BudgetViewModel>>(budgets);
    }

    public async Task<List<BudgetViewModel>> GetActiveBudgetsByUserIdAsync(Guid userId)
    {
        var budgets = await _unitOfWork.Repository<Budget, Guid>()
            .GetNoTrackingEntities()
            .Where(b => b.UserId == userId && b.Status == BudgetStatus.Active && string.IsNullOrEmpty(b.Deleted))
            .OrderByDescending(b => b.CreateAt)
            .ToListAsync();

        return _mapper.Map<List<BudgetViewModel>>(budgets);
    }

    public async Task<List<BudgetViewModel>> GetBudgetsByCategoryAsync(Guid userId, string category)
    {
        var budgets = await _unitOfWork.Repository<Budget, Guid>()
            .GetNoTrackingEntities()
            .Where(b => b.UserId == userId && b.Category == category && string.IsNullOrEmpty(b.Deleted))
            .ToListAsync();

        return _mapper.Map<List<BudgetViewModel>>(budgets);
    }

    public async Task<List<BudgetViewModel>> GetBudgetsByPeriodAsync(Guid userId, BudgetPeriod period)
    {
        var budgets = await _unitOfWork.Repository<Budget, Guid>()
            .GetNoTrackingEntities()
            .Where(b => b.UserId == userId && b.Period == period && string.IsNullOrEmpty(b.Deleted))
            .ToListAsync();

        return _mapper.Map<List<BudgetViewModel>>(budgets);
    }

    public async Task<BudgetViewModel> UpdateBudgetSpentAmountAsync(Guid budgetId, decimal amount, Guid userId)
    {
        var budget = await _unitOfWork.Repository<Budget, Guid>().GetByIdAsync(budgetId);
        if (budget == null || budget.UserId != userId)
            throw new InvalidOperationException($"Budget with ID {budgetId} not found or access denied");

        budget.SpentAmount += amount;
        budget.SetValueUpdate(userId.ToString());

        await _unitOfWork.Repository<Budget, Guid>().UpdateAsync(budget);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<BudgetViewModel>(budget);
    }

    public async Task<List<BudgetViewModel>> GetBudgetsReachedAlertThresholdAsync(Guid userId)
    {
        var budgets = await _unitOfWork.Repository<Budget, Guid>()
            .GetNoTrackingEntities()
            .Where(b => b.UserId == userId && b.Status == BudgetStatus.Active && 
                       b.AlertThreshold.HasValue && b.EnableNotifications && string.IsNullOrEmpty(b.Deleted))
            .ToListAsync();

        var alertBudgets = budgets.Where(b => b.IsAlertThresholdReached).ToList();
        return _mapper.Map<List<BudgetViewModel>>(alertBudgets);
    }

    public async Task<List<BudgetViewModel>> GetOverBudgetBudgetsAsync(Guid userId)
    {
        var budgets = await _unitOfWork.Repository<Budget, Guid>()
            .GetNoTrackingEntities()
            .Where(b => b.UserId == userId && b.Status == BudgetStatus.Active && string.IsNullOrEmpty(b.Deleted))
            .ToListAsync();

        var overBudgets = budgets.Where(b => b.IsOverBudget).ToList();
        return _mapper.Map<List<BudgetViewModel>>(overBudgets);
    }

    public async Task<BudgetViewModel> ChangeBudgetStatusAsync(Guid budgetId, BudgetStatus status, Guid userId)
    {
        var budget = await _unitOfWork.Repository<Budget, Guid>().GetByIdAsync(budgetId);
        if (budget == null || budget.UserId != userId)
            throw new InvalidOperationException($"Budget with ID {budgetId} not found or access denied");

        budget.Status = status;
        budget.SetValueUpdate(userId.ToString());

        await _unitOfWork.Repository<Budget, Guid>().UpdateAsync(budget);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<BudgetViewModel>(budget);
    }
} 