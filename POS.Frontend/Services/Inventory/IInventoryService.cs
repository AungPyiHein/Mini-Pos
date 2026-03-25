using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using POS.Frontend.Models;
using POS.Frontend.Models.Inventory;

namespace POS.Frontend.Services.Inventory;

public interface IInventoryService
{
    Task<ApiResponse<IEnumerable<InventoryResponseDto>>> GetBranchInventoryAsync(Guid branchId);
    Task<ApiResponse<bool>> AdjustStockAsync(UpdateStockRequest request);
}
