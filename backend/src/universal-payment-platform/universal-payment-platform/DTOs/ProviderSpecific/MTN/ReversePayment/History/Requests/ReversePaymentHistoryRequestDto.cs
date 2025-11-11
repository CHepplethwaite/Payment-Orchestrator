public class ReversePaymentHistoryRequestDto
{
    public string TransactionType { get; set; }
    public decimal? Amount { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? Limit { get; set; } = 50;
    public int? PageNo { get; set; }
    public string NodeId { get; set; }
    public string OtherFri { get; set; }
    public string PosMsisdn { get; set; }
    public string QuoteId { get; set; }
    public string TransactionStatus { get; set; }
}
