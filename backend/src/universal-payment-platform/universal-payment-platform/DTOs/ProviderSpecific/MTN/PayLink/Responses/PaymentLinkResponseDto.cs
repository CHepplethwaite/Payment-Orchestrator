using System.Text.Json.Serialization;

public record PaymentLinkResponseDto
{
    [JsonPropertyName("statusCode")]
    public string StatusCode { get; init; }

    [JsonPropertyName("statusMessage")]
    public string StatusMessage { get; init; }

    [JsonPropertyName("transactionId")]
    public string TransactionId { get; init; }

    [JsonPropertyName("sequenceNo")]
    public string SequenceNo { get; init; }

    [JsonPropertyName("data")]
    public PaymentLinkDataDto Data { get; init; }
}
