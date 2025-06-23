using Identity.Contracts;
using Identity.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ApiKeysController(IApiKeyService apiKeyService, ILogger<ApiKeysController> logger)
    : ControllerBase
{
    /// <summary>
    ///     Create a new API key for the current user
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<CreateApiKeyResponse>> CreateApiKey([FromBody] CreateApiKeyRequest request)
    {
        try
        {
            var userIdClaim = User.FindFirst("user_id")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                return BadRequest("Invalid user ID");

            if (string.IsNullOrEmpty(request.Name)) return BadRequest("API key name is required");

            var response = await apiKeyService.CreateApiKeyAsync(userId, request);

            if (response == null) return StatusCode(500, "Failed to create API key");

            logger.LogInformation("API key {KeyId} created for user {UserId}", response.Id, userId);

            return Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating API key");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    ///     Get all API keys for the current user
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<ApiKeyInfo>>> GetApiKeys()
    {
        try
        {
            var userIdClaim = User.FindFirst("user_id")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                return BadRequest("Invalid user ID");

            var apiKeys = await apiKeyService.GetUserApiKeysAsync(userId);
            return Ok(apiKeys);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting API keys");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    ///     Get specific API key information
    /// </summary>
    [HttpGet("{keyId:guid}")]
    public async Task<ActionResult<ApiKeyInfo>> GetApiKey(Guid keyId)
    {
        try
        {
            var userIdClaim = User.FindFirst("user_id")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                return BadRequest("Invalid user ID");

            var apiKey = await apiKeyService.GetApiKeyInfoAsync(keyId, userId);

            if (apiKey == null) return NotFound("API key not found");

            return Ok(apiKey);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting API key {KeyId}", keyId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    ///     Revoke an API key
    /// </summary>
    [HttpDelete("{keyId:guid}")]
    public async Task<ActionResult> RevokeApiKey(Guid keyId)
    {
        try
        {
            var userIdClaim = User.FindFirst("user_id")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                return BadRequest("Invalid user ID");

            var success = await apiKeyService.RevokeApiKeyAsync(keyId, userId);

            if (!success) return NotFound("API key not found");

            logger.LogInformation("API key {KeyId} revoked by user {UserId}", keyId, userId);

            return Ok(new { Message = "API key revoked successfully" });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error revoking API key {KeyId}", keyId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    ///     Validate an API key (for internal use by other services)
    /// </summary>
    [HttpPost("validate")]
    [AllowAnonymous]
    public async Task<ActionResult> ValidateApiKey([FromBody] string apiKey)
    {
        try
        {
            if (string.IsNullOrEmpty(apiKey)) return BadRequest("API key is required");

            var user = await apiKeyService.ValidateApiKeyAsync(apiKey);

            if (user == null) return Unauthorized("Invalid API key");

            return Ok(new
            {
                Valid = true,
                UserId = user.Id,
                user.Email,
                user.Name
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error validating API key");
            return StatusCode(500, "Internal server error");
        }
    }
}