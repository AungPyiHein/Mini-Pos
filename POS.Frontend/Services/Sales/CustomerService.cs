using System.Net.Http.Json;
using POS.Frontend.Models;
using POS.Frontend.Models.Sales;
using POS.Shared.Models;

namespace POS.Frontend.Services.Sales;

public class CustomerService : ICustomerService
{
    private readonly HttpClient _http;

    public CustomerService(HttpClient http)
    {
        _http = http;
    }

    public async Task<ApiResponse<PagedResponse<CustomerResponseDto>>> GetCustomersAsync(Guid merchantId, PaginationFilter filter)
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<PagedResponse<CustomerResponseDto>>>($"/api/customers/merchant/{merchantId}?pageNumber={filter.PageNumber}&pageSize={filter.PageSize}&searchTerm={filter.SearchTerm}");
        return response ?? new ApiResponse<PagedResponse<CustomerResponseDto>> { Message = "Error connecting to server" };
    }

    public async Task<ApiResponse<CustomerResponseDto>> GetCustomerByIdAsync(Guid id)
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<CustomerResponseDto>>($"/api/customers/{id}");
        return response ?? new ApiResponse<CustomerResponseDto> { Message = "Error connecting to server" };
    }

    public async Task<ApiResponse<Guid>> CreateCustomerAsync(CreateCustomerRequest request)
    {
        var response = await _http.PostAsJsonAsync("/api/customers", request);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<Guid>>();
        return result ?? new ApiResponse<Guid> { Message = "Error connecting to server" };
    }

    public async Task<ApiResponse> UpdateCustomerAsync(Guid id, CreateCustomerRequest request)
    {
        var response = await _http.PutAsJsonAsync($"/api/customers/{id}", request);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse>();
        return result ?? new ApiResponse { Message = "Error connecting to server" };
    }

    public async Task<ApiResponse> DeleteCustomerAsync(Guid id)
    {
        var response = await _http.DeleteAsync($"/api/customers/{id}");
        var result = await response.Content.ReadFromJsonAsync<ApiResponse>();
        return result ?? new ApiResponse { Message = "Error connecting to server" };
    }
}
