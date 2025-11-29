using Microsoft.AspNetCore.Mvc;
using MediatR;
using universal_payment_platform.CQRS.Queries;

namespace universal_payment_platform.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProvidersController(IMediator mediator) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetSupportedProviders()
        {
            // Create a query to get providers
            var query = new GetSupportedProvidersQuery();

            // Send the query through MediatR
            var providers = await mediator.Send(query);

            return Ok(providers);
        }
    }
}
