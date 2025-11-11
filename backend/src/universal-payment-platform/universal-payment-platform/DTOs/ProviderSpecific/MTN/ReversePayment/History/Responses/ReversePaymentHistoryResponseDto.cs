using universal_payment_platform.DTOs.ProviderSpecific.MTN.ReversePayment.History.Responses;

public class ReversePaymentHistoryResponseDto
{
    public string StatusCode { get; set; }
    public string StatusMessage { get; set; }
    public string TransactionId { get; set; }
    public string CorrelatorId { get; set; }
    public string SequenceNo { get; set; }
    public ReversePaymentDataDto Data { get; set; }
    public LinksDto Links { get; set; }
}
