using System.Net.Http.Json;
using POS.Frontend.Models;
using POS.Frontend.Models.Sales;

namespace POS.Frontend.Services.Sales;

public class SaleService : ISaleService
{
    private readonly HttpClient _http;

    public SaleService(HttpClient http)
    {
        _http = http;
    }

    public async Task<ApiResponse<IEnumerable<OrderResponseDto>>> GetAllOrdersAsync()
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<IEnumerable<OrderResponseDto>>>("/api/sales");
        return response ?? new ApiResponse<IEnumerable<OrderResponseDto>> 
        { 
            Message = "Error connecting to server", 
            Data = new List<OrderResponseDto>() 
        };
    }

    public async Task<ApiResponse<OrderResponseDto>> GetOrderByIdAsync(Guid id)
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<OrderResponseDto>>($"/api/sales/{id}");
        return response ?? new ApiResponse<OrderResponseDto> { Message = "Error connecting to server" };
    }

    public async Task<ApiResponse<Guid>> CreateOrderAsync(CreateOrderRequest request)
    {
        var response = await _http.PostAsJsonAsync("/api/sales", request);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<Guid>>();
        return result ?? new ApiResponse<Guid> { Message = "Error connecting to server" };
    }
}
