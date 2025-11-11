public class UssdUnsubscribeResponseDto
{
    public string StatusCode { get; set; }      // e.g., 0000
    public string StatusMessage { get; set; }   // e.g., Created / OK
    public string TransactionId { get; set; }   // e.g., xyz-0hij0hjh0-9y6
    public UssdSubscriptionDataDto Data { get; set; } // same as subscription
    public LinkDto _Links { get; set; }        // same as subscription
}
