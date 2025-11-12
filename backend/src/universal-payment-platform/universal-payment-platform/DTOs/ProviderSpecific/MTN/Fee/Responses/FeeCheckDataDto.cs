using System.Text.Json.Serialization;

public record FeeCheckDataDto
{
    [JsonPropertyName("status_code")]
    public string StatusCode { get; init; }

    [JsonPropertyName("provider_transaction_id")]
    public string ProviderTransactionId { get; init; }

    [JsonPropertyName("status_message")]
    public string StatusMessage { get; init; }

    [JsonPropertyName("fee_details")]
    public IReadOnlyList<FeeDetailDto> FeeDetails { get; init; }
}
