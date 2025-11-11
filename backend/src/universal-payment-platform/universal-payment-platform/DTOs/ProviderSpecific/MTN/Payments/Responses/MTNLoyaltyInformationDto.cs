using System.Text.Json.Serialization;

namespace UniversalPaymentPlatform.DTOs.ProviderSpecific.MTN.Payments.Responses
{
    public record MTNLoyaltyInformationDto
    {
        [JsonPropertyName("points")]
        public decimal? Points { get; init; }

        [JsonPropertyName("tier")]
        public string? Tier { get; init; }

        [JsonPropertyName("expiryDate")]
        public string? ExpiryDate { get; init; }
    }
}
