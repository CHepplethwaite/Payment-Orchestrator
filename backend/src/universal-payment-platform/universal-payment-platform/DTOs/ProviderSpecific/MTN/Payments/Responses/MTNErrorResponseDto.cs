using System.Text.Json.Serialization;

namespace UniversalPaymentPlatform.DTOs.ProviderSpecific.MTN.Payments.Responses
{
    public record MTNErrorResponseDto
    {
        [JsonPropertyName("statusCode")]
        public string? StatusCode { get; init; }

        [JsonPropertyName("statusMessage")]
        public string? StatusMessage { get; init; }

        [JsonPropertyName("supportMessage")]
        public string? SupportMessage { get; init; }

        [JsonPropertyName("sequenceNo")]
        public string? SequenceNo { get; init; }

        [JsonPropertyName("correlatorId")]
        public string? CorrelatorId { get; init; }
    }
}
