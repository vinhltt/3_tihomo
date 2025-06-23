namespace Identity.Contracts;

/// <summary>
/// User information returned after successful authentication
/// Thông tin người dùng trả về sau khi xác thực thành công
/// </summary>
public class UserInfo
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? PictureUrl { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<string> Providers { get; set; } = new();
}

/// <summary>
/// Request model for social login token verification
/// Model yêu cầu để xác minh token đăng nhập xã hội
/// </summary>
public class SocialLoginRequest
{
    public string Provider { get; set; } = string.Empty; // "Google", "Facebook", etc.
    public string Token { get; set; } = string.Empty; // JWT token from the provider
}

/// <summary>
/// Response model after successful login
/// Model phản hồi sau khi đăng nhập thành công
/// </summary>
public class LoginResponse
{
    public UserInfo User { get; set; } = null!;
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}

/// <summary>
/// Request model for API key creation
/// Model yêu cầu để tạo API key
/// </summary>
public class CreateApiKeyRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public List<string>? Scopes { get; set; }
}

/// <summary>
/// Response model for API key creation
/// Model phản hồi để tạo API key
/// </summary>
public class CreateApiKeyResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty; // The actual key (only shown once)
    public string KeyPrefix { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

/// <summary>
/// API key information (without the actual key)
/// Thông tin API key (không bao gồm key thực tế)
/// </summary>
public class ApiKeyInfo
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string KeyPrefix { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime? LastUsedAt { get; set; }
    public string? Description { get; set; }
    public List<string>? Scopes { get; set; }
}
