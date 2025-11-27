using System.Text.Json.Serialization;

public record PaymentLinkDataDto
{
    [JsonPropertyName("providerTransactionId")]
    public string? ProviderTransactionId { get; init; }

    [JsonPropertyName("orderRedirectUrl")]
    public string? OrderRedirectUrl { get; init; }

    [JsonPropertyName("links")]
    public LinkDto? Links { get; init; }
}
