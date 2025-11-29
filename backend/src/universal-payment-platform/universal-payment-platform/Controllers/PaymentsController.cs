// File: Controllers/PaymentsController.cs

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using universal_payment_platform.CQRS.Commands;
using universal_payment_platform.CQRS.Queries;
using universal_payment_platform.Common; // Needed for PaymentStatus

namespace universal_payment_platform.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PaymentsController(IMediator mediator) : ControllerBase
    {
        private string GetUserId()
        {
            // Fallback: If User ID is truly essential and missing, throw an exception.
            return User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                       throw new BadHttpRequestException("User ID not found in token. Authentication setup may be incorrect.");
        }

        [HttpPost]
        public async Task<IActionResult> ProcessPayment([FromBody] PaymentRequestCommand request)
        {
            if (request == null)
                return BadRequest("Request body is required.");

            try
            {
                // CRITICAL FIX: Inject the UserId from the authenticated token *before* MediatR validation.
                request.UserId = GetUserId();
            }
            catch (BadHttpRequestException ex)
            {
                // Handle case where authenticated user ID is missing (e.g., token is bad/incomplete)
                return Unauthorized(new { error = ex.Message });
            }

            // All necessary data (client body + security context) is now in the command.
            // MediatR pipeline (including Fluent Validation) runs here.
            var response = await mediator.Send(request);

            // Assuming PaymentStatus is correctly imported from universal_payment_platform.Common
            if (response.Status == PaymentStatus.Failed)
                return BadRequest(response);

            return Ok(response);
        }

        [HttpGet("{transactionId}/status")]
        public async Task<IActionResult> GetPaymentStatus(string transactionId, [FromQuery] string provider)
        {
            if (string.IsNullOrEmpty(provider))
                return BadRequest("Provider must be specified.");

            if (string.IsNullOrEmpty(transactionId))
                return BadRequest("Transaction ID must be specified.");

            var query = new GetPaymentQuery(transactionId, provider);
            var response = await mediator.Send(query);

            return Ok(response);
        }
    }
}