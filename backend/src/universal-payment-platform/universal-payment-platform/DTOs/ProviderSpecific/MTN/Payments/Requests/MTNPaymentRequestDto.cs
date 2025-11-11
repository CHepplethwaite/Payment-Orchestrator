using System.Text.Json.Serialization;
using universal_payment_platform.DTOs.ProviderSpecific.MTN.Payments.Requests;

namespace UniversalPaymentPlatform.DTOs.ProviderSpecific.MTN.Payments.Requests
{
    public record MTNPaymentRequestDto
    {
        [JsonPropertyName("correlatorId")]
        public required string CorrelatorId { get; init; }

        [JsonPropertyName("paymentDate")]
        public string? PaymentDate { get; init; }

        [JsonPropertyName("name")]
        public string? Name { get; init; }

        [JsonPropertyName("callingSystem")]
        public string? CallingSystem { get; init; }

        [JsonPropertyName("transactionType")]
        public required string TransactionType { get; init; }

        [JsonPropertyName("targetSystem")]
        public string? TargetSystem { get; init; }

        [JsonPropertyName("callbackURL")]
        public required string CallbackURL { get; init; }

        [JsonPropertyName("quoteId")]
        public string? QuoteId { get; init; }

        [JsonPropertyName("channel")]
        public string? Channel { get; init; }

        [JsonPropertyName("description")]
        public string? Description { get; init; }

        [JsonPropertyName("authorizationCode")]
        public string? AuthorizationCode { get; init; }

        [JsonPropertyName("feeBearer")]
        public string? FeeBearer { get; init; }

        [JsonPropertyName("amount")]
        public required MTNMonetaryAmountDto Amount { get; init; }

        [JsonPropertyName("taxAmount")]
        public MTNMonetaryAmountDto? TaxAmount { get; init; }

        [JsonPropertyName("totalAmount")]
        public required MTNMonetaryAmountDto TotalAmount { get; init; }

        [JsonPropertyName("payer")]
        public required MTNPayerDto Payer { get; init; }

        [JsonPropertyName("payee")]
        public required List<MTNPayeeDto> Payee { get; init; }

        [JsonPropertyName("paymentMethod")]
        public required MTNPaymentMethodDto PaymentMethod { get; init; }

        [JsonPropertyName("additionalInformation")]
        public List<MTNAdditionalInfoDto>? AdditionalInformation { get; init; }
    }
}
