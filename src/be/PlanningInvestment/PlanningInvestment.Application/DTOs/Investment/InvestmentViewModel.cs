using PlanningInvestment.Domain.Enums;
using Shared.EntityFramework.DTOs;

namespace PlanningInvestment.Application.DTOs.Investment;

/// <summary>
///     View model for displaying investment information. (EN)<br />
///     Mô hình hiển thị để hiển thị thông tin đầu tư. (VI)
/// </summary>
public class InvestmentViewModel : BaseViewModel<Guid>
{
    /// <summary>
    ///     Foreign key linking to user. (EN)<br />
    ///     Khóa ngoại liên kết với người dùng. (VI)
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    ///     Investment symbol or ticker. (EN)<br />
    ///     Ký hiệu hoặc mã cổ phiếu đầu tư. (VI)
    /// </summary>
    public string Symbol { get; set; } = string.Empty;

    /// <summary>
    ///     Type of investment. (EN)<br />
    ///     Loại đầu tư. (VI)
    /// </summary>
    public InvestmentType InvestmentType { get; set; }

    /// <summary>
    ///     Purchase price per unit. (EN)<br />
    ///     Giá mua mỗi đơn vị. (VI)
    /// </summary>
    public decimal PurchasePrice { get; set; }

    /// <summary>
    ///     Quantity of units owned. (EN)<br />
    ///     Số lượng đơn vị sở hữu. (VI)
    /// </summary>
    public decimal Quantity { get; set; }

    /// <summary>
    ///     Current market price per unit. (EN)<br />
    ///     Giá thị trường hiện tại mỗi đơn vị. (VI)
    /// </summary>
    public decimal? CurrentMarketPrice { get; set; }

    /// <summary>
    ///     Date when investment was purchased. (EN)<br />
    ///     Ngày mua đầu tư. (VI)
    /// </summary>
    public DateTime PurchaseDate { get; set; }

    /// <summary>
    ///     Additional notes about the investment. (EN)<br />
    ///     Ghi chú bổ sung về đầu tư. (VI)
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    ///     Calculated property: Total amount invested. (EN)<br />
    ///     Thuộc tính tính toán: Tổng số tiền đầu tư. (VI)
    /// </summary>
    public decimal TotalInvestedAmount { get; set; }

    /// <summary>
    ///     Calculated property: Current total value. (EN)<br />
    ///     Thuộc tính tính toán: Giá trị tổng hiện tại. (VI)
    /// </summary>
    public decimal? CurrentTotalValue { get; set; }

    /// <summary>
    ///     Calculated property: Profit/Loss amount. (EN)<br />
    ///     Thuộc tính tính toán: Số tiền lãi/lỗ. (VI)
    /// </summary>
    public decimal? ProfitLoss { get; set; }

    /// <summary>
    ///     Calculated property: Profit/Loss percentage. (EN)<br />
    ///     Thuộc tính tính toán: Phần trăm lãi/lỗ. (VI)
    /// </summary>
    public decimal? ProfitLossPercentage { get; set; }

    /// <summary>
    ///     Calculated property: Whether investment is profitable. (EN)<br />
    ///     Thuộc tính tính toán: Đầu tư có sinh lời hay không. (VI)
    /// </summary>
    public bool? IsProfitable { get; set; }
}
