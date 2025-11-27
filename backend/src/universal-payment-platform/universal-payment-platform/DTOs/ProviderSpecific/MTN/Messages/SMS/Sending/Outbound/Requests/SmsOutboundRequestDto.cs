using System.Collections.Generic;
using System.Text.Json.Serialization;

public record SmsOutboundRequestDto
{
    [JsonPropertyName("senderAddress")]
    public string? SenderAddress { get; init; }

    [JsonPropertyName("receiverAddress")]
    public IReadOnlyList<string>? ReceiverAddress { get; init; }

    [JsonPropertyName("message")]
    public string? Message { get; init; }

    [JsonPropertyName("clientCorrelator")]
    public string? ClientCorrelator { get; init; }
}
