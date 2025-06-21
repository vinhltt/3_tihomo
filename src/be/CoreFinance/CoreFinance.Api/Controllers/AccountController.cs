using CoreFinance.Api.Controllers.Base;
using CoreFinance.Application.DTOs.Account;
using CoreFinance.Application.Interfaces;
using CoreFinance.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Shared.Contracts.BaseEfModels;
using Shared.Contracts.DTOs;

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
    ///     (EN) Gets a list of account selections for UI components.<br />
    ///     (VI) Lấy danh sách lựa chọn tài khoản cho các thành phần giao diện người dùng.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<ActionResult<IBasePaging<AccountViewModel>>> GetAccountSelectionAsync()
    {
        var result = await accountService.GetAccountSelectionAsync();
        return Ok(result);
    }
}