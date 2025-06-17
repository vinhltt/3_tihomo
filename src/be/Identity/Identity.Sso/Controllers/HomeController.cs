using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Sso.Controllers;

/// <summary>
/// Home controller for SSO portal
/// Controller trang chủ cho SSO portal
/// </summary>
[Route("")]
public class HomeController : Controller
{
    /// <summary>
    /// Default home page - redirect to appropriate page based on auth status
    /// Trang chủ mặc định - chuyển hướng dựa trên trạng thái xác thực
    /// </summary>
    [HttpGet("")]
    [HttpGet("home")]
    public IActionResult Index()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Dashboard");
        }

        return View();
    }

    /// <summary>
    /// Dashboard for authenticated users
    /// Bảng điều khiển cho người dùng đã xác thực
    /// </summary>
    [HttpGet("dashboard")]
    [Authorize]
    public IActionResult Dashboard()
    {
        return View();
    }

    /// <summary>
    /// SSO Configuration and status page
    /// Trang cấu hình và trạng thái SSO
    /// </summary>
    [HttpGet("info")]
    public IActionResult Info()
    {
        return View();
    }

    /// <summary>
    /// Error page
    /// </summary>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View();
    }
}
