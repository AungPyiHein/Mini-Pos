using System.Collections.Generic;
using System.Threading.Tasks;
using POS.Frontend.Models;
using POS.Frontend.Models.Merchants;

namespace POS.Frontend.Services.Merchants;

public interface IBranchService
{
    Task<ApiResponse<IEnumerable<BranchResponseDto>>> GetAllBranchesAsync();
    Task<ApiResponse<IEnumerable<BranchResponseDto>>> GetBranchesByMerchantIdAsync(Guid merchantId);
}
