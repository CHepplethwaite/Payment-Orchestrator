using System.Text.Json.Serialization;

public record UssdSubscriptionRequestDto
{
    [JsonPropertyName("serviceCode")]
    public string ServiceCode { get; init; }

    [JsonPropertyName("callbackUrl")]
    public string CallbackUrl { get; init; }

    [JsonPropertyName("targetSystem")]
    public string TargetSystem { get; init; }
}
