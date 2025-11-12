namespace UniversalPaymentPlatform.DTOs.ProviderSpecific.MTN.SMS.Delivery
{
    public record SmsDeliveryNotificationSubscriptionDataDto(
        string SubscriptionId, // Unique identifier of the subscription
        LinkDto Links          // _link object containing self reference
    );
}
