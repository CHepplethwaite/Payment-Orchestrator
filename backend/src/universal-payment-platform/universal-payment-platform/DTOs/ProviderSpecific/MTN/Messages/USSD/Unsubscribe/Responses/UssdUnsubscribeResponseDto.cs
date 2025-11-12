using universal_payment_platform.DTOs.ProviderSpecific.MTN.Messages.USSD.Responses;

public record UssdUnsubscribeResponseDto(
    string StatusCode,            // e.g., 0000
    string StatusMessage,         // e.g., Created / OK
    string TransactionId,         // e.g., xyz-0hij0hjh0-9y6
    UssdSubscriptionDataDto Data, // same as subscription
    LinkDto _Links                // same as subscription
);
