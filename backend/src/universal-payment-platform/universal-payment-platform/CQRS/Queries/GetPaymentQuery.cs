using MediatR;
using universal_payment_platform.DTOs.@public;

namespace universal_payment_platform.CQRS.Queries
{
    public class GetPaymentQuery(string transactionId, string provider) : IRequest<PaymentResponse>
    {
        public string TransactionId { get; } = transactionId;
        public string Provider { get; } = provider;
    }
}