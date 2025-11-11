using universal_payment_platform.DTOs.ProviderSpecific.MTN.Fee.Responses;

public class FeeCheckDataDto
{
    public string StatusCode { get; set; } // e.g., SUCCESSFUL
    public string ProviderTransactionId { get; set; }
    public string StatusMessage { get; set; }
    public List<FeeDetailDto> FeeDetails { get; set; }
}
