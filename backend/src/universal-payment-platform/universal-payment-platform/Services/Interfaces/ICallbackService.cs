using System.Text.Json;
using universal_payment_platform.Services.Interfaces;
using universal_payment_platform.Services.Interfaces.Models;

namespace universal_payment_platform.Services.Implementations
{
    public class CallbackService : ICallbackService
    {
        private readonly ILogger<CallbackService> _logger;
        private readonly ITransactionService _transactionService;

        public CallbackService(ILogger<CallbackService> logger, ITransactionService transactionService)
        {
            _logger = logger;
            _transactionService = transactionService;
        }

        public async Task HandleCallbackAsync(string providerName, object callbackPayload)
        {
            try
            {
                // Convert callbackPayload (usually JSON) into a strongly-typed object
                var json = JsonSerializer.Serialize(callbackPayload);
                _logger.LogInformation("Received callback from {Provider}: {Payload}", providerName, json);

                // TODO: Parse provider-specific callback here.
                // Example dummy parse:
                var transactionId = ExtractTransactionId(callbackPayload);
                var status = ExtractPaymentStatus(callbackPayload);

                if (transactionId == null)
                {
                    _logger.LogWarning("Callback from {Provider} missing transaction ID", providerName);
                    return;
                }

                await _transactionService.UpdateTransactionStatusAsync(transactionId, status);

                _logger.LogInformation("Updated transaction {TransactionId} status to {Status}", transactionId, status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling callback from {Provider}", providerName);
            }
        }

        private string? ExtractTransactionId(object payload)
        {
            // TODO: Replace this with real parsing logic for each provider
            return payload?.ToString()?.Contains("TXN") == true ? "TXN12345" : null;
        }

        private PaymentStatus ExtractPaymentStatus(object payload)
        {
            // TODO: Replace this with actual parsing logic
            return PaymentStatus.Completed;
        }
    }
}
