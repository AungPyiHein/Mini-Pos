using Microsoft.EntityFrameworkCore;
using POS.Core.Common;
using POS.data.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POS.Core.Features.Category
{


    public class CategoryResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public int ProductCount { get; set; }

    }
    public class CreateCategoryRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid MerchantId { get; set; }
    }
    public class UpdateCategoryRequest
    {
        public Guid id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
    }

    public interface ICategoryServices
    {
        Task<Result<IEnumerable<CategoryResponseDto>>> GetAllCategoriesAsync();
        Task<Result<CategoryResponseDto>> GetCategoryByIdAsync(Guid id);
        Task<Result<Guid>> CreateCategoryAsync(CreateCategoryRequest request);
        Task<Result> UpdateCategoryAsync(UpdateCategoryRequest request);
        Task<Result> DeleteCategoryAsync(Guid id);
        Task<Result> RestoreCategoryAsync(Guid id);
    }

    public class CategoryService : ICategoryServices
    {
        private readonly AppDbContext _context;
        public CategoryService(AppDbContext context)
        {
            _context = context;
        }
        public async Task<Result<IEnumerable<CategoryResponseDto>>> GetAllCategoriesAsync()
        {
            var category = await _context.Categories
                .AsNoTracking()
                .Select(c => new CategoryResponseDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    ProductCount = _context.Products.Count(p => p.CategoryId == c.Id)
                }).ToListAsync();
            return Result<IEnumerable<CategoryResponseDto>>.Success(category);
        }
        public async Task<Result<CategoryResponseDto>> GetCategoryByIdAsync(Guid id)
        {
            var category = await _context.Categories
                .AsNoTracking()
                .Where(c => c.Id == id && !c.IsDeleted)
                .Select(c => new CategoryResponseDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    ProductCount = _context.Products.Count(p => p.CategoryId == c.Id)
                }).FirstOrDefaultAsync();
            if (category == null)
                return Result<CategoryResponseDto>.Failure("Category not found");
            return Result<CategoryResponseDto>.Success(category);
        }

        public async Task<Result<Guid>> CreateCategoryAsync(CreateCategoryRequest request)
        {
            var merchantExists = await _context.Merchants.AnyAsync(m => m.Id == request.MerchantId);
            if (!merchantExists)
            {
                return Result<Guid>.Failure("Merchant not found. Please provide a valid Merchant ID.");
            }
            var category = new data.Entities.Category
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                MerchantId = request.MerchantId
            };
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return Result<Guid>.Success(category.Id);
        }
        public async Task<Result> UpdateCategoryAsync(UpdateCategoryRequest request)
        {
            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == request.id);
            if (category == null)
                return Result.Failure("Category not found");
            if (!string.IsNullOrEmpty(request.Name))
                category.Name = request.Name;
            if (!string.IsNullOrEmpty(request.Description))
                category.Description = request.Description;
            category.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Result.Success();
        }
        public async Task<Result> DeleteCategoryAsync(Guid id)
        {
            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

            if (category == null) return Result.Failure("Category not found or already Deleted.");
            var hasProducts = await _context.Products.AnyAsync(p => p.CategoryId == id);

            if (hasProducts)
            {
                return Result.Failure("Cannot delete a category that still contains products.");
            }

            category.IsDeleted = true;

            await _context.SaveChangesAsync();
            return Result.Success();
        }
        public async Task<Result> RestoreCategoryAsync(Guid id)
        {
            var category = await _context.Categories
                .Include(c=> c.Merchant)
                .FirstOrDefaultAsync(c => c.Id == id && c.IsDeleted);
            if (category.Merchant != null && category.Merchant.IsDeleted)
            {
                return Result.Failure("Cannot restore this category because the Merchant is deleted.");
            }

            category.IsDeleted = false;
            await _context.SaveChangesAsync();
            return Result.Success();
        }
    }
}
