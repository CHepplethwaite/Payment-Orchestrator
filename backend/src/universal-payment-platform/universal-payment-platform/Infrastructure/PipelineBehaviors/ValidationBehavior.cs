using FluentValidation;
using MediatR;
// using universal_payment_platform.Infrastructure.Exceptions; // <-- REMOVED: This using is not needed here

namespace universal_payment_platform.Infrastructure.PipelineBehaviors
{
    /// <summary>
    /// MediatR Pipeline Behavior that runs Fluent Validation before processing the request.
    /// </summary>
    /// <typeparam name="TRequest">The request type (e.g., PaymentRequestCommand)</typeparam>
    /// <typeparam name="TResponse">The response type (e.g., PaymentResponse)</typeparam>
    public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators) : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            if (validators.Any())
            {
                var context = new ValidationContext<TRequest>(request);

                var validationResults = await Task.WhenAll(
                    validators.Select(v => v.ValidateAsync(context, cancellationToken)));

                var failures = validationResults
                    .SelectMany(r => r.Errors)
                    .Where(f => f != null)
                    .ToList();

                if (failures.Count != 0)
                {
                    // Throw a specific exception (e.g., ValidationException) 
                    // which your ExceptionHandlerMiddleware should catch and map to a 400 Bad Request.
                    // The FluentValidation.ValidationException type is fully qualified, so it's correct.
                    throw new FluentValidation.ValidationException(failures);
                }
            }

            // Validation passed, proceed to the handler
            return await next();
        }
    }
}