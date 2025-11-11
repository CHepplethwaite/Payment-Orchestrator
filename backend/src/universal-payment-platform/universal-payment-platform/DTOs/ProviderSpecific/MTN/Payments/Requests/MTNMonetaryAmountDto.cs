using System.Text.Json.Serialization;

namespace UniversalPaymentPlatform.DTOs.ProviderSpecific.MTN.Payments.Requests
{
    public record MTNMonetaryAmountDto
    {
        [JsonPropertyName("amount")]
        public required decimal Amount { get; init; }

        [JsonPropertyName("units")]
        public required string Units { get; init; }
    }
}
