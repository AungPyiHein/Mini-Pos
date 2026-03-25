using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Threading.Tasks;
using POS.Frontend.Models;
using POS.Frontend.Models.Merchants;

namespace POS.Frontend.Services.Merchants;

public class BranchService : IBranchService
{
    private readonly HttpClient _http;

    public BranchService(HttpClient http)
    {
        _http = http;
    }

    public async Task<ApiResponse<IEnumerable<BranchResponseDto>>> GetAllBranchesAsync()
    {
        try
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<IEnumerable<BranchResponseDto>>>("/api/branch");
            if (response != null) response.IsSuccess = true;
            return response ?? new ApiResponse<IEnumerable<BranchResponseDto>> { IsSuccess = false, Message = "Empty response from server" };
        }
        catch (Exception ex)
        {
            return new ApiResponse<IEnumerable<BranchResponseDto>> { IsSuccess = false, Message = $"Error: {ex.Message}" };
        }
    }

    public async Task<ApiResponse<IEnumerable<BranchResponseDto>>> GetBranchesByMerchantIdAsync(Guid merchantId)
    {
        try
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<IEnumerable<BranchResponseDto>>>($"/api/branch/merchant/{merchantId}");
            if (response != null) response.IsSuccess = true;
            return response ?? new ApiResponse<IEnumerable<BranchResponseDto>> { IsSuccess = false, Message = "Empty response from server" };
        }
        catch (Exception ex)
        {
            return new ApiResponse<IEnumerable<BranchResponseDto>> { IsSuccess = false, Message = $"Error: {ex.Message}" };
        }
    }

    public async Task<ApiResponse<Guid>> CreateBranchAsync(CreateBranchRequest request)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("/api/branch", request);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<Guid>>();
            if (result != null) result.IsSuccess = response.IsSuccessStatusCode;
            return result ?? new ApiResponse<Guid> { IsSuccess = false, Message = "Error creating branch" };
        }
        catch (Exception ex)
        {
            return new ApiResponse<Guid> { IsSuccess = false, Message = $"Error: {ex.Message}" };
        }
    }

    public async Task<ApiResponse> UpdateBranchAsync(Guid id, UpdateBranchRequest request)
    {
        try
        {
            var response = await _http.PutAsJsonAsync($"/api/branch/{id}", request);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse>();
            if (result != null) result.IsSuccess = response.IsSuccessStatusCode;
            return result ?? new ApiResponse { IsSuccess = false, Message = "Error updating branch" };
        }
        catch (Exception ex)
        {
            return new ApiResponse { IsSuccess = false, Message = $"Error: {ex.Message}" };
        }
    }

    public async Task<ApiResponse> DeleteBranchAsync(Guid id)
    {
        try
        {
            var response = await _http.DeleteAsync($"/api/branch/{id}");
            var result = await response.Content.ReadFromJsonAsync<ApiResponse>();
            if (result != null) result.IsSuccess = response.IsSuccessStatusCode;
            return result ?? new ApiResponse { IsSuccess = false, Message = "Error deleting branch" };
        }
        catch (Exception ex)
        {
            return new ApiResponse { IsSuccess = false, Message = $"Error: {ex.Message}" };
        }
    }
}
