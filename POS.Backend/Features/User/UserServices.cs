using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using POS.Backend.Common;
using POS.data.Data;

namespace POS.Backend.Features.User
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
    public class UpdateUserRequest
    {
        public string? Email { get; set; }
        public string? PlainPassword { get; set; }
        public UserRole? Role { get; set; }
    }

    public class UserResponseDto
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Role { get; set; } = null!;
        public bool IsActive { get; set; }
        public Guid? MerchantId { get; set; }
        public Guid? BranchId { get; set; }
    }

    public interface IUserServices
    {
        Task<Result<Guid>> CreateUserAsync(CreateUserRequest request);
        Task<Result<UserResponseDto>> GetUserByIdAsync(Guid id);
        Task<Result<PagedResponse<UserResponseDto>>> GetAllUsersAsync(PaginationFilter filter);
        Task<Result<bool>> UpdateUserAsync(Guid id, UpdateUserRequest request);
        Task<Result<bool>> DeleteUserAsync(Guid id);
    }

    public class UserServices : IUserServices
    {
        private readonly AppDbContext _context;
        private readonly PasswordHasher<POS.data.Entities.User> _passwordHasher;
        private readonly ICurrentUserService _currentUser;

        public UserServices(AppDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<POS.data.Entities.User>();
            _currentUser = currentUser;
        }

        public async Task<Result<Guid>> CreateUserAsync(CreateUserRequest request)
        {
            var userExists = await _context.Users.AnyAsync(u => (u.Username == request.Username || u.Email == request.Email) && u.DeletedAt == null);
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

        public async Task<Result<UserResponseDto>> GetUserByIdAsync(Guid id)
        {
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id && u.DeletedAt == null);
            if (user == null) return Result<UserResponseDto>.Failure("User not found.");

            return Result<UserResponseDto>.Success(new UserResponseDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                IsActive = user.DeletedAt == null,
                MerchantId = user.MerchantId,
                BranchId = user.BranchId
            });
        }

        public async Task<Result<PagedResponse<UserResponseDto>>> GetAllUsersAsync(PaginationFilter filter)
        {
            var query = _context.Users
                .AsNoTracking()
                .Where(u => u.DeletedAt == null)
                .AsQueryable();

            if (_currentUser.Role == POS.Shared.Models.UserRole.MerchantAdmin)
            {
                query = query.Where(u => u.MerchantId == _currentUser.MerchantId);
            }

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                query = query.Where(u => EF.Functions.Like(u.Username, $"%{filter.SearchTerm}%") || 
                                         EF.Functions.Like(u.Email, $"%{filter.SearchTerm}%"));
            }

            var totalRecords = await query.CountAsync();

            var users = await query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(u => new UserResponseDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    Role = u.Role,
                    IsActive = u.DeletedAt == null,
                    MerchantId = u.MerchantId,
                    BranchId = u.BranchId
                })
                .ToListAsync();

            var pagedResponse = new PagedResponse<UserResponseDto>(users, totalRecords, filter.PageNumber, filter.PageSize);
            return Result<PagedResponse<UserResponseDto>>.Success(pagedResponse);
        }

        public async Task<Result<bool>> UpdateUserAsync(Guid id, UpdateUserRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id && u.DeletedAt == null);
            if (user == null) return Result<bool>.Failure("User not found.");

            if (!string.IsNullOrEmpty(request.Email) && request.Email != user.Email)
            {
                var emailExists = await _context.Users.AnyAsync(u => u.Email == request.Email && u.Id != id && u.DeletedAt == null);
                if (emailExists) return Result<bool>.Failure("Email already in use.");
                user.Email = request.Email;
            }

            if (request.Role.HasValue)
            {
                user.Role = request.Role.Value.ToString();
            }

            if (!string.IsNullOrEmpty(request.PlainPassword))
            {
                user.PasswordHash = _passwordHasher.HashPassword(user, request.PlainPassword);
            }

            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> DeleteUserAsync(Guid id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id && u.DeletedAt == null);
            if (user == null) return Result<bool>.Failure("User not found.");

            user.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Result<bool>.Success(true);
        }
    }


}
