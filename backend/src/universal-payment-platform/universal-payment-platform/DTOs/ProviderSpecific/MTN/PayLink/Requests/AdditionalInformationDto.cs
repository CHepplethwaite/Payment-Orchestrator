using System.Text.Json.Serialization;

public record AdditionalInformationDto
{
    [JsonPropertyName("key")]
    public string? Key { get; init; }

    [JsonPropertyName("value")]
    public string? Value { get; init; }
}
