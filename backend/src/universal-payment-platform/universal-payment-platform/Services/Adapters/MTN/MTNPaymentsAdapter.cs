using System;
using System.Threading.Tasks;
using universal_payment_platform.Common;
using universal_payment_platform.DTOs; // PaymentRequest, PaymentResponse, AuthResponse
using universal_payment_platform.DTOs.@public;
using universal_payment_platform.Services.Interfaces;

namespace universal_payment_platform.Services.Adapters.MTN
{
    public class MTNPaymentsAdapter : IPaymentAdapter
    {
        // Mock authentication
        public async Task<AuthResponse> AuthenticateAsync()
        {
            await Task.Delay(10); // simulate async call

            // Return only the existing properties in AuthResponse
            return new AuthResponse
            {
                Token = "mock-mtn-token"
                // Expiry removed because it doesn't exist in your DTO
            };
        }

        public string GetAdapterName()
        {
            return "MTN";
        }

        // Mock payment processing
        public async Task<PaymentResponse> MakePaymentAsync(PaymentRequest request)
        {
            await Task.Delay(50); // simulate async call

            return new PaymentResponse
            {
                TransactionId = Guid.NewGuid().ToString(),
                Message = "Payment successful",
                ProviderReference = "MTN-" + Guid.NewGuid(),
                Currency = request.Currency ?? "ZMW",
                Status = PaymentStatus.Success // enum, not string
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
                Status = PaymentStatus.Success // enum
            };
        }
    }
}
