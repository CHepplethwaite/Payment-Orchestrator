using System.Text.Json.Serialization;
using universal_payment_platform.DTOs.ProviderSpecific.MTN.MTNTransactionStatus;

namespace UniversalPaymentPlatform.DTOs.ProviderSpecific.MTN
{
    public record MTNTransactionStatusResponseDto
    {
        [JsonPropertyName("statusCode")]
        public required string StatusCode { get; init; } // MADAPI canonical code

        [JsonPropertyName("statusMessage")]
        public required string StatusMessage { get; init; }

        [JsonPropertyName("supportMessage")]
        public string? SupportMessage { get; init; } // internal troubleshooting info

        [JsonPropertyName("transactionId")]
        public string? TransactionId { get; init; }

        [JsonPropertyName("timestamp")]
        public string? Timestamp { get; init; } // ISO 8601 datetime

        [JsonPropertyName("sequenceNo")]
        public string? SequenceNo { get; init; }

        [JsonPropertyName("path")]
        public string? Path { get; init; }

        [JsonPropertyName("method")]
        public string? Method { get; init; }

        [JsonPropertyName("data")]
        public MTNTransactionStatusDataDto? Data { get; init; } // optional transaction details
    }
}
