using Shared.Contracts.DTOs;

namespace CoreFinance.Application.DTOs.Account;

/// <summary>
///     Represents a request to update an existing account. (EN)<br />
///     Đại diện cho request cập nhật tài khoản hiện có. (VI)
/// </summary>
public class AccountUpdateRequest : BaseUpdateRequest<Guid>
{
    /// <summary>
    ///     The updated name of the account (optional). (EN)<br />
    ///     Tên tài khoản được cập nhật (tùy chọn). (VI)
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    ///     The updated type of the account (optional). (EN)<br />
    ///     Loại tài khoản được cập nhật (tùy chọn). (VI)
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    ///     The updated card number associated with the account (optional). (EN)<br />
    ///     Số thẻ được cập nhật liên kết với tài khoản (tùy chọn). (VI)
    /// </summary>
    public string? CardNumber { get; set; }

    /// <summary>
    ///     The updated currency of the account (optional). (EN)<br />
    ///     Tiền tệ của tài khoản được cập nhật (tùy chọn). (VI)
    /// </summary>
    public string? Currency { get; set; }

    /// <summary>
    ///     The updated available credit limit for credit card accounts (optional). (EN)<br />
    ///     Hạn mức tín dụng khả dụng được cập nhật cho tài khoản thẻ tín dụng (tùy chọn). (VI)
    /// </summary>
    public decimal? AvailableLimit { get; set; }
}