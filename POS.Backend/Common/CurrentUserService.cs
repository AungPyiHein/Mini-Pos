using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using POS.Shared.Models;
using System.IdentityModel.Tokens.Jwt;

namespace POS.Backend.Common;

public interface ICurrentUserService
{
    Guid UserId { get; }
    string Username { get; }
    UserRole Role { get; }
    Guid? MerchantId { get; }
    Guid? BranchId { get; }
    bool IsAuthenticated { get; }
    void SetUser(POS.data.Entities.User user);
}

public class CurrentUserService : ICurrentUserService
{
    private POS.data.Entities.User? _cachedUser;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public void SetUser(POS.data.Entities.User user)
    {
        _cachedUser = user;
    }

    public Guid UserId
    {
        get
        {
            if (_cachedUser != null) return _cachedUser.Id;
            var id = User?.FindFirst("UserId")?.Value 
                     ?? User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(id, out var result) ? result : Guid.Empty;
        }
    }

    public string Username => _cachedUser?.Username 
                              ?? User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                              ?? User?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                              ?? string.Empty;

    public UserRole Role
    {
        get
        {
            if (_cachedUser != null)
            {
                return Enum.TryParse<UserRole>(_cachedUser.Role, true, out var r) ? r : UserRole.Staff;
            }
            var roleStr = User?.FindFirst(ClaimTypes.Role)?.Value 
                          ?? User?.FindFirst("role")?.Value;
            return Enum.TryParse<UserRole>(roleStr, true, out var role) ? role : UserRole.Staff;
        }
    }

    public Guid? MerchantId => _cachedUser != null ? _cachedUser.MerchantId : GetClaimGuid("MerchantId");

    public Guid? BranchId => _cachedUser != null ? _cachedUser.BranchId : GetClaimGuid("BranchId");

    private Guid? GetClaimGuid(string claimType)
    {
        var idString = User?.FindFirst(claimType)?.Value;
        return string.IsNullOrWhiteSpace(idString) ? null : Guid.Parse(idString);
    }

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;
}
