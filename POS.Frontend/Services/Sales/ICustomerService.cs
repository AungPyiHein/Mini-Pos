using POS.Frontend.Models;
using POS.Frontend.Models.Sales;
using POS.Shared.Models;

namespace POS.Frontend.Services.Sales;

public interface ICustomerService
{
    Task<ApiResponse<PagedResponse<CustomerResponseDto>>> GetCustomersAsync(Guid merchantId, PaginationFilter filter);
    Task<ApiResponse<CustomerResponseDto>> GetCustomerByIdAsync(Guid id);
    Task<ApiResponse<Guid>> CreateCustomerAsync(CreateCustomerRequest request);
    Task<ApiResponse> UpdateCustomerAsync(Guid id, CreateCustomerRequest request);
    Task<ApiResponse> DeleteCustomerAsync(Guid id);
}
