public class UssdInboundRequestDto
{
    public string SessionId { get; set; }      // Unique identifier of the session
    public string MessageType { get; set; }    // 0-Begin|1-Continue|2-End|3-Notification|4-Cancel|5-Timeout
    public string Msisdn { get; set; }         // Mobile number of the recipient
    public string ServiceCode { get; set; }    // USSD service code
    public string UssdString { get; set; }     // USSD message content
    public string CellId { get; set; }         // Subscriber's GSM Cell ID
    public string Language { get; set; }       // Subscriber's language preference
    public string Imsi { get; set; }           // Subscriber's IMSI
}
