using PlanningInvestment.Domain.Enums;

namespace PlanningInvestment.Application.DTOs.Investment;

/// <summary>
///     View model for displaying portfolio summary information. (EN)<br />
///     Mô hình hiển thị để hiển thị thông tin tóm tắt danh mục. (VI)
/// </summary>
public class PortfolioSummaryViewModel
{
    /// <summary>
    ///     Total amount invested across all investments. (EN)<br />
    ///     Tổng số tiền đầu tư trên tất cả các khoản đầu tư. (VI)
    /// </summary>
    public decimal TotalInvestedAmount { get; set; }

    /// <summary>
    ///     Current total value of all investments. (EN)<br />
    ///     Giá trị tổng hiện tại của tất cả các khoản đầu tư. (VI)
    /// </summary>
    public decimal? CurrentTotalValue { get; set; }

    /// <summary>
    ///     Total profit/loss across all investments. (EN)<br />
    ///     Tổng lãi/lỗ trên tất cả các khoản đầu tư. (VI)
    /// </summary>
    public decimal? TotalProfitLoss { get; set; }

    /// <summary>
    ///     Total profit/loss percentage. (EN)<br />
    ///     Phần trăm tổng lãi/lỗ. (VI)
    /// </summary>
    public decimal? TotalProfitLossPercentage { get; set; }

    /// <summary>
    ///     Total number of investments. (EN)<br />
    ///     Tổng số khoản đầu tư. (VI)
    /// </summary>
    public int InvestmentCount { get; set; }

    /// <summary>
    ///     Breakdown by investment type. (EN)<br />
    ///     Phân tích theo loại đầu tư. (VI)
    /// </summary>
    public List<InvestmentTypeBreakdown> InvestmentTypeBreakdown { get; set; } = new();

    /// <summary>
    ///     Last updated timestamp. (EN)<br />
    ///     Dấu thời gian cập nhật cuối cùng. (VI)
    /// </summary>
    public DateTime LastUpdated { get; set; }
}

/// <summary>
///     Breakdown information by investment type. (EN)<br />
///     Thông tin phân tích theo loại đầu tư. (VI)
/// </summary>
public class InvestmentTypeBreakdown
{
    /// <summary>
    ///     Type of investment. (EN)<br />
    ///     Loại đầu tư. (VI)
    /// </summary>
    public InvestmentType InvestmentType { get; set; }

    /// <summary>
    ///     Number of investments of this type. (EN)<br />
    ///     Số lượng đầu tư thuộc loại này. (VI)
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    ///     Total invested amount for this type. (EN)<br />
    ///     Tổng số tiền đầu tư cho loại này. (VI)
    /// </summary>
    public decimal TotalInvestedAmount { get; set; }

    /// <summary>
    ///     Current total value for this type. (EN)<br />
    ///     Giá trị tổng hiện tại cho loại này. (VI)
    /// </summary>
    public decimal? CurrentTotalValue { get; set; }

    /// <summary>
    ///     Profit/loss for this type. (EN)<br />
    ///     Lãi/lỗ cho loại này. (VI)
    /// </summary>
    public decimal? ProfitLoss { get; set; }
}
