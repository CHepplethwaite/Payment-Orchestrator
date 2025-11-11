public class PaymentAgreementResponseDto
{
    public string StatusCode { get; set; }
    public string StatusMessage { get; set; }
    public string TransactionId { get; set; }
    public string SequenceNo { get; set; }
    public List<PromiseDetailDto> Data { get; set; }
    public LinkDto Links { get; set; }
}
