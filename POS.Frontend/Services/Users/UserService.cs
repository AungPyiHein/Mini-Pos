using System.Net.Http.Json;
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

public class UserService : IUserService
{
    private readonly HttpClient _http;

    public UserService(HttpClient http)
    {
        _http = http;
    }

    public async Task<ApiResponse<IEnumerable<UserResponseDto>>> GetAllUsersAsync()
    {
        try
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<IEnumerable<UserResponseDto>>>("/api/users");
            if (response != null) response.IsSuccess = true;
            return response ?? new ApiResponse<IEnumerable<UserResponseDto>> { IsSuccess = false, Message = "Error connecting to server", Data = Enumerable.Empty<UserResponseDto>() };
        }
        catch (Exception ex)
        {
            return new ApiResponse<IEnumerable<UserResponseDto>> { IsSuccess = false, Message = $"Error: {ex.Message}", Data = Enumerable.Empty<UserResponseDto>() };
        }
    }

    public async Task<ApiResponse<UserResponseDto>> GetUserByIdAsync(Guid id)
    {
        try
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<UserResponseDto>>($"/api/users/{id}");
            if (response != null) response.IsSuccess = true;
            return response ?? new ApiResponse<UserResponseDto> { IsSuccess = false, Message = "Error connecting to server" };
        }
        catch (Exception ex)
        {
            return new ApiResponse<UserResponseDto> { IsSuccess = false, Message = $"Error: {ex.Message}" };
        }
    }

    public async Task<ApiResponse<Guid>> CreateUserAsync(CreateUserRequest request)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("/api/users", request);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<Guid>>();
            if (result != null) result.IsSuccess = response.IsSuccessStatusCode;
            return result ?? new ApiResponse<Guid> { IsSuccess = false, Message = "Error connecting to server" };
        }
        catch (Exception ex)
        {
            return new ApiResponse<Guid> { IsSuccess = false, Message = $"Error: {ex.Message}" };
        }
    }

    public async Task<ApiResponse> UpdateUserAsync(Guid id, UpdateUserRequest request)
    {
        try
        {
            var response = await _http.PutAsJsonAsync($"/api/users/{id}", request);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse>();
            if (result != null) result.IsSuccess = response.IsSuccessStatusCode;
            return result ?? new ApiResponse { IsSuccess = false, Message = "Error connecting to server" };
        }
        catch (Exception ex)
        {
            return new ApiResponse { IsSuccess = false, Message = $"Error: {ex.Message}" };
        }
    }

    public async Task<ApiResponse> DeleteUserAsync(Guid id)
    {
        try
        {
            var response = await _http.DeleteAsync($"/api/users/{id}");
            var result = await response.Content.ReadFromJsonAsync<ApiResponse>();
            if (result != null) result.IsSuccess = response.IsSuccessStatusCode;
            return result ?? new ApiResponse { IsSuccess = false, Message = "Error connecting to server" };
        }
        catch (Exception ex)
        {
            return new ApiResponse { IsSuccess = false, Message = $"Error: {ex.Message}" };
        }
    }
}
