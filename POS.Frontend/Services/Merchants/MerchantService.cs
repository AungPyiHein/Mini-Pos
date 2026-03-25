using System.Net.Http.Json;
using POS.Frontend.Models;

namespace POS.Frontend.Services.Merchants;

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
}
