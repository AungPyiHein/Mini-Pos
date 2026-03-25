using System.Net.Http.Json;
using POS.Frontend.Models;
using POS.Frontend.Models.Inventory;

namespace POS.Frontend.Services.Inventory;

public class InventoryService : IInventoryService
{
    private readonly HttpClient _http;

    public InventoryService(HttpClient http)
    {
        _http = http;
    }

    public async Task<ApiResponse<IEnumerable<InventoryResponseDto>>> GetBranchInventoryAsync(Guid branchId)
    {
        try
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<IEnumerable<InventoryResponseDto>>>($"/api/inventory/branch/{branchId}");
            if (response != null) response.IsSuccess = true;
            return response ?? new ApiResponse<IEnumerable<InventoryResponseDto>> { IsSuccess = false, Message = "Empty response from server" };
        }
        catch (Exception ex)
        {
            return new ApiResponse<IEnumerable<InventoryResponseDto>> { IsSuccess = false, Message = $"Error: {ex.Message}" };
        }
    }

    public async Task<ApiResponse<bool>> AdjustStockAsync(UpdateStockRequest request)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("/api/inventory/adjust", request);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
            if (result != null) result.IsSuccess = response.IsSuccessStatusCode;
            return result ?? new ApiResponse<bool> { IsSuccess = false, Message = "Error adjusting stock" };
        }
        catch (Exception ex)
        {
            return new ApiResponse<bool> { IsSuccess = false, Message = $"Error: {ex.Message}" };
        }
    }
}
