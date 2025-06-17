using CoreFinance.Application.DTOs.Account;
using CoreFinance.Application.Services.Base;
using Shared.Contracts.BaseEfModels;
using Shared.Contracts.DTOs;
using CoreFinance.Domain.Entities;

namespace CoreFinance.Application.Interfaces;

/// <summary>
/// (EN) Represents the service interface for managing accounts.<br/>
/// (VI) Đại diện cho interface dịch vụ quản lý tài khoản.
/// </summary>
public interface
    IAccountService : IBaseService<Account, AccountCreateRequest, AccountUpdateRequest, AccountViewModel, Guid>
{
    /// <summary>
    /// (EN) Gets a paginated list of accounts.<br/>
    /// (VI) Lấy danh sách tài khoản có phân trang.
    /// </summary>
    /// <param name="request">The filter request body.</param>
    /// <returns>A paginated list of accounts.</returns>
    Task<IBasePaging<AccountViewModel>?> GetPagingAsync(IFilterBodyRequest request);

    /// <summary>
    /// (EN) Gets a list of account selections for UI components.<br/>
    /// (VI) Lấy danh sách lựa chọn tài khoản cho các thành phần giao diện người dùng.
    /// </summary>
    /// <returns></returns>
    Task<List<AccountSelectionViewModel>?> GetAccountSelectionAsync();
}