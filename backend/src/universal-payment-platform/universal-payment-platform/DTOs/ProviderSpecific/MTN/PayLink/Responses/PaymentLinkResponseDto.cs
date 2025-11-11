using universal_payment_platform.DTOs.ProviderSpecific.MTN.PayLink.Responses;

public class PaymentLinkResponseDto
{
    public string StatusCode { get; set; }
    public string StatusMessage { get; set; }
    public string TransactionId { get; set; }
    public string SequenceNo { get; set; }
    public PaymentLinkDataDto Data { get; set; }
}
