using POS.Frontend.Models;
using POS.Frontend.Models.Sales;

namespace POS.Frontend.Services.Sales;

public interface ISaleService
{
    Task<ApiResponse<IEnumerable<OrderResponseDto>>> GetAllOrdersAsync();
    Task<ApiResponse<OrderResponseDto>> GetOrderByIdAsync(Guid id);
    Task<ApiResponse<Guid>> CreateOrderAsync(CreateOrderRequest request);
}
