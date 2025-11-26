using System.Text.Json.Serialization;
using universal_payment_platform.DTOs.ProviderSpecific.MTN.Shared;

namespace universal_payment_platform.DTOs.ProviderSpecific.MTN.Fee.Responses
{
    public record FeeDetailDto
    {
        [JsonPropertyName("feeType")]
        public string? FeeType { get; init; }

        [JsonPropertyName("amount")]
        public MonetaryTypeDto? Amount { get; init; }

        [JsonPropertyName("description")]
        public string? Description { get; init; }

        [JsonPropertyName("recipient")]
        public string? Recipient { get; init; }
    }
}
