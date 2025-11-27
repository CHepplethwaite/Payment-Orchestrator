using universal_payment_platform.Data;
using Microsoft.EntityFrameworkCore;
using universal_payment_platform.Data.Entities;
using universal_payment_platform.Repositories.Interfaces;

public class PaymentRepository : IPaymentRepository
{
    private readonly ApplicationDbContext _context;

    public PaymentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Payment> AddAsync(Payment payment)
    {
        payment.CreatedAt = DateTime.UtcNow;
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
        // Simple implementation - search in JSON
        var allPayments = await _context.Payments.ToListAsync();
        return allPayments.FirstOrDefault(p =>
            !string.IsNullOrEmpty(p.ProviderMetadata) &&
            p.ProviderMetadata.Contains($"\"ProviderTransactionId\":\"{providerTransactionId}\""));
    }

    public async Task<Payment?> GetByIdAsync(Guid id)
    {
        return await _context.Payments
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task UpdateAsync(Payment payment)
    {
        // Remove UpdatedAt since your entity doesn't have this property
        _context.Payments.Update(payment);
        await _context.SaveChangesAsync();
    }
}