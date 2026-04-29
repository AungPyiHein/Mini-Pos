using Microsoft.AspNetCore.Mvc;
using POS.Backend.Features.Merchants;
using POS.Backend.Common;
using POS.Shared.Models;

namespace POS.Backend.Features.Merchants
{
    [ApiController]
    [Route("api/[controller]")]
    public class MerchantsController : ControllerBase
    {
        private readonly IMerchantsServices _merchantsServices;
        public MerchantsController(IMerchantsServices merchantsServices)
        {
            _merchantsServices = merchantsServices;
        }
        [HttpGet]
        [RequireRole(UserRole.Admin, UserRole.MerchantAdmin)]
        public async Task<IActionResult> GetAllMerchants([FromQuery] PaginationFilter filter)
        {
            var result = await _merchantsServices.GetAllMerchantsAsync(filter);
            return result.IsSuccess ? Ok(new { IsSuccess = true, Message = "Merchants retrieved successfully", Data = result.Value }) : BadRequest(new { IsSuccess = false, Message = result.Error });
        }
        [HttpGet("deleted")]
        [RequireRole(UserRole.Admin, UserRole.MerchantAdmin)]
        public async Task<IActionResult> GetDeletedMerchants([FromQuery] PaginationFilter filter)
        {
            var result = await _merchantsServices.GetDeletedMerchantsAsync(filter);
            return result.IsSuccess ? Ok(new { IsSuccess = true, Message = "Deleted Merchants retrieved successfully", Data = result.Value }) : BadRequest(new { IsSuccess = false, Message = result.Error });
        }
        [HttpGet("{id}")]
        [RequireRole(UserRole.Admin, UserRole.MerchantAdmin, UserRole.Staff)]
        public async Task<IActionResult> GetMerchantById(Guid id)
        {
            var result = await _merchantsServices.GetMerchantByIdAsync(id);
            return result.IsSuccess ? Ok(new { IsSuccess = true, Message = "Merchant retrieved successfully", Data = result.Value }) : NotFound(new { IsSuccess = false, Message = result.Error });
        }
        [HttpPost]
        [RequireRole(UserRole.Admin)]
        public async Task<IActionResult> CreateMerchant(CreateMerchantRequest request)
        {
            var result = await _merchantsServices.CreateMerchantAsync(request);
            return result.IsSuccess ? Ok(new { IsSuccess = true, Message = "Merchant Created", Data = result.Value }) : BadRequest(new { IsSuccess = false, Message = result.Error });
        }
        [HttpPut("{id}")]
        [RequireRole(UserRole.Admin)]
        public async Task<IActionResult> UpdateMerchant(Guid id, UpdateMerchantRequest request)
        {
            if (id != request.Id)
            {
                return BadRequest(new { IsSuccess = false, Message = "ID mismatch" });
            }
            var result = await _merchantsServices.UpdateMerchantAsync(request);
            return result.IsSuccess ? Ok(new { IsSuccess = true, Message = "Merchant Updated" }) : BadRequest(new { IsSuccess = false, Message = result.Error });
        }
        [HttpDelete("{id}")]
        [RequireRole(UserRole.Admin)]
        public async Task<IActionResult> DeleteMerchant(Guid id, [FromQuery] bool force = false)
        {
            var result = await _merchantsServices.DeleteMerchantAsync(id, force);
            return result.IsSuccess ? Ok(new { IsSuccess = true, Message = "Merchant Deleted" }) : BadRequest(new { IsSuccess = false, Message = result.Error });
        }
        [HttpPatch("{id}/restore")]
        [RequireRole(UserRole.Admin)]
        public async Task<IActionResult> RestoreMerchant(Guid id, [FromQuery] bool restoreAll = false)
        {
            var result = await _merchantsServices.RestoreMerchantAsync(id, restoreAll);
            return result.IsSuccess ? Ok(new { IsSuccess = true, Message = "Merchant Restored" }) : BadRequest(new { IsSuccess = false, Message = result.Error });
        }
    }
}

