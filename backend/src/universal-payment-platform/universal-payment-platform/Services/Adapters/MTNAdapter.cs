using universal_payment_platform.Common;
using universal_payment_platform.DTOs.Requests;
using universal_payment_platform.DTOs.Responses;
using universal_payment_platform.Services.Interfaces;

namespace universal_payment_platform.Services.Adapters
{
    public class MTNAdapter : IPaymentAdapter
    {
        private static readonly Random _random = new Random();

        public string GetAdapterName() => "MTN";

        public Task<AuthResponse> AuthenticateAsync()
        {
            return Task.FromResult(new AuthResponse
            {
                Token = "mock-mtn-token",
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            });
        }

        public Task<PaymentResponse> MakePaymentAsync(PaymentRequest request)
        {
            var success = _random.Next(0, 10) < 8;

            return Task.FromResult(new PaymentResponse
            {
                TransactionId = request.TransactionId,
                Status = success ? PaymentStatus.Success : PaymentStatus.Failed,
                Message = success ? "MTN payment succeeded" : "MTN payment failed",
                Currency = request.Currency ?? "ZMW",
                ProviderReference = $"MTN-{Guid.NewGuid()}"
            });
        }

        public Task<PaymentResponse> CheckTransactionStatusAsync(string transactionId)
        {
            return Task.FromResult(new PaymentResponse
            {
                TransactionId = transactionId,
                Status = PaymentStatus.Success,
                Message = "Mock MTN transaction completed",
                Currency = "ZMW",
                ProviderReference = $"MTN-{Guid.NewGuid()}"
            });
        }
    }
}