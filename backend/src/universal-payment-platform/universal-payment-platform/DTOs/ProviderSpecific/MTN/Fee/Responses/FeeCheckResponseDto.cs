using universal_payment_platform.DTOs.ProviderSpecific.MTN.Fee.Responses;

public class FeeCheckResponseDto
{
    public string StatusCode { get; set; } // e.g., 0000
    public string Error { get; set; }
    public string SequenceNo { get; set; }
    public FeeCheckDataDto Data { get; set; }
}
