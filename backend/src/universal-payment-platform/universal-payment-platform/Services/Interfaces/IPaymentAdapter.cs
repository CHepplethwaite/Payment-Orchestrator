using universal_payment_platform.Services.Interfaces.Models;

namespace universal_payment_platform.Services.Interfaces
{
    public interface IPaymentAdapter
    {
        Task<PaymentResponse> MakePaymentAsync(PaymentRequest request);
        Task<PaymentResponse> CheckTransactionStatusAsync(string transactionId);
        Task<AuthResponse> AuthenticateAsync();
        string GetAdapterName();
    }
}