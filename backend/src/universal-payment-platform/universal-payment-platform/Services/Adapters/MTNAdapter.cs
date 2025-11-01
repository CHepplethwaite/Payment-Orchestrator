using universal_payment_platform.Services.Interfaces;
using universal_payment_platform.Services.Interfaces.Models;

namespace universal_payment_platform.Services.Adapters
{
    public class MTNAdapter : IPaymentAdapter
    {
        public string GetAdapterName() => "MTN";

        public Task<AuthResponse> AuthenticateAsync()
        {
            return Task.FromResult(new AuthResponse { Token = "mock-mtn-token" });
        }

        public Task<PaymentResponse> MakePaymentAsync(PaymentRequest request)
        {
            var success = new Random().Next(0, 2) == 1;
            return Task.FromResult(new PaymentResponse
            {
                TransactionId = Guid.NewGuid().ToString(),
                Status = success ? PaymentStatus.Success : PaymentStatus.Failed,
                Message = success ? "MTN payment succeeded" : "MTN payment failed"
            });
        }

        public Task<PaymentResponse> CheckTransactionStatusAsync(string transactionId)
        {
            return Task.FromResult(new PaymentResponse
            {
                TransactionId = transactionId,
                Status = PaymentStatus.Success,
                Message = "Mock MTN transaction completed"
            });
        }
    }
}
