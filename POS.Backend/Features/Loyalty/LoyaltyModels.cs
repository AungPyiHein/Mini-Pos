using System.Text.Json.Serialization;

namespace POS.Backend.Features.Loyalty
{
    public class LoyaltyEventProcessRequest
    {
        [JsonPropertyName("externalUserId")]
        public string ExternalUserId { get; set; } = string.Empty;

        [JsonPropertyName("eventKey")]
        public string EventKey { get; set; } = string.Empty;

        [JsonPropertyName("eventValue")]
        public double EventValue { get; set; }

        [JsonPropertyName("referenceId")]
        public string? ReferenceId { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("mobile")]
        public string? Mobile { get; set; }

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("metadata")]
        public object? Metadata { get; set; }
    }

    public class LoyaltyAccountResponse
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("systemId")]
        public string SystemId { get; set; } = string.Empty;

        [JsonPropertyName("externalUserId")]
        public string ExternalUserId { get; set; } = string.Empty;

        [JsonPropertyName("tier")]
        public string? Tier { get; set; }

        [JsonPropertyName("totalPointsEarned")]
        public decimal TotalPointsEarned { get; set; }

        [JsonPropertyName("totalPointsSpent")]
        public decimal TotalPointsSpent { get; set; }

        [JsonPropertyName("currentBalance")]
        public decimal CurrentBalance { get; set; }

        [JsonPropertyName("mobile")]
        public string? Mobile { get; set; }

        [JsonPropertyName("email")]
        public string? Email { get; set; }
    }

    public class LoyaltyReward
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("pointCost")]
        public int PointCost { get; set; }

        [JsonPropertyName("stockQuantity")]
        public int? StockQuantity { get; set; }

        [JsonPropertyName("isActive")]
        public bool IsActive { get; set; }
    }

    public class LoyaltyRuleDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("systemId")]
        public string SystemId { get; set; } = string.Empty;

        [JsonPropertyName("eventKey")]
        public string EventKey { get; set; } = string.Empty;

        [JsonPropertyName("calculationType")]
        public string CalculationType { get; set; } = string.Empty;

        [JsonPropertyName("value")]
        public decimal Value { get; set; }

        [JsonPropertyName("isActive")]
        public bool IsActive { get; set; }
    }
}
