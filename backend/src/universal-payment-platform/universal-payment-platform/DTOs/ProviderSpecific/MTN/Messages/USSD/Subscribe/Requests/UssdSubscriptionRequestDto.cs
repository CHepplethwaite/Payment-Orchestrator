public class UssdSubscriptionRequestDto
{
    public string ServiceCode { get; set; }  // e.g., *1234*356#
    public string CallbackUrl { get; set; }  // URL to receive MO messages
    public string TargetSystem { get; set; } // e.g., AYO
}
