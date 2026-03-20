using Microsoft.AspNetCore.Mvc;
using POS.Core.Features.Products;

namespace POS.Api.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductsServices _productsServices;
        public ProductsController(IProductsServices productsServices)
        {
            _productsServices = productsServices;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllProducts()
        {
            var result = await _productsServices.GetAllProductsAsync();
            return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
        }
        [HttpPost]
        public async Task<IActionResult> CreateProducts(CreateProductRequest request)
        {
            var result = await _productsServices.CreateProductAsync(request);

            return result.IsSuccess ? Ok(new { Message = "Product Created", Id = result.Value }) : BadRequest(result.Error);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProducts(Guid id, UpdateProductRequest request)
        {
            if (id != request.Id)
            {
                return BadRequest("ID mismatch");
            }
            var result = await _productsServices.UpdateProductAsync(request);
            return result.IsSuccess ? Ok(new { Message = "Product Updated" }) : BadRequest(result.Error);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById(Guid id)
        {
            var result = await _productsServices.GetProductById(id);
            return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);

        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(Guid id)
        {
            var result = await _productsServices.DeleteProductAsync(id);
            return result.IsSuccess ? Ok(new { Message = "Product Deleted" }) : BadRequest(result.Error);
        }
    }
}
