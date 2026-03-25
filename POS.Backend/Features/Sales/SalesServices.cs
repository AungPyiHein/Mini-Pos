using Microsoft.EntityFrameworkCore;
using POS.Backend.Common;
using POS.Backend.Features.Inventory;
using POS.data.Data;
using POS.data.Entities;

namespace POS.Backend.Features.Sales
{
    public class CreateOrderRequest
    {
        public Guid BranchId { get; set; }
        public Guid? CustomerId { get; set; }
        public Guid? ProcessedById { get; set; }
        public List<CreateOrderItemRequest> Items { get; set; } = new();
    }

    public class CreateOrderItemRequest
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
    }

    public class OrderResponseDto
    {
        public Guid Id { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string CustomerName { get; set; } = "Walk-in Customer";
        public string Status { get; set; } = "Completed";
        public List<OrderItemResponseDto> Items { get; set; } = new();
    }

    public class OrderItemResponseDto
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal SubTotal { get; set; }
    }

    public interface ISalesServices
    {
        Task<Result<Guid>> CreateOrderAsync(CreateOrderRequest request);
        Task<Result<OrderResponseDto>> GetOrderByIdAsync(Guid id);
        Task<Result<IEnumerable<OrderResponseDto>>> GetAllOrdersAsync();
    }

    public class SalesServices : ISalesServices
    {
        private readonly AppDbContext _context;
        private readonly IInventoryServices _inventoryServices;

        public SalesServices(AppDbContext context, IInventoryServices inventoryServices)
        {
            _context = context;
            _inventoryServices = inventoryServices;
        }

        public async Task<Result<Guid>> CreateOrderAsync(CreateOrderRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var order = new Order
                {
                    Id = Guid.NewGuid(),
                    BranchId = request.BranchId,
                    CustomerId = request.CustomerId,
                    ProcessedById = request.ProcessedById,
                    OrderDate = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    TotalAmount = 0
                };

                foreach (var itemRequest in request.Items)
                {
                    var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == itemRequest.ProductId && p.DeletedAt == null);
                    if (product == null) return Result<Guid>.Failure($"Product with ID {itemRequest.ProductId} not found or is deleted.");

                    var subTotal = product.Price * itemRequest.Quantity;
                    var orderItem = new OrderItem
                    {
                        Id = Guid.NewGuid(),
                        OrderId = order.Id,
                        ProductId = itemRequest.ProductId,
                        Quantity = itemRequest.Quantity,
                        UnitPrice = product.Price,
                        SubTotal = subTotal,
                        CreatedAt = DateTime.UtcNow
                    };

                    order.TotalAmount += subTotal;
                    order.OrderItems.Add(orderItem);

                    // Adjust inventory
                    var adjResult = await _inventoryServices.AdjustStockAsync(new UpdateStockRequest
                    {
                        BranchId = request.BranchId,
                        ProductId = itemRequest.ProductId,
                        QuantityChange = -itemRequest.Quantity
                    });

                    if (!adjResult.IsSuccess) return Result<Guid>.Failure($"Failed to adjust inventory for product {product.Name}: {adjResult.Error}");
                }

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Result<Guid>.Success(order.Id);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Result<Guid>.Failure($"An error occurred while creating order: {ex.Message}");
            }
        }

        public async Task<Result<OrderResponseDto>> GetOrderByIdAsync(Guid id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Where(o => o.Id == id && o.DeletedAt == null)
                .Select(o => new OrderResponseDto
                {
                    Id = o.Id,
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount,
                    Items = o.OrderItems.Select(oi => new OrderItemResponseDto
                    {
                        ProductId = oi.ProductId,
                        ProductName = oi.Product.Name,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice,
                        SubTotal = oi.SubTotal
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (order == null) return Result<OrderResponseDto>.Failure("Order not found.");

            return Result<OrderResponseDto>.Success(order);
        }

        public async Task<Result<IEnumerable<OrderResponseDto>>> GetAllOrdersAsync()
        {
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Include(o => o.Customer)
                .Where(o => o.DeletedAt == null)
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new OrderResponseDto
                {
                    Id = o.Id,
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount,
                    CustomerName = o.Customer != null ? o.Customer.Name : "Walk-in Customer",
                    Status = "Completed",
                    Items = o.OrderItems.Select(oi => new OrderItemResponseDto
                    {
                        ProductId = oi.ProductId,
                        ProductName = oi.Product.Name,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice,
                        SubTotal = oi.SubTotal
                    }).ToList()
                })
                .ToListAsync();

            return Result<IEnumerable<OrderResponseDto>>.Success(orders);
        }
    }
}
