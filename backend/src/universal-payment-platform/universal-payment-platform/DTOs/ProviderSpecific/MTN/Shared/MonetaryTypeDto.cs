using System.Text.Json.Serialization;

namespace universal_payment_platform.DTOs.ProviderSpecific.MTN.Shared
{
    public record MonetaryTypeDto
    {
        [JsonPropertyName("amount")]
        public required decimal Amount { get; init; }

        [JsonPropertyName("units")]
        public required string Units { get; init; }
    }
}
