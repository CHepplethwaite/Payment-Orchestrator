using MediatR;
using Polly;
using Polly.Retry;
using universal_payment_platform.Data.Entities;
using universal_payment_platform.Repositories.Interfaces;
using universal_payment_platform.Services.Interfaces;
using universal_payment_platform.Services.Interfaces.Models;

namespace universal_payment_platform.CQRS.Commands
{
    public class CreatePaymentCommandHandler : IRequestHandler<CreatePaymentCommand, PaymentResponse>
    {
        private readonly IEnumerable<IPaymentAdapter> _adapters;
        private readonly IPaymentRepository _paymentRepository;
        private readonly ILogger<CreatePaymentCommandHandler> _logger;
        private readonly AsyncRetryPolicy<PaymentResponse> _retryPolicy;

        public CreatePaymentCommandHandler(
            IEnumerable<IPaymentAdapter> adapters,
            IPaymentRepository paymentRepository,
            ILogger<CreatePaymentCommandHandler> logger)
        {
            _adapters = adapters;
            _paymentRepository = paymentRepository;
            _logger = logger;

            // This retry logic is moved from PaymentOrchestrator
            _retryPolicy = Policy<PaymentResponse>
                .Handle<Exception>()
                .WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                (outcome, timeSpan, attempt, context) =>
                {
                    _logger.LogWarning(outcome.Exception,
                        "Retry {Attempt} after {Delay}s due to transient error for {TransactionId}",
                        attempt, timeSpan.TotalSeconds, context.GetValueOrDefault("TransactionId"));
                });
        }

        public async Task<PaymentResponse> Handle(CreatePaymentCommand command, CancellationToken cancellationToken)
        {
            var adapter = _adapters.FirstOrDefault(a =>
                a.GetAdapterName().Equals(command.Provider, StringComparison.OrdinalIgnoreCase));

            if (adapter == null)
            {
                _logger.LogError("No adapter found for provider {Provider}", command.Provider);
                return new PaymentResponse
                {
                    TransactionId = command.Request.TransactionId,
                    Status = PaymentStatus.Failed,
                    Message = $"No adapter found for provider {command.Provider}"
                };
            }

            // 1. Create Payment record in DB *before* processing
            var payment = new Payment
            {
                ExternalTransactionId = command.Request.TransactionId,
                Amount = command.Request.Amount,
                Provider = command.Provider,
                Status = PaymentStatus.Pending.ToString(),
                UserId = command.UserId // Use the User ID from the command
            };
            await _paymentRepository.AddAsync(payment);

            try
            {
                _logger.LogInformation(
                    "Initiating payment via {Provider} for ExternalTransactionId {TransactionId}",
                    command.Provider, command.Request.TransactionId);

                var context = new Context { ["TransactionId"] = command.Request.TransactionId };

                // 2. Call the provider adapter
                var response = await _retryPolicy.ExecuteAsync(ctx =>
                    adapter.MakePaymentAsync(command.Request), context);

                // 3. Update Payment record with the result
                payment.Status = response.Status.ToString();
                payment.Message = response.Message;
                payment.ProviderTransactionId = response.ProviderReference; // Use ProviderReference
                await _paymentRepository.UpdateAsync(payment);

                // Map the DB ID back to the response
                response.TransactionId = payment.ExternalTransactionId;

                if (response.Status == PaymentStatus.Success)
                {
                    _logger.LogInformation(
                        "Payment succeeded for ExternalTransactionId {TransactionId} via {Provider}",
                        command.Request.TransactionId, command.Provider);
                }
                else
                {
                    _logger.LogWarning(
                        "Payment failed for ExternalTransactionId {TransactionId} via {Provider}: {Message}",
                        command.Request.TransactionId, command.Provider, response.Message);
                }

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Payment processing failed for ExternalTransactionId {TransactionId} via {Provider}",
                    command.Request.TransactionId, command.Provider);

                // 4. Update Payment record with the exception
                payment.Status = PaymentStatus.Failed.ToString();
                payment.Message = $"Exception: {ex.Message}";
                await _paymentRepository.UpdateAsync(payment);

                return new PaymentResponse
                {
                    TransactionId = command.Request.TransactionId,
                    Status = PaymentStatus.Failed,
                    Message = $"Exception: {ex.Message}"
                };
            }
        }
    }
}