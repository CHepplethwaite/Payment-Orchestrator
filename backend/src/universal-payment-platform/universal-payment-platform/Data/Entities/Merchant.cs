using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace universal_payment_platform.Data.Entities
{
    public class Merchant
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = null!;

        [MaxLength(200)]
        public string? Address { get; set; }

        [MaxLength(50)]
        public string? ContactNumber { get; set; }

        public ICollection<SettlementAccount> SettlementAccounts { get; set; } = new List<SettlementAccount>();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
