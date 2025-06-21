using CoreFinance.Api.Controllers.Base;
using CoreFinance.Application.DTOs.Transaction;
using CoreFinance.Application.Interfaces;
using CoreFinance.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Shared.Contracts.BaseEfModels;
using Shared.Contracts.DTOs;

namespace CoreFinance.Api.Controllers;

/// <summary>
///     Controller for managing transactions. (EN)<br />
///     Controller để quản lý các giao dịch. (VI)
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TransactionController(
    ILogger<TransactionController> logger,
    ITransactionService transactionService)
    : CrudController<Transaction, TransactionCreateRequest,
        TransactionUpdateRequest, TransactionViewModel, Guid>(logger,
        transactionService)
{
    [HttpPost("filter")]
    public override async Task<ActionResult<IBasePaging<TransactionViewModel>>> GetPagingAsync(
        FilterBodyRequest request)
    {
        var result = await transactionService.GetPagingAsync(request);
        return Ok(result);
    }
}