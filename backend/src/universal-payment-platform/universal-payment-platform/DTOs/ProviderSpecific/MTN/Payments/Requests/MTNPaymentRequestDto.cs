using UniversalPaymentPlatform.DTOs.ProviderSpecific.MTN.Payments.Enums;
using System.Text.Json.Serialization;
using universal_payment_platform.DTOs.ProviderSpecific.MTN.Shared.Enums;
using universal_payment_platform.DTOs.ProviderSpecific.MTN.Shared;

namespace UniversalPaymentPlatform.DTOs.ProviderSpecific.MTN.Payments.Requests
{
    public record MTNPaymentRequestDto
    {
        [JsonPropertyName("correlatorId")]
        public required string CorrelatorId { get; init; }

        [JsonPropertyName("paymentDate")]
        public DateTime? PaymentDate { get; init; }

        [JsonPropertyName("name")]
        public string? Name { get; init; }

        [JsonPropertyName("callingSystem")]
        public CallingSystem? CallingSystem { get; init; }

        [JsonPropertyName("transactionType")]
        public required TransactionType TransactionType { get; init; }

        [JsonPropertyName("targetSystem")]
        public TargetSystem? TargetSystem { get; init; }

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
        public FeeBearer? FeeBearer { get; init; }

        [JsonPropertyName("amount")]
        public required MonetaryTypeDto Amount { get; init; }

        [JsonPropertyName("taxAmount")]
        public MonetaryTypeDto? TaxAmount { get; init; }

        [JsonPropertyName("totalAmount")]
        public required MonetaryTypeDto TotalAmount { get; init; }

        [JsonPropertyName("payer")]
        public required PayerDto Payer { get; init; }

        [JsonPropertyName("payee")]
        public required List<PayeeDto> Payees { get; init; }

        [JsonPropertyName("paymentMethod")]
        public required PaymentMethodDto PaymentMethod { get; init; }

        [JsonPropertyName("additionalInformation")]
        public List<MTNAdditionalInfoDto>? AdditionalInformation { get; init; }

        [JsonPropertyName("segment")]
        public Segment? Segment { get; init; }
    }
}