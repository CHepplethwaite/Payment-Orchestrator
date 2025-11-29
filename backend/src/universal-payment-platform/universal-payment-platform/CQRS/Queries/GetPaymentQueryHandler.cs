using MediatR;
using universal_payment_platform.Data.Entities;
using universal_payment_platform.Repositories.Interfaces;
using universal_payment_platform.Services.Interfaces;
using universal_payment_platform.Common;
using universal_payment_platform.DTOs.@public;
using System.Text.Json;

namespace universal_payment_platform.CQRS.Queries
{
    public class GetPaymentQueryHandler(
        IEnumerable<IPaymentAdapter> adapters,
        IPaymentRepository paymentRepository,
        ILogger<GetPaymentQueryHandler> logger) : IRequestHandler<GetPaymentQuery, PaymentResponse>
    {
        public async Task<PaymentResponse> Handle(GetPaymentQuery query, CancellationToken cancellationToken)
        {
            var payment = await paymentRepository.GetByExternalIdAsync(query.TransactionId);

            if (payment != null &&
                (payment.Status == Common.PaymentStatus.Success || payment.Status == Common.PaymentStatus.Failed))
            {
                logger.LogInformation(
                    "Returning final status '{Status}' from DB for {TransactionId}",
                    payment.Status, query.TransactionId
                );

                // Deserialize the ProviderMetadata JSON string
                var metadata = DeserializeProviderMetadata(payment.ProviderMetadata);

                return new PaymentResponse
                {
                    TransactionId = payment.ExternalTransactionId,
                    Status = payment.Status,
                    Message = metadata?.Message ?? string.Empty,
                    ProviderReference = metadata?.ProviderTransactionId ?? string.Empty,
                    Currency = payment.Currency
                };
            }

            var adapter = adapters.FirstOrDefault(a =>
                a.GetAdapterName().Equals(query.Provider, StringComparison.OrdinalIgnoreCase));

            if (adapter == null)
            {
                logger.LogError("No adapter found for provider {Provider}", query.Provider);
                return new PaymentResponse
                {
                    TransactionId = query.TransactionId,
                    Status = PaymentStatus.Failed, // Use DTO PaymentStatus directly
                    Message = $"Payment provider '{query.Provider}' not supported",
                    ProviderReference = string.Empty,
                    Currency = "ZMW"
                };
            }

            try
            {
                var response = await adapter.CheckTransactionStatusAsync(query.TransactionId);

                if (payment == null)
                {
                    // Create new payment record
                    payment = new Payment
                    {
                        Id = Guid.NewGuid(),
                        Provider = query.Provider,
                        ExternalTransactionId = query.TransactionId,
                        Amount = 0, // Amount might not be available in status check
                        Currency = response.Currency ?? "ZMW",
                        Status = ConvertToEntityStatus(response.Status), // Convert DTO status to entity status
                        Description = "Payment created via status query",
                        CreatedAt = DateTime.UtcNow,
                        Payer = new PayerInfo(),
                        Payees = new List<PayeeInfo>(),
                        AuditTrail = new List<PaymentAudit>()
                    };

                    // Set initial provider metadata
                    var initialMetadata = new
                    {
                        query.TransactionId,
                        query.Provider,
                        ProviderTransactionId = response.ProviderReference ?? string.Empty,
                        Message = response.Message ?? string.Empty,
                        CreatedVia = "StatusQuery",
                        QueriedAt = DateTime.UtcNow
                    };
                    payment.ProviderMetadata = JsonSerializer.Serialize(initialMetadata);

                    await paymentRepository.AddAsync(payment);
                }
                else
                {
                    // Update existing payment
                    payment.Status = ConvertToEntityStatus(response.Status); // Convert DTO status to entity status

                    // Update provider metadata
                    var updatedMetadata = new
                    {
                        query.TransactionId,
                        query.Provider,
                        ProviderTransactionId = response.ProviderReference ?? string.Empty,
                        Message = response.Message ?? string.Empty,
                        UpdatedVia = "StatusQuery",
                        QueriedAt = DateTime.UtcNow,
                        PreviousStatus = payment.Status.ToString()
                    };
                    payment.ProviderMetadata = JsonSerializer.Serialize(updatedMetadata);

                    // Update completed time if successful - compare using converted status
                    if (ConvertToEntityStatus(response.Status) == Common.PaymentStatus.Success)
                    {
                        payment.CompletedAt = DateTime.UtcNow;
                    }

                    await paymentRepository.UpdateAsync(payment);
                }

                return new PaymentResponse
                {
                    TransactionId = response.TransactionId ?? query.TransactionId,
                    Status = response.Status,
                    Message = response.Message ?? string.Empty,
                    ProviderReference = response.ProviderReference ?? string.Empty,
                    Currency = response.Currency ?? "ZMW"
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error checking payment status for {TransactionId}", query.TransactionId);

                return new PaymentResponse
                {
                    TransactionId = query.TransactionId,
                    Status = PaymentStatus.Failed, // Use DTO PaymentStatus directly
                    Message = $"Error querying provider: {ex.Message}",
                    ProviderReference = string.Empty,
                    Currency = "ZMW"
                };
            }
        }

        // Helper method to deserialize ProviderMetadata JSON
        private ProviderMetadataDto? DeserializeProviderMetadata(string? providerMetadataJson)
        {
            if (string.IsNullOrEmpty(providerMetadataJson))
                return null;

            try
            {
                return JsonSerializer.Deserialize<ProviderMetadataDto>(providerMetadataJson);
            }
            catch (JsonException ex)
            {
                logger.LogWarning(ex, "Failed to deserialize ProviderMetadata JSON");
                return null;
            }
        }

        // Helper method to convert Entity PaymentStatus to DTO PaymentStatus
        private PaymentStatus ConvertToDtoStatus(Common.PaymentStatus entityStatus)
        {
            return entityStatus switch
            {
                Common.PaymentStatus.Pending => PaymentStatus.Pending,
                Common.PaymentStatus.Success => PaymentStatus.Success,
                Common.PaymentStatus.Failed => PaymentStatus.Failed,
                _ => PaymentStatus.Pending
            };
        }

        // Helper method to convert DTO PaymentStatus to Entity PaymentStatus
        private Common.PaymentStatus ConvertToEntityStatus(PaymentStatus dtoStatus)
        {
            return dtoStatus switch
            {
                PaymentStatus.Pending => Common.PaymentStatus.Pending,
                PaymentStatus.Success => Common.PaymentStatus.Success,
                PaymentStatus.Failed => Common.PaymentStatus.Failed,
                _ =>    Common.PaymentStatus.Pending
            };
        }
    }

    // DTO for deserializing ProviderMetadata JSON
    public class ProviderMetadataDto
    {
        public string? TransactionId { get; set; }
        public string? Provider { get; set; }
        public string? Message { get; set; }
        public string? ProviderTransactionId { get; set; }
        public string? Currency { get; set; }
    }
}