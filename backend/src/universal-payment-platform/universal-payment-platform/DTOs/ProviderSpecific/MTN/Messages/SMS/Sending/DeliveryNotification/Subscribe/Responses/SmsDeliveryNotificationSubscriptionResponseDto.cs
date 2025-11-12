namespace UniversalPaymentPlatform.DTOs.ProviderSpecific.MTN.SMS.Delivery
{
    public record SmsDeliveryNotificationSubscriptionResponseDto(
        string StatusCode,        // MADAPI canonical code, e.g., 0000
        string StatusMessage,     // Descriptive message
        string TransactionId,     // MADAPI generated ID for tracing
        SmsDeliveryNotificationSubscriptionDataDto Data // Nested data object
    );
}
