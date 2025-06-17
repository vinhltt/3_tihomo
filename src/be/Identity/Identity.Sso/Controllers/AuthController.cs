using Identity.Application.Services.Authentication;
using Identity.Application.Services.Users;
using Identity.Contracts.Authentication;
using Identity.Contracts.Users;
using Identity.Contracts.Common;
using Identity.Sso.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Identity.Sso.Controllers;

/// <summary>
/// Authentication controller for SSO portal
/// Controller xác thực cho SSO portal
/// </summary>
[Route("auth")]
public class AuthController(IAuthService authService, IUserService userService) : Controller
{
    /// <summary>
    /// Display login page
    /// Hiển thị trang đăng nhập
    /// </summary>
    [HttpGet("login")]
    public async Task<IActionResult> Login(string? returnUrl = null)
    {
        // Clear any existing external cookie to ensure a clean login process
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        var model = new LoginViewModel
        {
            ReturnUrl = returnUrl
        };

        return View(model);
    }

    /// <summary>
    /// Handle login form submission
    /// Xử lý form đăng nhập
    /// </summary>
    [HttpPost("login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {            var loginRequest = new LoginRequest(model.UsernameOrEmail, model.Password);

            var loginResponse = await authService.LoginAsync(loginRequest);

            // Create claims for the authenticated user
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, loginResponse.User.Id.ToString()),
                new(ClaimTypes.Name, loginResponse.User.FullName),
                new(ClaimTypes.Email, loginResponse.User.Email),
                new("username", loginResponse.User.Username)
            };

            // Add roles to claims
            foreach (var role in loginResponse.User.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(24)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            return RedirectToAction("Dashboard", "Home");
        }
        catch (UnauthorizedAccessException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
        catch (Exception)
        {
            ModelState.AddModelError(string.Empty, "An error occurred during login. Please try again.");
            return View(model);
        }
    }

    /// <summary>
    /// Display registration page
    /// Hiển thị trang đăng ký
    /// </summary>
    [HttpGet("register")]
    public IActionResult Register(string? returnUrl = null)
    {
        var model = new RegisterViewModel
        {
            ReturnUrl = returnUrl
        };

        return View(model);
    }

    /// <summary>
    /// Handle registration form submission
    /// Xử lý form đăng ký
    /// </summary>
    [HttpPost("register")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {            var createUserRequest = new CreateUserRequest(
                model.Email,
                model.Username,
                model.FullName,
                model.FullName.Split(' ').FirstOrDefault() ?? "",
                model.FullName.Split(' ').LastOrDefault() ?? "",
                null,
                model.Password);

            await userService.CreateAsync(createUserRequest);// Auto-login after successful registration
            var loginRequest = new LoginRequest(model.Username, model.Password);

            var loginResponse = await authService.LoginAsync(loginRequest);

            // Create claims for the authenticated user
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, loginResponse.User.Id.ToString()),
                new(ClaimTypes.Name, loginResponse.User.FullName),
                new(ClaimTypes.Email, loginResponse.User.Email),
                new("username", loginResponse.User.Username)
            };

            // Add roles to claims
            foreach (var role in loginResponse.User.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = false,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(24)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            return RedirectToAction("Dashboard", "Home");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    /// <summary>
    /// Handle logout
    /// Xử lý đăng xuất
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout(string? returnUrl = null)
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }        return RedirectToAction("Index", "Home");
    }

    /// <summary>
    /// Access denied page
    /// Trang từ chối truy cập
    /// </summary>
    [HttpGet("access-denied")]
    public IActionResult AccessDenied()
    {
        return View();
    }

    #region API Endpoints - Merged from Identity.Api

    /// <summary>
    /// Traditional login with username/email and password (API endpoint)
    /// </summary>
    [HttpPost("api/login")]
    public async Task<ActionResult<LoginResponse>> LoginAsync([FromBody] LoginRequest request)
    {
        try
        {
            var response = await authService.LoginAsync(request);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Register a new user (API endpoint - Temporary for testing)
    /// </summary>
    [HttpPost("api/register")]
    public async Task<ActionResult<ApiResponse<UserResponse>>> RegisterAsync([FromBody] CreateUserRequest request)
    {
        try
        {
            var user = await userService.CreateAsync(request);
            return Ok(new ApiResponse<UserResponse>
            {
                Success = true,
                Message = "User registered successfully",
                Data = user
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiResponse<UserResponse>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new ApiResponse<UserResponse>
            {
                Success = false,
                Message = ex.Message
            });
        }
    }

    /// <summary>
    /// Google OAuth2 login (API endpoint)
    /// </summary>
    [HttpPost("api/login/google")]
    public async Task<ActionResult<LoginResponse>> GoogleLoginAsync([FromBody] GoogleLoginRequest request)
    {
        try
        {
            var response = await authService.GoogleLoginAsync(request);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Refresh access token using refresh token (API endpoint)
    /// </summary>
    [HttpPost("api/token/refresh")]
    public async Task<ActionResult<RefreshTokenResponse>> RefreshTokenAsync([FromBody] RefreshTokenRequest request)
    {
        try
        {
            var response = await authService.RefreshTokenAsync(request);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Logout and revoke refresh token (API endpoint)
    /// </summary>
    [HttpPost("api/logout")]
    [Authorize]
    public async Task<IActionResult> LogoutApiAsync([FromBody] LogoutRequest request)
    {
        try
        {
            await authService.LogoutAsync(request);
            return Ok(new { message = "Logged out successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Verify API key (internal use by API Gateway)
    /// </summary>
    [HttpPost("api/apikey/verify")]
    public async Task<ActionResult<ApiKeyVerificationResponse>> VerifyApiKeyAsync([FromBody] string apiKey)
    {
        try
        {
            var response = await authService.VerifyApiKeyAsync(apiKey);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    #endregion
}
