using System.Text.Json.Serialization;

public record UssdInboundRequestDto
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

    [JsonPropertyName("cellId")]
    public string? CellId { get; init; }

    [JsonPropertyName("language")]
    public string? Language { get; init; }

    [JsonPropertyName("imsi")]
    public string? Imsi { get; init; }
}
