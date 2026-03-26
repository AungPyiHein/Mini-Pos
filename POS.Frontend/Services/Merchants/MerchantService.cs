using System.Net.Http.Json;
using POS.Frontend.Models;
using POS.Frontend.Models.Merchants;

namespace POS.Frontend.Services.Merchants;

public interface IMerchantService
{
    Task<ApiResponse<IEnumerable<MerchantResponseDto>>> GetAllMerchantsAsync();
    Task<ApiResponse<Guid>> CreateMerchantAsync(CreateMerchantRequest request);
    Task<ApiResponse> UpdateMerchantAsync(Guid id, UpdateMerchantRequest request);
    Task<ApiResponse> DeleteMerchantAsync(Guid id);
}
public class MerchantService : IMerchantService
{
    private readonly HttpClient _http;

    public MerchantService(HttpClient http)
    {
        _http = http;
    }

    public async Task<ApiResponse<IEnumerable<MerchantResponseDto>>> GetAllMerchantsAsync()
    {
        try
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<IEnumerable<MerchantResponseDto>>>("/api/merchants");
            if (response != null) response.IsSuccess = true;
            return response ?? new ApiResponse<IEnumerable<MerchantResponseDto>> { IsSuccess = false, Message = "Error fetching merchants" };
        }
        catch (Exception ex)
        {
            return new ApiResponse<IEnumerable<MerchantResponseDto>> { IsSuccess = false, Message = $"Error: {ex.Message}" };
        }
    }

    public async Task<ApiResponse<Guid>> CreateMerchantAsync(CreateMerchantRequest request)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("/api/merchants", request);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<Guid>>();
            if (result != null) result.IsSuccess = response.IsSuccessStatusCode;
            return result ?? new ApiResponse<Guid> { IsSuccess = false, Message = "Error creating merchant" };
        }
        catch (Exception ex)
        {
            return new ApiResponse<Guid> { IsSuccess = false, Message = $"Error: {ex.Message}" };
        }
    }

    public async Task<ApiResponse> UpdateMerchantAsync(Guid id, UpdateMerchantRequest request)
    {
        try
        {
            var response = await _http.PutAsJsonAsync($"/api/merchants/{id}", request);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse>();
            if (result != null) result.IsSuccess = response.IsSuccessStatusCode;
            return result ?? new ApiResponse { IsSuccess = false, Message = "Error updating merchant" };
        }
        catch (Exception ex)
        {
            return new ApiResponse { IsSuccess = false, Message = $"Error: {ex.Message}" };
        }
    }

    public async Task<ApiResponse> DeleteMerchantAsync(Guid id)
    {
        try
        {
            var response = await _http.DeleteAsync($"/api/merchants/{id}");
            var result = await response.Content.ReadFromJsonAsync<ApiResponse>();
            if (result != null) result.IsSuccess = response.IsSuccessStatusCode;
            return result ?? new ApiResponse { IsSuccess = false, Message = "Error deleting merchant" };
        }
        catch (Exception ex)
        {
            return new ApiResponse { IsSuccess = false, Message = $"Error: {ex.Message}" };
        }
    }
}
