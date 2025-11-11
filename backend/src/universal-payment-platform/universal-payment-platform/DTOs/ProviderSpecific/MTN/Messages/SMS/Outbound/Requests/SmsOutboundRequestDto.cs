public class SmsOutboundRequestDto
{
    public string SenderAddress { get; set; }          // MSISDN or virtual MSISDN of sender
    public List<string> ReceiverAddress { get; set; }  // Array of recipient MSISDN(s)
    public string Message { get; set; }                // SMS message (max 160 characters)
    public string ClientCorrelator { get; set; }      // Optional unique request identifier
}
