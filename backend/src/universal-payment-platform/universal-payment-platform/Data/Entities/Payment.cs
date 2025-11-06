using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using universal_payment_platform.Services.Interfaces.Models;

namespace universal_payment_platform.Data.Entities
{
    public class Payment
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        // The ID from the client/request, for idempotency
        [Required]
        [MaxLength(100)]
        public string ExternalTransactionId { get; set; } = null!;

        // The ID from the provider (e.g., Airtel, MTN)
        [MaxLength(100)]
        public string? ProviderTransactionId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Provider { get; set; } = null!;

        [Required]
        public decimal Amount { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = PaymentStatus.Pending.ToString(); // Use enum/string

        public string? Message { get; set; } // Store failure messages

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Relationship to ApplicationUser
        [Required]
        public string UserId { get; set; } = null!;

        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; } = null!;
    }
}