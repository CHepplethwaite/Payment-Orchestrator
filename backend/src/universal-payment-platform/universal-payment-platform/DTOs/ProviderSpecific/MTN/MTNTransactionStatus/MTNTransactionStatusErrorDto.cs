using System.Text.Json.Serialization;

namespace UniversalPaymentPlatform.DTOs.ProviderSpecific.MTN
{
    public record MTNTransactionStatusErrorDto
    {
        [JsonPropertyName("statusCode")]
        public required string StatusCode { get; init; }

        [JsonPropertyName("statusMessage")]
        public required string StatusMessage { get; init; }

        [JsonPropertyName("supportMessage")]
        public string? SupportMessage { get; init; }

        [JsonPropertyName("transactionId")]
        public string? TransactionId { get; init; }

        [JsonPropertyName("timestamp")]
        public string? Timestamp { get; init; }

        [JsonPropertyName("sequenceNo")]
        public string? SequenceNo { get; init; }

        [JsonPropertyName("path")]
        public string? Path { get; init; }

        [JsonPropertyName("method")]
        public string? Method { get; init; }
    }
}
