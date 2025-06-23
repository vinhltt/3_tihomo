using Identity.Domain.Enums;
using Shared.EntityFramework.BaseEfModels;

namespace Identity.Domain.Entities;

public class ApiKey : BaseEntity<Guid>
{    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string KeyHash { get; set; } = string.Empty; // SHA256 hashed
    public string KeyPrefix { get; set; } = string.Empty; // First few characters for identification
    public string? Description { get; set; }
    public List<string> Scopes { get; set; } = []; // JSON array
    public ApiKeyStatus Status { get; set; } = ApiKeyStatus.Active;
    public bool IsActive => Status == ApiKeyStatus.Active && (ExpiresAt == null || ExpiresAt > DateTime.UtcNow);
    public DateTime? ExpiresAt { get; set; }
    public DateTime? LastUsedAt { get; set; }
    public int UsageCount { get; set; } = 0;

    public virtual User User { get; set; } = null!;
}