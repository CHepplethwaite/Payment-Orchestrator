// File: CQRS/Commands/PaymentRequestCommand.cs

using MediatR;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation; // <-- NEW: Required for [ValidateNever]
using universal_payment_platform.DTOs.Responses;

namespace universal_payment_platform.CQRS.Commands
{
    // This command encapsulates the payment request and will return a PaymentResponse
    public class PaymentRequestCommand : IRequest<PaymentResponse>
    {
        // These fields are bound directly from the JSON request body
        public string Provider { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string Description { get; set; }

        // JWT user ID — Must be decorated with:
        // 1. [JsonIgnore] to prevent clients from setting it via the body.
        // 2. [ValidateNever] to skip ASP.NET Core's initial validation (allowing controller code to run).
        [JsonIgnore]
        [ValidateNever] // <--- CRITICAL FIX
        public string UserId { get; set; }

        public PaymentRequestCommand() { }

        public PaymentRequestCommand(string provider, decimal amount, string currency, string description, string userId = null)
        {
            Provider = provider;
            Amount = amount;
            Currency = currency;
            Description = description;
            UserId = userId;
        }
    }
}