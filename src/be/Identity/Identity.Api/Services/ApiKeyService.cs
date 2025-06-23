using System.Security.Cryptography;
using System.Text;
using Identity.Infrastructure.Data;
using Identity.Domain.Entities;
using Identity.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Identity.Api.Services;

public interface IApiKeyService
{
    Task<CreateApiKeyResponse?> CreateApiKeyAsync(Guid userId, CreateApiKeyRequest request);
    Task<List<ApiKeyInfo>> GetUserApiKeysAsync(Guid userId);
    Task<ApiKeyInfo?> GetApiKeyInfoAsync(Guid keyId, Guid userId);
    Task<bool> RevokeApiKeyAsync(Guid keyId, Guid userId);
    Task<User?> ValidateApiKeyAsync(string apiKey);
    Task<bool> UpdateLastUsedAsync(Guid keyId);
}

public class ApiKeyService(IdentityDbContext context, ILogger<ApiKeyService> logger) : IApiKeyService
{
    public async Task<CreateApiKeyResponse?> CreateApiKeyAsync(Guid userId, CreateApiKeyRequest request)
    {
        try
        {
            // Generate a secure API key
            var apiKey = GenerateApiKey();
            var keyHash = HashApiKey(apiKey);
            var keyPrefix = apiKey[..8]; // First 8 characters for identification

            var newApiKey = new ApiKey
            {
                UserId = userId,
                Name = request.Name,                KeyHash = keyHash,
                KeyPrefix = keyPrefix,
                Description = request.Description,
                ExpiresAt = request.ExpiresAt,
                Scopes = request.Scopes ?? []
            };

            context.ApiKeys.Add(newApiKey);
            await context.SaveChangesAsync();

            return new CreateApiKeyResponse
            {
                Id = newApiKey.Id,
                Name = newApiKey.Name,
                ApiKey = apiKey, // Return the actual key only once
                KeyPrefix = keyPrefix,
                CreatedAt = newApiKey.CreatedAt ?? DateTime.UtcNow,
                ExpiresAt = newApiKey.ExpiresAt
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create API key for user {UserId}", userId);
            return null;
        }
    }

    public async Task<List<ApiKeyInfo>> GetUserApiKeysAsync(Guid userId)
    {
        var apiKeys = await context.ApiKeys
            .Where(ak => ak.UserId == userId)
            .OrderByDescending(ak => ak.CreatedAt)
            .ToListAsync();

        return apiKeys.Select(MapToApiKeyInfo).ToList();
    }

    public async Task<ApiKeyInfo?> GetApiKeyInfoAsync(Guid keyId, Guid userId)
    {
        var apiKey = await context.ApiKeys
            .FirstOrDefaultAsync(ak => ak.Id == keyId && ak.UserId == userId);

        return apiKey != null ? MapToApiKeyInfo(apiKey) : null;
    }

    public async Task<bool> RevokeApiKeyAsync(Guid keyId, Guid userId)
    {
        try
        {
            var apiKey = await context.ApiKeys
                .FirstOrDefaultAsync(ak => ak.Id == keyId && ak.UserId == userId);

            if (apiKey == null) return false;

            apiKey.Status = Domain.Enums.ApiKeyStatus.Revoked;
            await context.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to revoke API key {KeyId} for user {UserId}", keyId, userId);
            return false;
        }
    }

    public async Task<User?> ValidateApiKeyAsync(string apiKey)
    {
        try
        {
            if (string.IsNullOrEmpty(apiKey) || apiKey.Length < 8)
                return null;

            var keyPrefix = apiKey[..8];
            var keyHash = HashApiKey(apiKey);            var dbApiKey = await context.ApiKeys
                .Include(ak => ak.User)
                .FirstOrDefaultAsync(ak => ak.KeyPrefix == keyPrefix &&
                                           ak.KeyHash == keyHash &&
                                           ak.Status == Domain.Enums.ApiKeyStatus.Active &&
                                           (ak.ExpiresAt == null || ak.ExpiresAt > DateTime.UtcNow));

            if (dbApiKey == null) return null;

            // Check if key is expired
            if (dbApiKey.ExpiresAt.HasValue && dbApiKey.ExpiresAt.Value < DateTime.UtcNow) return null;

            // Check if user is active
            if (!dbApiKey.User.IsActive) return null;

            // Update last used time asynchronously
            _ = Task.Run(async () => await UpdateLastUsedAsync(dbApiKey.Id));

            return dbApiKey.User;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to validate API key");
            return null;
        }
    }

    public async Task<bool> UpdateLastUsedAsync(Guid keyId)
    {
        try
        {
            var apiKey = await context.ApiKeys.FindAsync(keyId);
            if (apiKey == null) return false;

            apiKey.LastUsedAt = DateTime.UtcNow;
            await context.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to update last used time for API key {KeyId}", keyId);
            return false;
        }
    }

    private static string GenerateApiKey()
    {
        // Generate a secure 32-byte key and encode it in base64
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[32];
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes).Replace("/", "_").Replace("+", "-").TrimEnd('=');
    }

    private static string HashApiKey(string apiKey)
    {
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(apiKey));
        return Convert.ToBase64String(hashBytes);
    }

    private static ApiKeyInfo MapToApiKeyInfo(ApiKey apiKey)
    {
        return new ApiKeyInfo
        {
            Id = apiKey.Id,
            Name = apiKey.Name,
            KeyPrefix = apiKey.KeyPrefix,
            IsActive = apiKey.IsActive,
            CreatedAt = apiKey.CreatedAt ?? DateTime.UtcNow,
            ExpiresAt = apiKey.ExpiresAt,
            LastUsedAt = apiKey.LastUsedAt,
            Description = apiKey.Description,            Scopes = apiKey.Scopes
        };
    }
}