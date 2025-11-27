using System.Text.Json.Serialization;

namespace Application.DTOs.Payments.Responses
{
    public record PaymentHistoryResponseDto
    {
        [JsonPropertyName("statusCode")]
        public string? StatusCode { get; init; }

        [JsonPropertyName("statusMessage")]
        public string? StatusMessage { get; init; }

        [JsonPropertyName("transactionId")]
        public string? TransactionId { get; init; }

        [JsonPropertyName("customerId")]
        public string? CustomerId { get; init; }

        [JsonPropertyName("sequenceNo")]
        public string? SequenceNo { get; init; }

        [JsonPropertyName("data")]
        public PaymentHistoryDataDto? Data { get; init; }
    }
}
