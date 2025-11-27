using MediatR;
using Polly;
using Polly.Retry;
using universal_payment_platform.Data.Entities;
using universal_payment_platform.Repositories.Interfaces;
using universal_payment_platform.Services.Interfaces;
using universal_payment_platform.DTOs.@public;
using universal_payment_platform.Common;
using System.Text.Json;

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
                    Currency = command.Currency ?? "ZMW",
                    ProviderReference = string.Empty
                };
            }

            // 1. Create Payment record in DB - EXACTLY MATCHING YOUR ENTITY
            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                Provider = command.Provider,
                ExternalTransactionId = command.TransactionId,
                Amount = command.Amount,
                Currency = command.Currency ?? "ZMW",
                Status = (PaymentStatus)PaymentStatus.Pending, // Explicit cast to PaymentStatus
                Description = command.Description ?? string.Empty,
                CreatedAt = DateTime.UtcNow,
                Payer = new PayerInfo(), // Initialize empty
                Payees = new List<PayeeInfo>(),
                AuditTrail = new List<PaymentAudit>()
            };

            // Add initial provider metadata as JSON
            var providerMetadata = new
            {
                TransactionId = command.TransactionId,
                Provider = command.Provider,
                Currency = command.Currency ?? "ZMW",
                RequestedAt = DateTime.UtcNow
            };

            payment.ProviderMetadata = JsonSerializer.Serialize(providerMetadata);

            // Add initial audit trail
            payment.AddAuditTrail($"Payment created for {command.Provider} with amount {command.Amount} {command.Currency}");

            await _paymentRepository.AddAsync(payment);

            try
            {
                _logger.LogInformation(
                    "Initiating payment via {Provider} for TransactionId {TransactionId}",
                    command.Provider, command.TransactionId
                );

                var context = new Context { ["TransactionId"] = command.TransactionId };

                var response = await _retryPolicy.ExecuteAsync(ctx =>
                    adapter.MakePaymentAsync(command), context
                );

                // Update Payment record with provider response
                // Convert DTO PaymentStatus to Entity PaymentStatus using cast
                payment.Status = (PaymentStatus)response.Status;

                // Update provider metadata with response details
                var updatedMetadata = new
                {
                    TransactionId = command.TransactionId,
                    Provider = command.Provider,
                    Currency = command.Currency ?? "ZMW",
                    RequestedAt = payment.CreatedAt,
                    ProviderTransactionId = response.ProviderReference ?? string.Empty,
                    ProviderMessage = response.Message ?? string.Empty,
                    RespondedAt = DateTime.UtcNow,
                    Status = response.Status.ToString()
                };

                payment.ProviderMetadata = JsonSerializer.Serialize(updatedMetadata);

                // If we got a provider reference, update the external transaction ID
                if (!string.IsNullOrEmpty(response.ProviderReference))
                {
                    payment.ExternalTransactionId = response.ProviderReference;
                }

                // Update completed time if successful
                if (response.Status == PaymentStatus.Success)
                {
                    payment.CompletedAt = DateTime.UtcNow;
                }

                // Add to audit trail
                payment.AddAuditTrail($"Payment status updated to {response.Status} via {command.Provider}");

                await _paymentRepository.UpdateAsync(payment);

                // Ensure response has all required fields
                response.TransactionId = payment.ExternalTransactionId;
                response.Currency = payment.Currency;
                response.ProviderReference = response.ProviderReference ?? string.Empty;

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Exception occurred while processing payment for TransactionId {TransactionId} via {Provider}",
                    command.TransactionId, command.Provider
                );

                // Update payment as failed
                payment.Status = (PaymentStatus)PaymentStatus.Failed;

                // Update provider metadata with error
                var errorMetadata = new
                {
                    TransactionId = command.TransactionId,
                    Provider = command.Provider,
                    Currency = command.Currency ?? "ZMW",
                    RequestedAt = payment.CreatedAt,
                    Error = ex.Message,
                    FailedAt = DateTime.UtcNow,
                    Status = "Failed"
                };

                payment.ProviderMetadata = JsonSerializer.Serialize(errorMetadata);

                // Add to audit trail
                payment.AddAuditTrail($"Payment failed: {ex.Message}");

                await _paymentRepository.UpdateAsync(payment);

                return new PaymentResponse
                {
                    TransactionId = command.TransactionId,
                    Status = PaymentStatus.Failed,
                    Message = ex.Message,
                    Currency = command.Currency ?? "ZMW",
                    ProviderReference = string.Empty
                };
            }
        }
    }
}