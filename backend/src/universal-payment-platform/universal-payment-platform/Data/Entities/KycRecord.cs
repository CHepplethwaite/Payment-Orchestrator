using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace universal_payment_platform.Data.Entities
{
    public class KycRecord
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string DocumentType { get; set; } = null!;

        [Required]
        public string DocumentNumber { get; set; } = null!;

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public string UserId { get; set; } = null!;

        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; } = null!;
    }
}
