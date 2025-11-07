using universal_payment_platform.Common;
using universal_payment_platform.DTOs.Requests;
using universal_payment_platform.DTOs.Responses;
using universal_payment_platform.Services.Interfaces;

namespace universal_payment_platform.Services.Adapters
{
    public class AirtelAdapter : IPaymentAdapter
    {
        private static readonly Random _random = new Random();

        public string GetAdapterName() => "Airtel";

        public Task<AuthResponse> AuthenticateAsync()
        {
            // Mock authentication token
            return Task.FromResult(new AuthResponse { Token = "mock-airtel-token" });
        }

        public Task<PaymentResponse> MakePaymentAsync(PaymentRequest request)
        {
            // Simulate success/failure
            var success = _random.Next(0, 10) < 8;

            // Return PaymentResponse with all required fields
            return Task.FromResult(new PaymentResponse
            {
                TransactionId = request.TransactionId,
                Status = success ? PaymentStatus.Success : PaymentStatus.Failed,
                Message = success ? "Airtel payment succeeded" : "Airtel payment failed",
                Currency = request.Currency ?? "ZMW",            // Required field
                ProviderReference = $"AIRTEL-{Guid.NewGuid()}"   // Required field, simulate provider reference
            });
        }

        public Task<PaymentResponse> CheckTransactionStatusAsync(string transactionId)
        {
            // Simulate transaction status check
            return Task.FromResult(new PaymentResponse
            {
                TransactionId = transactionId,
                Status = PaymentStatus.Success,
                Message = "Mock Airtel transaction completed",
                Currency = "ZMW",                                // Required field
                ProviderReference = $"AIRTEL-{Guid.NewGuid()}"   // Required field
            });
        }
    }
}
