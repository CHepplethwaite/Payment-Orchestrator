// Controllers/WebhookController.cs
using Microsoft.AspNetCore.Mvc;
using universal_payment_platform.Services.Interfaces;

namespace universal_payment_platform.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WebhookController : ControllerBase
    {
        private readonly ICallbackService _callbackService;
        private readonly ILogger<WebhookController> _logger;

        public WebhookController(ICallbackService callbackService, ILogger<WebhookController> logger)
        {
            _callbackService = callbackService;
            _logger = logger;
        }

        [HttpPost("{provider}")]
        public async Task<IActionResult> HandleCallback(string provider, [FromBody] object payload)
        {
            try
            {
                _logger.LogInformation("Received webhook from {Provider}", provider);
                await _callbackService.HandleCallbackAsync(provider, payload.ToString() ?? string.Empty);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing webhook from {Provider}", provider);
                return StatusCode(500, "Error processing webhook");
            }
        }
    }
}