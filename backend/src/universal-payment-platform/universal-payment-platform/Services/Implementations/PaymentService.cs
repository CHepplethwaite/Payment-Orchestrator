using universal_payment_platform.Services.Interfaces;
using universal_payment_platform.Services.Interfaces.Models;

namespace universal_payment_platform.Services.Implementations
{
    public class PaymentService : IPaymentService
    {
        private readonly IBankIntegrationService _bankIntegrationService;
        private readonly IEnumerable<IPaymentAdapter> _adapters;

        public PaymentService(IBankIntegrationService bankIntegrationService, IEnumerable<IPaymentAdapter> adapters)
        {
            _bankIntegrationService = bankIntegrationService;
            _adapters = adapters;
        }

        public Task<PaymentResponse> ProcessPaymentAsync(PaymentRequest request, string provider)
        {
            request.Provider = provider; // ensure provider is set
            return _bankIntegrationService.ProcessPaymentAsync(request);
        }

        public Task<PaymentResponse> GetPaymentStatusAsync(string transactionId, string provider)
        {
            return _bankIntegrationService.CheckPaymentStatusAsync(transactionId, provider);
        }

        public Task<List<string>> GetSupportedProvidersAsync()
        {
            return Task.FromResult(_adapters.Select(a => a.GetAdapterName()).ToList());
        }
    }
}
