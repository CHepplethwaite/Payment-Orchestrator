using Microsoft.AspNetCore.Identity;
using universal_payment_platform.Data.Entities;
using universal_payment_platform.Infrastructure.Email.Services;

namespace universal_payment_platform.Infrastructure.Security
{
    public static class SeedData
    {
        // ===== Roles =====
        public static async Task InitializeRoles(RoleManager<IdentityRole> roleManager)
        {
            string[] roles = [ "SuperAdmin", "Admin", "Finance", "PaymentProcessor", "Agent", "Merchant", "Compliance", "Auditor", "CustomerSupport", "EndUser" ];

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        // ===== Super Admin =====
        public static async Task InitializeSuperAdmin(
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IEmailService? emailService = null,
            ILogger? logger = null)
        {
            var email = Environment.GetEnvironmentVariable("SUPERADMIN_EMAIL");
            var password = Environment.GetEnvironmentVariable("SUPERADMIN_PASSWORD");

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                logger?.LogWarning("SuperAdmin credentials not found in environment variables");
                return;
            }

            var existingUser = await userManager.FindByEmailAsync(email);
            if (existingUser != null)
            {
                logger?.LogInformation("SuperAdmin user already exists: {Email}", email);
                return;
            }

            var superAdmin = new AppUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FullName = "Super Admin", // Using existing FullName property
                PhoneNumber = "+1234567890"
                // Note: CompanyName, CreatedAt, IsActive are not set since they don't exist in AppUser
            };

            var result = await userManager.CreateAsync(superAdmin, password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                logger?.LogError("Failed to create SuperAdmin: {Errors}", errors);
                throw new Exception($"Failed to create SuperAdmin: {errors}");
            }

            if (!await roleManager.RoleExistsAsync("SuperAdmin"))
                await roleManager.CreateAsync(new IdentityRole("SuperAdmin"));

            await userManager.AddToRoleAsync(superAdmin, "SuperAdmin");

            logger?.LogInformation("SuperAdmin user created successfully: {Email}", email);

            // Send welcome email if email service is available
            if (emailService != null)
            {
                await SendSuperAdminWelcomeEmail(emailService, superAdmin, logger);
            }
        }

        private static async Task SendSuperAdminWelcomeEmail(
            IEmailService emailService,
            AppUser superAdmin,
            ILogger? logger = null)
        {
            try
            {
                // Create a simple anonymous object for the email model
                var model = new
                {
                    Name = superAdmin.FullName ?? "Super Admin",
                    superAdmin.Email,
                    LoginUrl = "https://payments.tumpetech.com/admin/login",
                    SupportEmail = "support@tumpetech.com",
                    DashboardUrl = "https://payments.tumpetech.com/admin/dashboard"
                };

                await emailService.SendTemplatedEmailAsync(
                    superAdmin.Email!,
                    "Welcome to Universal Payment Platform - Super Admin Account Created",
                    "welcome",
                    model);

                logger?.LogInformation("Welcome email sent to SuperAdmin: {Email}", superAdmin.Email);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Failed to send welcome email to SuperAdmin: {Email}", superAdmin.Email);
                // Don't throw - email failure shouldn't break the seeding process
            }
        }
    }
}