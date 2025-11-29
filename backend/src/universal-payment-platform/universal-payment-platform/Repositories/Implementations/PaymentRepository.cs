using universal_payment_platform.Data;
using Microsoft.EntityFrameworkCore;
using universal_payment_platform.Data.Entities;
using universal_payment_platform.Repositories.Interfaces;

public class PaymentRepository(ApplicationDbContext context) : IPaymentRepository
{
    public async Task<Payment> AddAsync(Payment payment)
    {
        payment.CreatedAt = DateTime.UtcNow;
        await context.Payments.AddAsync(payment);
        await context.SaveChangesAsync();
        return payment;
    }

    public async Task<Payment?> GetByExternalIdAsync(string externalTransactionId)
    {
        return await context.Payments
            .FirstOrDefaultAsync(p => p.ExternalTransactionId == externalTransactionId);
    }

    public async Task<Payment?> GetByProviderIdAsync(string providerTransactionId)
    {
        // Simple implementation - search in JSON
        var allPayments = await context.Payments.ToListAsync();
        return allPayments.FirstOrDefault(p =>
            !string.IsNullOrEmpty(p.ProviderMetadata) &&
            p.ProviderMetadata.Contains($"\"ProviderTransactionId\":\"{providerTransactionId}\""));
    }

    public async Task<Payment?> GetByIdAsync(Guid id)
    {
        return await context.Payments
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task UpdateAsync(Payment payment)
    {
        // Remove UpdatedAt since your entity doesn't have this property
        context.Payments.Update(payment);
        await context.SaveChangesAsync();
    }
}