using System.Net.Http.Json;
using POS.Frontend.Models;
using POS.Frontend.Models.Users;

namespace POS.Frontend.Services.Users;

public class UserService : IUserService
{
    private readonly HttpClient _http;

    public UserService(HttpClient http)
    {
        _http = http;
    }

    public async Task<ApiResponse<IEnumerable<UserResponseDto>>> GetAllUsersAsync()
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<IEnumerable<UserResponseDto>>>("/api/users");
        return response ?? new ApiResponse<IEnumerable<UserResponseDto>> { Message = "Error connecting to server", Data = Enumerable.Empty<UserResponseDto>() };
    }

    public async Task<ApiResponse<UserResponseDto>> GetUserByIdAsync(Guid id)
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<UserResponseDto>>($"/api/users/{id}");
        return response ?? new ApiResponse<UserResponseDto> { Message = "Error connecting to server" };
    }

    public async Task<ApiResponse<Guid>> CreateUserAsync(CreateUserRequest request)
    {
        var response = await _http.PostAsJsonAsync("/api/users", request);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<Guid>>();
        return result ?? new ApiResponse<Guid> { Message = "Error connecting to server" };
    }

    public async Task<ApiResponse> UpdateUserAsync(Guid id, UpdateUserRequest request)
    {
        var response = await _http.PutAsJsonAsync($"/api/users/{id}", request);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse>();
        return result ?? new ApiResponse { Message = "Error connecting to server" };
    }

    public async Task<ApiResponse> DeleteUserAsync(Guid id)
    {
        var response = await _http.DeleteAsync($"/api/users/{id}");
        var result = await response.Content.ReadFromJsonAsync<ApiResponse>();
        return result ?? new ApiResponse { Message = "Error connecting to server" };
    }
}
