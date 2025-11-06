using universal_payment_platform.Services.Interfaces;
using universal_payment_platform.Services.Interfaces.Models;

namespace universal_payment_platform.Services.Adapters
{
    public class AirtelAdapter : IPaymentAdapter
    {
        // Reuse a single Random instance to avoid repeated false results
        private static readonly Random _random = new Random();

        public string GetAdapterName() => "Airtel";

        public Task<AuthResponse> AuthenticateAsync()
        {
            return Task.FromResult(new AuthResponse { Token = "mock-airtel-token" });
        }

        public Task<PaymentResponse> MakePaymentAsync(PaymentRequest request)
        {
            // 80% success rate instead of 50%
            var success = _random.Next(0, 10) < 8;
            return Task.FromResult(new PaymentResponse
            {
                TransactionId = Guid.NewGuid().ToString(),
                Status = success ? PaymentStatus.Success : PaymentStatus.Failed,
                Message = success ? "Airtel payment succeeded" : "Airtel payment failed"
            });
        }

        public Task<PaymentResponse> CheckTransactionStatusAsync(string transactionId)
        {
            return Task.FromResult(new PaymentResponse
            {
                TransactionId = transactionId,
                Status = PaymentStatus.Success,
                Message = "Mock Airtel transaction completed"
            });
        }
    }
}
