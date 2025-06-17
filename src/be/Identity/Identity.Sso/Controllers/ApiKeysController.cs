using Identity.Application.Services.ApiKeys;
using Identity.Contracts.ApiKeys;
using Identity.Contracts.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Identity.Sso.Controllers;

/// <summary>
/// Controller for API key management operations (EN)<br/>
/// Controller để quản lý các thao tác khóa API (VI)
/// </summary>
/// <param name="apiKeyService">
/// Service for API key operations (EN)<br/>
/// Dịch vụ cho các thao tác khóa API (VI)
/// </param>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ApiKeysController(IApiKeyService apiKeyService) : ControllerBase
{
    private const string AdminRole = "Admin";
    private const string ApiKeyNotFoundMessage = "API key not found";

    /// <summary>
    /// Get current user's API keys
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<ApiKeyResponse>>>> GetMyApiKeys()
    {
        var userId = GetCurrentUserId();
        var apiKeys = await apiKeyService.GetUserApiKeysAsync(userId);
        
        return Ok(new ApiResponse<List<ApiKeyResponse>>
        {
            Success = true,
            Message = "API keys retrieved successfully",
            Data = apiKeys.ToList()
        });
    }

    /// <summary>
    /// Get API key by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ApiKeyResponse>>> GetApiKey(Guid id)
    {
        var userId = GetCurrentUserId();
        var apiKey = await apiKeyService.GetApiKeyByIdAsync(id);
        
        if (apiKey == null)
        {
            return NotFound(new ApiResponse<ApiKeyResponse>
            {
                Success = false,
                Message = ApiKeyNotFoundMessage
            });
        }

        // Ensure user can only access their own API keys (unless admin)
        if (apiKey.UserId != userId && !User.IsInRole(AdminRole))
        {
            return Forbid();
        }

        return Ok(new ApiResponse<ApiKeyResponse>
        {
            Success = true,
            Message = "API key retrieved successfully",
            Data = apiKey
        });
    }    /// <summary>
    /// Create new API key
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<ApiKeyResponse>>> CreateApiKey(
        [FromBody] CreateApiKeyRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await apiKeyService.CreateApiKeyAsync(userId, request);
        
        return CreatedAtAction(
            nameof(GetApiKey),
            new { id = result.Id },
            new ApiResponse<ApiKeyResponse>
            {
                Success = true,
                Message = "API key created successfully",
                Data = result
            });
    }

    /// <summary>
    /// Update API key
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ApiKeyResponse>>> UpdateApiKey(
        Guid id,
        [FromBody] UpdateApiKeyRequest request)
    {        var userId = GetCurrentUserId();
        
        // Verify ownership
        var existingApiKey = await apiKeyService.GetApiKeyByIdAsync(id);
        if (existingApiKey == null)
        {
            return NotFound(new ApiResponse<ApiKeyResponse>
            {
                Success = false,
                Message = ApiKeyNotFoundMessage
            });
        }

        if (existingApiKey.UserId != userId && !User.IsInRole(AdminRole))
        {
            return Forbid();
        }

        var apiKey = await apiKeyService.UpdateApiKeyAsync(id, request);
        
        return Ok(new ApiResponse<ApiKeyResponse>
        {
            Success = true,
            Message = "API key updated successfully",
            Data = apiKey
        });
    }    /// <summary>
    /// Revoke API key
    /// </summary>
    [HttpPost("{id:guid}/revoke")]
    public async Task<ActionResult<ApiResponse<object>>> RevokeApiKey(Guid id)
    {
        var userId = GetCurrentUserId();
        
        // Verify ownership
        var existingApiKey = await apiKeyService.GetApiKeyByIdAsync(id);
        if (existingApiKey == null)
        {
            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = ApiKeyNotFoundMessage
            });
        }

        if (existingApiKey.UserId != userId && !User.IsInRole(AdminRole))
        {
            return Forbid();
        }

        await apiKeyService.RevokeApiKeyAsync(id);
        
        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "API key revoked successfully"
        });
    }    /// <summary>
    /// Delete API key
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteApiKey(Guid id)
    {
        var userId = GetCurrentUserId();
        
        // Verify ownership
        var existingApiKey = await apiKeyService.GetApiKeyByIdAsync(id);
        if (existingApiKey == null)
        {
            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = ApiKeyNotFoundMessage
            });
        }

        if (existingApiKey.UserId != userId && !User.IsInRole(AdminRole))
        {
            return Forbid();
        }

        await apiKeyService.DeleteApiKeyAsync(id);
        
        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "API key deleted successfully"
        });
    }    /// <summary>
    /// Get all API keys (Admin only)
    /// </summary>
    [HttpGet("all")]
    [Authorize(Policy = "RequireAdmin")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ApiKeyResponse>>>> GetAllApiKeys(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null)
    {
        // For admin, we'd need a different service method that gets all users' API keys
        // For now, let's just return the current user's API keys
        var userId = GetCurrentUserId();
        var apiKeys = await apiKeyService.GetUserApiKeysAsync(userId);
        
        return Ok(new ApiResponse<IEnumerable<ApiKeyResponse>>
        {
            Success = true,
            Message = "API keys retrieved successfully",
            Data = apiKeys
        });
    }

    /// <summary>
    /// Verify API key (internal use)
    /// </summary>
    [HttpGet("verify/{key}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<VerifyApiKeyResponse>>> VerifyApiKey(string key)
    {
        var result = await apiKeyService.VerifyApiKeyAsync(key);
        
        if (result == null)
        {
            return Unauthorized(new ApiResponse<VerifyApiKeyResponse>
            {
                Success = false,
                Message = "Invalid API key"
            });
        }

        return Ok(new ApiResponse<VerifyApiKeyResponse>
        {
            Success = true,
            Message = "API key verified successfully",
            Data = result
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
