using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace universal_payment_platform.Data.Entities
{
    public class SettlementAccount
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid MerchantId { get; set; }

        [ForeignKey("MerchantId")]
        public Merchant Merchant { get; set; } = null!;

        [Required]
        [MaxLength(20)]
        public string AccountNumber { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string BankName { get; set; } = null!;

        [Required]
        [MaxLength(10)]
        public string Currency { get; set; } = "ZMW";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
