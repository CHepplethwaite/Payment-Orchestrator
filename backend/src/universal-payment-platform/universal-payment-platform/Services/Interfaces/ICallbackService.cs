using System.Threading.Tasks;

namespace universal_payment_platform.Services.Interfaces
{
    /// <summary>
    /// Handles callbacks from payment providers.
    /// Implementations should parse provider-specific payloads
    /// and update transaction statuses accordingly.
    /// </summary>
    public interface ICallbackService
    {
        /// <summary>
        /// Processes a callback from a payment provider.
        /// </summary>
        /// <param name="providerName">The name of the payment provider.</param>
        /// <param name="callbackJson">The raw callback payload as JSON.</param>
        Task HandleCallbackAsync(string providerName, string callbackJson);
    }
}
