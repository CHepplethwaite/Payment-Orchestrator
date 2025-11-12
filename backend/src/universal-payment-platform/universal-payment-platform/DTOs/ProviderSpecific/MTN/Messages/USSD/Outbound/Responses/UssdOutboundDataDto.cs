using System.Text.Json.Serialization;

public record UssdOutboundDataDto
{
    [JsonPropertyName("outboundResponse")]
    public string OutboundResponse { get; init; }

    [JsonPropertyName("sessionId")]
    public string SessionId { get; init; }

    [JsonPropertyName("msisdn")]
    public string Msisdn { get; init; }
}
