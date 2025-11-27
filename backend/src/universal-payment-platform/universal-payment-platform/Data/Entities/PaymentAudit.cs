namespace universal_payment_platform.Data.Entities
{
    public class PaymentAudit
    {
        public Guid Id { get; set; }
        public Guid PaymentId { get; set; }
        public string? Message { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
