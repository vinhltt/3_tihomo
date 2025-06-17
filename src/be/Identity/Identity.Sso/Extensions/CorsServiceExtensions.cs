using Identity.Contracts.ConfigurationOptions;

namespace Identity.Sso.Extensions;

/// <summary>
/// Provides extension methods for configuring CORS in the Identity API. (EN)<br/>
/// Cung cấp các extension methods để cấu hình CORS trong Identity API. (VI)
/// </summary>
public static class CorsServiceExtensions
{
    /// <summary>
    /// Adds CORS configuration based on the provided options. (EN)<br/>
    /// Thêm cấu hình CORS dựa trên các tùy chọn được cung cấp. (VI)
    /// </summary>
    /// <param name="services">
    /// The service collection. (EN)<br/>
    /// Bộ sưu tập dịch vụ. (VI)
    /// </param>
    /// <param name="policyName">
    /// The name of the CORS policy. (EN)<br/>
    /// Tên của chính sách CORS. (VI)
    /// </param>
    /// <param name="corsOptions">
    /// The CORS options configuration. (EN)<br/>
    /// Cấu hình tùy chọn CORS. (VI)
    /// </param>
    public static void AddIdentityCors(this IServiceCollection services, string policyName, CorsOptions corsOptions)
    {
        services.AddCors(options =>
        {
            options.AddPolicy(policyName, policyBuilder =>
            {
                // Configure origins
                if (corsOptions.AllowedOrigins.IsAllowedAll())
                    policyBuilder.AllowAnyOrigin();
                else
                    policyBuilder.WithOrigins(corsOptions.AllowedOrigins);

                // Configure methods
                if (corsOptions.AllowedMethods.IsAllowedAll())
                    policyBuilder.AllowAnyMethod();
                else
                    policyBuilder.WithMethods(corsOptions.AllowedMethods);

                // Configure headers
                if (corsOptions.AllowedHeaders.IsAllowedAll())
                    policyBuilder.AllowAnyHeader();
                else
                    policyBuilder.WithHeaders(corsOptions.AllowedHeaders);

                // Configure exposed headers
                if (!corsOptions.ExposedHeaders.IsAllowedAll() && corsOptions.ExposedHeaders.Length > 0)
                    policyBuilder.WithExposedHeaders(corsOptions.ExposedHeaders);

                // Configure credentials - only if not using AllowAnyOrigin
                if (!corsOptions.AllowedOrigins.IsAllowedAll())
                {
                    policyBuilder.AllowCredentials();
                }

                // Configure preflight max age
                if (!string.IsNullOrWhiteSpace(corsOptions.PreflightMaxAgeInMinutes))
                {
                    if (int.TryParse(corsOptions.PreflightMaxAgeInMinutes, out var maxAge))
                    {
                        policyBuilder.SetPreflightMaxAge(TimeSpan.FromMinutes(maxAge));
                    }
                }
            });
        });
    }

    /// <summary>
    /// Checks if the provided collection of strings indicates that all values are allowed (contains "*"). (EN)<br/>
    /// Kiểm tra xem tập hợp chuỗi được cung cấp có cho biết tất cả các giá trị đều được phép hay không (chứa "*"). (VI)
    /// </summary>
    /// <param name="values">
    /// The collection of strings to check. (EN)<br/>
    /// Tập hợp chuỗi cần kiểm tra. (VI)
    /// </param>
    /// <returns>
    /// True if the collection is null, empty, or contains "*"; otherwise, false. (EN)<br/>
    /// True nếu tập hợp là null, rỗng hoặc chứa "*"; ngược lại là false. (VI)
    /// </returns>
    private static bool IsAllowedAll(this string[]? values)
    {
        return values == null || values.Length == 0 || values.Contains("*");
    }
}
