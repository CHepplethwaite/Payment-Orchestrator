using Microsoft.AspNetCore.Identity;
using universal_payment_platform.Data.Entities;

namespace universal_payment_platform.Infrastructure.Security
{
    public static class SeedData
    {
        public static async Task InitializeRoles(RoleManager<IdentityRole> roleManager)
        {
            string[] roles =
            {
                "SuperAdmin", "Admin", "Finance", "PaymentProcessor",
                "Agent", "Merchant", "Compliance", "Auditor", "CustomerSupport",
                "EndUser"
            };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        public static async Task InitializeSuperAdmin(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            var email = Environment.GetEnvironmentVariable("SUPERADMIN_EMAIL");
            var password = Environment.GetEnvironmentVariable("SUPERADMIN_PASSWORD");

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                return; // Env vars not set, skip creation

            var existingUser = await userManager.FindByEmailAsync(email);
            if (existingUser != null)
                return; // Already exists

            var superAdmin = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true // optional
            };

            var result = await userManager.CreateAsync(superAdmin, password);
            if (!result.Succeeded)
                throw new Exception($"Failed to create SuperAdmin: {string.Join(", ", result.Errors.Select(e => e.Description))}");

            // Ensure role exists
            if (!await roleManager.RoleExistsAsync("SuperAdmin"))
                await roleManager.CreateAsync(new IdentityRole("SuperAdmin"));

            await userManager.AddToRoleAsync(superAdmin, "SuperAdmin");
        }
    }
}
