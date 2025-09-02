using System.ComponentModel.DataAnnotations;

namespace PlanningInvestment.Application.DTOs.Investment;

/// <summary>
///     DTO for updating market price of an investment. (EN)<br />
///     DTO để cập nhật giá thị trường của đầu tư. (VI)
/// </summary>
public class UpdateMarketPriceRequest
{
    /// <summary>
    ///     Current market price per unit. (EN)<br />
    ///     Giá thị trường hiện tại mỗi đơn vị. (VI)
    /// </summary>
    [Required(ErrorMessage = "Current market price is required")]
    [Range(0.0001, double.MaxValue, ErrorMessage = "Current market price must be greater than 0")]
    public decimal CurrentMarketPrice { get; set; }
}
