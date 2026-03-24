using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Backend.Features.Sales;

namespace POS.Backend.Features.Sales
{
    [Authorize(Roles = "Admin,MerchantAdmin,Staff")]
    [ApiController]
    [Route("api/[controller]")]
    public class SalesController : ControllerBase
    {
        private readonly ISalesServices _salesServices;

        public SalesController(ISalesServices salesServices)
        {
            _salesServices = salesServices;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
        {
            var result = await _salesServices.CreateOrderAsync(request);
            return result.IsSuccess ? CreatedAtAction(nameof(GetOrder), new { id = result.Value }, new { Message = "Order Created", Data = result.Value }) : BadRequest(new { Message = result.Error });
        }

        [HttpGet]
        public async Task<IActionResult> GetAllOrders()
        {
            var result = await _salesServices.GetAllOrdersAsync();
            return result.IsSuccess ? Ok(new { Message = "Orders retrieved successfully", Data = result.Value }) : BadRequest(new { Message = result.Error });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrder(Guid id)
        {
            var result = await _salesServices.GetOrderByIdAsync(id);
            return result.IsSuccess ? Ok(new { Message = "Order retrieved successfully", Data = result.Value }) : NotFound(new { Message = result.Error });
        }
    }
}
