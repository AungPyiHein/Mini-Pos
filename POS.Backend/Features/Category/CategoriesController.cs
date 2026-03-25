using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using POS.Backend.Features.Category;
using POS.Backend.Common;

namespace POS.Backend.Features.Category
{
   // [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryServices _categoryServices;
        public CategoriesController(ICategoryServices categoryServices)
        {
            _categoryServices = categoryServices;
        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllCategories([FromQuery] PaginationFilter filter)
        {
            var result = await _categoryServices.GetAllCategoriesAsync(filter);
            return result.IsSuccess ? Ok(new { Message = "Categories retrieved successfully", Data = result.Value }) : BadRequest(new { Message = result.Error });
        }
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCategoryById(Guid id)
        {
            var result = await _categoryServices.GetCategoryByIdAsync(id);
            return result.IsSuccess ? Ok(new { Message = "Category retrieved successfully", Data = result.Value }) : NotFound(new { Message = result.Error });
        }
        [HttpPost]
        //[Authorize(Roles = "Admin,MerchantAdmin")]
        public async Task<IActionResult> CreateCategory(CreateCategoryRequest request)
        {
            var result = await _categoryServices.CreateCategoryAsync(request);
            return result.IsSuccess ? Ok(new { Message = "Category Created", Data = result.Value }) : BadRequest(new { Message = result.Error });
        }
        [HttpPut("{id}")]
       // [Authorize(Roles = "Admin,MerchantAdmin")]
        public async Task<IActionResult> UpdateCategory(Guid id, UpdateCategoryRequest request)
        {
            if (id != request.id)
                return BadRequest(new { Message = "Invalid category ID." });
            var result = await _categoryServices.UpdateCategoryAsync(request);
            return result.IsSuccess ? Ok(new { Message = "Category Updated" }) : BadRequest(new { Message = result.Error });
        }
        [HttpDelete("{id}")]
        //[Authorize(Roles = "Admin,MerchantAdmin")]
        public async Task<IActionResult> DeleteCategory(Guid id)
        {
            var result = await _categoryServices.DeleteCategoryAsync(id);
            return result.IsSuccess ? Ok(new { Message = "Category Deleted" }) : BadRequest(new { Message = result.Error });
        }
        [HttpPatch("{id}/restore")]
        public async Task<IActionResult> RestoreCategory(Guid id)
        {
            var result = await _categoryServices.RestoreCategoryAsync(id);
            return result.IsSuccess ? Ok(new { Message = "Category Restored" }) : BadRequest(new { Message = result.Error });
        }
    }
}
