using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using POS.Backend.Common;

namespace POS.Backend.Features.Loyalty
{
    public interface ILoyaltyServices
    {
        Task<Result<bool>> ProcessSaleEventAsync(Guid customerId, string customerName, decimal amount, Guid orderId, string? email = null, string? mobile = null, string? eventKey = null, string? systemId = null, string? apiKey = null);
        Task<Result<LoyaltyAccountResponse>> GetCustomerLoyaltyAsync(Guid customerId, string? systemId = null, string? apiKey = null);
        Task<Result<List<LoyaltyReward>>> GetActiveRewardsAsync(string? systemId = null, string? apiKey = null);
        Task<Result<bool>> ClaimRewardAsync(Guid customerId, Guid rewardId, string? notes = null, string? systemId = null, string? apiKey = null);
        Task<Result<List<LoyaltyRuleDto>>> GetActiveRulesAsync(string? systemId = null, string? apiKey = null);
        Task<Result<List<LoyaltyHistoryDto>>> GetCustomerHistoryAsync(Guid customerId, string? systemId = null, string? apiKey = null);
        Task<Result<LoyaltyAdminStatsResponse>> GetAdminStatsAsync();
        Task<Result<PagedRedemptionHistoryResponse>> GetRedemptionHistoryAsync(int page = 1, int pageSize = 10, string? status = null, string? searchTerm = null, string? systemId = null, string? apiKey = null);
        Task<Result<PagedLedgerHistoryResponse>> GetGlobalLedgerAsync(int page = 1, int pageSize = 10, string? searchTerm = null, string? systemId = null, string? apiKey = null);
    }

    public class LoyaltySettings
    {
        public string BaseUrl { get; set; } = string.Empty;
        public string SystemId { get; set; } = string.Empty;
        public string? ApiKey { get; set; }
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
        }

        private async Task<HttpResponseMessage> SendWithHeadersAsync(HttpMethod method, string url, object? body = null, string? systemId = null, string? apiKey = null)
        {
            var request = new HttpRequestMessage(method, url);
            
            var targetSystemId = !string.IsNullOrWhiteSpace(systemId) ? systemId : _settings.SystemId;
            var targetApiKey = !string.IsNullOrWhiteSpace(apiKey) ? apiKey : _settings.ApiKey;

            if (!string.IsNullOrWhiteSpace(targetSystemId))
                request.Headers.Add("x-system-id", targetSystemId);
            
            if (!string.IsNullOrWhiteSpace(targetApiKey))
                request.Headers.Add("x-api-key", targetApiKey);

            if (body != null)
            {
                request.Content = JsonContent.Create(body);
            }

            return await _httpClient.SendAsync(request);
        }

        public async Task<Result<bool>> ProcessSaleEventAsync(Guid customerId, string customerName, decimal amount, Guid orderId, string? email = null, string? mobile = null, string? eventKey = null, string? systemId = null, string? apiKey = null)
        {
            try
            {
                var eventProcessRequest = new LoyaltyEventProcessRequest
                {
                    ExternalUserId = customerId.ToString(),
                    EventKey = eventKey ?? _settings.DefaultEventKey,
                    EventValue = (double)amount,
                    ReferenceId = orderId.ToString(),
                    Description = $"Purchase by {customerName} - Order {orderId}",
                    Email = email,
                    Mobile = mobile
                };

                var response = await SendWithHeadersAsync(HttpMethod.Post, "api/v1/events/process", eventProcessRequest, systemId, apiKey);
                
                if (response.IsSuccessStatusCode)
                {
                    return Result<bool>.Success(true);
                }

                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Loyalty API Error processing event: {Status} - {Error}", response.StatusCode, error);
                return Result<bool>.Failure($"Loyalty API error: {response.StatusCode} - {error}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process loyalty event");
                return Result<bool>.Failure($"Failed to connect to Loyalty API: {ex.Message}");
            }
        }

