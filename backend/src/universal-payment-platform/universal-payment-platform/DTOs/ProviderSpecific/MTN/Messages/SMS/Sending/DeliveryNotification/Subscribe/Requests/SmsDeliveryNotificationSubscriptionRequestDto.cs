using System.Text.Json.Serialization;

namespace UniversalPaymentPlatform.DTOs.ProviderSpecific.MTN.SMS.Delivery
{
    public record SmsDeliveryNotificationSubscriptionRequestDto
    {
        [JsonPropertyName("notifyUrl")]
        public string? NotifyUrl { get; init; }

        [JsonPropertyName("targetSystem")]
        public string? TargetSystem { get; init; }
    }
}
