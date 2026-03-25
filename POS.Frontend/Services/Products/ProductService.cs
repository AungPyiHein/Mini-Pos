using System.Net.Http.Json;
using POS.Frontend.Models;
using POS.Frontend.Models.Products;
using POS.Shared.Models;

namespace POS.Frontend.Services.Products;

public class ProductService : IProductService
{
    private readonly HttpClient _http;

    public ProductService(HttpClient http)
    {
        _http = http;
    }

    public async Task<ApiResponse<PagedResponse<ProductsResponseDto>>> GetProductsAsync(PaginationFilter filter)
    {
        var url = $"/api/products?pageNumber={filter.PageNumber}&pageSize={filter.PageSize}";
        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            url += $"&searchTerm={Uri.EscapeDataString(filter.SearchTerm)}";
        }
        
        var response = await _http.GetFromJsonAsync<ApiResponse<PagedResponse<ProductsResponseDto>>>(url);
        return response ?? new ApiResponse<PagedResponse<ProductsResponseDto>> 
        { 
            Message = "Error connecting to server", 
            Data = new PagedResponse<ProductsResponseDto>(new List<ProductsResponseDto>(), 0, 1, 10) 
        };
    }
}