        public async Task<Result<LoyaltyAccountResponse>> GetCustomerLoyaltyAsync(Guid customerId, string? systemId = null, string? apiKey = null)
        {
            try
            {
                var targetSystemId = !string.IsNullOrWhiteSpace(systemId) ? systemId : _settings.SystemId;
                var response = await SendWithHeadersAsync(HttpMethod.Get, $"api/v1/accounts/lookup/{targetSystemId}/{customerId}", null, systemId, apiKey);

                if (response.IsSuccessStatusCode)
                {
                    var account = await response.Content.ReadFromJsonAsync<LoyaltyAccountResponse>();
                    return account != null ? Result<LoyaltyAccountResponse>.Success(account) : Result<LoyaltyAccountResponse>.Failure("Empty response");
                }

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return Result<LoyaltyAccountResponse>.Failure("Customer loyalty account not found");

                return Result<LoyaltyAccountResponse>.Failure($"Loyalty API error: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch loyalty account");
                return Result<LoyaltyAccountResponse>.Failure($"Failed to connect to Loyalty API: {ex.Message}");
            }
        }

        public async Task<Result<List<LoyaltyReward>>> GetActiveRewardsAsync(string? systemId = null, string? apiKey = null)
        {
            try
            {
                var targetSystemId = !string.IsNullOrWhiteSpace(systemId) ? systemId : _settings.SystemId;
                var response = await SendWithHeadersAsync(HttpMethod.Get, $"api/v1/rewards/active/{targetSystemId}", null, systemId, apiKey);

                if (response.IsSuccessStatusCode)
                {
                    var rewards = await response.Content.ReadFromJsonAsync<List<LoyaltyReward>>();
                    return rewards != null ? Result<List<LoyaltyReward>>.Success(rewards) : Result<List<LoyaltyReward>>.Failure("Empty response");
                }

                return Result<List<LoyaltyReward>>.Failure($"Loyalty API error: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch active rewards");
                return Result<List<LoyaltyReward>>.Failure($"Failed to connect to Loyalty API: {ex.Message}");
            }
        }

        public async Task<Result<bool>> ClaimRewardAsync(Guid customerId, Guid rewardId, string? notes = null, string? systemId = null, string? apiKey = null)
        {
            try
            {
                var body = new
                {
                    externalUserId = customerId.ToString(),
                    rewardId = rewardId,
                    notes = notes
                };

                var claimResponse = await SendWithHeadersAsync(HttpMethod.Post, "api/v1/redemption/claim", body, systemId, apiKey);

                if (!claimResponse.IsSuccessStatusCode)
                {
                    var error = await claimResponse.Content.ReadAsStringAsync();
                    return Result<bool>.Failure($"Loyalty claim error: {error}");
                }

                // Auto-fulfill: find the newly created pending redemption and mark it Fulfilled
                await AutoFulfillRedemptionAsync(customerId, rewardId, systemId, apiKey);

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure($"Failed to connect to Loyalty API: {ex.Message}");
            }
        }

