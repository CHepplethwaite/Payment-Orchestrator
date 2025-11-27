using System.Text.Json.Serialization;
using universal_payment_platform.DTOs.ProviderSpecific.MTN.Fee.Responses;

namespace universal_payment_platform.DTOs.ProviderSpecific.MTN.FeeCheck
{
    public record FeeCheckDataDto
    {
        [JsonPropertyName("status_code")]
        public string? StatusCode { get; init; }

        [JsonPropertyName("provider_transaction_id")]
        public string? ProviderTransactionId { get; init; }

        [JsonPropertyName("status_message")]
        public string? StatusMessage { get; init; }

        [JsonPropertyName("fee_details")]
        public IReadOnlyList<FeeDetailDto>? FeeDetails { get; init; }
    }
}
