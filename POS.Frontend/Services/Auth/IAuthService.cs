using POS.Shared.Models;
using POS.Shared.Models.Auth;

namespace POS.Frontend.Services.Auth;

public interface IAuthService
{
    Task<Result<AuthResponse>> LoginAsync(LoginRequest request);
    Task LogoutAsync();
    Task<string?> RefreshTokenAsync();
    Task<string?> GetTokenAsync();
}
