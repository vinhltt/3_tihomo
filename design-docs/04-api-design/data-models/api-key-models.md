# API Key Data Models

## 📋 Overview

Data models cho Enhanced API Key Management feature trong TiHoMo system. Định nghĩa các DTOs, entities, và request/response models.

---

## 🏗️ Domain Models

### ApiKey Entity
```csharp
/// <summary>
/// API Key entity - Thực thể API Key cho third-party integration
/// </summary>
public class ApiKey : BaseEntity
{
    /// <summary>
    /// User ID - ID của user sở hữu API key
    /// </summary>
    public Guid UserId { get; set; }
    
    /// <summary>
    /// Name - Tên mô tả của API key
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Description - Mô tả chi tiết API key
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Key Hash - Hash của API key (SHA-256)
    /// </summary>
    public string KeyHash { get; set; } = string.Empty;
    
    /// <summary>
    /// Key Prefix - Prefix của API key (pfm_xxxxx)
    /// </summary>
    public string KeyPrefix { get; set; } = string.Empty;
    
    /// <summary>
    /// Scopes - Danh sách permissions
    /// </summary>
    public List<string> Scopes { get; set; } = new();
    
    /// <summary>
    /// Status - Trạng thái API key
    /// </summary>
    public ApiKeyStatus Status { get; set; } = ApiKeyStatus.Active;
    
    /// <summary>
    /// Rate Limit Per Minute - Giới hạn request per minute
    /// </summary>
    public int RateLimitPerMinute { get; set; } = 100;
    
    /// <summary>
    /// Daily Usage Quota - Giới hạn request per day
    /// </summary>
    public int DailyUsageQuota { get; set; } = 10000;
    
    /// <summary>
    /// Usage Count - Tổng số lần sử dụng
    /// </summary>
    public long UsageCount { get; set; } = 0;
    
    /// <summary>
    /// IP Whitelist - Danh sách IP được phép
    /// </summary>
    public List<string> IpWhitelist { get; set; } = new();
    
    /// <summary>
    /// Security Settings - Cài đặt bảo mật
    /// </summary>
    public ApiKeySecuritySettings SecuritySettings { get; set; } = new();
    
    /// <summary>
    /// Expires At - Thời gian hết hạn
    /// </summary>
    public DateTime? ExpiresAt { get; set; }
    
    /// <summary>
    /// Last Used At - Lần sử dụng cuối
    /// </summary>
    public DateTime? LastUsedAt { get; set; }
    
    /// <summary>
    /// Revoked At - Thời gian thu hồi
    /// </summary>
    public DateTime? RevokedAt { get; set; }
    
    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual ICollection<ApiKeyUsageLog> UsageLogs { get; set; } = new List<ApiKeyUsageLog>();
}
```

### ApiKeySecuritySettings
```csharp
/// <summary>
/// API Key Security Settings - Cài đặt bảo mật cho API key
/// </summary>
public class ApiKeySecuritySettings
{
    /// <summary>
    /// Require HTTPS - Yêu cầu HTTPS
    /// </summary>
    public bool RequireHttps { get; set; } = true;
    
    /// <summary>
    /// Allow CORS Requests - Cho phép CORS
    /// </summary>
    public bool AllowCorsRequests { get; set; } = false;
    
    /// <summary>
    /// Allowed Origins - Danh sách origins được phép
    /// </summary>
    public List<string> AllowedOrigins { get; set; } = new();
}
```

### ApiKeyUsageLog Entity
```csharp
/// <summary>
/// API Key Usage Log - Log sử dụng API key
/// </summary>
public class ApiKeyUsageLog : BaseEntity
{
    /// <summary>
    /// API Key ID - ID của API key
    /// </summary>
    public Guid ApiKeyId { get; set; }
    
    /// <summary>
    /// Timestamp - Thời gian request
    /// </summary>
    public DateTime Timestamp { get; set; }
    
    /// <summary>
    /// Method - HTTP method
    /// </summary>
    public string Method { get; set; } = string.Empty;
    
    /// <summary>
    /// Endpoint - API endpoint
    /// </summary>
    public string Endpoint { get; set; } = string.Empty;
    
    /// <summary>
    /// Status Code - HTTP status code
    /// </summary>
    public int StatusCode { get; set; }
    
    /// <summary>
    /// Response Time - Thời gian response (ms)
    /// </summary>
    public int ResponseTime { get; set; }
    
    /// <summary>
    /// IP Address - IP address của client
    /// </summary>
    public string IpAddress { get; set; } = string.Empty;
    
    /// <summary>
    /// User Agent - User agent string
    /// </summary>
    public string? UserAgent { get; set; }
    
    /// <summary>
    /// Request Size - Kích thước request (bytes)
    /// </summary>
    public long RequestSize { get; set; }
    
    /// <summary>
    /// Response Size - Kích thước response (bytes)
    /// </summary>
    public long ResponseSize { get; set; }
    
    /// <summary>
    /// Error Message - Thông báo lỗi (nếu có)
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    // Navigation properties
    public virtual ApiKey ApiKey { get; set; } = null!;
}
```

