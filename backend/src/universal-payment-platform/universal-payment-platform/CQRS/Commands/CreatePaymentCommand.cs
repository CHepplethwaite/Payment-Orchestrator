using MediatR;
using universal_payment_platform.Services.Interfaces.Models;

namespace universal_payment_platform.CQRS.Commands
{
    // This command encapsulates the payment request and will return a PaymentResponse
    public class CreatePaymentCommand : IRequest<PaymentResponse>
    {
        public PaymentRequest Request { get; }
        public string Provider { get; }
        public string UserId { get; } // Pass the authenticated User ID here

        public CreatePaymentCommand(PaymentRequest request, string provider, string userId)
        {
            Request = request;
            Provider = provider;
            UserId = userId;
        }
    }
}