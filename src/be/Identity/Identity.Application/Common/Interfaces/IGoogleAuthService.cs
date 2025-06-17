using Identity.Contracts.Authentication;

namespace Identity.Application.Common.Interfaces;

public interface IGoogleAuthService
{
    Task<GoogleUserInfo> VerifyGoogleTokenAsync(string idToken);
}