---

## 🔧 Enums

### ApiKeyStatus
```csharp
/// <summary>
/// API Key Status - Trạng thái API key
/// </summary>
public enum ApiKeyStatus
{
    /// <summary>
    /// Active - Đang hoạt động
    /// </summary>
    Active = 1,
    
    /// <summary>
    /// Revoked - Đã thu hồi
    /// </summary>
    Revoked = 2,
    
    /// <summary>
    /// Expired - Đã hết hạn
    /// </summary>
    Expired = 3,
    
    /// <summary>
    /// Suspended - Tạm ngưng
    /// </summary>
    Suspended = 4
}
```

### ApiKeyScope
```csharp
/// <summary>
/// API Key Scope - Phạm vi quyền của API key
/// </summary>
public static class ApiKeyScope
{
    /// <summary>
    /// Read - Quyền đọc dữ liệu
    /// </summary>
    public const string Read = "read";
    
    /// <summary>
    /// Write - Quyền ghi dữ liệu
    /// </summary>
    public const string Write = "write";
    
    /// <summary>
    /// Transactions Read - Quyền đọc giao dịch
    /// </summary>
    public const string TransactionsRead = "transactions:read";
    
    /// <summary>
    /// Transactions Write - Quyền ghi giao dịch
    /// </summary>
    public const string TransactionsWrite = "transactions:write";
    
    /// <summary>
    /// Accounts Read - Quyền đọc tài khoản
    /// </summary>
    public const string AccountsRead = "accounts:read";
    
    /// <summary>
    /// Accounts Write - Quyền ghi tài khoản
    /// </summary>
    public const string AccountsWrite = "accounts:write";
    
    /// <summary>
    /// Reports Read - Quyền đọc báo cáo
    /// </summary>
    public const string ReportsRead = "reports:read";
    
    /// <summary>
    /// Admin - Quyền quản trị
    /// </summary>
    public const string Admin = "admin";
    
    /// <summary>
    /// All Available Scopes - Tất cả scopes có sẵn
    /// </summary>
    public static readonly string[] AllScopes = new[]
    {
        Read, Write,
        TransactionsRead, TransactionsWrite,
        AccountsRead, AccountsWrite,
        ReportsRead,
        Admin
    };
}
```

---

## 📄 Request/Response DTOs

### Create API Key Request
```csharp
/// <summary>
/// Create API Key Request - Request tạo API key mới
/// </summary>
public class CreateApiKeyRequest
{
    /// <summary>
    /// Name - Tên API key (required, 1-100 chars)
    /// </summary>
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Description - Mô tả API key (optional, max 500 chars)
    /// </summary>
    [StringLength(500)]
    public string? Description { get; set; }
    
    /// <summary>
    /// Scopes - Danh sách permissions (required, at least one)
    /// </summary>
    [Required]
    [MinLength(1)]
    public List<string> Scopes { get; set; } = new();
    
    /// <summary>
    /// Expires At - Thời gian hết hạn (optional)
    /// </summary>
    public DateTime? ExpiresAt { get; set; }
    
    /// <summary>
    /// Rate Limit Per Minute - Giới hạn request/minute (default: 100, max: 1000)
    /// </summary>
    [Range(1, 1000)]
    public int RateLimitPerMinute { get; set; } = 100;
    
    /// <summary>
    /// Daily Usage Quota - Giới hạn request/day (default: 10000, max: 100000)
    /// </summary>
    [Range(1, 100000)]
    public int DailyUsageQuota { get; set; } = 10000;
    
    /// <summary>
    /// IP Whitelist - Danh sách IP được phép (optional)
    /// </summary>
    public List<string> IpWhitelist { get; set; } = new();
    
    /// <summary>
    /// Security Settings - Cài đặt bảo mật (optional)
    /// </summary>
    public CreateApiKeySecuritySettings? SecuritySettings { get; set; }
}
```

