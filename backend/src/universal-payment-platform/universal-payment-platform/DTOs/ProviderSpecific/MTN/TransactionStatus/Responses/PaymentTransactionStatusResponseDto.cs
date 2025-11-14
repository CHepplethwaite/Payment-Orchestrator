using System.Text.Json.Serialization;

namespace TransactionStatus.Responses
{
    public record PaymentTransactionStatusResponseDto
    {
        [JsonPropertyName("statusCode")]
        public string? StatusCode { get; init; }

        [JsonPropertyName("statusMessage")]
        public string? StatusMessage { get; init; }

        [JsonPropertyName("correlatorId")]
        public string? CorrelatorId { get; init; }

        [JsonPropertyName("customerId")]
        public string? CustomerId { get; init; }

        [JsonPropertyName("sequenceNo")]
        public int SequenceNo { get; init; }

        [JsonPropertyName("providerTransactionId")]
        public string? ProviderTransactionId { get; init; }

        [JsonPropertyName("data")]
        public PaymentTransactionStatusDataDto? Data { get; init; }

        [JsonPropertyName("additionalInformation")]
        public PaymentTransactionStatusAdditionalInformationDto AdditionalInformation { get; init; }

        [JsonPropertyName("details")]
        public PaymentTransactionStatusDetailsDto Details { get; init; }

        [JsonPropertyName("_links")]
        public PaymentTransactionStatusLinksDto Links { get; init; }
    }
}
