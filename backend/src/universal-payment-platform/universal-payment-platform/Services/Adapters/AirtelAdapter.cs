using universal_payment_platform.Services.Interfaces;
using universal_payment_platform.Services.Interfaces.Models;
using System;
using System.Threading.Tasks;

namespace universal_payment_platform.Services.Adapters
{
    public class AirtelAdapter : IPaymentAdapter
    {
        public string GetAdapterName() => "Airtel";

        public Task<AuthResponse> AuthenticateAsync()
        {
            return Task.FromResult(new AuthResponse { Token = "mock-airtel-token" });
        }

        public Task<PaymentResponse> MakePaymentAsync(PaymentRequest request)
        {
            var success = new Random().Next(0, 2) == 1;
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
