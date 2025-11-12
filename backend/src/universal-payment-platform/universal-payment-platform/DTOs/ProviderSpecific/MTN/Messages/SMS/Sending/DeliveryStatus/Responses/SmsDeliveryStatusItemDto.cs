public record SmsDeliveryStatusItemDto(
    string Address,              // MSISDN of the recipient
    string Status,               // Delivery status, e.g., DELIVERED, PENDING
    DateTime? DeliveredTime      // Optional delivered timestamp
);
