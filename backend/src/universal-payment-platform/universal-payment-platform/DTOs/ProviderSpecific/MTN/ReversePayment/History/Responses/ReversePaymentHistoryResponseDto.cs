using System.Text.Json.Serialization;
public record ReversePaymentHistoryResponseDto
{
    [JsonPropertyName("statusCode")]
    public string? StatusCode { get; init; }

    [JsonPropertyName("statusMessage")]
    public string? StatusMessage { get; init; }

    [JsonPropertyName("transactionId")]
    public string? TransactionId { get; init; }

    [JsonPropertyName("correlatorId")]
    public string? CorrelatorId { get; init; }

    [JsonPropertyName("sequenceNo")]
    public string? SequenceNo { get; init; }

    [JsonPropertyName("data")]
    public ReversePaymentDataDto? Data { get; init; }

    [JsonPropertyName("links")]
    public LinkDto? Links { get; init; }
}