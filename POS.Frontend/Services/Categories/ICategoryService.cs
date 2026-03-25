using POS.Frontend.Models;
using POS.Frontend.Models.Categories;
using POS.Shared.Models;

namespace POS.Frontend.Services.Categories;

public interface ICategoryService
{
    Task<ApiResponse<PagedResponse<CategoryResponseDto>>> GetCategoriesAsync(PaginationFilter filter);
    Task<ApiResponse<CategoryResponseDto>> GetCategoryByIdAsync(Guid id);
    Task<ApiResponse<Guid>> CreateCategoryAsync(CreateCategoryRequest request);
    Task<ApiResponse<bool>> UpdateCategoryAsync(UpdateCategoryRequest request);
    Task<ApiResponse<bool>> DeleteCategoryAsync(Guid id);
}
