using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace universal_payment_platform.Data.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        // Navigation properties (optional)
        public ICollection<Payment>? Payments { get; set; }
        public ICollection<Transaction>? Transactions { get; set; }
        public ICollection<KycRecord>? KycRecords { get; set; }
    }
}
