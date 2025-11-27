using universal_payment_platform.Common;

namespace universal_payment_platform.Data.Entities
{
    public class Payment
    {
        public Guid Id { get; set; }

        // Enum backed status
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

        // Audit and timestamps
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }

        // Provider transaction reference
        public string? TransactionId { get; set; }

        // How much hit the provider actually charges/settles
        public decimal? SettlementAmount { get; set; }

        // Who initiated it
        public string? UserId { get; set; }

        public decimal Amount { get; set; }

        public ApplicationUser User { get; set; } = null!;

        // Domain audit trail (optional)
        public List<PaymentAudit> AuditTrail { get; set; } = new();

        public void AddAuditTrail(string message)
        {
            AuditTrail.Add(new PaymentAudit
            {
                PaymentId = this.Id,
                Message = message,
                Timestamp = DateTime.UtcNow
            });
        }
    }
}
