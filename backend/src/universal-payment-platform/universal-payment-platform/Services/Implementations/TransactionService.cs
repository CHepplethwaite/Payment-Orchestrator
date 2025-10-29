using System.Collections.Concurrent;
using System.Threading.Tasks;
using universal_payment_platform.Services.Interfaces;
using universal_payment_platform.Services.Interfaces.Models;

namespace universal_payment_platform.Services.Implementations
{
    public class TransactionService : ITransactionService
    {
        // In-memory storage (replace with DB later)
        private static readonly ConcurrentDictionary<string, PaymentResponse> _transactions = new();

        public Task SaveTransactionAsync(PaymentRequest request, PaymentResponse response)
        {
            if (!string.IsNullOrEmpty(response.TransactionId))
                _transactions[response.TransactionId] = response;

            return Task.CompletedTask;
        }

        public Task<PaymentResponse?> GetTransactionByIdAsync(string transactionId)
        {
            _transactions.TryGetValue(transactionId, out var response);
            return Task.FromResult(response);
        }

        public Task UpdateTransactionStatusAsync(string transactionId, PaymentStatus newStatus, string? providerReference = null)
        {
            if (_transactions.TryGetValue(transactionId, out var response))
            {
                response.Status = newStatus;
                response.ProviderReference = providerReference ?? response.ProviderReference;
                _transactions[transactionId] = response;
            }

            return Task.CompletedTask;
        }
    }
}
