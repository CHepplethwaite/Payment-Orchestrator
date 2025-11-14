using System.Text.Json.Serialization;

namespace universal_payment_platform.DTOs.ProviderSpecific.MTN.Shared
{
    public record ErrorResponseDto
    {
        [JsonPropertyName("statusCode")]
        public required string StatusCode { get; init; }  // MADAPI Canonical Error Code

        [JsonPropertyName("statusMessage")]
        public required string StatusMessage { get; init; }  // Message for the client

        [JsonPropertyName("supportMessage")]
        public string? SupportMessage { get; init; }  // Internal troubleshooting message

        [JsonPropertyName("transactionId")]
        public string? TransactionId { get; init; }  // Transaction ID from the request

        [JsonPropertyName("timestamp")]
        public string? Timestamp { get; init; }  // ISO date-time

        [JsonPropertyName("sequenceNo")]
        public string? SequenceNo { get; init; }  // Unique trace ID

        [JsonPropertyName("path")]
        public string? Path { get; init; }  // API path that caused the error

        [JsonPropertyName("method")]
        public string? Method { get; init; }  // HTTP method type
    }
}
