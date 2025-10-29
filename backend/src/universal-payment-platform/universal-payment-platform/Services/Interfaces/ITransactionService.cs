using System.Threading.Tasks;
using universal_payment_platform.Services.Interfaces.Models;

namespace universal_payment_platform.Services.Interfaces
{
    public interface ITransactionService
    {
        Task SaveTransactionAsync(PaymentRequest request, PaymentResponse response);
        Task<PaymentResponse?> GetTransactionByIdAsync(string transactionId);
        Task UpdateTransactionStatusAsync(string transactionId, PaymentStatus newStatus, string? providerReference = null);
    }
}
