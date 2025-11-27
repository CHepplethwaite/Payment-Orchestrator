using System.Text.Json.Serialization;

namespace UniversalPaymentPlatform.DTOs.ProviderSpecific.MTN.SMS.Delivery
{
    public record SmsDeliveryNotificationUnsubscribeResponseDto
    {
        [JsonPropertyName("statusCode")]
        public string? StatusCode { get; init; }

        [JsonPropertyName("statusMessage")]
        public string? StatusMessage { get; init; }

        [JsonPropertyName("transactionId")]
        public string? TransactionId { get; init; }

        [JsonPropertyName("data")]
        public SmsDeliveryNotificationUnsubscribeDataDto? Data { get; init; }
    }
}
