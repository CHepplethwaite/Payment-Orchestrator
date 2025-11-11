using universal_payment_platform.DTOs.ProviderSpecific.MTN.PaymentAgreements.Eligibility.Responses;

public class PaymentAgreementEligibilityResponseDto
{
    public string StatusCode { get; set; }
    public string StatusMessage { get; set; }
    public string TransactionId { get; set; }
    public PaymentAgreementEligibilityDataDto Data { get; set; }
}
