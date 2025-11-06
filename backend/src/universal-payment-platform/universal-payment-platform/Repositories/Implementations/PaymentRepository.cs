using Microsoft.EntityFrameworkCore;
using universal_payment_platform.Data; // For ApplicationDbContext
using universal_payment_platform.Data.Entities;
using universal_payment_platform.Repositories.Interfaces;

namespace universal_payment_platform.Repositories.Implementations
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly ApplicationDbContext _context;

        public PaymentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Payment> AddAsync(Payment payment)
        {
            await _context.Payments.AddAsync(payment);
            await _context.SaveChangesAsync();
            return payment;
        }

        public async Task<Payment?> GetByExternalIdAsync(string externalTransactionId)
        {
            return await _context.Payments
                .FirstOrDefaultAsync(p => p.ExternalTransactionId == externalTransactionId);
        }

        public async Task<Payment?> GetByProviderIdAsync(string providerTransactionId)
        {
            return await _context.Payments
                .FirstOrDefaultAsync(p => p.ProviderTransactionId == providerTransactionId);
        }

        public async Task UpdateAsync(Payment payment)
        {
            payment.UpdatedAt = DateTime.UtcNow;
            _context.Payments.Update(payment);
            await _context.SaveChangesAsync();
        }
    }
}