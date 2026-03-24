using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Backend.Features.Inventory;

namespace POS.Backend.Features.Inventory
{
    [Authorize(Roles = "Admin,MerchantAdmin,Staff")]
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryServices _inventoryServices;

        public InventoryController(IInventoryServices inventoryServices)
        {
            _inventoryServices = inventoryServices;
        }

        [HttpGet("branch/{branchId}")]
        public async Task<IActionResult> GetBranchInventory(Guid branchId)
        {
            var result = await _inventoryServices.GetBranchInventoryAsync(branchId);
            return result.IsSuccess ? Ok(new { Message = "Inventory retrieved successfully", Data = result.Value }) : BadRequest(new { Message = result.Error });
        }

        [HttpPost("adjust")]
        public async Task<IActionResult> AdjustStock([FromBody] UpdateStockRequest request)
        {
            var result = await _inventoryServices.AdjustStockAsync(request);
            return result.IsSuccess ? Ok(new { Message = "Stock adjusted successfully", Data = result.Value }) : BadRequest(new { Message = result.Error });
        }
    }
}
