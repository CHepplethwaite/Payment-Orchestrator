using universal_payment_platform.DTOs.ProviderSpecific.MTN.PayLink.Responses;

public record PaymentLinkResponseDto(
    string StatusCode,
    string StatusMessage,
    string TransactionId,
    string SequenceNo,
    PaymentLinkDataDto Data
);
