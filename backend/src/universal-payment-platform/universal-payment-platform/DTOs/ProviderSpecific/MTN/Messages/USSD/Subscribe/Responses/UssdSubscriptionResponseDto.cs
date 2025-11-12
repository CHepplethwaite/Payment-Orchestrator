using universal_payment_platform.DTOs.ProviderSpecific.MTN.Messages.USSD.Responses;

public record UssdSubscriptionResponseDto(
    string StatusCode,           // e.g., 0000
    string StatusMessage,        // e.g., Created
    string TransactionId,        // e.g., xyz-0hij0hjh0-9y6
    UssdSubscriptionDataDto Data,
    LinkDto _Links
);
