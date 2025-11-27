using System.Text.Json.Serialization;

public record UssdOutboundRequestDto
{
    [JsonPropertyName("sessionId")]
    public string? SessionId { get; init; }

    [JsonPropertyName("messageType")]
    public string? MessageType { get; init; }

    [JsonPropertyName("msisdn")]
    public string? Msisdn { get; init; }

    [JsonPropertyName("serviceCode")]
    public string? ServiceCode { get; init; }

    [JsonPropertyName("ussdString")]
    public string? UssdString { get; init; }
}
