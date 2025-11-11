using universal_payment_platform.DTOs.ProviderSpecific.MTN.Messages.SMS.DeliveryStatus.Responses;

public class SmsDeliveryStatusResponseDto
{
    public string StatusCode { get; set; }        // MADAPI canonical code, e.g., 0000
    public string StatusMessage { get; set; }     // Descriptive message
    public string TransactionId { get; set; }     // MADAPI generated ID for tracing
    public SmsDeliveryStatusDataDto Data { get; set; } // Nested data object
}