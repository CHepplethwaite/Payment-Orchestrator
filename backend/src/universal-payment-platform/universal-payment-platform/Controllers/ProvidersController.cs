using Microsoft.AspNetCore.Mvc;
using MediatR;
using universal_payment_platform.CQRS.Queries;

namespace universal_payment_platform.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProvidersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProvidersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetSupportedProviders()
        {
            // Create a query to get providers
            var query = new GetSupportedProvidersQuery();

            // Send the query through MediatR
            var providers = await _mediator.Send(query);

            return Ok(providers);
        }
    }
}
