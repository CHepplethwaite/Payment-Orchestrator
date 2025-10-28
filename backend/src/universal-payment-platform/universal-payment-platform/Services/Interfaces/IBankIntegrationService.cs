using Microsoft.AspNetCore.Mvc.ModelBinding;
using universal_payment_platform.Services.Interfaces.Models;

namespace universal_payment_platform.Services.Interfaces
{
    public interface IBankIntegrationService
    {
        Task<PaymentResponse> ProcessPaymentAsync(PaymentRequest request);
        Task<PaymentResponse> CheckPaymentStatusAsync(string transactionId, string provider);
        Task<bool> ValidateAccountAsync(string accountNumber);
    }
}