using POS.Frontend.Models;
using POS.Frontend.Models.Merchants;

namespace POS.Frontend.Services.Merchants;

public interface IMerchantService
{
    Task<ApiResponse<IEnumerable<MerchantResponseDto>>> GetAllMerchantsAsync();
    Task<ApiResponse<Guid>> CreateMerchantAsync(CreateMerchantRequest request);
    Task<ApiResponse> UpdateMerchantAsync(Guid id, UpdateMerchantRequest request);
    Task<ApiResponse> DeleteMerchantAsync(Guid id);
}
