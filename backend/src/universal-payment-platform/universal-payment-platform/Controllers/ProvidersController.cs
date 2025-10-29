using Microsoft.AspNetCore.Mvc;
using universal_payment_platform.Services.Interfaces;

namespace universal_payment_platform.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProvidersController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public ProvidersController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpGet]
        public async Task<IActionResult> GetSupportedProviders()
        {
            var providers = await _paymentService.GetSupportedProvidersAsync();
            return Ok(providers);
        }
    }
}
