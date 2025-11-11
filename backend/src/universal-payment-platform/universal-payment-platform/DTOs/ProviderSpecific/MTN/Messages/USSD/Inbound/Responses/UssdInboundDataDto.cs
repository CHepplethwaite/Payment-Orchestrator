public class UssdInboundDataDto
{
    public string InboundResponse { get; set; }  // Response value, e.g., null
    public bool UserInputRequired { get; set; }  // Is user input required
    public int MessageType { get; set; }         // MessageType for response
    public string ServiceCode { get; set; }      // Echoed service code
    public string Msisdn { get; set; }           // Recipient msisdn
}