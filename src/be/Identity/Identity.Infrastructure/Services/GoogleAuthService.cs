using Google.Apis.Auth;
using Identity.Application.Common.Interfaces;
using Identity.Contracts.Authentication;
using Microsoft.Extensions.Configuration;

namespace Identity.Infrastructure.Services;

public class GoogleAuthService(IConfiguration configuration) : IGoogleAuthService
{
    private readonly string _googleClientId = configuration["Google:ClientId"] ??
                                              throw new InvalidOperationException("Google Client ID not configured");

    public async Task<GoogleUserInfo> VerifyGoogleTokenAsync(string idToken)
    {
        try
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken,
                new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = [_googleClientId]
                });

            return new GoogleUserInfo(
                payload.Subject,
                payload.Email,
                payload.Name,
                payload.Picture,
                payload.EmailVerified
            );
        }
        catch (Exception ex)
        {
            throw new UnauthorizedAccessException("Invalid Google token", ex);
        }
    }
}