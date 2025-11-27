using System.Text.Json.Serialization;

public record SmsDeliveryStatusResponseDto
{
    [JsonPropertyName("statusCode")]
    public string? StatusCode { get; init; }

    [JsonPropertyName("statusMessage")]
    public string? StatusMessage { get; init; }

    [JsonPropertyName("transactionId")]
    public string? TransactionId { get; init; }

    [JsonPropertyName("data")]
    public SmsDeliveryStatusDataDto? Data { get; init; }
}
