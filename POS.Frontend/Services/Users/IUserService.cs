using POS.Frontend.Models;
using POS.Frontend.Models.Users;

namespace POS.Frontend.Services.Users;

public interface IUserService
{
    Task<ApiResponse<IEnumerable<UserResponseDto>>> GetAllUsersAsync();
    Task<ApiResponse<UserResponseDto>> GetUserByIdAsync(Guid id);
    Task<ApiResponse<Guid>> CreateUserAsync(CreateUserRequest request);
    Task<ApiResponse> UpdateUserAsync(Guid id, UpdateUserRequest request);
    Task<ApiResponse> DeleteUserAsync(Guid id);
}
