using Identity.Contracts.Authentication;

namespace Identity.Application.Common.Interfaces;

public interface IJwtTokenService
{
    TimeSpan AccessTokenLifetime { get; }
    TimeSpan RefreshTokenLifetime { get; }
    string GenerateAccessToken(UserProfile user);
    string GenerateRefreshToken();
    Task<string?> ValidateTokenAsync(string token);
}