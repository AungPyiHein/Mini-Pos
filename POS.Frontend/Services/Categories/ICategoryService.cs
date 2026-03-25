using POS.Frontend.Models;
using POS.Frontend.Models.Categories;
using POS.Shared.Models;

namespace POS.Frontend.Services.Categories;

public interface ICategoryService
{
    Task<ApiResponse<PagedResponse<CategoryResponseDto>>> GetCategoriesAsync(PaginationFilter filter);
    Task<ApiResponse<CategoryResponseDto>> GetCategoryByIdAsync(Guid id);
}