### Update API Key Request
```csharp
/// <summary>
/// Update API Key Request - Request cập nhật API key
/// </summary>
public class UpdateApiKeyRequest
{
    /// <summary>
    /// Name - Tên API key (optional, 1-100 chars)
    /// </summary>
    [StringLength(100, MinimumLength = 1)]
    public string? Name { get; set; }
    
    /// <summary>
    /// Description - Mô tả API key (optional, max 500 chars)
    /// </summary>
    [StringLength(500)]
    public string? Description { get; set; }
    
    /// <summary>
    /// Scopes - Danh sách permissions (optional)
    /// </summary>
    public List<string>? Scopes { get; set; }
    
    /// <summary>
    /// Expires At - Thời gian hết hạn (optional, null to remove)
    /// </summary>
    public DateTime? ExpiresAt { get; set; }
    
    /// <summary>
    /// Rate Limit Per Minute - Giới hạn request/minute (optional, 1-1000)
    /// </summary>
    [Range(1, 1000)]
    public int? RateLimitPerMinute { get; set; }
    
    /// <summary>
    /// Daily Usage Quota - Giới hạn request/day (optional, 1-100000)
    /// </summary>
    [Range(1, 100000)]
    public int? DailyUsageQuota { get; set; }
    
    /// <summary>
    /// IP Whitelist - Danh sách IP được phép (optional)
    /// </summary>
    public List<string>? IpWhitelist { get; set; }
    
    /// <summary>
    /// Security Settings - Cài đặt bảo mật (optional)
    /// </summary>
    public UpdateApiKeySecuritySettings? SecuritySettings { get; set; }
}
```

### API Key Response
```csharp
/// <summary>
/// API Key Response - Response chứa thông tin API key
/// </summary>
public class ApiKeyResponse
{
    /// <summary>
    /// ID - UUID của API key
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Name - Tên API key
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Description - Mô tả API key
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// API Key - Full API key (chỉ hiển thị khi tạo mới hoặc regenerate)
    /// </summary>
    public string? ApiKey { get; set; }
    
    /// <summary>
    /// Key Prefix - Prefix của API key
    /// </summary>
    public string KeyPrefix { get; set; } = string.Empty;
    
    /// <summary>
    /// Scopes - Danh sách permissions
    /// </summary>
    public List<string> Scopes { get; set; } = new();
    
    /// <summary>
    /// Status - Trạng thái API key
    /// </summary>
    public string Status { get; set; } = string.Empty;
    
    /// <summary>
    /// Rate Limit Per Minute - Giới hạn request/minute
    /// </summary>
    public int RateLimitPerMinute { get; set; }
    
    /// <summary>
    /// Daily Usage Quota - Giới hạn request/day
    /// </summary>
    public int DailyUsageQuota { get; set; }
    
    /// <summary>
    /// Today Usage Count - Số lần sử dụng hôm nay
    /// </summary>
    public int TodayUsageCount { get; set; }
    
    /// <summary>
    /// Usage Count - Tổng số lần sử dụng
    /// </summary>
    public long UsageCount { get; set; }
    
    /// <summary>
    /// IP Whitelist - Danh sách IP được phép
    /// </summary>
    public List<string> IpWhitelist { get; set; } = new();
    
    /// <summary>
    /// Security Settings - Cài đặt bảo mật
    /// </summary>
    public ApiKeySecuritySettingsResponse SecuritySettings { get; set; } = new();
    
    /// <summary>
    /// Created At - Thời gian tạo
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Updated At - Thời gian cập nhật
    /// </summary>
    public DateTime UpdatedAt { get; set; }
    
    /// <summary>
    /// Expires At - Thời gian hết hạn
    /// </summary>
    public DateTime? ExpiresAt { get; set; }
    
    /// <summary>
    /// Last Used At - Lần sử dụng cuối
    /// </summary>
    public DateTime? LastUsedAt { get; set; }
    
    /// <summary>
    /// Revoked At - Thời gian thu hồi
    /// </summary>
    public DateTime? RevokedAt { get; set; }
}
```

### Usage Statistics Response
```csharp
/// <summary>
/// Usage Statistics Response - Response thống kê sử dụng API key
/// </summary>
public class UsageStatisticsResponse
{
    /// <summary>
    /// API Key ID - ID của API key
    /// </summary>
    public Guid ApiKeyId { get; set; }
    
    /// <summary>
    /// Period - Khoảng thời gian thống kê
    /// </summary>
    public StatisticsPeriod Period { get; set; } = new();
    
    /// <summary>
    /// Summary - Tổng quan thống kê
    /// </summary>
    public UsageSummary Summary { get; set; } = new();
    
    /// <summary>
    /// Daily Stats - Thống kê theo ngày
    /// </summary>
    public List<DailyUsageStats> DailyStats { get; set; } = new();
    
    /// <summary>
    /// Endpoint Stats - Thống kê theo endpoint
    /// </summary>
    public List<EndpointStats> EndpointStats { get; set; } = new();
    
    /// <summary>
    /// Error Stats - Thống kê lỗi
    /// </summary>
    public List<ErrorStats> ErrorStats { get; set; } = new();
}
```

