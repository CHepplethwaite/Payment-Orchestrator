using System.Text.Json.Serialization;

public record PromiseToPayEligibilityDetailsDto
{
    [JsonPropertyName("isEligible")]
    public bool IsEligible { get; init; }

    [JsonPropertyName("reason")]
    public string? Reason { get; init; }

    [JsonPropertyName("validUntil")]
    public DateTime? ValidUntil { get; init; }
}
