using System.Text.Json.Serialization;

public record SmsDeliveryStatusItemDto
{
    [JsonPropertyName("address")]
    public string Address { get; init; }

    [JsonPropertyName("status")]
    public string Status { get; init; }

    [JsonPropertyName("deliveredTime")]
    public DateTime? DeliveredTime { get; init; }
}
