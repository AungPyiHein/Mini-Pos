using Microsoft.AspNetCore.Mvc;
using POS.Core.Features.Category;
namespace POS.Api.Controllers
{
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
        public async Task<IActionResult> GetAllCategories()
        {
            var result = await _categoryServices.GetAllCategoriesAsync();
            return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategoryById(Guid id)
        {
            var result = await _categoryServices.GetCategoryByIdAsync(id);
            return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
        }
        [HttpPost]
        public async Task<IActionResult> CreateCategory(CreateCategoryRequest request)
        {
            var result = await _categoryServices.CreateCategoryAsync(request);
            return result.IsSuccess ? Ok(new { Message = "Category Created", Id = result.Value }) : BadRequest(result.Error);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(Guid id, UpdateCategoryRequest request)
        {
            if (id != request.id)
                return BadRequest("Invalid category ID.");
            var result = await _categoryServices.UpdateCategoryAsync(request);
            return result.IsSuccess ? Ok(new { Message = "Category Updated" }) : BadRequest(result.Error);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(Guid id)
        {
            var result = await _categoryServices.DeleteCategoryAsync(id);
            return result.IsSuccess ? Ok(new { Message = "Category Deleted" }) : BadRequest(result.Error);
        }
        [HttpPatch("{id}/restore")]
        public async Task<IActionResult> RestoreCategory(Guid id)
        {
            var result = await _categoryServices.RestoreCategoryAsync(id);
            return result.IsSuccess ? Ok(new { Message = "Category Restored" }) : BadRequest(result.Error);
        }
    }
}
