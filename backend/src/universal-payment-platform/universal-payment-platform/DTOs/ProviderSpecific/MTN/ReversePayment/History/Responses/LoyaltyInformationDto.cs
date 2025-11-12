using System.Text.Json.Serialization;

public record LoyaltyInformationDto
{
    [JsonPropertyName("generatedAmount")]
    public MonetaryDto GeneratedAmount { get; init; }

    [JsonPropertyName("consumedAmount")]
    public MonetaryDto ConsumedAmount { get; init; }

    [JsonPropertyName("newBalance")]
    public MonetaryDto NewBalance { get; init; }
}