using System.Net.Http.Json;
using POS.Frontend.Models;
using POS.Frontend.Models.Categories;
using POS.Shared.Models;

namespace POS.Frontend.Services.Categories;

public class CategoryService : ICategoryService
{
    private readonly HttpClient _http;

    public CategoryService(HttpClient http)
    {
        _http = http;
    }

    public async Task<ApiResponse<PagedResponse<CategoryResponseDto>>> GetCategoriesAsync(PaginationFilter filter)
    {
        try
        {
            var url = $"/api/categories?pageNumber={filter.PageNumber}&pageSize={filter.PageSize}";
            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                url += $"&searchTerm={Uri.EscapeDataString(filter.SearchTerm)}";
            }
            
            var response = await _http.GetFromJsonAsync<ApiResponse<PagedResponse<CategoryResponseDto>>>(url);
            if (response != null) response.IsSuccess = true;
            return response ?? new ApiResponse<PagedResponse<CategoryResponseDto>> 
            { 
                IsSuccess = false,
                Message = "Error connecting to server", 
                Data = new PagedResponse<CategoryResponseDto>(new List<CategoryResponseDto>(), 0, 1, 10) 
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<PagedResponse<CategoryResponseDto>> 
            { 
                IsSuccess = false,
                Message = $"Error: {ex.Message}", 
                Data = new PagedResponse<CategoryResponseDto>(new List<CategoryResponseDto>(), 0, 1, 10) 
            };
        }
    }

    public async Task<ApiResponse<CategoryResponseDto>> GetCategoryByIdAsync(Guid id)
    {
        try
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<CategoryResponseDto>>($"/api/categories/{id}");
            if (response != null) response.IsSuccess = true;
            return response ?? new ApiResponse<CategoryResponseDto> { IsSuccess = false, Message = "Error connecting to server" };
        }
        catch (Exception ex)
        {
            return new ApiResponse<CategoryResponseDto> { IsSuccess = false, Message = $"Error: {ex.Message}" };
        }
    }
}
