// File: Validators/PaymentRequestValidator.cs

using FluentValidation;
using universal_payment_platform.CQRS.Commands;

namespace universal_payment_platform.Validators
{
    public class PaymentRequestValidator : AbstractValidator<PaymentRequestCommand>
    {
        public PaymentRequestValidator()
        {
            // UserId check is now valid because the controller sets it before validation runs.
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("UserId is required.");

            RuleFor(x => x.Provider)
                .NotEmpty().WithMessage("Provider is required.");

            // These properties are correctly present on PaymentRequestCommand now.
            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Amount must be greater than zero.");

            RuleFor(x => x.Currency)
                .NotEmpty().WithMessage("Currency is required.");

            // Add other rules if needed, e.g., Description, MerchantId, etc.
        }
    }
}