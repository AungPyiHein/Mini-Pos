using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using POS.Backend.Features.Merchants;

namespace POS.Backend.Features.Merchants
{
   // [Authorize(Roles = "Admin")]
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
        public async Task<IActionResult> GetAllMerchants()
        {
            var result = await _merchantsServices.GetAllMerchantsAsync();
            return result.IsSuccess ? Ok(new { Message = "Merchants retrieved successfully", Data = result.Value }) : BadRequest(new { Message = result.Error });
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetMerchantById(Guid id)
        {
            var result = await _merchantsServices.GetMerchantByIdAsync(id);
            return result.IsSuccess ? Ok(new { Message = "Merchant retrieved successfully", Data = result.Value }) : NotFound(new { Message = result.Error });
        }
        [HttpPost]
        public async Task<IActionResult> CreateMerchant(CreateMerchantRequest request)
        {
            var result = await _merchantsServices.CreateMerchantAsync(request);
            return result.IsSuccess ? Ok(new { Message = "Merchant Created", Data = result.Value }) : BadRequest(new { Message = result.Error });
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMerchant(Guid id, UpdateMerchantRequest request)
        {
            if (id != request.Id)
            {
                return BadRequest(new { Message = "ID mismatch" });
            }
            var result = await _merchantsServices.UpdateMerchantAsync(request);
            return result.IsSuccess ? Ok(new { Message = "Merchant Updated" }) : BadRequest(new { Message = result.Error });
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMerchant(Guid id)
        {
            var result = await _merchantsServices.DeleteMerchantAsync(id);
            return result.IsSuccess ? Ok(new { Message = "Merchant Deleted" }) : BadRequest(new { Message = result.Error });
        }
        [HttpPatch("{id}/restore")]
        public async Task<IActionResult> RestoreMerchant(Guid id)
        {
            var result = await _merchantsServices.RestoreMerchantAsync(id);
            return result.IsSuccess ? Ok(new { Message = "Merchant Restored" }) : BadRequest(new { Message = result.Error });
        }
    }
}

