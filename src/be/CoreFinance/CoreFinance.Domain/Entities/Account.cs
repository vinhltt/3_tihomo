using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CoreFinance.Domain.Enums;
using Shared.EntityFramework.BaseEfModels;

namespace CoreFinance.Domain.Entities;

public class Account : UserOwnedEntity<Guid>
{
    /// <summary>
    ///     Account name for easy identification (EN)
    ///     Tên tài khoản để dễ nhận biết (VI)
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     Type of financial account (EN)
    ///     Loại tài khoản tài chính (VI)
    /// </summary>
    [Required]
    public AccountType Type { get; set; }

    /// <summary>
    ///     Card number (optional, for card accounts) (EN)
    ///     Số thẻ (tùy chọn, cho tài khoản thẻ) (VI)
    /// </summary>
    [MaxLength(32)]
    public string? CardNumber { get; set; }

    /// <summary>
    ///     Currency of the account (EN)
    ///     Đơn vị tiền tệ của tài khoản (VI)
    /// </summary>
    [Required]
    [MaxLength(10)]
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    ///     Initial balance when the account was created (EN)
    ///     Số dư ban đầu khi tài khoản được tạo (VI)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal InitialBalance { get; set; }

    /// <summary>
    ///     Current balance of the account (EN)
    ///     Số dư hiện tại của tài khoản (VI)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal CurrentBalance { get; set; }

    /// <summary>
    ///     Available credit limit (credit card only) (EN)
    ///     Hạn mức tín dụng khả dụng (chỉ áp dụng cho thẻ tín dụng) (VI)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? AvailableLimit { get; set; }

    /// <summary>
    ///     Whether the account is active (EN)
    ///     Tài khoản có đang hoạt động hay không (VI)
    /// </summary>
    [Required]
    public bool IsActive { get; set; }

    // Navigation property
    [InverseProperty("Account")] public ICollection<Transaction>? Transactions { get; set; }
}