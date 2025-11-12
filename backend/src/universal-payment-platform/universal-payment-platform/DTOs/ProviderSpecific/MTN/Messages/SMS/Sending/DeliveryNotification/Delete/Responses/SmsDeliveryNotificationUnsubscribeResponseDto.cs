namespace UniversalPaymentPlatform.DTOs.ProviderSpecific.MTN.SMS.Delivery
{
    public record SmsDeliveryNotificationUnsubscribeResponseDto(
        string StatusCode,          // Canonical error code, e.g. 0000
        string StatusMessage,       // Descriptive message (e.g. Successful)
        string TransactionId,       // MADAPI generated transaction ID
        SmsDeliveryNotificationUnsubscribeDataDto Data
    );
}
