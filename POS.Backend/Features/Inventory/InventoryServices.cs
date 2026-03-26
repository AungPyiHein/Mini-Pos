using Microsoft.EntityFrameworkCore;
using POS.Backend.Common;
using POS.data.Data;
using POS.data.Entities;

namespace POS.Backend.Features.Inventory
{
    public class InventoryResponseDto
    {
        public Guid Id { get; set; }
        public Guid BranchId { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public int StockQuantity { get; set; }
    }

    public class UpdateStockRequest
    {
        public Guid BranchId { get; set; }
        public Guid ProductId { get; set; }
        public int QuantityChange { get; set; }
    }

    public interface IInventoryServices
    {
        Task<Result<PagedResponse<InventoryResponseDto>>> GetBranchInventoryAsync(Guid branchId, PaginationFilter filter);
        Task<Result<bool>> AdjustStockAsync(UpdateStockRequest request);
    }

    public class InventoryServices : IInventoryServices
    {
        private readonly AppDbContext _context;

        public InventoryServices(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Result<PagedResponse<InventoryResponseDto>>> GetBranchInventoryAsync(Guid branchId, PaginationFilter filter)
        {
            var query = _context.BranchInventories
                .Include(i => i.Product)
                .Where(i => i.BranchId == branchId && i.DeletedAt == null)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                query = query.Where(i => EF.Functions.Like(i.Product.Name, $"%{filter.SearchTerm}%"));
            }

            var totalRecords = await query.CountAsync();

            var inventory = await query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(i => new InventoryResponseDto
                {
                    Id = i.Id,
                    BranchId = i.BranchId,
                    ProductId = i.ProductId,
                    ProductName = i.Product.Name,
                    StockQuantity = i.StockQuantity
                })
                .ToListAsync();

            var pagedResponse = new PagedResponse<InventoryResponseDto>(inventory, totalRecords, filter.PageNumber, filter.PageSize);
            return Result<PagedResponse<InventoryResponseDto>>.Success(pagedResponse);
        }

        public async Task<Result<bool>> AdjustStockAsync(UpdateStockRequest request)
        {
            var inventory = await _context.BranchInventories
                .FirstOrDefaultAsync(i => i.BranchId == request.BranchId && i.ProductId == request.ProductId && i.DeletedAt == null);

            if (inventory == null)
            {
                // Create new inventory record if not exists
                inventory = new BranchInventory
                {
                    Id = Guid.NewGuid(),
                    BranchId = request.BranchId,
                    ProductId = request.ProductId,
                    StockQuantity = request.QuantityChange,
                    CreatedAt = DateTime.UtcNow
                };
                _context.BranchInventories.Add(inventory);
            }
            else
            {
                inventory.StockQuantity += request.QuantityChange;
                inventory.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return Result<bool>.Success(true);
        }
    }
}
