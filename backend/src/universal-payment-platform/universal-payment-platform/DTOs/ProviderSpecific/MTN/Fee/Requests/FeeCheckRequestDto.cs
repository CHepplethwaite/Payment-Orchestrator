using System.Text.Json.Serialization;
using universal_payment_platform.DTOs.ProviderSpecific.MTN.Shared;
using universal_payment_platform.DTOs.ProviderSpecific.MTN.Shared.Enums;

namespace universal_payment_platform.DTOs.ProviderSpecific.MTN.FeeCheck
{
    public record FeeCheckRequestDto
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
        public required string CallbackUrl { get; init; }

        [JsonPropertyName("quoteId")]
        public string? QuoteId { get; init; }

        [JsonPropertyName("channel")]
        public string? Channel { get; init; }

        [JsonPropertyName("description")]
        public string? Description { get; init; }

        [JsonPropertyName("authorizationCode")]
        public string? AuthorizationCode { get; init; }

        [JsonPropertyName("feeBearer")]
        public FeeBearer FeeBearer { get; init; }

        [JsonPropertyName("amount")]
        public MonetaryTypeDto? Amount { get; init; }

        [JsonPropertyName("taxAmount")]
        public MonetaryTypeDto? TaxAmount { get; init; }

        [JsonPropertyName("totalAmount")]
        public required MonetaryTypeDto TotalAmount { get; init; }

        [JsonPropertyName("payer")]
        public PayerDto? Payer { get; init; }

        [JsonPropertyName("payee")]
        public List<PayeeDto>? Payees { get; init; }

        [JsonPropertyName("paymentMethod")]
        public required PaymentMethodDto PaymentMethod { get; init; }

        [JsonPropertyName("status")]
        public string? Status { get; init; }

        [JsonPropertyName("statusDate")]
        public string? StatusDate { get; init; }

        [JsonPropertyName("additionalInformation")]
        public List<AdditionalInformationDto>? AdditionalInformation { get; init; }

        [JsonPropertyName("segment")]
        public Segment Segment { get; init; }
    }
}