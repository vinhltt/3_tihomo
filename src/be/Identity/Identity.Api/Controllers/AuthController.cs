using Microsoft.AspNetCore.Mvc;
using Identity.Api.Models;
using Identity.Api.Services;

namespace Identity.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ITokenVerificationService _tokenVerificationService;
    private readonly IUserService _userService;
    private readonly IJwtService _jwtService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        ITokenVerificationService tokenVerificationService,
        IUserService userService,
        IJwtService jwtService,
        ILogger<AuthController> logger)
    {
        _tokenVerificationService = tokenVerificationService;
        _userService = userService;
        _jwtService = jwtService;
        _logger = logger;
    }

    /// <summary>
    /// Authenticate user with social login token
    /// </summary>
    [HttpPost("social-login")]
    public async Task<ActionResult<LoginResponse>> SocialLogin([FromBody] SocialLoginRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.Provider) || string.IsNullOrEmpty(request.Token))
            {
                return BadRequest("Provider and token are required");
            }

            // Verify the token with the social provider
            var socialUserInfo = await _tokenVerificationService.VerifyTokenAsync(request.Provider, request.Token);
            
            if (socialUserInfo == null)
            {
                return Unauthorized("Invalid token");
            }

            // Get or create user
            var user = await _userService.GetOrCreateUserAsync(socialUserInfo);
            
            if (user == null)
            {
                return StatusCode(500, "Failed to process user information");
            }

            // Check if user is active
            if (!user.IsActive)
            {
                return Unauthorized("Account is deactivated");
            }

            // Generate JWT tokens
            var accessToken = _jwtService.GenerateAccessToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken();
            var tokenExpiration = _jwtService.GetTokenExpiration();

            // Map user to response
            var userInfo = await _userService.MapToUserInfoAsync(user);

            var response = new LoginResponse
            {
                User = userInfo,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = tokenExpiration
            };

            _logger.LogInformation("User {UserId} logged in successfully via {Provider}", user.Id, request.Provider);
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during social login with provider {Provider}", request.Provider);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Validate access token
    /// </summary>
    [HttpPost("validate-token")]
    public ActionResult ValidateToken([FromBody] string token)
    {
        try
        {
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest("Token is required");
            }

            var principal = _jwtService.ValidateToken(token);
            
            if (principal == null)
            {
                return Unauthorized("Invalid token");
            }

            var userId = principal.FindFirst("user_id")?.Value;
            var email = principal.FindFirst("email")?.Value;
            var name = principal.FindFirst("name")?.Value;

            return Ok(new
            {
                Valid = true,
                UserId = userId,
                Email = email,
                Name = name
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating token");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Refresh access token
    /// </summary>
    [HttpPost("refresh-token")]
    public async Task<ActionResult<LoginResponse>> RefreshToken([FromBody] string refreshToken)
    {
        try
        {
            // In a real implementation, you would:
            // 1. Validate the refresh token against stored tokens
            // 2. Check if it's expired
            // 3. Get user from the refresh token
            // For now, we'll return a simplified response
            
            return BadRequest("Refresh token functionality not implemented yet");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Logout user (revoke tokens)
    /// </summary>
    [HttpPost("logout")]
    public ActionResult Logout()
    {
        try
        {
            // In a real implementation, you would:
            // 1. Add token to blacklist
            // 2. Remove refresh token from storage
            // For now, we'll return success
            
            return Ok(new { Message = "Logged out successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            return StatusCode(500, "Internal server error");
        }
    }
}
