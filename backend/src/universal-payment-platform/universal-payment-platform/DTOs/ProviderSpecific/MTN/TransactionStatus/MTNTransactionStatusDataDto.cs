using System.Text.Json.Serialization;

namespace UniversalPaymentPlatform.DTOs.ProviderSpecific.MTN
{
    public record MTNTransactionStatusDataDto
    {
        [JsonPropertyName("amount")]
        public decimal? Amount { get; init; }

        [JsonPropertyName("paymentType")]
        public string? PaymentType { get; init; }

        [JsonPropertyName("customerId")]
        public string? CustomerId { get; init; }

        [JsonPropertyName("status")]
        public string? Status { get; init; }

        [JsonPropertyName("description")]
        public string? Description { get; init; }

        [JsonPropertyName("targetSystem")]
        public string? TargetSystem { get; init; }
    }
}
