using System.Text.Json.Serialization;

public record LinksDto
{
    [JsonPropertyName("self")]
    public SelfLinkDto Self { get; init; }
}

public record SelfLinkDto
{
    [JsonPropertyName("href")]
    public string Href { get; init; }
}