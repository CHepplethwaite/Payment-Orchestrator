using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims; // For getting User ID
using universal_payment_platform.CQRS.Commands.Requests;
using universal_payment_platform.CQRS.Queries.Requests;
using universal_payment_platform.Services.Interfaces.Models;

namespace universal_payment_platform.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Require authentication for all payment actions
    public class PaymentsController : ControllerBase
    {
        // Inject MediatR instead of IPaymentService
        private readonly IMediator _mediator;

        public PaymentsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        private string GetUserId()
        {
            // Get the user ID from the JWT token
            return User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                   throw new BadHttpRequestException("User ID not found in token.");
        }

        [HttpPost]
        public async Task<IActionResult> ProcessPayment([FromBody] PaymentRequest request)
        {
            if (string.IsNullOrEmpty(request.Provider))
                return BadRequest("Provider must be specified.");

            var userId = GetUserId(); // Get authenticated user's ID

            // Create and send the command
            var command = new CreatePaymentCommand(request, request.Provider, userId);
            var response = await _mediator.Send(command);

            if (response.Status == PaymentStatus.Failed)
                return BadRequest(response); // Or Ok(response) depending on your API contract

            return Ok(response);
        }

        [HttpGet("{transactionId}/status")]
        public async Task<IActionResult> GetPaymentStatus(string transactionId, [FromQuery] string provider)
        {
            if (string.IsNullOrEmpty(provider))
                return BadRequest("Provider must be specified.");

            // Create and send the query
            var query = new GetPaymentQuery(transactionId, provider);
            var response = await _mediator.Send(query);

            return Ok(response);
        }
    }
}