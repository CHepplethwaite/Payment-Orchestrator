using MediatR;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using universal_payment_platform.DTOs.@public;


namespace universal_payment_platform.CQRS.Commands
{
    // This command encapsulates the payment request and will return a PaymentResponse
    public class PaymentRequestCommand : IRequest<PaymentResponse>
    {
        // These fields are bound directly from the JSON request body
        public required string Provider { get; set; }
        public decimal Amount { get; set; }
        public required string Currency { get; set; }
        public required string Description { get; set; }

        // JWT user ID — set internally from the token, not from the JSON body
        [JsonIgnore]
        [ValidateNever]
        public string UserId { get; set; } = string.Empty; // removed 'required'

        // Parameterless constructor for deserialization
        public PaymentRequestCommand() { }

        // Optional constructor for manual instantiation (internal use)
        public PaymentRequestCommand(string provider, decimal amount, string currency, string description, string userId)
        {
            Provider = provider;
            Amount = amount;
            Currency = currency;
            Description = description;
            UserId = userId;
        }
    }
}
