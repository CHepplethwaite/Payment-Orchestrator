using System.Text.Json.Serialization;

namespace universal_payment_platform.DTOs.ProviderSpecific.MTN.PaymentAgreements.Eligibility.Responses
{
    public record PromiseToPayEligibilityResponseDto
    {
        [JsonPropertyName("statusCode")]
        public required string StatusCode { get; init; }  // MADAPI Canonical Error Code

        [JsonPropertyName("statusMessage")]
        public required string StatusMessage { get; init; }  // Transaction message

        [JsonPropertyName("transactionId")]
        public required string TransactionId { get; init; }  // Unique transaction ID

        [JsonPropertyName("data")]
        public PromiseToPayEligibilityDataDto? Data { get; init; }  // Eligibility details
    }
}