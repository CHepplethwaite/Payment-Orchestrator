using System.Text.Json.Serialization;

public record SmsOutboundDataDto
{
    [JsonPropertyName("status")]
    public string? Status { get; init; }
}
