using System;
using System.Text.Json.Serialization;

namespace UniversalPaymentPlatform.DTOs.ProviderSpecific.MTN.Payments.Requests
{
    public record MTNPayeeDto
    {
        [JsonPropertyName("amount")]
        public MTNMonetaryAmountDto? Amount { get; init; }

        [JsonPropertyName("taxAmount")]
        public MTNMonetaryAmountDto? TaxAmount { get; init; }

        [JsonPropertyName("totalAmount")]
        public required MTNMonetaryAmountDto TotalAmount { get; init; }

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
