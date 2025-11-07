using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using universal_payment_platform.Common;

namespace universal_payment_platform.Data.Entities
{
    public class Payment
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid(); // Unique payment ID

        [Required]
        [MaxLength(100)]
        public string ExternalTransactionId { get; set; } = null!; // External transaction reference

        [MaxLength(100)]
        public string? ProviderTransactionId { get; set; } // Optional provider transaction ID

        [Required]
        [MaxLength(50)]
        public string Provider { get; set; } = null!; // e.g., "AirtelMoney", "MTNMoney"

        [Required]
        public decimal Amount { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = PaymentStatus.Pending.ToString(); // Initial status

        [Required]
        [MaxLength(10)]
        public string Currency { get; set; } = "ZMW"; // Currency code

        public string? Message { get; set; } // Optional payment message or error

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Foreign key for the user who made the payment
        [Required]
        public string UserId { get; set; } = null!;

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; } = null!;
    }
}
