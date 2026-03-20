using Microsoft.EntityFrameworkCore;
using POS.Core.Common;
using POS.data.Data;


namespace POS.Core.Features.Branch
{
    public class CreateBranchRequest
    {
        public string Name { get; set; } = null!;
        public string Address { get; set; } = null!;
        public Guid MerchantId { get; set; }
    }
    public class UpdateBranchRequest
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Address { get; set; } = null!;
    }
    public class BranchResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Address { get; set; }
        public int ActiveUsersCount { get; set; }
    }
    public interface IBranchServices
    {
        Task<Result<Guid>> CreateBranchAsync(CreateBranchRequest request);
        Task<Result> UpdateBranchAsync(UpdateBranchRequest request);
        Task<Result> DeleteBranchAsync(Guid id);
        Task<Result> RestoreBranchAsync(Guid id);
        Task<Result<IEnumerable<BranchResponse>>> GetBranchesByMerchantIdAsync(Guid merchantId);
    }
    public class BranchServices : IBranchServices
    {
        private readonly AppDbContext _context;
        public BranchServices(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Result<Guid>> CreateBranchAsync(CreateBranchRequest request)
        {
            var merchantExists = await _context.Merchants.AnyAsync(m => m.Id == request.MerchantId);
            if (!merchantExists) return Result<Guid>.Failure("Merchant not found.");

            var nameExists = await _context.Branches.AnyAsync(b =>
                b.MerchantId == request.MerchantId && b.Name == request.Name && b.DeletedAt == null);

            if (nameExists) return Result<Guid>.Failure("A branch with this name already exists.");

            var branch = new data.Entities.Branch
            {
                Id = Guid.NewGuid(),
                MerchantId = request.MerchantId,
                Name = request.Name,
                Address = request.Address,
                CreatedAt = DateTime.UtcNow
            };

            _context.Branches.Add(branch);
            await _context.SaveChangesAsync();

            return Result<Guid>.Success(branch.Id);
        }

       
        public async Task<Result> UpdateBranchAsync(UpdateBranchRequest request)
        {
            var branch = await _context.Branches.FirstOrDefaultAsync(b => b.Id == request.Id && b.DeletedAt == null);
            if (branch == null) return Result.Failure("Branch not found.");
            var nameExists = await _context.Branches.AnyAsync(b =>
                b.MerchantId == branch.MerchantId && b.Name == request.Name && b.Id != request.Id && b.DeletedAt == null);
            if (nameExists) return Result.Failure("A branch with this name already exists.");
            branch.Name = request.Name;
            branch.Address = request.Address;
            branch.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Result.Success();
        }
        public async Task<Result<IEnumerable<BranchResponse>>> GetBranchesByMerchantIdAsync(Guid merchantId)
        {
            var branches = await _context.Branches
                .Where(b => b.MerchantId == merchantId && b.DeletedAt == null)
                .Select(b => new BranchResponse
                {
                    Id = b.Id,
                    Name = b.Name,
                    Address = b.Address,
                    ActiveUsersCount = b.Users.Count(u => u.DeletedAt == null)
                })
                .ToListAsync();
            return Result<IEnumerable<BranchResponse>>.Success(branches);
        }
        public async Task<Result> DeleteBranchAsync(Guid id)
        {
            var branch = await _context.Branches.FirstOrDefaultAsync(b => b.Id == id && b.DeletedAt == null);
            if (branch == null) return Result.Failure("Branch not found.");
            branch.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Result.Success();
        }
        public async Task<Result> RestoreBranchAsync(Guid id)
        {
            var branch = await _context.Branches.FirstOrDefaultAsync(b => b.Id == id && b.DeletedAt != null);
            if (branch == null) return Result.Failure("Branch not found.");
            branch.DeletedAt = null;
            await _context.SaveChangesAsync();
            return Result.Success();
        }
    }
}
