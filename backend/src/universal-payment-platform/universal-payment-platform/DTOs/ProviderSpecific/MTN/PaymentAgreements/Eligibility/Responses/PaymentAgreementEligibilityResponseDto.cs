using universal_payment_platform.DTOs.ProviderSpecific.MTN.PaymentAgreements.Eligibility.Responses;

public record PaymentAgreementEligibilityResponseDto(
    string StatusCode,
    string StatusMessage,
    string TransactionId,
    PaymentAgreementEligibilityDataDto Data
);
