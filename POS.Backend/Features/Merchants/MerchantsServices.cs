using Microsoft.EntityFrameworkCore;
using POS.Backend.Common;
using POS.data.Data;


namespace POS.Backend.Features.Merchants
{
    public class CreateMerchantRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? ContactEmail { get; set; }
    }
    public class MerchantResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string ContactEmail { get; set; }
        public bool isActive { get; set; }
        public int CategoryCount { get; set; }
        public int ProductCount { get; set; }
    }

    public class UpdateMerchantRequest
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? ContactEmail { get; set; }
        public bool? IsActive { get; set; }
    }

    public interface IMerchantsServices
    {
        Task<Result<IEnumerable<MerchantResponseDto>>> GetAllMerchantsAsync();
        Task<Result<MerchantResponseDto>> GetMerchantByIdAsync(Guid id);
        Task<Result<Guid>> CreateMerchantAsync(CreateMerchantRequest request);
        Task<Result> UpdateMerchantAsync(UpdateMerchantRequest request);
        Task<Result> DeleteMerchantAsync(Guid id);
        Task<Result> RestoreMerchantAsync(Guid id);
    }

    public class MerchantsServices : IMerchantsServices
    {
        private readonly AppDbContext _context;
        public MerchantsServices(AppDbContext context)
        {
            _context = context;
        }
        public async Task<Result<IEnumerable<MerchantResponseDto>>> GetAllMerchantsAsync()
        {
            var merchants = await _context.Merchants
                .AsNoTracking()
                .Where(m => !m.IsDeleted)
                .Select(m => new MerchantResponseDto
                {
                    Id = m.Id,
                    Name = m.Name,
                    ContactEmail = m.ContactEmail,
                    isActive = m.IsActive,
                    CategoryCount = m.Categories.Count(),
                    ProductCount = m.Products.Count()
                })
                .ToListAsync();
            return Result<IEnumerable<MerchantResponseDto>>.Success(merchants);
        }
        public async Task<Result<Guid>> CreateMerchantAsync(CreateMerchantRequest request)
        {
            if (!string.IsNullOrEmpty(request.ContactEmail))
            {
                var emailExists = await _context.Merchants
                    .AnyAsync(m => m.ContactEmail == request.ContactEmail);
                if (emailExists)
                    return Result<Guid>.Failure("Email is already registered to another merchant.");
            }
            var merchant = new data.Entities.Merchant
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                ContactEmail = request.ContactEmail,
                IsActive = true,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            };
            _context.Merchants.Add(merchant);
            await _context.SaveChangesAsync();

            return Result<Guid>.Success(merchant.Id);
        }
        public async Task<Result> DeleteMerchantAsync(Guid id)
        {
            var merchant = await _context.Merchants.FirstOrDefaultAsync(m => m.Id == id);
            if (merchant == null || merchant.IsDeleted)
                return Result.Failure("Merchant not found.");
            merchant.IsDeleted = true;
            merchant.DeletedAt = DateTime.UtcNow;
            merchant.IsActive = false;
            await _context.SaveChangesAsync();
            return Result.Success();
        }
        public async Task<Result<MerchantResponseDto>> GetMerchantByIdAsync(Guid id)
        {
            var merchants = await _context.Merchants
                 .AsNoTracking()
                 .Where(m => !m.IsDeleted && m.Id == id)
                 .Select(m => new MerchantResponseDto
                 {
                     Id = m.Id,
                     Name = m.Name,
                     ContactEmail = m.ContactEmail,
                     isActive = m.IsActive,
                     CategoryCount = m.Categories.Count(),
                     ProductCount = m.Products.Count()
                 })
                 .FirstOrDefaultAsync();
            if (merchants == null)
                return Result<MerchantResponseDto>.Failure("Merchant not found.");
            return Result<MerchantResponseDto>.Success(merchants);
        }
        public async Task<Result> UpdateMerchantAsync(UpdateMerchantRequest request)
        {
            var existingMerchant = await _context.Merchants
                .FirstOrDefaultAsync(m => m.Id == request.Id && !m.IsDeleted);

            if (existingMerchant == null)
                return Result.Failure("Merchant not found.");

            if (!string.IsNullOrWhiteSpace(request.Name))
                existingMerchant.Name = request.Name;

            if (!string.IsNullOrWhiteSpace(request.ContactEmail))
                existingMerchant.ContactEmail = request.ContactEmail;

            if (request.IsActive.HasValue)
                existingMerchant.IsActive = request.IsActive.Value;

            existingMerchant.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Result.Success();
        }
        public async Task<Result> RestoreMerchantAsync(Guid id)
        {
            var merchant = await _context.Merchants.IgnoreQueryFilters().FirstOrDefaultAsync(m => m.Id == id);
            if (merchant == null)
                return Result.Failure("Merchant not found.");
            if (!merchant.IsDeleted)
                return Result.Failure("Merchant is not deleted.");
            merchant.IsDeleted = false;
            merchant.DeletedAt = null;
            merchant.IsActive = true;
            await _context.SaveChangesAsync();
            return Result.Success();
        }
    }
}
