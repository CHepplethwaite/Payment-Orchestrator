using universal_payment_platform.DTOs.ProviderSpecific.MTN.Messages.SMS.DeliveryStatus.Responses;

public record SmsDeliveryStatusResponseDto(
    string StatusCode,                // MADAPI canonical code, e.g., 0000
    string StatusMessage,             // Descriptive message
    string TransactionId,             // MADAPI generated ID for tracing
    SmsDeliveryStatusDataDto Data     // Nested data object
);
