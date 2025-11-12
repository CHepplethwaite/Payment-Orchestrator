using System.Text.Json.Serialization;

public record PaymentAgreementResponseDto
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
    public List<PromiseDetailDto> Data { get; init; }

    [JsonPropertyName("links")]
    public LinkDto Links { get; init; }
}