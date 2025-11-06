using Microsoft.AspNetCore.Identity;

namespace universal_payment_platform.Data.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
        public virtual ICollection<KycRecord> KycRecords { get; set; } = new List<KycRecord>();
    }
}