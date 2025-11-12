using System.Text.Json.Serialization;

public record UssdInboundDataDto
{
    [JsonPropertyName("inboundResponse")]
    public string InboundResponse { get; init; }

    [JsonPropertyName("userInputRequired")]
    public bool UserInputRequired { get; init; }

    [JsonPropertyName("messageType")]
    public int MessageType { get; init; }

    [JsonPropertyName("serviceCode")]
    public string ServiceCode { get; init; }

    [JsonPropertyName("msisdn")]
    public string Msisdn { get; init; }
}
