using System.Text.Json.Serialization;

public record FeeDetailDto
{
    [JsonPropertyName("feeType")]
    public string FeeType { get; init; }

    [JsonPropertyName("amount")]
    public MonetaryDto Amount { get; init; }

    [JsonPropertyName("description")]
    public string Description { get; init; }

    [JsonPropertyName("recipient")]
    public string Recipient { get; init; }
}
