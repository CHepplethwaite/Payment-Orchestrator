using Microsoft.AspNetCore.Identity;

namespace universal_payment_platform.Data.Entities
{
    public class AppUser : IdentityUser
    {
        public string? FullName { get; set; }

        // Add these missing properties
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
        public bool PasswordChangeRequired { get; set; } = false;

        // Navigation property
        public ICollection<Payment> Payments { get; set; } = [];
    }
}