        private async Task AutoFulfillRedemptionAsync(Guid customerId, Guid rewardId, string? systemId, string? apiKey)
        {
            try
            {
                var pendingResponse = await SendWithHeadersAsync(HttpMethod.Get, "api/v1/admin/redemptions/pending", null, systemId, apiKey);
                if (!pendingResponse.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Auto-fulfill: could not fetch pending redemptions (HTTP {Status})", pendingResponse.StatusCode);
                    return;
                }

                var root = await pendingResponse.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();

                // Support both a raw array [ {...} ] and a wrapped object { items: [...] }
                System.Text.Json.JsonElement pendingArray = default;
                if (root.ValueKind == System.Text.Json.JsonValueKind.Array)
                {
                    pendingArray = root;
                }
                else if (root.ValueKind == System.Text.Json.JsonValueKind.Object)
                {
                    if (root.TryGetProperty("items", out var items) && items.ValueKind == System.Text.Json.JsonValueKind.Array)
                        pendingArray = items;
                    else if (root.TryGetProperty("data", out var data) && data.ValueKind == System.Text.Json.JsonValueKind.Array)
                        pendingArray = data;
                }

                if (pendingArray.ValueKind != System.Text.Json.JsonValueKind.Array)
                {
                    _logger.LogWarning("Auto-fulfill: unexpected pending redemptions response shape");
                    return;
                }

                foreach (var pending in pendingArray.EnumerateArray())
                {
                    // Safely read externalUserId
                    if (!pending.TryGetProperty("externalUserId", out var userIdProp) ||
                        userIdProp.GetString() != customerId.ToString())
                        continue;

                    // Safely read rewardId (may be Guid or string)
                    Guid pendingRewardId = Guid.Empty;
                    if (pending.TryGetProperty("rewardId", out var rewardIdProp))
                    {
                        if (rewardIdProp.ValueKind == System.Text.Json.JsonValueKind.String)
                            Guid.TryParse(rewardIdProp.GetString(), out pendingRewardId);
                        else
                            rewardIdProp.TryGetGuid(out pendingRewardId);
                    }

                    if (pendingRewardId != rewardId) continue;

                    // Safely read redemption id
                    if (!pending.TryGetProperty("id", out var idProp) || !idProp.TryGetGuid(out var redemptionId))
                    {
                        _logger.LogWarning("Auto-fulfill: matched pending redemption has no valid 'id'");
                        continue;
                    }

                    var fulfillResponse = await SendWithHeadersAsync(
                        HttpMethod.Put,
                        $"api/v1/admin/redemptions/{redemptionId}/status",
                        new { status = "Fulfilled" },
                        systemId, apiKey);

                    if (fulfillResponse.IsSuccessStatusCode)
                        _logger.LogInformation("Auto-fulfill: redemption {RedemptionId} fulfilled for customer {CustomerId}", redemptionId, customerId);
                    else
                        _logger.LogWarning("Auto-fulfill: failed to fulfill redemption {RedemptionId} (HTTP {Status})", redemptionId, fulfillResponse.StatusCode);

                    break; // only fulfill the first match
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Auto-fulfill: unexpected error during auto-fulfillment for customer {CustomerId}, reward {RewardId}", customerId, rewardId);
            }
        }

        public async Task<Result<List<LoyaltyRuleDto>>> GetActiveRulesAsync(string? systemId = null, string? apiKey = null)
        {
            try
            {
                var response = await SendWithHeadersAsync(HttpMethod.Get, "api/v1/admin/rules", null, systemId, apiKey);
                if (response.IsSuccessStatusCode)
                {
                    var rules = await response.Content.ReadFromJsonAsync<List<LoyaltyRuleDto>>();
                    return rules != null ? Result<List<LoyaltyRuleDto>>.Success(rules) : Result<List<LoyaltyRuleDto>>.Failure("Empty response");
                }
                return Result<List<LoyaltyRuleDto>>.Failure($"Loyalty API error: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                return Result<List<LoyaltyRuleDto>>.Failure($"Failed to connect to Loyalty API: {ex.Message}");
            }
        }

        public async Task<Result<List<LoyaltyHistoryDto>>> GetCustomerHistoryAsync(Guid customerId, string? systemId = null, string? apiKey = null)
        {
            try
            {
                var accountResult = await GetCustomerLoyaltyAsync(customerId, systemId, apiKey);
                if (!accountResult.IsSuccess || accountResult.Value == null)
                    return Result<List<LoyaltyHistoryDto>>.Failure(accountResult.Error ?? "Account not found");

                var accountId = accountResult.Value.AccountId != Guid.Empty ? accountResult.Value.AccountId : accountResult.Value.Id;
                var response = await SendWithHeadersAsync(HttpMethod.Get, $"api/v1/accounts/{accountId}/history", null, systemId, apiKey);

                if (response.IsSuccessStatusCode)
                {
                    var history = await response.Content.ReadFromJsonAsync<List<LoyaltyHistoryDto>>();
                    return history != null ? Result<List<LoyaltyHistoryDto>>.Success(history) : Result<List<LoyaltyHistoryDto>>.Failure("Empty response");
                }
                return Result<List<LoyaltyHistoryDto>>.Failure($"Loyalty API error: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                return Result<List<LoyaltyHistoryDto>>.Failure($"Failed to connect to Loyalty API: {ex.Message}");
            }
        }

        public async Task<Result<LoyaltyAdminStatsResponse>> GetAdminStatsAsync()
        {
            try
            {
                var response = await SendWithHeadersAsync(HttpMethod.Get, "api/v1/admin/stats");
                if (response.IsSuccessStatusCode)
                {
                    var stats = await response.Content.ReadFromJsonAsync<LoyaltyAdminStatsResponse>();
                    return stats != null ? Result<LoyaltyAdminStatsResponse>.Success(stats) : Result<LoyaltyAdminStatsResponse>.Failure("Empty response");
                }
                return Result<LoyaltyAdminStatsResponse>.Failure($"Loyalty API error: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                return Result<LoyaltyAdminStatsResponse>.Failure($"Failed to connect to Loyalty API: {ex.Message}");
            }
        }

        public async Task<Result<PagedRedemptionHistoryResponse>> GetRedemptionHistoryAsync(int page = 1, int pageSize = 10, string? status = null, string? searchTerm = null, string? systemId = null, string? apiKey = null)
        {
            try
            {
                var targetSystemId = !string.IsNullOrWhiteSpace(systemId) ? systemId : _settings.SystemId;
                var url = $"api/v1/admin/redemptions/history?SystemId={targetSystemId}&Page={page}&PageSize={pageSize}";
                if (!string.IsNullOrEmpty(status)) url += $"&Status={status}";
                if (!string.IsNullOrEmpty(searchTerm)) url += $"&SearchTerm={searchTerm}";

                var response = await SendWithHeadersAsync(HttpMethod.Get, url, null, systemId, apiKey);
                if (response.IsSuccessStatusCode)
                {
                    var history = await response.Content.ReadFromJsonAsync<PagedRedemptionHistoryResponse>();
                    return history != null ? Result<PagedRedemptionHistoryResponse>.Success(history) : Result<PagedRedemptionHistoryResponse>.Failure("Empty response");
                }
                return Result<PagedRedemptionHistoryResponse>.Failure($"Loyalty API error: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                return Result<PagedRedemptionHistoryResponse>.Failure($"Failed to connect to Loyalty API: {ex.Message}");
            }
        }

        public async Task<Result<PagedLedgerHistoryResponse>> GetGlobalLedgerAsync(int page = 1, int pageSize = 10, string? searchTerm = null, string? systemId = null, string? apiKey = null)
        {
            try
            {
                var targetSystemId = !string.IsNullOrWhiteSpace(systemId) ? systemId : _settings.SystemId;
                var url = $"api/v1/admin/ledger/search-paged?SystemId={targetSystemId}&Page={page}&PageSize={pageSize}";
                if (!string.IsNullOrEmpty(searchTerm)) url += $"&SearchTerm={searchTerm}";

                var response = await SendWithHeadersAsync(HttpMethod.Get, url, null, systemId, apiKey);
                if (response.IsSuccessStatusCode)
                {
                    var history = await response.Content.ReadFromJsonAsync<PagedLedgerHistoryResponse>();
                    return history != null ? Result<PagedLedgerHistoryResponse>.Success(history) : Result<PagedLedgerHistoryResponse>.Failure("Empty response");
                }
                return Result<PagedLedgerHistoryResponse>.Failure($"Loyalty API error: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                return Result<PagedLedgerHistoryResponse>.Failure($"Failed to connect to Loyalty API: {ex.Message}");
            }
        }
    }
}
