using Microsoft.AspNetCore.Mvc;
using universal_payment_platform.Services.Interfaces;
using universal_payment_platform.Services.Interfaces.Models;

namespace universal_payment_platform.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentsController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpPost]
        public async Task<IActionResult> ProcessPayment([FromBody] PaymentRequest request)
        {
            if (string.IsNullOrEmpty(request.Provider))
                return BadRequest("Provider must be specified.");

            var response = await _paymentService.ProcessPaymentAsync(request, request.Provider);
            return Ok(response);
        }

        [HttpGet("{transactionId}/status")]
        public async Task<IActionResult> GetPaymentStatus(string transactionId, [FromQuery] string provider)
        {
            if (string.IsNullOrEmpty(provider))
                return BadRequest("Provider must be specified.");

            var response = await _paymentService.GetPaymentStatusAsync(transactionId, provider);
            return Ok(response);
        }
    }
}
