using Microsoft.AspNetCore.Identity;

namespace universal_payment_platform.Data.Entities
{
    public class ApplicationUser : IdentityUser
    {
        // Navigation properties
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
        public ICollection<KycRecord> KycRecords { get; set; } = new List<KycRecord>();
    }
}
