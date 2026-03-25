using POS.Frontend.Models;
using POS.Frontend.Models.Products;
using POS.Shared.Models;

namespace POS.Frontend.Services.Products;

public interface IProductService
{
    Task<ApiResponse<PagedResponse<ProductsResponseDto>>> GetProductsAsync(PaginationFilter filter);
    Task<ApiResponse<Guid>> CreateProductAsync(CreateProductRequest request);
    Task<ApiResponse<bool>> UpdateProductAsync(UpdateProductRequest request);
    Task<ApiResponse<bool>> DeleteProductAsync(Guid id);
}
