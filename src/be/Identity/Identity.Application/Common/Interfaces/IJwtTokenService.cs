using Identity.Contracts.Authentication;

namespace Identity.Application.Common.Interfaces;

public interface IJwtTokenService
{
    string GenerateAccessToken(UserProfile user);
    string GenerateRefreshToken();
    Task<string?> ValidateTokenAsync(string token);
    TimeSpan AccessTokenLifetime { get; }
    TimeSpan RefreshTokenLifetime { get; }
}
