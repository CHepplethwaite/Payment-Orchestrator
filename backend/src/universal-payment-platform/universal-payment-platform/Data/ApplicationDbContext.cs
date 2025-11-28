using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using universal_payment_platform.Data.Entities;

namespace universal_payment_platform.Data
{
    public class ApplicationDbContext
        : IdentityDbContext<AppUser, IdentityRole, string>
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options
        ) : base(options)
        {
        }

        public DbSet<Payment> Payments { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            //----------------------------------------------------------------------
            // IDENTITY TABLE NAMING
            //----------------------------------------------------------------------
            builder.Entity<AppUser>().ToTable("Users");
            builder.Entity<IdentityRole>().ToTable("Roles");
            builder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
            builder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
            builder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
            builder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");
            builder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");


            //----------------------------------------------------------------------
            // PAYMENT CONFIG
            //----------------------------------------------------------------------
            builder.Entity<Payment>(entity =>
            {
                entity.ToTable("Payments");
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Amount)
                      .HasPrecision(18, 2)
                      .IsRequired();

                entity.Property(x => x.Status)
                      .HasMaxLength(50);

                entity.Property(x => x.ExternalTransactionId)
                      .HasMaxLength(128);

                // Foreign key User → Payments
                entity.HasOne(x => x.User)
                      .WithMany(u => u.Payments)
                      .HasForeignKey(x => x.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Indexes for performance
                entity.HasIndex(x => x.ExternalTransactionId);
                entity.HasIndex(x => x.CreatedAt);
                entity.HasIndex(x => x.Status);
            });
        }
    }
}
