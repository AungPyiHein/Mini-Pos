using Microsoft.EntityFrameworkCore;
using POS.Backend.Common;
using POS.data.Data;
using POS.data.Entities;

namespace POS.Backend.Features.Customers
{
    public class CustomerResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
    }

    public class CreateCustomerRequest
    {
        public Guid MerchantId { get; set; }
        public string Name { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
    }

    public interface ICustomerServices
    {
        Task<Result<PagedResponse<CustomerResponseDto>>> GetCustomersAsync(Guid merchantId, PaginationFilter filter);
        Task<Result<CustomerResponseDto>> GetCustomerByIdAsync(Guid id);
        Task<Result<Guid>> CreateCustomerAsync(CreateCustomerRequest request);
        Task<Result<bool>> UpdateCustomerAsync(Guid id, CreateCustomerRequest request);
        Task<Result<bool>> DeleteCustomerAsync(Guid id);
    }

    public class CustomerServices : ICustomerServices
    {
        private readonly AppDbContext _context;

        public CustomerServices(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Result<PagedResponse<CustomerResponseDto>>> GetCustomersAsync(Guid merchantId, PaginationFilter filter)
        {
            var query = _context.Customers
                .Where(c => c.MerchantId == merchantId && c.DeletedAt == null)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                query = query.Where(c => EF.Functions.Like(c.Name, $"%{filter.SearchTerm}%") || 
                                         EF.Functions.Like(c.Email, $"%{filter.SearchTerm}%") || 
                                         EF.Functions.Like(c.PhoneNumber, $"%{filter.SearchTerm}%"));
            }

            var totalRecords = await query.CountAsync();

            var customers = await query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(c => new CustomerResponseDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    PhoneNumber = c.PhoneNumber,
                    Email = c.Email
                })
                .ToListAsync();

            var pagedResponse = new PagedResponse<CustomerResponseDto>(customers, totalRecords, filter.PageNumber, filter.PageSize);
            return Result<PagedResponse<CustomerResponseDto>>.Success(pagedResponse);
        }

        public async Task<Result<CustomerResponseDto>> GetCustomerByIdAsync(Guid id)
        {
            var customer = await _context.Customers
                .Where(c => c.Id == id && c.DeletedAt == null)
                .Select(c => new CustomerResponseDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    PhoneNumber = c.PhoneNumber,
                    Email = c.Email
                })
                .FirstOrDefaultAsync();

            if (customer == null) return Result<CustomerResponseDto>.Failure("Customer not found.");

            return Result<CustomerResponseDto>.Success(customer);
        }

        public async Task<Result<Guid>> CreateCustomerAsync(CreateCustomerRequest request)
        {
            var customer = new Customer
            {
                Id = Guid.NewGuid(),
                MerchantId = request.MerchantId,
                Name = request.Name,
                PhoneNumber = request.PhoneNumber,
                Email = request.Email,
                CreatedAt = DateTime.UtcNow
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            return Result<Guid>.Success(customer.Id);
        }

        public async Task<Result<bool>> UpdateCustomerAsync(Guid id, CreateCustomerRequest request)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null || customer.DeletedAt != null) return Result<bool>.Failure("Customer not found.");

            customer.Name = request.Name;
            customer.PhoneNumber = request.PhoneNumber;
            customer.Email = request.Email;
            customer.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> DeleteCustomerAsync(Guid id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null || customer.DeletedAt != null) return Result<bool>.Failure("Customer not found.");

            customer.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Result<bool>.Success(true);
        }
    }
}
