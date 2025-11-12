using universal_payment_platform.DTOs.ProviderSpecific.MTN.PayLink.Responses;

public record PaymentLinkDataDto(
    string ProviderTransactionId,
    string OrderRedirectUrl,
    LinkDto Links
);
