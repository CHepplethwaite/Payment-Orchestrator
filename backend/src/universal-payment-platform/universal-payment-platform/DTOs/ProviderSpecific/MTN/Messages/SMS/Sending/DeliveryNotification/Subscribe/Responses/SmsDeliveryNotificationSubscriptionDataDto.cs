using System.Text.Json.Serialization;

namespace UniversalPaymentPlatform.DTOs.ProviderSpecific.MTN.SMS.Delivery
{
    public record SmsDeliveryNotificationSubscriptionDataDto
    {
        [JsonPropertyName("subscriptionId")]
        public string? SubscriptionId { get; init; }

        [JsonPropertyName("links")]
        public LinkDto? Links { get; init; }
    }
}
