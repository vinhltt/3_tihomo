using System.ComponentModel.DataAnnotations;
using MoneyManagement.Domain.Enums;

namespace MoneyManagement.Application.DTOs.Jar;

/// <summary>
///     Request model for creating a new jar (EN)<br />
///     Model yêu cầu để tạo lọ mới (VI)
/// </summary>
public class CreateJarRequestDto
{
    /// <summary>
    ///     Jar name (EN)<br />
    ///     Tên lọ (VI)
    /// </summary>
    [Required(ErrorMessage = "Jar name is required")]
    [MaxLength(200, ErrorMessage = "Jar name cannot exceed 200 characters")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     Jar description (EN)<br />
    ///     Mô tả lọ (VI)
    /// </summary>
    [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }

    /// <summary>
    ///     Jar type (EN)<br />
    ///     Loại lọ (VI)
    /// </summary>
    [Required(ErrorMessage = "Jar type is required")]
    public JarType JarType { get; set; }

    /// <summary>
    ///     Target amount for this jar (EN)<br />
    ///     Số tiền mục tiêu cho lọ này (VI)
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "Target amount must be non-negative")]
    public decimal? TargetAmount { get; set; }

    /// <summary>
    ///     Percentage allocation from income (EN)<br />
    ///     Phần trăm phân bổ từ thu nhập (VI)
    /// </summary>
    [Range(0, 100, ErrorMessage = "Allocation percentage must be between 0 and 100")]
    public decimal? AllocationPercentage { get; set; }

    /// <summary>
    ///     Color code for UI display (EN)<br />
    ///     Mã màu để hiển thị UI (VI)
    /// </summary>
    [RegularExpression(@"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$", ErrorMessage = "Color code must be a valid hex color")]
    public string? ColorCode { get; set; }

    /// <summary>
    ///     Icon name for UI display (EN)<br />
    ///     Tên icon để hiển thị UI (VI)
    /// </summary>
    [MaxLength(100, ErrorMessage = "Icon name cannot exceed 100 characters")]
    public string? IconName { get; set; }
}