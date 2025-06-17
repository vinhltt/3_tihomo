using Identity.Application.Services.Users;
using Identity.Contracts.Users;
using Identity.Contracts.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Identity.Sso.Controllers;

/// <summary>
/// Controller for user management operations (EN)<br/>
/// Controller cho các thao tác quản lý người dùng (VI)
/// </summary>
/// <param name="userService">
/// Service for user operations (EN)<br/>
/// Dịch vụ cho các thao tác người dùng (VI)
/// </param>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController(IUserService userService) : ControllerBase
{
    /// <summary>
    /// Get current user profile
    /// </summary>
    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<UserResponse>>> GetCurrentUser()
    {
        var userId = GetCurrentUserId();
        var user = await userService.GetByIdAsync(userId);
        
        if (user == null)
        {
            return NotFound(new ApiResponse<UserResponse>
            {
                Success = false,
                Message = "User not found"
            });
        }

        return Ok(new ApiResponse<UserResponse>
        {
            Success = true,
            Message = "User profile retrieved successfully",
            Data = user
        });
    }

    /// <summary>
    /// Update current user profile
    /// </summary>
    [HttpPut("me")]
    public async Task<ActionResult<ApiResponse<UserResponse>>> UpdateCurrentUser(
        [FromBody] UpdateUserRequest request)
    {
        var userId = GetCurrentUserId();
        var user = await userService.UpdateAsync(userId, request);
        
        return Ok(new ApiResponse<UserResponse>
        {
            Success = true,
            Message = "User profile updated successfully",
            Data = user
        });
    }

    /// <summary>
    /// Change password
    /// </summary>
    [HttpPost("change-password")]
    public async Task<ActionResult<ApiResponse<object>>> ChangePassword(
        [FromBody] ChangePasswordRequest request)
    {
        var userId = GetCurrentUserId();
        await userService.ChangePasswordAsync(userId, request);
        
        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Password changed successfully"
        });
    }

    /// <summary>
    /// Delete current user account
    /// </summary>
    [HttpDelete("me")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteCurrentUser()
    {
        var userId = GetCurrentUserId();
        await userService.DeleteAsync(userId);
        
        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "User account deleted successfully"
        });
    }

    /// <summary>
    /// Get all users (Admin only)
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "RequireAdmin")]
    public async Task<ActionResult<ApiResponse<PagedResponse<UserResponse>>>> GetUsers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null)
    {
        var users = await userService.GetPagedAsync(page, pageSize, search);
        
        return Ok(new ApiResponse<PagedResponse<UserResponse>>
        {
            Success = true,
            Message = "Users retrieved successfully",
            Data = users
        });
    }

    /// <summary>
    /// Get user by ID (Admin only)
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = "RequireAdmin")]
    public async Task<ActionResult<ApiResponse<UserResponse>>> GetUser(Guid id)
    {
        var user = await userService.GetByIdAsync(id);
        
        if (user == null)
        {
            return NotFound(new ApiResponse<UserResponse>
            {
                Success = false,
                Message = "User not found"
            });
        }

        return Ok(new ApiResponse<UserResponse>
        {
            Success = true,
            Message = "User retrieved successfully",
            Data = user
        });
    }

    /// <summary>
    /// Update user (Admin only)
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "RequireAdmin")]
    public async Task<ActionResult<ApiResponse<UserResponse>>> UpdateUser(
        Guid id,
        [FromBody] UpdateUserRequest request)
    {
        var user = await userService.UpdateAsync(id, request);
        
        return Ok(new ApiResponse<UserResponse>
        {
            Success = true,
            Message = "User updated successfully",
            Data = user
        });
    }

    /// <summary>
    /// Delete user (Admin only)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "RequireAdmin")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteUser(Guid id)
    {
        await userService.DeleteAsync(id);
        
        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "User deleted successfully"
        });
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                         User.FindFirst("sub")?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid user ID in token");
        }
        
        return userId;
    }
}
