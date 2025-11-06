using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using universal_payment_platform.Data.Entities;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

        // ------------------------
        // DbSets for your entities
        // ------------------------
        public DbSet<Payment> Payments { get; set; } = null!;
        public DbSet<Transaction> Transactions { get; set; } = null!;
        public DbSet<KycRecord> KycRecords { get; set; } = null!;
        public DbSet<Merchant> Merchants { get; set; } = null!;
        public DbSet<SettlementAccount> SettlementAccounts { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ------------------------
            // Configure relationships
            // ------------------------

            // ApplicationUser → Payments
            builder.Entity<Payment>()
                .HasOne(p => p.User)
                .WithMany(u => u.Payments)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // ApplicationUser → Transactions
            builder.Entity<Transaction>()
                .HasOne(t => t.User)
                .WithMany(u => u.Transactions)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // ApplicationUser → KycRecords
            builder.Entity<KycRecord>()
                .HasOne(k => k.User)
                .WithMany(u => u.KycRecords)
                .HasForeignKey(k => k.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Merchant → SettlementAccounts
            builder.Entity<SettlementAccount>()
                .HasOne(sa => sa.Merchant)
                .WithMany(m => m.SettlementAccounts)
                .HasForeignKey(sa => sa.MerchantId)
                .OnDelete(DeleteBehavior.Cascade);

            // Optional: Configure decimal precision for money fields
            builder.Entity<Payment>()
                .Property(p => p.Amount)
                .HasPrecision(18, 2);

            builder.Entity<Transaction>()
                .Property(t => t.Amount)
                .HasPrecision(18, 2);
        }
    
}
