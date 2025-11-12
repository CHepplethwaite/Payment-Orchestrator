using universal_payment_platform.DTOs.ProviderSpecific.MTN.Messages.SMS.Outbound.Responses;

public record SmsOutboundResponseDto(
    string StatusCode,           // MADAPI canonical code, e.g., 0000
    string StatusMessage,        // Descriptive message
    string TransactionId,        // Echoed clientCorrelator or MADAPI-generated ID
    SmsOutboundDataDto Data      // Wrapper for detailed data
);
