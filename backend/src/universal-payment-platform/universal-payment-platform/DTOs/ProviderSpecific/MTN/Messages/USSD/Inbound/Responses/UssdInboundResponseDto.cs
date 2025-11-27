using System.Text.Json.Serialization;

public record UssdInboundResponseDto
{
    [JsonPropertyName("statusCode")]
    public string? StatusCode { get; init; }

    [JsonPropertyName("statusMessage")]
    public string? StatusMessage { get; init; }

    [JsonPropertyName("transactionId")]
    public string? TransactionId { get; init; }

    [JsonPropertyName("data")]
    public UssdInboundDataDto? Data { get; init; }

    [JsonPropertyName("links")]
    public LinkDto? Links { get; init; }
}
