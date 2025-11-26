using MediatR;
using universal_payment_platform.DTOs.@public;

namespace universal_payment_platform.CQRS.Queries
{
    public class GetPaymentQuery : IRequest<PaymentResponse>
    {
        public string TransactionId { get; }
        public string Provider { get; }

        public GetPaymentQuery(string transactionId, string provider)
        {
            TransactionId = transactionId;
            Provider = provider;
        }
    }
}