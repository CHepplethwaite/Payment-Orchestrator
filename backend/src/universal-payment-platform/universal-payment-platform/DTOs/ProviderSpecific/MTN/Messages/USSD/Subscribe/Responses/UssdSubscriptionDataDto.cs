using System.Text.Json.Serialization;

public record UssdSubscriptionDataDto
{
    [JsonPropertyName("subscriptionId")]
    public string? SubscriptionId { get; init; }
}