---

## 📊 Supporting DTOs

### Security Settings DTOs
```csharp
/// <summary>
/// Create API Key Security Settings - Cài đặt bảo mật khi tạo API key
/// </summary>
public class CreateApiKeySecuritySettings
{
    /// <summary>
    /// Require HTTPS - Yêu cầu HTTPS (default: true)
    /// </summary>
    public bool RequireHttps { get; set; } = true;
    
    /// <summary>
    /// Allow CORS Requests - Cho phép CORS (default: false)
    /// </summary>
    public bool AllowCorsRequests { get; set; } = false;
    
    /// <summary>
    /// Allowed Origins - Origins được phép (required if AllowCorsRequests = true)
    /// </summary>
    public List<string> AllowedOrigins { get; set; } = new();
}

/// <summary>
/// Update API Key Security Settings - Cài đặt bảo mật khi cập nhật API key
/// </summary>
public class UpdateApiKeySecuritySettings
{
    /// <summary>
    /// Require HTTPS - Yêu cầu HTTPS (optional)
    /// </summary>
    public bool? RequireHttps { get; set; }
    
    /// <summary>
    /// Allow CORS Requests - Cho phép CORS (optional)
    /// </summary>
    public bool? AllowCorsRequests { get; set; }
    
    /// <summary>
    /// Allowed Origins - Origins được phép (optional)
    /// </summary>
    public List<string>? AllowedOrigins { get; set; }
}

/// <summary>
/// API Key Security Settings Response - Response cài đặt bảo mật
/// </summary>
public class ApiKeySecuritySettingsResponse
{
    /// <summary>
    /// Require HTTPS - Yêu cầu HTTPS
    /// </summary>
    public bool RequireHttps { get; set; }
    
    /// <summary>
    /// Allow CORS Requests - Cho phép CORS
    /// </summary>
    public bool AllowCorsRequests { get; set; }
    
    /// <summary>
    /// Allowed Origins - Origins được phép
    /// </summary>
    public List<string> AllowedOrigins { get; set; } = new();
}
```

### Statistics DTOs
```csharp
/// <summary>
/// Statistics Period - Khoảng thời gian thống kê
/// </summary>
public class StatisticsPeriod
{
    /// <summary>
    /// Start Date - Ngày bắt đầu
    /// </summary>
    public DateTime StartDate { get; set; }
    
    /// <summary>
    /// End Date - Ngày kết thúc
    /// </summary>
    public DateTime EndDate { get; set; }
    
    /// <summary>
    /// Granularity - Độ chi tiết (hour/day)
    /// </summary>
    public string Granularity { get; set; } = "day";
}

/// <summary>
/// Usage Summary - Tổng quan sử dụng
/// </summary>
public class UsageSummary
{
    /// <summary>
    /// Total Requests - Tổng số request
    /// </summary>
    public long TotalRequests { get; set; }
    
    /// <summary>
    /// Successful Requests - Số request thành công
    /// </summary>
    public long SuccessfulRequests { get; set; }
    
    /// <summary>
    /// Failed Requests - Số request thất bại
    /// </summary>
    public long FailedRequests { get; set; }
    
    /// <summary>
    /// Success Rate - Tỷ lệ thành công (%)
    /// </summary>
    public double SuccessRate { get; set; }
    
    /// <summary>
    /// Average Response Time - Thời gian phản hồi trung bình (ms)
    /// </summary>
    public double AverageResponseTime { get; set; }
    
    /// <summary>
    /// Unique IP Addresses - Số IP duy nhất
    /// </summary>
    public int UniqueIpAddresses { get; set; }
}

/// <summary>
/// Daily Usage Stats - Thống kê sử dụng theo ngày
/// </summary>
public class DailyUsageStats
{
    /// <summary>
    /// Date - Ngày
    /// </summary>
    public string Date { get; set; } = string.Empty;
    
    /// <summary>
    /// Requests - Số request
    /// </summary>
    public long Requests { get; set; }
    
    /// <summary>
    /// Successful - Số request thành công
    /// </summary>
    public long Successful { get; set; }
    
    /// <summary>
    /// Failed - Số request thất bại
    /// </summary>
    public long Failed { get; set; }
    
    /// <summary>
    /// Average Response Time - Thời gian phản hồi trung bình
    /// </summary>
    public double AverageResponseTime { get; set; }
}

/// <summary>
/// Endpoint Stats - Thống kê theo endpoint
/// </summary>
public class EndpointStats
{
    /// <summary>
    /// Endpoint - Endpoint path
    /// </summary>
    public string Endpoint { get; set; } = string.Empty;
    
    /// <summary>
    /// Requests - Số request
    /// </summary>
    public long Requests { get; set; }
    
    /// <summary>
    /// Percentage - Tỷ lệ phần trăm
    /// </summary>
    public double Percentage { get; set; }
}

/// <summary>
/// Error Stats - Thống kê lỗi
/// </summary>
public class ErrorStats
{
    /// <summary>
    /// Status Code - HTTP status code
    /// </summary>
    public int StatusCode { get; set; }
    
    /// <summary>
    /// Count - Số lượng
    /// </summary>
    public long Count { get; set; }
    
    /// <summary>
    /// Percentage - Tỷ lệ phần trăm
    /// </summary>
    public double Percentage { get; set; }
}
```

