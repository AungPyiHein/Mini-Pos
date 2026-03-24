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
        Task<Result<List<InventoryResponseDto>>> GetBranchInventoryAsync(Guid branchId);
        Task<Result<bool>> AdjustStockAsync(UpdateStockRequest request);
    }

    public class InventoryServices : IInventoryServices
    {
        private readonly AppDbContext _context;

        public InventoryServices(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Result<List<InventoryResponseDto>>> GetBranchInventoryAsync(Guid branchId)
        {
            var inventory = await _context.BranchInventories
                .Include(i => i.Product)
                .Where(i => i.BranchId == branchId && i.DeletedAt == null)
                .Select(i => new InventoryResponseDto
                {
                    Id = i.Id,
                    BranchId = i.BranchId,
                    ProductId = i.ProductId,
                    ProductName = i.Product.Name,
                    StockQuantity = i.StockQuantity
                })
                .ToListAsync();

            return Result<List<InventoryResponseDto>>.Success(inventory);
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
