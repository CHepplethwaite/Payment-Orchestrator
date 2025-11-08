using MediatR;
using Polly;
using Polly.Retry;
using universal_payment_platform.Data.Entities;
using universal_payment_platform.Repositories.Interfaces;
using universal_payment_platform.Services.Interfaces;
using universal_payment_platform.Common;
using universal_payment_platform.DTOs.@public;

namespace universal_payment_platform.CQRS.Commands
{
    public class PaymentRequestCommandHandler : IRequestHandler<PaymentRequest, PaymentResponse>
    {
        private readonly IEnumerable<IPaymentAdapter> _adapters;
        private readonly IPaymentRepository _paymentRepository;
        private readonly ILogger<PaymentRequestCommandHandler> _logger;
        private readonly AsyncRetryPolicy<PaymentResponse> _retryPolicy;

        public PaymentRequestCommandHandler(
            IEnumerable<IPaymentAdapter> adapters,
            IPaymentRepository paymentRepository,
            ILogger<PaymentRequestCommandHandler> logger)
        {
            _adapters = adapters;
            _paymentRepository = paymentRepository;
            _logger = logger;

            _retryPolicy = Policy<PaymentResponse>
                .Handle<Exception>()
                .WaitAndRetryAsync(
                    3,
                    attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                    (outcome, timeSpan, attempt, context) =>
                    {
                        _logger.LogWarning(
                            outcome.Exception,
                            "Retry {Attempt} after {Delay}s due to transient error for {TransactionId}",
                            attempt, timeSpan.TotalSeconds, context.GetValueOrDefault("TransactionId")
                        );
                    }
                );
        }

        public async Task<PaymentResponse> Handle(PaymentRequest command, CancellationToken cancellationToken)
        {
            var adapter = _adapters.FirstOrDefault(a =>
                a.GetAdapterName().Equals(command.Provider, StringComparison.OrdinalIgnoreCase));

            if (adapter == null)
            {
                _logger.LogError("No adapter found for provider {Provider}", command.Provider);
                return new PaymentResponse
                {
                    TransactionId = command.TransactionId,
                    Status = PaymentStatus.Failed,
                    Message = $"No adapter found for provider {command.Provider}",
                    Currency = command.Currency ?? "ZMW",              // Required
                    ProviderReference = string.Empty                  // Required
                };
            }

            // 1. Create Payment record in DB *before* processing
            var payment = new Payment
            {
                ExternalTransactionId = command.TransactionId,
                Amount = command.Amount,
                Provider = command.Provider,
                Status = PaymentStatus.Pending.ToString(),
            };
            await _paymentRepository.AddAsync(payment);

            try
            {
                _logger.LogInformation(
                    "Initiating payment via {Provider} for ExternalTransactionId {TransactionId}",
                    command.Provider, command.TransactionId
                );

                var context = new Context { ["TransactionId"] = command.TransactionId };

                // 2. Call the provider adapter
                var response = await _retryPolicy.ExecuteAsync(ctx =>
                    adapter.MakePaymentAsync(command), context
                );

                // 3. Update Payment record with the result
                payment.Status = response.Status.ToString();
                payment.Message = response.Message;
                payment.ProviderTransactionId = response.ProviderReference;
                await _paymentRepository.UpdateAsync(payment);

                // Ensure required fields are set
                response.TransactionId = payment.ExternalTransactionId;
                response.Currency ??= command.Currency ?? "ZMW";
                response.ProviderReference ??= payment.ProviderTransactionId ?? string.Empty;

                if (response.Status == PaymentStatus.Success)
                {
                    _logger.LogInformation(
                        "Payment succeeded for ExternalTransactionId {TransactionId} via {Provider}",
                        command.TransactionId, command.Provider
                    );
                }
                else
                {
                    _logger.LogWarning(
                        "Payment failed for ExternalTransactionId {TransactionId} via {Provider}: {Message}",
                        command.TransactionId, command.Provider, response.Message
                    );
                }

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Exception occurred while processing payment for ExternalTransactionId {TransactionId} via {Provider}",
                    command.TransactionId, command.Provider
                );

                // Update Payment record as failed
                payment.Status = PaymentStatus.Failed.ToString();
                payment.Message = ex.Message;
                await _paymentRepository.UpdateAsync(payment);

                return new PaymentResponse
                {
                    TransactionId = command.TransactionId,
                    Status = PaymentStatus.Failed,
                    Message = ex.Message,
                    Currency = command.Currency ?? "ZMW",     // Required
                    ProviderReference = string.Empty          // Required
                };
            }
        }
    }
}
