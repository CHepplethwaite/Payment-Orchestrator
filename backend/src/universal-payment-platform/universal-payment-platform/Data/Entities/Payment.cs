using universal_payment_platform.Common;

namespace universal_payment_platform.Data.Entities
{
    public class Payment
    {
        public Guid Id { get; set; }

        // Core generic info
        public string Provider { get; set; } = null!;                  // MTN, Airtel, Stripe...
        public string ExternalTransactionId { get; set; } = null!;    // Provider transaction id
        public decimal Amount { get; set; }                            // Amount of payment
        public string Currency { get; set; } = "ZMW";                  // ISO currency code
        public PaymentStatus Status { get; set; }

        public string Description { get; set; } = string.Empty;

        // Optional provider-specific payload stored as JSON
        public string? ProviderMetadata { get; set; }

        // Timestamps
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }

        // Payer and Payee info
        public PayerInfo Payer { get; set; } = new();
        public List<PayeeInfo> Payees { get; set; } = new();

        public string? UserId { get; set; }

        // Optional navigation property
        public AppUser? User { get; set; }

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

    public class PayerInfo
    {
        public string Id { get; set; } = null!;         // e.g., MSISDN, Email
        public string Name { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Note { get; set; }
    }

    public class PayeeInfo
    {
        public string Id { get; set; } = null!;        // Merchant or user ID
        public string Name { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "ZMW";
        public string? Note { get; set; }
    }

}
