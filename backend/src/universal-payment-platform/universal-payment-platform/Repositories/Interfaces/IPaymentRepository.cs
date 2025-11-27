using universal_payment_platform.Data.Entities;

namespace universal_payment_platform.Repositories.Interfaces
{
    public interface IPaymentRepository
    {
        Task<Payment?> GetByExternalIdAsync(string externalTransactionId);
        Task<Payment?> GetByProviderIdAsync(string providerTransactionId);
        Task<Payment> AddAsync(Payment payment);
        Task UpdateAsync(Payment payment);
        Task<Payment?> GetByIdAsync(Guid id);
    }
}