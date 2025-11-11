public class SmsDeliveryStatusItemDto
{
    public string Address { get; set; }                  // MSISDN of the recipient
    public string Status { get; set; }                   // Delivery status, e.g., DELIVERED, PENDING
    public DateTime? DeliveredTime { get; set; }        // Optional delivered timestamp
}