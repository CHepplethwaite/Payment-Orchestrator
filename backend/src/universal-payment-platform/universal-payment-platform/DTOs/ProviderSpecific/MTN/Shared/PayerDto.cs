using System.Text.Json.Serialization;

namespace universal_payment_platform.DTOs.ProviderSpecific.MTN.Shared
{
    public record PayerDto
    {
        [JsonPropertyName("payerIdType")]
        public string? PayerIdType { get; init; }

        [JsonPropertyName("payerId")]
        public required string PayerId { get; init; }

        [JsonPropertyName("payerNote")]
        public string? PayerNote { get; init; }

        [JsonPropertyName("payerName")]
        public string? PayerName { get; init; }

        [JsonPropertyName("payerEmail")]
        public string? PayerEmail { get; init; }

        [JsonPropertyName("payerRef")]
        public string? PayerRef { get; init; }

        [JsonPropertyName("payerSurname")]
        public string? PayerSurname { get; init; }

        [JsonPropertyName("includePayerCharges")]
        public bool? IncludePayerCharges { get; init; }
    }
}
