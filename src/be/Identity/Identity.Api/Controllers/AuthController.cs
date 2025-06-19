using Microsoft.AspNetCore.Mvc;
using Identity.Api.Models;
using Identity.Api.Services;

namespace Identity.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(
    ITokenVerificationService tokenVerificationService,
    IUserService userService,
    IJwtService jwtService,
    ILogger<AuthController> logger)
    : ControllerBase
{
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
            var socialUserInfo = await tokenVerificationService.VerifyTokenAsync(request.Provider, request.Token);
            
            if (socialUserInfo == null)
            {
                return Unauthorized("Invalid token");
            }

            // Get or create user
            var user = await userService.GetOrCreateUserAsync(socialUserInfo);
            
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
            var accessToken = jwtService.GenerateAccessToken(user);
            var refreshToken = jwtService.GenerateRefreshToken();
            var tokenExpiration = jwtService.GetTokenExpiration();

            // Map user to response
            var userInfo = await userService.MapToUserInfoAsync(user);

            var response = new LoginResponse
            {
                User = userInfo,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = tokenExpiration
            };

            logger.LogInformation("User {UserId} logged in successfully via {Provider}", user.Id, request.Provider);
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during social login with provider {Provider}", request.Provider);
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

            var principal = jwtService.ValidateToken(token);
            
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
            logger.LogError(ex, "Error validating token");
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
            logger.LogError(ex, "Error refreshing token");
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
            logger.LogError(ex, "Error during logout");
            return StatusCode(500, "Internal server error");
        }
    }
}
