using System;
using universal_payment_platform.Common;
using universal_payment_platform.DTOs; // PaymentRequest, PaymentResponse, AuthResponse
using universal_payment_platform.DTOs.@public;
using universal_payment_platform.Services.Interfaces;

namespace universal_payment_platform.Services.Adapters.Airtel
{
    public class AirtelPaymentsAdapter : IPaymentAdapter
    {
        // Mock authentication
        public async Task<AuthResponse> AuthenticateAsync()
        {
            await Task.Delay(10); // simulate async call

            return new AuthResponse
            {
                Token = "mock-airtel-token"
                // Removed Expiry as it's not in your AuthResponse DTO
            };
        }

        public string GetAdapterName()
        {
            return "Airtel";
        }

        // Mock payment processing
        public async Task<PaymentResponse> MakePaymentAsync(PaymentRequest request)
        {
            await Task.Delay(50); // simulate async call

            return new PaymentResponse
            {
                TransactionId = Guid.NewGuid().ToString(),
                Message = "Payment successful",
                ProviderReference = "AIRTEL-" + Guid.NewGuid(),
                Currency = request.Currency ?? "ZMW",
                Status = PaymentStatus.Success
            };
        }

        // Mock transaction status check
        public async Task<PaymentResponse> CheckTransactionStatusAsync(string transactionId)
        {
            await Task.Delay(20); // simulate async call

            return new PaymentResponse
            {
                TransactionId = transactionId,
                Message = "Transaction found",
                ProviderReference = transactionId,
                Currency = "ZMW",
                Status = PaymentStatus.Success
            };
        }
    }
}
