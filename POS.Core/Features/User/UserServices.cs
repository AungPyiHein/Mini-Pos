using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using POS.Core.Common;
using POS.data.Data;

namespace POS.Core.Features.User
{


    public class CreateUserRequest
    {
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PlainPassword { get; set; } = null!;
        public UserRole Role { get; set; } 
        public Guid? MerchantId { get; set; }
        public Guid? BranchId { get; set; }
    }
    public class UserResponseDto
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = null!;
        public string Role { get; set; } = null!;
        public bool IsActive { get; set; }
    }

    public interface IUserServices
    {
        Task<Result<Guid>> CreateUserAsync(CreateUserRequest request);
    }

    public class UserServices : IUserServices
    {
        private readonly AppDbContext _context;
        private readonly PasswordHasher<POS.data.Entities.User> _passwordHasher;

        public UserServices(AppDbContext context)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<POS.data.Entities.User>();
        }

        public async Task<Result<Guid>> CreateUserAsync(CreateUserRequest request)
        {
            var userExists = await _context.Users.AnyAsync(u => u.Username == request.Username || u.Email == request.Email);
            if (userExists) return Result<Guid>.Failure("Username or Email already exists.");

            var user = new data.Entities.User
            {
                Id = Guid.NewGuid(),
                Username = request.Username,
                Email = request.Email,
                MerchantId = request.MerchantId,
                BranchId = request.BranchId, 
                Role = request.Role.ToString(), 
                CreatedAt = DateTime.UtcNow
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, request.PlainPassword);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Result<Guid>.Success(user.Id);
        }
    }


}
