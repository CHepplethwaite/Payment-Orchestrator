using MediatR;
using universal_payment_platform.Services.Interfaces.Models;

namespace universal_payment_platform.CQRS.Queries.Requests
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