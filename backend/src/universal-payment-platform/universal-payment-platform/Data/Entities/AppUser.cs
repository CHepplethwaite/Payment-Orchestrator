using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace universal_payment_platform.Data.Entities
{
    public class AppUser : IdentityUser
    {
        public string? FullName { get; set; }

        // Navigation property
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
