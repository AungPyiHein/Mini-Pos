using POS.Frontend.Models;

namespace POS.Frontend.Services.Merchants;

public interface IMerchantService
{
    Task<ApiResponse<IEnumerable<MerchantResponseDto>>> GetAllMerchantsAsync();
}
