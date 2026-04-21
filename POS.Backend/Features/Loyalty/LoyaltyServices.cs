using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using POS.Backend.Common;

namespace POS.Backend.Features.Loyalty
{
    public interface ILoyaltyServices
    {
        Task<Result<bool>> ProcessSaleEventAsync(Guid customerId, string customerName, decimal amount, Guid orderId, string? email = null, string? mobile = null, string? eventKey = null);
        Task<Result<LoyaltyAccountResponse>> GetCustomerLoyaltyAsync(Guid customerId);
        Task<Result<List<LoyaltyReward>>> GetActiveRewardsAsync();
        Task<Result<bool>> ClaimRewardAsync(Guid customerId, Guid rewardId, string? notes = null);
        Task<Result<List<LoyaltyRuleDto>>> GetActiveRulesAsync();
    }

    public class LoyaltySettings
    {
        public string BaseUrl { get; set; } = string.Empty;
        public string SystemId { get; set; } = string.Empty;
        public string DefaultEventKey { get; set; } = "PURCHASE";
    }


    public class LoyaltyServices : ILoyaltyServices
    {
        private readonly HttpClient _httpClient;
        private readonly LoyaltySettings _settings;
        private readonly ILogger<LoyaltyServices> _logger;

        public LoyaltyServices(HttpClient httpClient, IOptions<LoyaltySettings> settings, ILogger<LoyaltyServices> logger)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
            _logger = logger;

            if (string.IsNullOrEmpty(_httpClient.BaseAddress?.ToString()) && !string.IsNullOrEmpty(_settings.BaseUrl))
            {
                _httpClient.BaseAddress = new Uri(_settings.BaseUrl);
            }
            
            if (!_httpClient.DefaultRequestHeaders.Contains("x-system-id"))
            {
                _httpClient.DefaultRequestHeaders.Add("x-system-id", _settings.SystemId);
            }
        }

        public async Task<Result<bool>> ProcessSaleEventAsync(Guid customerId, string customerName, decimal amount, Guid orderId, string? email = null, string? mobile = null, string? eventKey = null)
        {
            try
            {
                var request = new LoyaltyEventProcessRequest
                {
                    ExternalUserId = customerId.ToString(),
                    EventKey = eventKey ?? _settings.DefaultEventKey,
                    EventValue = (double)amount,
                    ReferenceId = orderId.ToString(),
                    Description = $"Purchase by {customerName} - Order {orderId}",
                    Email = email,
                    Mobile = mobile
                };

                var response = await _httpClient.PostAsJsonAsync("api/v1/events/process", request);
                
                if (response.IsSuccessStatusCode)
                {
                    return Result<bool>.Success(true);
                }

                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Loyalty API Error processing event for customer {CustomerId} (Order {OrderId}): {Status} - {Error}", 
                    customerId, orderId, response.StatusCode, error);
                return Result<bool>.Failure($"Loyalty API error: {response.StatusCode} - {error}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process loyalty event for customer {CustomerId} (Order {OrderId}) at {Url}", 
                    customerId, orderId, _httpClient.BaseAddress);
                return Result<bool>.Failure($"Failed to connect to Loyalty API: {ex.Message}");
            }
        }

        public async Task<Result<LoyaltyAccountResponse>> GetCustomerLoyaltyAsync(Guid customerId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/v1/accounts/lookup/{_settings.SystemId}/{customerId}");

                if (response.IsSuccessStatusCode)
                {
                    var account = await response.Content.ReadFromJsonAsync<LoyaltyAccountResponse>();
                    return account != null ? Result<LoyaltyAccountResponse>.Success(account) : Result<LoyaltyAccountResponse>.Failure("Empty response from Loyalty API");
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("Customer loyalty account not found for {CustomerId}", customerId);
                    return Result<LoyaltyAccountResponse>.Failure("Customer loyalty account not found");
                }

                _logger.LogError("Loyalty API Error fetching account for {CustomerId}: {Status} - {Error}", 
                    customerId, response.StatusCode, errorContent);
                return Result<LoyaltyAccountResponse>.Failure($"Loyalty API error: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch loyalty account for {CustomerId}", customerId);
                return Result<LoyaltyAccountResponse>.Failure($"Failed to connect to Loyalty API: {ex.Message}");
            }
        }

        public async Task<Result<List<LoyaltyReward>>> GetActiveRewardsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/v1/rewards/active/{_settings.SystemId}");

                if (response.IsSuccessStatusCode)
                {
                    var rewards = await response.Content.ReadFromJsonAsync<List<LoyaltyReward>>();
                    return rewards != null ? Result<List<LoyaltyReward>>.Success(rewards) : Result<List<LoyaltyReward>>.Failure("Empty response from Loyalty API");
                }

                return Result<List<LoyaltyReward>>.Failure($"Loyalty API error: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch active rewards");
                return Result<List<LoyaltyReward>>.Failure($"Failed to connect to Loyalty API: {ex.Message}");
            }
        }

        public async Task<Result<bool>> ClaimRewardAsync(Guid customerId, Guid rewardId, string? notes = null)
        {
            try
            {
                var request = new
                {
                    externalUserId = customerId.ToString(),
                    rewardId = rewardId,
                    notes = notes
                };

                var response = await _httpClient.PostAsJsonAsync("api/v1/redemption/claim", request);

                if (response.IsSuccessStatusCode)
                {
                    return Result<bool>.Success(true);
                }

                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Loyalty Claim Error: {Error}", error);
                return Result<bool>.Failure($"Loyalty claim error: {error}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to claim reward");
                return Result<bool>.Failure($"Failed to connect to Loyalty API: {ex.Message}");
            }
        }
        public async Task<Result<List<LoyaltyRuleDto>>> GetActiveRulesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/v1/admin/rules");

                if (response.IsSuccessStatusCode)
                {
                    var rules = await response.Content.ReadFromJsonAsync<List<LoyaltyRuleDto>>();
                    return rules != null ? Result<List<LoyaltyRuleDto>>.Success(rules) : Result<List<LoyaltyRuleDto>>.Failure("Empty response from Loyalty API");
                }

                return Result<List<LoyaltyRuleDto>>.Failure($"Loyalty API error: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch active rules");
                return Result<List<LoyaltyRuleDto>>.Failure($"Failed to connect to Loyalty API: {ex.Message}");
            }
        }
    }
}
