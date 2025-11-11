using System.Text.Json.Serialization;
using universal_payment_platform.DTOs.ProviderSpecific.MTN.Payments.Responses;

namespace UniversalPaymentPlatform.DTOs.ProviderSpecific.MTN.Payments.Responses
{
    public record MTNPaymentResponseDto
    {
        [JsonPropertyName("statusCode")]
        public required string StatusCode { get; init; }

        [JsonPropertyName("statusMessage")]
        public string? StatusMessage { get; init; }

        [JsonPropertyName("supportMessage")]
        public string? SupportMessage { get; init; }

        [JsonPropertyName("fulfillmentStatus")]
        public string? FulfillmentStatus { get; init; }

        [JsonPropertyName("providerTransactionId")]
        public string? ProviderTransactionId { get; init; }

        [JsonPropertyName("sequenceNo")]
        public string? SequenceNo { get; init; }

        [JsonPropertyName("data")]
        public MTNPaymentDataDto? Data { get; init; }
    }
}
