using universal_payment_platform.Services.Interfaces.Models;

namespace universal_payment_platform.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<PaymentResponse> ProcessPaymentAsync(PaymentRequest request, string provider);
        Task<PaymentResponse> GetPaymentStatusAsync(string transactionId, string provider);
        Task<List<string>> GetSupportedProvidersAsync();
    }
}