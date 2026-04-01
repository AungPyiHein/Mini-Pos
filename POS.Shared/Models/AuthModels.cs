namespace POS.Shared.Models;

public class LoginRequest
{
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
}

public class AuthResponse
{
    public string Token { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
    public DateTime RefreshTokenExpiration { get; set; }
    public string Username { get; set; } = null!;
    public string Role { get; set; } = null!;
    public Guid? MerchantId { get; set; }
}

public class RefreshTokenRequest
{
    public string Token { get; set; } = null!;
}
