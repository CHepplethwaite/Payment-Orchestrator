using Microsoft.EntityFrameworkCore;
using universal_payment_platform.Data.Entities;

namespace universal_payment_platform.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Payment> Payments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Payment entity
            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasKey(p => p.Id);

                // Remove User configuration since your Payment entity doesn't have User property
                // Configure owned types if needed for PayerInfo, PayeeInfo, etc.

                // Configure indexes
                entity.HasIndex(p => p.ExternalTransactionId);
                entity.HasIndex(p => p.CreatedAt);
                entity.HasIndex(p => p.Status);
            });
        }
    }
}