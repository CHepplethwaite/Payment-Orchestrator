public class UssdOutboundDataDto
{
    public string OutboundResponse { get; set; } // 0 indicates success
    public string SessionId { get; set; }        // Echoed session id
    public string Msisdn { get; set; }           // Mobile recipient msisdn
}
