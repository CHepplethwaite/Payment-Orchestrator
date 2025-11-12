namespace UniversalPaymentPlatform.DTOs.ProviderSpecific.MTN.SMS.Delivery
{
    public record SmsDeliveryNotificationSubscriptionRequestDto(
        string NotifyUrl,      // URL where delivery notifications will be sent
        string TargetSystem    // Name of the system handling notifications
    );
}
