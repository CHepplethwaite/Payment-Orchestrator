// Controllers/WebhookController.cs
using Microsoft.AspNetCore.Mvc;
using universal_payment_platform.Services.Interfaces;

namespace universal_payment_platform.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WebhookController(ICallbackService callbackService, ILogger<WebhookController> logger) : ControllerBase
    {
        [HttpPost("{provider}")]
        public async Task<IActionResult> HandleCallback(string provider, [FromBody] object payload)
        {
            try
            {
                logger.LogInformation("Received webhook from {Provider}", provider);
                await callbackService.HandleCallbackAsync(provider, payload.ToString() ?? string.Empty);
                return Ok();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing webhook from {Provider}", provider);
                return StatusCode(500, "Error processing webhook");
            }
        }
    }
}