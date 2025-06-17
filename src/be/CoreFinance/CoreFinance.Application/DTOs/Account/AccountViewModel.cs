using Shared.Contracts.DTOs;
using CoreFinance.Domain.Enums;

namespace CoreFinance.Application.DTOs.Account;

/// <summary>
/// Represents a view model for account data. (EN)<br/>
/// Đại diện cho view model dữ liệu tài khoản. (VI)
/// </summary>
public class AccountViewModel : BaseViewModel<Guid>
{
    /// <summary>
    /// The ID of the user who owns the account (optional). (EN)<br/>
    /// ID của người dùng sở hữu tài khoản (tùy chọn). (VI)
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// The name of the account. (EN)<br/>
    /// Tên tài khoản. (VI)
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// The type of the account (e.g., checking, savings, credit card). (EN)<br/>
    /// Loại tài khoản (ví dụ: tài khoản thanh toán, tiết kiệm, thẻ tín dụng). (VI)
    /// </summary>
    public AccountType Type { get; set; }

    /// <summary>
    /// The card number associated with the account (optional). (EN)<br/>
    /// Số thẻ liên kết với tài khoản (tùy chọn). (VI)
    /// </summary>
    public string? CardNumber { get; set; }

    /// <summary>
    /// The currency of the account (e.g., "VND", "USD"). (EN)<br/>
    /// Tiền tệ của tài khoản (ví dụ: "VND", "USD"). (VI)
    /// </summary>
    public string? Currency { get; set; }

    /// <summary>
    /// The initial balance of the account. (EN)<br/>
    /// Số dư ban đầu của tài khoản. (VI)
    /// </summary>
    public decimal InitialBalance { get; set; }

    /// <summary>
    /// The current balance of the account. (EN)<br/>
    /// Số dư hiện tại của tài khoản. (VI)
    /// </summary>
    public decimal CurrentBalance { get; set; }

    /// <summary>
    /// The available credit limit for credit card accounts (optional). (EN)<br/>
    /// Hạn mức tín dụng khả dụng cho tài khoản thẻ tín dụng (tùy chọn). (VI)
    /// </summary>
    public decimal? AvailableLimit { get; set; }
}