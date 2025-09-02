using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PlanningInvestment.Domain.Enums;
using Shared.EntityFramework.BaseEfModels;

namespace PlanningInvestment.Domain.Entities;

/// <summary>
///     Represents an investment entity for portfolio tracking. (EN)<br />
///     Đại diện cho thực thể đầu tư để theo dõi danh mục. (VI)
/// </summary>
public class Investment : BaseEntity<Guid>
{
    /// <summary>
    ///     Foreign key linking to user (EN)<br />
    ///     Khóa ngoại liên kết với người dùng (VI)
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    ///     Investment symbol or ticker (EN)<br />
    ///     Ký hiệu hoặc mã cổ phiếu đầu tư (VI)
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Symbol { get; set; } = string.Empty;

    /// <summary>
    ///     Type of investment (EN)<br />
    ///     Loại đầu tư (VI)
    /// </summary>
    [Required]
    public InvestmentType InvestmentType { get; set; }

    /// <summary>
    ///     Purchase price per unit (EN)<br />
    ///     Giá mua mỗi đơn vị (VI)
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18,4)")]
    [Range(0.0001, double.MaxValue, ErrorMessage = "Purchase price must be greater than 0")]
    public decimal PurchasePrice { get; set; }

    /// <summary>
    ///     Quantity of units owned (EN)<br />
    ///     Số lượng đơn vị sở hữu (VI)
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18,8)")]
    [Range(0.00000001, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
    public decimal Quantity { get; set; }

    /// <summary>
    ///     Current market price per unit (EN)<br />
    ///     Giá thị trường hiện tại mỗi đơn vị (VI)
    /// </summary>
    [Column(TypeName = "decimal(18,4)")]
    [Range(0.0001, double.MaxValue, ErrorMessage = "Current market price must be greater than 0")]
    public decimal? CurrentMarketPrice { get; set; }

    /// <summary>
    ///     Date when investment was purchased (EN)<br />
    ///     Ngày mua đầu tư (VI)
    /// </summary>
    [Required]
    public DateTime PurchaseDate { get; set; }

    /// <summary>
    ///     Additional notes about the investment (EN)<br />
    ///     Ghi chú bổ sung về đầu tư (VI)
    /// </summary>
    [MaxLength(500)]
    public string? Notes { get; set; }

    /// <summary>
    ///     Calculated property: Total amount invested (EN)<br />
    ///     Thuộc tính tính toán: Tổng số tiền đầu tư (VI)
    /// </summary>
    [NotMapped]
    public decimal TotalInvestedAmount => PurchasePrice * Quantity;

    /// <summary>
    ///     Calculated property: Current total value (EN)<br />
    ///     Thuộc tính tính toán: Giá trị tổng hiện tại (VI)
    /// </summary>
    [NotMapped]
    public decimal? CurrentTotalValue => CurrentMarketPrice.HasValue ? CurrentMarketPrice.Value * Quantity : null;

    /// <summary>
    ///     Calculated property: Profit/Loss amount (EN)<br />
    ///     Thuộc tính tính toán: Số tiền lãi/lỗ (VI)
    /// </summary>
    [NotMapped]
    public decimal? ProfitLoss => CurrentTotalValue.HasValue ? CurrentTotalValue.Value - TotalInvestedAmount : null;

    /// <summary>
    ///     Calculated property: Profit/Loss percentage (EN)<br />
    ///     Thuộc tính tính toán: Phần trăm lãi/lỗ (VI)
    /// </summary>
    [NotMapped]
    public decimal? ProfitLossPercentage => ProfitLoss.HasValue && TotalInvestedAmount > 0 
        ? (ProfitLoss.Value / TotalInvestedAmount) * 100 
        : null;

    /// <summary>
    ///     Calculated property: Whether investment is profitable (EN)<br />
    ///     Thuộc tính tính toán: Đầu tư có sinh lời hay không (VI)
    /// </summary>
    [NotMapped]
    public bool? IsProfitable => ProfitLoss.HasValue ? ProfitLoss.Value > 0 : null;
}
