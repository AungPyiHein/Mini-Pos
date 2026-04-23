using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POS.Backend.Features.Loyalty;
using POS.Backend.Common;
using System.Text.RegularExpressions;

namespace POS.Backend.Features.Loyalty
{
    [ApiController]
    [Route("api/v1/loyalty")]
    public class LoyaltyController : ControllerBase
    {
        private readonly ILoyaltyServices _loyaltyServices;
        private readonly POS.data.Data.AppDbContext _context;
        private readonly ICurrentUserService _currentUser;

        public LoyaltyController(ILoyaltyServices loyaltyServices, POS.data.Data.AppDbContext context, ICurrentUserService currentUser)
        {
            _loyaltyServices = loyaltyServices;
            _context = context;
            _currentUser = currentUser;
        }

        /// <summary>
        /// Derives the Loyalty Engine system ID as "APH_POS_{MERCHANTNAME}" (e.g. "APH_POS_UNIQUE").
        /// Each merchant must have a matching system registered on the Loyalty Engine.
        /// </summary>
        private async Task<(string? systemId, string? apiKey)> GetMerchantLoyaltyInfoAsync(Guid? merchantId = null)
        {
            var targetId = merchantId ?? _currentUser.MerchantId;
            if (!targetId.HasValue) return (null, null);

            var merchant = await _context.Merchants.FirstOrDefaultAsync(m => m.Id == targetId.Value);
            if (merchant == null) return (null, null);

            var safeName = Regex.Replace(merchant.Name.ToUpperInvariant(), @"[^A-Z0-9]", "_");
            return ($"APH_POS_{safeName}", null);
        }

        private async Task<(string? systemId, string? apiKey)> GetCustomerMerchantLoyaltyInfoAsync(Guid customerId)
        {
            var customer = await _context.Customers
                .Include(c => c.Merchant)
                .FirstOrDefaultAsync(c => c.Id == customerId);
            if (customer?.Merchant == null) return (null, null);

            var safeName = Regex.Replace(customer.Merchant.Name.ToUpperInvariant(), @"[^A-Z0-9]", "_");
            return ($"APH_POS_{safeName}", null);
        }

        [HttpGet("customer/{customerId}")]
        public async Task<ActionResult<Result<LoyaltyAccountResponse>>> GetCustomerLoyalty(Guid customerId)
        {
            var (systemId, apiKey) = await GetCustomerMerchantLoyaltyInfoAsync(customerId);
            var result = await _loyaltyServices.GetCustomerLoyaltyAsync(customerId, systemId, apiKey);
            return Ok(result);
        }

        [HttpGet("customer/{customerId}/history")]
        public async Task<ActionResult<Result<List<LoyaltyHistoryDto>>>> GetCustomerHistory(Guid customerId)
        {
            var (systemId, apiKey) = await GetCustomerMerchantLoyaltyInfoAsync(customerId);
            var result = await _loyaltyServices.GetCustomerHistoryAsync(customerId, systemId, apiKey);
            return Ok(result);
        }

        [HttpGet("rewards")]
        public async Task<ActionResult<Result<List<LoyaltyReward>>>> GetRewards()
        {
            var (systemId, apiKey) = await GetMerchantLoyaltyInfoAsync();
            var result = await _loyaltyServices.GetActiveRewardsAsync(systemId, apiKey);
            return Ok(result);
        }

        [HttpGet("rules")]
        public async Task<ActionResult<Result<List<LoyaltyRuleDto>>>> GetRules()
        {
            var (systemId, apiKey) = await GetMerchantLoyaltyInfoAsync();
            var result = await _loyaltyServices.GetActiveRulesAsync(systemId, apiKey);
            return Ok(result);
        }

        [HttpPost("claim")]
        public async Task<ActionResult<Result<bool>>> ClaimReward([FromBody] ClaimRewardRequest request)
        {
            var (systemId, apiKey) = await GetCustomerMerchantLoyaltyInfoAsync(request.CustomerId);
            var result = await _loyaltyServices.ClaimRewardAsync(request.CustomerId, request.RewardId, request.Notes, systemId, apiKey);
            return Ok(result);
        }

        [HttpGet("admin/stats")]
        public async Task<ActionResult<Result<LoyaltyAdminStatsResponse>>> GetAdminStats()
        {
            var result = await _loyaltyServices.GetAdminStatsAsync();
            return Ok(result);
        }

        [HttpGet("admin/redemptions/history")]
        public async Task<ActionResult<Result<PagedRedemptionHistoryResponse>>> GetRedemptionHistory([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? status = null, [FromQuery] string? searchTerm = null)
        {
            var (systemId, apiKey) = await GetMerchantLoyaltyInfoAsync();
            var result = await _loyaltyServices.GetRedemptionHistoryAsync(page, pageSize, status, searchTerm, systemId, apiKey);
            return Ok(result);
        }

        [HttpGet("admin/global-ledger")]
        public async Task<ActionResult<Result<PagedLedgerHistoryResponse>>> GetGlobalLedger([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? searchTerm = null)
        {
            var (systemId, apiKey) = await GetMerchantLoyaltyInfoAsync();
            var result = await _loyaltyServices.GetGlobalLedgerAsync(page, pageSize, searchTerm, systemId, apiKey);
            return Ok(result);
        }
    }

    public class ClaimRewardRequest
    {
        public Guid CustomerId { get; set; }
        public Guid RewardId { get; set; }
        public string? Notes { get; set; }
    }
}
