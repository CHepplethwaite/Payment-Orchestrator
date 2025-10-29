using universal_payment_platform.Services.Interfaces;
using universal_payment_platform.Services.Interfaces.Models;

namespace universal_payment_platform.Services.Adapters
{
    public class StanchartAdapter : IPaymentAdapter
    {
        public string GetAdapterName() => "Stanchart";

        public Task<AuthResponse> AuthenticateAsync()
        {
            return Task.FromResult(new AuthResponse { Token = "mock-stanchart-token" });
        }

        public Task<PaymentResponse> MakePaymentAsync(PaymentRequest request)
        {
            var success = new Random().Next(0, 2) == 1;
            return Task.FromResult(new PaymentResponse
            {
                TransactionId = Guid.NewGuid().ToString(),
                Status = success ? "Completed" : "Failed",
                Message = success ? "Stanchart payment succeeded" : "Stanchart payment failed"
            });
        }

        public Task<PaymentResponse> CheckTransactionStatusAsync(string transactionId)
        {
            return Task.FromResult(new PaymentResponse
            {
                TransactionId = transactionId,
                Status = "Completed",
                Message = "Mock Stanchart transaction completed"
            });
        }
    }
}
