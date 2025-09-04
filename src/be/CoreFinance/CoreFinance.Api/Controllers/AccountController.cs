using CoreFinance.Api.Controllers.Base;
using CoreFinance.Application.DTOs.Account;
using CoreFinance.Application.Interfaces;
using CoreFinance.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Shared.EntityFramework.BaseEfModels;
using Shared.EntityFramework.DTOs;

namespace CoreFinance.Api.Controllers;

/// <summary>
///     Controller for managing accounts. (EN)<br />
///     Controller để quản lý các tài khoản. (VI)
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AccountController(
    ILogger<AccountController> logger,
    IAccountService accountService)
    : CrudController<Account, AccountCreateRequest,
        AccountUpdateRequest, AccountViewModel, Guid>(logger,
        accountService)
{
    /// <summary>
    ///     Gets a paginated list of accounts based on a filter request. (EN)<br />
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("filter")]
    public override async Task<ActionResult<IBasePaging<AccountViewModel>>> GetPagingAsync(FilterBodyRequest request)
    {
        var result = await accountService.GetPagingAsync(request);
        return Ok(result);
    }

    /// <summary>
    ///     (EN) Gets a paginated list of accounts with query parameters.<br />
    ///     (VI) Lấy danh sách tài khoản có phân trang với các tham số query.
    /// </summary>
    /// <param name="pageIndex">Page index (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20)</param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ActionResult<IBasePaging<AccountViewModel>>> GetAccountsAsync(
        [FromQuery] int pageIndex = 1, 
        [FromQuery] int pageSize = 20)
    {
        var request = new FilterBodyRequest
        {
            Pagination = new Pagination
            {
                PageIndex = pageIndex,
                PageSize = pageSize
            }
        };
        var result = await accountService.GetPagingAsync(request);
        return Ok(result);
    }

    /// <summary>
    ///     (EN) Gets a list of account selections for UI components.<br />
    ///     (VI) Lấy danh sách lựa chọn tài khoản cho các thành phần giao diện người dùng.
    /// </summary>
    /// <returns></returns>
    [HttpGet("selections")]
    public async Task<ActionResult<IBasePaging<AccountViewModel>>> GetAccountSelectionAsync()
    {
        var result = await accountService.GetAccountSelectionAsync();
        return Ok(result);
    }

    /// <summary>
    ///     Test endpoint to debug AutoMapper mapping (EN)<br />
    ///     Endpoint test để debug AutoMapper mapping (VI)
    /// </summary>
    [HttpPost("test")]
    public async Task<ActionResult> TestMapping([FromBody] AccountCreateRequest request)
    {
        Logger.LogWarning("Test endpoint - Request Name: '{Name}', Type: {Type}, Currency: '{Currency}'", 
            request.Name, request.Type, request.Currency);
        
        // Test AutoMapper manually
        var account = new Account
        {
            Name = request.Name ?? "Default Name",
            Type = request.Type,
            Currency = request.Currency ?? "VND",
            InitialBalance = request.InitialBalance,
            CurrentBalance = request.InitialBalance,
            UserId = request.UserId,
            IsActive = true
        };
        
        Logger.LogWarning("Test endpoint - Created Account Name: '{Name}', Type: {Type}, Currency: '{Currency}'", 
            account.Name, account.Type, account.Currency);
        
        return Ok(new { 
            RequestName = request.Name,
            AccountName = account.Name,
            Success = true
        });
    }
}