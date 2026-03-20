using Microsoft.EntityFrameworkCore;
using POS.data.Data;
using POS.Core.Common;

namespace POS.Core.Features.Products
{
    public class ProductsResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string SKU { get; set; }
        public string CategoryName { get; set; }
        public string CategoryDescription { get; set; }
        public string MerchantName { get; set; }

    }
    public class CreateProductRequest
    {
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string SKU { get; set; }
        public Guid CategoryId { get; set; }
        public Guid MerchantId { get; set; }
    }

    public class UpdateProductRequest
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public decimal? Price { get; set; }
        public string? SKU { get; set; }
        public Guid? CategoryId { get; set; }
    }
    public interface IProductsServices
    {
        Task<Result<IEnumerable<ProductsResponseDto>>> GetAllProductsAsync();
        Task<Result<ProductsResponseDto>> GetProductById(Guid id);
        Task<Result<Guid>> CreateProductAsync(CreateProductRequest  request);
        Task<Result> UpdateProductAsync(UpdateProductRequest request);
        Task<Result> DeleteProductAsync(Guid id);
    }

    public class ProductsServices : IProductsServices
    {
        public readonly AppDbContext _context;
        public ProductsServices(AppDbContext context)
        {
            _context = context;
        }
        public async Task<Result<IEnumerable<ProductsResponseDto>>> GetAllProductsAsync()
        {
            var products = await _context.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.Merchant)
                .ToListAsync();

            var response = products.Select(p => new ProductsResponseDto
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                SKU = p.Sku,
                CategoryName = p.Category?.Name ?? "No Category",
                CategoryDescription = p.Category?.Description ?? "No Description",
                MerchantName = p.Merchant?.Name ?? "Unknown Merchant"
            }).ToList();
            return Result<IEnumerable<ProductsResponseDto>>.Success(response); ;
        }
        public async Task<Result<ProductsResponseDto>> GetProductById(Guid id)
        {
            var product = await _context.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.Merchant)
                .Select(p => new ProductsResponseDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    SKU = p.Sku,
                    CategoryName = p.Category != null ? p.Category.Name : "No Category",
                    CategoryDescription = p.Category != null ? p.Category.Description : "No Description",
                    MerchantName = p.Merchant != null ? p.Merchant.Name : "Unknown Merchant"
                })
                .FirstOrDefaultAsync(p => p.Id == id);
            if (product == null)
                return Result<ProductsResponseDto>.Failure("Product not found");
            return Result<ProductsResponseDto>.Success(product);

        }

        public async Task<Result<Guid>> CreateProductAsync(CreateProductRequest request)
        {
            var categoryExist = await _context.Categories.AnyAsync(c => c.Id == request.CategoryId);
            if (!categoryExist)
                return Result<Guid>.Failure("Category not found");

            var newProduct = new data.Entities.Product
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Price = request.Price,
                Sku = request.SKU,
                CategoryId = request.CategoryId,
                MerchantId = request.MerchantId,
                CreatedAt = DateTime.UtcNow
            };
            _context.Products.Add(newProduct);
            await _context.SaveChangesAsync();

            return Result<Guid>.Success(newProduct.Id);
        }

        public async Task<Result> UpdateProductAsync(UpdateProductRequest request)
        {
            var existingProduct = await _context.Products.FirstOrDefaultAsync(p => p.Id == request.Id);
            if (existingProduct == null)
                return Result.Failure("Product not found");
            if (!string.IsNullOrWhiteSpace(request.Name))
                existingProduct.Name = request.Name;

            if (request.Price.HasValue)
                existingProduct.Price = request.Price.Value;

            if (!string.IsNullOrWhiteSpace(request.SKU))
                existingProduct.Sku = request.SKU;

            if (request.CategoryId.HasValue && request.CategoryId != Guid.Empty)
            {
                var categoryExists = await _context.Categories.AnyAsync(c => c.Id == request.CategoryId);
                if (!categoryExists) return Result.Failure("The selected Category does not exist.");

                existingProduct.CategoryId = request.CategoryId.Value;
            }

            existingProduct.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Result.Success();
        }
        public async Task<Result> DeleteProductAsync(Guid id)
        {
            var existingProduct = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (existingProduct == null)
                return Result.Failure("Product not found");
            _context.Products.Remove(existingProduct);
            await _context.SaveChangesAsync();
            return Result.Success();
        }
    }
}