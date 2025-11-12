using System.Text.Json.Serialization;

namespace UniversalPaymentPlatform.DTOs.ProviderSpecific.MTN.SMS.Delivery
{
    public record SmsDeliveryNotificationUnsubscribeDataDto
    {
        [JsonPropertyName("links")]
        public LinkDto Links { get; init; }
    }
}
