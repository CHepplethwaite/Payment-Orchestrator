public class UssdInboundResponseDto
{
    public string StatusCode { get; set; }       // e.g., 0000
    public string StatusMessage { get; set; }    // Description of the transaction
    public string TransactionId { get; set; }    // API transaction id
    public UssdInboundDataDto Data { get; set; }
    public LinkDto _Links { get; set; }          // Reuse generic link DTO
}