---

## 🔍 Query DTOs

### List API Keys Query
```csharp
/// <summary>
/// List API Keys Query - Query parameters cho list API keys
/// </summary>
public class ListApiKeysQuery : PaginationQuery
{
    /// <summary>
    /// Status - Lọc theo trạng thái
    /// </summary>
    public string? Status { get; set; }
    
    /// <summary>
    /// Scope - Lọc theo scope
    /// </summary>
    public string? Scope { get; set; }
    
    /// <summary>
    /// Search - Tìm kiếm theo tên
    /// </summary>
    public string? Search { get; set; }
}

/// <summary>
/// Usage Stats Query - Query parameters cho usage statistics
/// </summary>
public class UsageStatsQuery
{
    /// <summary>
    /// Period - Khoảng thời gian (today/week/month/custom)
    /// </summary>
    public string Period { get; set; } = "week";
    
    /// <summary>
    /// Start Date - Ngày bắt đầu (cho custom period)
    /// </summary>
    public DateTime? StartDate { get; set; }
    
    /// <summary>
    /// End Date - Ngày kết thúc (cho custom period)
    /// </summary>
    public DateTime? EndDate { get; set; }
    
    /// <summary>
    /// Granularity - Độ chi tiết (hour/day)
    /// </summary>
    public string Granularity { get; set; } = "day";
}

/// <summary>
/// Activity Logs Query - Query parameters cho activity logs
/// </summary>
public class ActivityLogsQuery : PaginationQuery
{
    /// <summary>
    /// Start Date - Ngày bắt đầu
    /// </summary>
    public DateTime? StartDate { get; set; }
    
    /// <summary>
    /// End Date - Ngày kết thúc
    /// </summary>
    public DateTime? EndDate { get; set; }
    
    /// <summary>
    /// Status Code - Lọc theo HTTP status code
    /// </summary>
    public int? StatusCode { get; set; }
    
    /// <summary>
    /// Endpoint - Lọc theo endpoint pattern
    /// </summary>
    public string? Endpoint { get; set; }
}
```

---

## 🎯 Validation Attributes

### Custom Validation Attributes
```csharp
/// <summary>
/// Valid API Key Scopes Attribute - Validate API key scopes
/// </summary>
public class ValidApiKeyScopesAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not List<string> scopes)
            return new ValidationResult("Scopes must be a list of strings");
        
        if (scopes.Count == 0)
            return new ValidationResult("At least one scope is required");
        
        var invalidScopes = scopes.Where(s => !ApiKeyScope.AllScopes.Contains(s)).ToList();
        if (invalidScopes.Any())
            return new ValidationResult($"Invalid scopes: {string.Join(", ", invalidScopes)}");
        
        return ValidationResult.Success;
    }
}

/// <summary>
/// Valid IP Address List Attribute - Validate IP address list
/// </summary>
public class ValidIpAddressListAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not List<string> ipList)
            return ValidationResult.Success; // Optional field
        
        foreach (var ip in ipList)
        {
            if (!IsValidIpOrCidr(ip))
                return new ValidationResult($"Invalid IP address or CIDR: {ip}");
        }
        
        return ValidationResult.Success;
    }
    
    private static bool IsValidIpOrCidr(string ipOrCidr)
    {
        // Implementation to validate IP address or CIDR notation
        // This is a simplified version
        return System.Net.IPAddress.TryParse(ipOrCidr.Split('/')[0], out _);
    }
}
```

---

*Last updated: December 28, 2024*
*Status: Design Complete, Ready for Implementation* 