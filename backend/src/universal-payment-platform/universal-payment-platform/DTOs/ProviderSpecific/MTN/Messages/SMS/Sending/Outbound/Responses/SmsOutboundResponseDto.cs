using System.Text.Json.Serialization;

public record SmsOutboundResponseDto
{
    [JsonPropertyName("statusCode")]
    public string StatusCode { get; init; }

    [JsonPropertyName("statusMessage")]
    public string StatusMessage { get; init; }

    [JsonPropertyName("transactionId")]
    public string TransactionId { get; init; }

    [JsonPropertyName("data")]
    public SmsOutboundDataDto Data { get; init; }
}
