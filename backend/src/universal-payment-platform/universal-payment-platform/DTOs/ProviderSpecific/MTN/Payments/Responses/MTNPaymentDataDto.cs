using System.Text.Json.Serialization;
using universal_payment_platform.DTOs.ProviderSpecific.MTN.Shared;
using UniversalPaymentPlatform.DTOs.ProviderSpecific.MTN.Payments.Requests;

namespace UniversalPaymentPlatform.DTOs.ProviderSpecific.MTN.Payments.Responses
{
    public record MTNPaymentDataDto
    {
        [JsonPropertyName("approvalId")]
        public string? ApprovalId { get; init; }

        [JsonPropertyName("transactionFee")]
        public MonetaryTypeDto? TransactionFee { get; init; }

        [JsonPropertyName("discount")]
        public MonetaryTypeDto? Discount { get; init; }

        [JsonPropertyName("newBalance")]
        public MonetaryTypeDto? NewBalance { get; init; }

        [JsonPropertyName("payerNote")]
        public string? PayerNote { get; init; }

        [JsonPropertyName("status")]
        public string? Status { get; init; }

        [JsonPropertyName("correlatorId")]
        public string? CorrelatorId { get; init; }

        [JsonPropertyName("statusDate")]
        public string? StatusDate { get; init; }

        [JsonPropertyName("additionalInformation")]
        public MTNAdditionalInfoDto? AdditionalInformation { get; init; }

        [JsonPropertyName("metaData")]
        public List<MTNAdditionalInfoDto>? MetaData { get; init; }

        [JsonPropertyName("loyaltyInformation")]
        public MTNLoyaltyInformationDto? LoyaltyInformation { get; init; }

        [JsonPropertyName("externalCode")]
        public string? ExternalCode { get; init; }

        [JsonPropertyName("_links")]
        public MTNPaymentLinksDto? Links { get; init; }
    }
}
