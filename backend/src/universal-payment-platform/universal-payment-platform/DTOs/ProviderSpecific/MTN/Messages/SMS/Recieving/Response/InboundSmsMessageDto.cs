using System.Text.Json.Serialization;

public record InboundSmsMessageDto
{
    [JsonPropertyName("senderAddress")]
    public string? SenderAddress { get; init; }

    [JsonPropertyName("destinationAddress")]
    public string? DestinationAddress { get; init; }

    [JsonPropertyName("message")]
    public string? Message { get; init; }

    [JsonPropertyName("messageId")]
    public string? MessageId { get; init; }

    [JsonPropertyName("dateTime")]
    public string? DateTime { get; init; }
}
