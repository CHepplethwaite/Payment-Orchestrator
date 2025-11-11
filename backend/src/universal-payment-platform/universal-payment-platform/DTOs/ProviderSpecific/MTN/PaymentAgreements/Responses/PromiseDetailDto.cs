public class PromiseDetailDto
{
    public string BillingAccountNo { get; set; }
    public string ServiceName { get; set; }
    public DateTime PromiseOpenDate { get; set; }
    public double PromiseAmount { get; set; }
    public string NumberOfInstallments { get; set; }
    public string DurationUOM { get; set; }
    public string PromiseThreshold { get; set; }
    // Add more fields if API specifies more details in each promise
}
