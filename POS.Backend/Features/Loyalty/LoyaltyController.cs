using Microsoft.AspNetCore.Mvc;
using POS.Backend.Features.Loyalty;
using POS.Backend.Common;

namespace POS.Backend.Features.Loyalty
{
    [ApiController]
    [Route("api/v1/loyalty")]
    public class LoyaltyController : ControllerBase
    {
        private readonly ILoyaltyServices _loyaltyServices;

        public LoyaltyController(ILoyaltyServices loyaltyServices)
        {
            _loyaltyServices = loyaltyServices;
        }

        [HttpGet("customer/{customerId}")]
        public async Task<ActionResult<Result<LoyaltyAccountResponse>>> GetCustomerLoyalty(Guid customerId)
        {
            var result = await _loyaltyServices.GetCustomerLoyaltyAsync(customerId);
            return Ok(result);
        }

        [HttpGet("rewards")]
        public async Task<ActionResult<Result<List<LoyaltyReward>>>> GetRewards()
        {
            var result = await _loyaltyServices.GetActiveRewardsAsync();
            return Ok(result);
        }

        [HttpGet("rules")]
        public async Task<ActionResult<Result<List<LoyaltyRuleDto>>>> GetRules()
        {
            var result = await _loyaltyServices.GetActiveRulesAsync();
            return Ok(result);
        }

        [HttpPost("claim")]
        public async Task<ActionResult<Result<bool>>> ClaimReward([FromBody] ClaimRewardRequest request)
        {
            var result = await _loyaltyServices.ClaimRewardAsync(request.CustomerId, request.RewardId, request.Notes);
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
