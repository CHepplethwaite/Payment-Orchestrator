public enum PaymentStatus
{
    Pending,         // Created but not executed
    Authorized,      // Validated or reserved
    Processing,      // Hitting provider API
    Completed,       // Provider success
    Failed,          // Provider or risk failure
    Cancelled        // User or timeout
}
