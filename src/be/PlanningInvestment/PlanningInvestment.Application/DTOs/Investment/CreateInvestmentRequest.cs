using System.ComponentModel.DataAnnotations;
using PlanningInvestment.Domain.Enums;
using Shared.EntityFramework.DTOs;

namespace PlanningInvestment.Application.DTOs.Investment;

/// <summary>
///     DTO for creating a new investment. (EN)<br />
///     DTO để tạo đầu tư mới. (VI)
/// </summary>
public class CreateInvestmentRequest : BaseCreateRequest
{
    /// <summary>
    ///     User identifier for the investment owner. (EN)<br />
    ///     Định danh người dùng sở hữu đầu tư. (VI)
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    ///     Investment symbol or ticker. (EN)<br />
    ///     Ký hiệu hoặc mã cổ phiếu đầu tư. (VI)
    /// </summary>
    [Required(ErrorMessage = "Symbol is required")]
    [MaxLength(50, ErrorMessage = "Symbol cannot exceed 50 characters")]
    public string Symbol { get; set; } = string.Empty;

    /// <summary>
    ///     Type of investment. (EN)<br />
    ///     Loại đầu tư. (VI)
    /// </summary>
    [Required(ErrorMessage = "Investment type is required")]
    public InvestmentType InvestmentType { get; set; }

    /// <summary>
    ///     Purchase price per unit. (EN)<br />
    ///     Giá mua mỗi đơn vị. (VI)
    /// </summary>
    [Required(ErrorMessage = "Purchase price is required")]
    [Range(0.0001, double.MaxValue, ErrorMessage = "Purchase price must be greater than 0")]
    public decimal PurchasePrice { get; set; }

    /// <summary>
    ///     Quantity of units owned. (EN)<br />
    ///     Số lượng đơn vị sở hữu. (VI)
    /// </summary>
    [Required(ErrorMessage = "Quantity is required")]
    [Range(0.00000001, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
    public decimal Quantity { get; set; }

    /// <summary>
    ///     Current market price per unit. (EN)<br />
    ///     Giá thị trường hiện tại mỗi đơn vị. (VI)
    /// </summary>
    [Range(0.0001, double.MaxValue, ErrorMessage = "Current market price must be greater than 0")]
    public decimal? CurrentMarketPrice { get; set; }

    /// <summary>
    ///     Date when investment was purchased. (EN)<br />
    ///     Ngày mua đầu tư. (VI)
    /// </summary>
    [Required(ErrorMessage = "Purchase date is required")]
    public DateTime PurchaseDate { get; set; }

    /// <summary>
    ///     Additional notes about the investment. (EN)<br />
    ///     Ghi chú bổ sung về đầu tư. (VI)
    /// </summary>
    [MaxLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
    public string? Notes { get; set; }
}
