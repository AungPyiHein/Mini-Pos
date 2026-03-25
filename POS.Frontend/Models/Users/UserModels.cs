using POS.Shared.Models;

namespace POS.Frontend.Models.Users;

public class UserResponseDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public Guid? MerchantId { get; set; }
    public Guid? BranchId { get; set; }
}

public class CreateUserRequest
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PlainPassword { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public Guid? MerchantId { get; set; }
    public Guid? BranchId { get; set; }
}

public class UpdateUserRequest
{
    public string? Email { get; set; }
    public string? PlainPassword { get; set; }
    public UserRole? Role { get; set; }
}
