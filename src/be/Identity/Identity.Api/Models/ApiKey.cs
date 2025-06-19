using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Identity.Api.Models;

[Table("ApiKeys")]
public class ApiKey
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    public Guid UserId { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty; // User-friendly name for the API key
    
    [Required]
    [MaxLength(64)]
    public string KeyHash { get; set; } = string.Empty; // Hashed version of the actual key
    
    [Required]
    [MaxLength(32)]
    public string KeyPrefix { get; set; } = string.Empty; // First 8 chars for identification
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? ExpiresAt { get; set; }
    
    public DateTime? LastUsedAt { get; set; }
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    // Scopes/Permissions (comma-separated or JSON)
    [MaxLength(1000)]
    public string? Scopes { get; set; }
    
    // Navigation properties
    [ForeignKey("UserId")]
    public virtual User User { get; set; } = null!;
}
