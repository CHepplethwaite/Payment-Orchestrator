using universal_payment_platform.DTOs.@public;

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
