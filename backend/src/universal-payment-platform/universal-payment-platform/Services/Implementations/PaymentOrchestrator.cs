using Polly;
using Polly.Retry;
using universal_payment_platform.Services.Interfaces;
using universal_payment_platform.Services.Interfaces.Models;

namespace universal_payment_platform.Services.Implementations
{
    public class PaymentOrchestrator : IPaymentService
    {
        private readonly IEnumerable<IPaymentAdapter> _adapters;
        private readonly ILogger<PaymentOrchestrator> _logger;
        private readonly ITransactionService _transactionService;
        private readonly AsyncRetryPolicy<PaymentResponse> _retryPolicy;

        public PaymentOrchestrator(
            IEnumerable<IPaymentAdapter> adapters,
            ILogger<PaymentOrchestrator> logger,
            ITransactionService transactionService)
        {
            _adapters = adapters;
            _logger = logger;
            _transactionService = transactionService;

            _retryPolicy = Policy<PaymentResponse>
                .Handle<Exception>()
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                    onRetry: (outcome, timeSpan, attempt, context) =>
                    {
                        if (outcome.Exception != null)
                        {
                            _logger.LogWarning(outcome.Exception,
                                "Retry {Attempt} after {Delay}s due to transient error",
                                attempt, timeSpan.TotalSeconds);
                        }
                        else
                        {
                            _logger.LogWarning("Retry {Attempt} after {Delay}s due to handled failure with status {Status}",
                                attempt, timeSpan.TotalSeconds, outcome.Result?.Status);
                        }
                    });
        }

        public async Task<PaymentResponse> ProcessPaymentAsync(PaymentRequest request, string provider)
        {
            if (string.IsNullOrEmpty(provider))
                throw new ArgumentException("Provider must be specified", nameof(provider));

            var adapter = _adapters.FirstOrDefault(a =>
                a.GetAdapterName().Equals(provider, StringComparison.OrdinalIgnoreCase));

            if (adapter == null)
            {
                _logger.LogError("No adapter found for provider {Provider}", provider);
                return new PaymentResponse
                {
                    TransactionId = request.TransactionId ?? Guid.NewGuid().ToString(),
                    Status = PaymentStatus.Failed,
                    Message = $"No adapter found for provider {provider}"
                };
            }

            try
            {
                _logger.LogInformation(
                    "Initiating payment via {Provider} for TransactionId {TransactionId}",
                    provider, request.TransactionId);

                var response = await _retryPolicy.ExecuteAsync(() => adapter.MakePaymentAsync(request));

                if (response.Status == PaymentStatus.Success)
                {
                    _logger.LogInformation(
                        "Payment succeeded for TransactionId {TransactionId} via {Provider}",
                        request.TransactionId, provider);
                }
                else
                {
                    _logger.LogWarning(
                        "Payment failed for TransactionId {TransactionId} via {Provider}: {Message}",
                        request.TransactionId, provider, response.Message);
                }

                // Persist transaction (optional)
                await _transactionService.SaveTransactionAsync(request, response);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Payment processing failed for TransactionId {TransactionId} via {Provider}",
                    request.TransactionId, provider);

                var failedResponse = new PaymentResponse
                {
                    TransactionId = request.TransactionId ?? Guid.NewGuid().ToString(),
                    Status = PaymentStatus.Failed,
                    Message = $"Exception: {ex.Message}"
                };

                await _transactionService.SaveTransactionAsync(request, failedResponse);

                return failedResponse;
            }
        }

        public async Task<PaymentResponse> GetPaymentStatusAsync(string transactionId, string provider)
        {
            if (string.IsNullOrEmpty(provider))
                throw new ArgumentException("Provider must be specified", nameof(provider));

            var adapter = _adapters.FirstOrDefault(a =>
                a.GetAdapterName().Equals(provider, StringComparison.OrdinalIgnoreCase));

            if (adapter == null)
            {
                _logger.LogError("No adapter found for provider {Provider} when checking status", provider);
                return new PaymentResponse
                {
                    TransactionId = transactionId,
                    Status = PaymentStatus.Failed,
                    Message = $"No adapter found for provider {provider}"
                };
            }

            try
            {
                _logger.LogInformation(
                    "Checking payment status for TransactionId {TransactionId} via {Provider}",
                    transactionId, provider);

                var response = await _retryPolicy.ExecuteAsync(() => adapter.CheckTransactionStatusAsync(transactionId));

                _logger.LogInformation(
                    "Payment status for TransactionId {TransactionId} via {Provider}: {Status}",
                    transactionId, provider, response.Status);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error checking payment status for TransactionId {TransactionId} via {Provider}",
                    transactionId, provider);

                return new PaymentResponse
                {
                    TransactionId = transactionId,
                    Status = PaymentStatus.Failed,
                    Message = $"Exception: {ex.Message}"
                };
            }
        }

        public Task<List<string>> GetSupportedProvidersAsync()
        {
            var providers = _adapters.Select(a => a.GetAdapterName()).ToList();
            return Task.FromResult(providers);
        }
    }
}
