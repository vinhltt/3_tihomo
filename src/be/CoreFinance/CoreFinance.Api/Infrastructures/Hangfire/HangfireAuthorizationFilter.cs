using Hangfire.Dashboard;

namespace CoreFinance.Api.Infrastructures.Hangfire;

/// <summary>
///     Hangfire authorization filter for dashboard access (EN)<br/>
///     Bộ lọc ủy quyền Hangfire cho truy cập dashboard (VI)
/// </summary>
public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    /// <summary>
    ///     Authorize access to Hangfire dashboard (EN)<br/>
    ///     Ủy quyền truy cập dashboard Hangfire (VI)
    /// </summary>
    /// <param name="context">Dashboard context</param>
    /// <returns>True if authorized</returns>
    public bool Authorize(DashboardContext context)
    {
        // In development, allow all access
        // In production, you should implement proper authorization
        var httpContext = context.GetHttpContext();
        
        // For now, allow all access in development
        // TODO: Implement proper authentication/authorization
        return true;
    }
}