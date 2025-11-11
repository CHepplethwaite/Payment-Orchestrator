using System.Text.Json.Serialization;

namespace UniversalPaymentPlatform.DTOs.ProviderSpecific.MTN
{
    public record MTNTransactionStatusCustomerDto
    {
        [JsonPropertyName("brand")]
        public string? Brand { get; init; }

        [JsonPropertyName("fulfillmentMsisdn")]
        public string? FulfillmentMsisdn { get; init; }

        [JsonPropertyName("issuer")]
        public string? Issuer { get; init; }
    }
}
