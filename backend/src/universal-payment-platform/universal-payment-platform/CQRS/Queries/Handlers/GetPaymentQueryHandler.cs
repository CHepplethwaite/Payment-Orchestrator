using MediatR;
using universal_payment_platform.CQRS.Queries.Requests;
using universal_payment_platform.Data.Entities;
using universal_payment_platform.Repositories.Interfaces;
using universal_payment_platform.Services.Interfaces;
using universal_payment_platform.Services.Interfaces.Models;

namespace universal_payment_platform.CQRS.Queries.Handlers
{
    public class GetPaymentQueryHandler : IRequestHandler<GetPaymentQuery, PaymentResponse>
    {
        private readonly IEnumerable<IPaymentAdapter> _adapters;
        private readonly IPaymentRepository _paymentRepository;
        private readonly ILogger<GetPaymentQueryHandler> _logger;
        // You can add the retry policy here too

        public GetPaymentQueryHandler(
            IEnumerable<IPaymentAdapter> adapters,
            IPaymentRepository paymentRepository,
            ILogger<GetPaymentQueryHandler> logger)
        {
            _adapters = adapters;
            _paymentRepository = paymentRepository;
            _logger = logger;
        }

        public async Task<PaymentResponse> Handle(GetPaymentQuery query, CancellationToken cancellationToken)
        {
            // 1. Check our database first
            var payment = await _paymentRepository.GetByExternalIdAsync(query.TransactionId);

            if (payment != null &&
                (payment.Status == PaymentStatus.Success.ToString() ||
                 payment.Status == PaymentStatus.Failed.ToString()))
            {
                _logger.LogInformation("Returning final status '{Status}' from DB for {TransactionId}",
                    payment.Status, query.TransactionId);

                return new PaymentResponse
                {
                    TransactionId = payment.ExternalTransactionId,
                    Status = Enum.Parse<PaymentStatus>(payment.Status),
                    Message = payment.Message,
                    ProviderReference = payment.ProviderTransactionId
                };
            }

            // 2. If status is Pending or not found, query the provider
            var adapter = _adapters.FirstOrDefault(a =>
                a.GetAdapterName().Equals(query.Provider, StringComparison.OrdinalIgnoreCase));

            if (adapter == null)
            {
                _logger.LogError("No adapter found for provider {Provider} when checking status", query.Provider);
                return new PaymentResponse
                {
                    TransactionId = query.TransactionId,
                    Status = PaymentStatus.Failed,
                    Message = $"No adapter found for provider {query.Provider}"
                };
            }

            _logger.LogInformation("Querying provider {Provider} for status of {TransactionId}",
                query.Provider, query.TransactionId);

            try
            {
                var response = await adapter.CheckTransactionStatusAsync(query.TransactionId);

                // 3. Update our database with the new status
                if (payment == null)
                {
                    // This is unusual, but we can create a record now
                    payment = new Payment
                    {
                        ExternalTransactionId = query.TransactionId,
                        Provider = query.Provider,
                        Status = response.Status.ToString(),
                        Message = response.Message,
                        ProviderTransactionId = response.ProviderReference,
                        UserId = "UNKNOWN" // We don't know the user at this point
                    };
                    await _paymentRepository.AddAsync(payment);
                }
                else
                {
                    payment.Status = response.Status.ToString();
                    payment.Message = response.Message;
                    payment.ProviderTransactionId = response.ProviderReference;
                    await _paymentRepository.UpdateAsync(payment);
                }

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking payment status for {TransactionId}", query.TransactionId);
                return new PaymentResponse
                {
                    TransactionId = query.TransactionId,
                    Status = PaymentStatus.Failed,
                    Message = $"Exception: {ex.Message}"
                };
            }
        }
    }
}