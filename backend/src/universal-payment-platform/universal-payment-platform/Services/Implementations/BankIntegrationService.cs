using universal_payment_platform.Services.Interfaces;
using universal_payment_platform.Services.Interfaces.Models;

namespace universal_payment_platform.Services.Implementations
{
    public class BankIntegrationService : IBankIntegrationService
    {
        private readonly IEnumerable<IPaymentAdapter> _adapters;
        private readonly ILogger<BankIntegrationService> _logger;

        public BankIntegrationService(IEnumerable<IPaymentAdapter> adapters, ILogger<BankIntegrationService> logger)
        {
            _adapters = adapters;
            _logger = logger;
        }

        // Process a payment using the correct adapter based on the provider name
        public async Task<PaymentResponse> ProcessPaymentAsync(PaymentRequest request)
        {
            _logger.LogInformation("Processing payment for transaction {TransactionId} via {Provider}",
                request.TransactionId, request.Provider);

            var adapter = _adapters.FirstOrDefault(a => a.GetAdapterName().Equals(request.Provider, StringComparison.OrdinalIgnoreCase));
            if (adapter == null)
                return new PaymentResponse
                {
                    TransactionId = request.TransactionId,
                    Status = "Failed",
                    Message = $"No adapter found for provider {request.Provider}",
                    IsSuccess = false,
                    StatusCode = "400",
                    Timestamp = DateTime.UtcNow
                };

            // Optional: Authenticate first (for real adapters)
            await adapter.AuthenticateAsync();

            return await adapter.MakePaymentAsync(request);
        }

        // Check status of a transaction using the provider - FIXED: Added provider parameter
        public async Task<PaymentResponse> CheckPaymentStatusAsync(string transactionId, string provider)
        {
            _logger.LogInformation("Checking payment status for transaction {TransactionId} via {Provider}",
                transactionId, provider);

            var adapter = _adapters.FirstOrDefault(a => a.GetAdapterName().Equals(provider, StringComparison.OrdinalIgnoreCase));
            if (adapter == null)
                return new PaymentResponse
                {
                    TransactionId = transactionId,
                    Status = "Failed",
                    Message = $"No adapter found for provider {provider}",
                    IsSuccess = false,
                    StatusCode = "400",
                    Timestamp = DateTime.UtcNow
                };

            return await adapter.CheckTransactionStatusAsync(transactionId);
        }

        // Optional: validate account (mocked here)
        public Task<bool> ValidateAccountAsync(string accountNumber)
        {
            // For Airtel, account number is MSISDN (phone number)
            // Implement validation logic for Airtel MSISDN format
            if (string.IsNullOrWhiteSpace(accountNumber))
                return Task.FromResult(false);

            // Basic MSISDN validation - adjust based on Airtel's requirements
            var isValid = accountNumber.Length >= 10 && accountNumber.All(char.IsDigit);
            return Task.FromResult(isValid);
        }
    }
}