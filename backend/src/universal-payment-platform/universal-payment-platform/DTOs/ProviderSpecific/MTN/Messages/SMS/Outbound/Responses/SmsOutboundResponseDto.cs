using universal_payment_platform.DTOs.ProviderSpecific.MTN.Messages.SMS.Outbound.Responses;

public class SmsOutboundResponseDto
{
    public string StatusCode { get; set; }           // MADAPI canonical code, e.g., 0000
    public string StatusMessage { get; set; }        // Descriptive message
    public string TransactionId { get; set; }        // Echoed clientCorrelator or MADAPI-generated ID
    public SmsOutboundDataDto Data { get; set; }     // Wrapper for detailed data
}