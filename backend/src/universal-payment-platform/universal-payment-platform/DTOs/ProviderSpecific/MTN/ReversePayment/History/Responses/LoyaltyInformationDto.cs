using universal_payment_platform.DTOs.ProviderSpecific.MTN.Shared;

using System.Text.Json.Serialization;

public record LoyaltyInformationDto
{
    [JsonPropertyName("generatedAmount")]
    public MonetaryTypeDto? GeneratedAmount { get; init; }

    [JsonPropertyName("consumedAmount")]
    public MonetaryTypeDto? ConsumedAmount { get; init; }

    [JsonPropertyName("newBalance")]
    public MonetaryTypeDto? NewBalance { get; init; }
}