public class PaymentAgreementRequestDto
{
    public string BillingAccountNo { get; set; }
    public string ServiceName { get; set; }
    public DateTime PromiseOpenDate { get; set; }
    public double PromiseAmount { get; set; }
    public string NumberOfInstallments { get; set; }
    public string DurationUOM { get; set; }
    public string PromiseThreshold { get; set; }
}
