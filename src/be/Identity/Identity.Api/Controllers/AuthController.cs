using Identity.Api.Models;
using Identity.Api.Services;
using Identity.Application.Services.RefreshTokens;
using Identity.Contracts.Authentication;
using Identity.Contracts.RefreshTokens;
using Microsoft.AspNetCore.Mvc;
using LoginResponse = Identity.Api.Models.LoginResponse;
using RefreshTokenRequest = Identity.Contracts.Authentication.RefreshTokenRequest;
using RefreshTokenResponse = Identity.Contracts.Authentication.RefreshTokenResponse;

namespace Identity.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(
    ITokenVerificationService tokenVerificationService,
    IUserService userService,
    IJwtService jwtService,
    IRefreshTokenService refreshTokenService,
    ILogger<AuthController> logger)
    : ControllerBase
{
    /// <summary>
    ///     Authenticate user with social login token
    /// </summary>
    [HttpPost("social-login")]
    public async Task<ActionResult<LoginResponse>> SocialLogin([FromBody] SocialLoginRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.Provider) || string.IsNullOrEmpty(request.Token))
                return BadRequest("Provider and token are required");

            // Verify the token with the social provider
            var socialUserInfo = await tokenVerificationService.VerifyTokenAsync(request.Provider, request.Token);

            if (socialUserInfo == null) return Unauthorized("Invalid token");

            // Get or create user
            var user = await userService.GetOrCreateUserAsync(socialUserInfo);

            if (user == null) return StatusCode(500, "Failed to process user information");

            // Check if user is active
            if (!user.IsActive) return Unauthorized("Account is deactivated"); // Generate JWT access token

            // Tạo JWT access token
            var accessToken = jwtService.GenerateAccessToken(user);

            // Generate and store refresh token in database
            // Tạo và lưu refresh token vào database
            var refreshToken = await refreshTokenService.GenerateRefreshTokenAsync(user.Id);

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
    ///     Validate access token
    /// </summary>
    [HttpPost("validate-token")]
    public ActionResult ValidateToken([FromBody] string token)
    {
        try
        {
            if (string.IsNullOrEmpty(token)) return BadRequest("Token is required");

            var principal = jwtService.ValidateToken(token);

            if (principal == null) return Unauthorized("Invalid token");

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
    ///     Refresh access token using refresh token
    ///     Làm mới access token bằng refresh token
    /// </summary>
    /// [HttpPost("refresh-token")]
    public async Task<ActionResult<RefreshTokenResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        try
        {
            // Validate refresh token and get user ID
            // Validate refresh token và lấy user ID
            var userId = await refreshTokenService.ValidateRefreshTokenAsync(request.RefreshToken);
            if (!userId.HasValue) return BadRequest("Invalid or expired refresh token");

            // Get user information
            // Lấy thông tin user
            var user = await userService.GetUserByIdAsync(userId.Value);
            if (user == null || !user.IsActive) return BadRequest("User not found or inactive");

            // Rotate refresh token (revoke old, generate new)
            // Xoay vòng refresh token (thu hồi cũ, tạo mới)
            var newRefreshToken = await refreshTokenService.RotateRefreshTokenAsync(request.RefreshToken);
            if (newRefreshToken == null)
                return BadRequest("Failed to rotate refresh token"); // Generate new access token using JWT service

            // Tạo access token mới bằng JWT service
            var newAccessToken = jwtService.GenerateAccessToken(user);
            var response = new RefreshTokenResponse(
                newAccessToken,
                newRefreshToken,
                DateTime.UtcNow.AddHours(1) // 1 hour expiration for access token
            );

            logger.LogInformation("Successfully refreshed tokens for user {UserId}", userId.Value);
            return Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error refreshing token");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    ///     Logout user and revoke refresh tokens
    ///     Đăng xuất user và thu hồi refresh tokens
    /// </summary>
    [HttpPost("logout")]
    public async Task<ActionResult> Logout([FromBody] LogoutRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.RefreshToken)) return BadRequest("Refresh token is required");

            // Revoke the specific refresh token
            // Thu hồi refresh token cụ thể
            var revokeSuccess = await refreshTokenService.RevokeRefreshTokenAsync(
                request.RefreshToken,
                "user_logout"
            );

            if (!revokeSuccess) logger.LogWarning("Failed to revoke refresh token during logout");
            // Still return success since user intent is to logout
            // Vẫn trả về success vì ý định của user là logout
            logger.LogInformation("User logged out successfully");
            return Ok(new { Message = "Logged out successfully" });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during logout");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    ///     Revoke all refresh tokens for current user
    ///     Thu hồi tất cả refresh token của user hiện tại
    /// </summary>
    [HttpPost("revoke-all-tokens")]
    public async Task<ActionResult> RevokeAllTokens([FromBody] RevokeAllTokensRequest request)
    {
        try
        {
            if (request.UserId == Guid.Empty) return BadRequest("Valid user ID is required");

            // Revoke all refresh tokens for the user
            // Thu hồi tất cả refresh token của user
            var revokedCount = await refreshTokenService.RevokeAllUserTokensAsync(
                request.UserId,
                "admin_revoke_all"
            );

            logger.LogInformation("Revoked {Count} refresh tokens for user {UserId}", revokedCount, request.UserId);

            return Ok(new
            {
                Message = "All refresh tokens revoked successfully",
                RevokedCount = revokedCount
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error revoking all tokens for user {UserId}", request.UserId);
            return StatusCode(500, "Internal server error");
        }
    }
}