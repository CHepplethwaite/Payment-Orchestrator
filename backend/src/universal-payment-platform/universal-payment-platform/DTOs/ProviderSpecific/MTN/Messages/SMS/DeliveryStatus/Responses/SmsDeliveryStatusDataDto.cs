using universal_payment_platform.DTOs.ProviderSpecific.MTN.Messages.SMS.DeliveryStatus.Responses;

public class SmsDeliveryStatusDataDto
{
    public string RequestId { get; set; }                 // Request ID returned during sending
    public string ClientCorrelator { get; set; }         // Client-provided correlator
    public List<SmsDeliveryStatusItemDto> DeliveryStatus { get; set; } // Array of status objects
    public LinkDto Links { get; set; }                   // _link object for self-reference
}