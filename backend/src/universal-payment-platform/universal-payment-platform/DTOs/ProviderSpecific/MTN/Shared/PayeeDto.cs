using System;
using System.Text.Json.Serialization;

namespace universal_payment_platform.DTOs.ProviderSpecific.MTN.Shared
{
    public record PayeeDto
    {
        [JsonPropertyName("amount")]
        public MonetaryAmountDto? Amount { get; init; }

        [JsonPropertyName("taxAmount")]
        public MonetaryAmountDto? TaxAmount { get; init; }

        [JsonPropertyName("totalAmount")]
        public required MonetaryAmountDto TotalAmount { get; init; }

        [JsonPropertyName("payeeIdType")]
        public string? PayeeIdType { get; init; }

        [JsonPropertyName("payeeId")]
        public string? PayeeId { get; init; }

        [JsonPropertyName("payeeNote")]
        public string? PayeeNote { get; init; }

        [JsonPropertyName("payeeName")]
        public string? PayeeName { get; init; }
    }
}
