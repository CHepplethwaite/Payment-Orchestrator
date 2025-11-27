using System.Text.Json.Serialization;

public record SmsDeliveryStatusDataDto
{
    [JsonPropertyName("requestId")]
    public string? RequestId { get; init; }

    [JsonPropertyName("clientCorrelator")]
    public string? ClientCorrelator { get; init; }

    [JsonPropertyName("deliveryStatus")]
    public List<SmsDeliveryStatusItemDto>? DeliveryStatus { get; init; }

    [JsonPropertyName("links")]
    public LinkDto? Links { get; init; }
}
