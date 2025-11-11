using System.Text.Json.Serialization;

namespace UniversalPaymentPlatform.DTOs.ProviderSpecific.MTN.Payments.Responses
{
    public record MTNPaymentLinksDto
    {
        [JsonPropertyName("self")]
        public string? Self { get; init; }

        [JsonPropertyName("related")]
        public string? Related { get; init; }
    }
}
