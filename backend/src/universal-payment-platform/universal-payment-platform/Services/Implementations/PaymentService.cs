using universal_payment_platform.Services.Interfaces;
using universal_payment_platform.Services.Interfaces.Models;
using universal_payment_platform.Services.ThirdPartyBankAdapters;

namespace universal_payment_platform.Services.Implementations
{
    public class PaymentService : IPaymentService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PaymentService> _logger;
        private readonly Dictionary<string, Type> _adapterTypes;

        public PaymentService(IServiceProvider serviceProvider, ILogger<PaymentService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;

            // Register available adapters
            _adapterTypes = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
            {
                { "Airtel", typeof(AirtelAdapter) },
                { "MTN", typeof(MTNAdapter) },
                { "FNB", typeof(FNBAdapter) },
                { "ABSA", typeof(ABSAAdapter) },
                { "Stanbic", typeof(StanbicAdapter) },
                { "Stanchart", typeof(StanchartAdapter) }
            };
        }

        public async Task<PaymentResponse> ProcessPaymentAsync(PaymentRequest request, string provider)
        {
            if (string.IsNullOrEmpty(provider))
                throw new ArgumentException("Provider cannot be null or empty", nameof(provider));

            var adapter = GetPaymentAdapter(provider);
            var integrationService = new BankIntegrationService(adapter,
                _serviceProvider.GetService<ILogger<BankIntegrationService>>());

            return await integrationService.ProcessPaymentAsync(request);
        }

        public async Task<PaymentResponse> GetPaymentStatusAsync(string transactionId, string provider)
        {
            if (string.IsNullOrEmpty(provider))
                throw new ArgumentException("Provider cannot be null or empty", nameof(provider));

            var adapter = GetPaymentAdapter(provider);
            var integrationService = new BankIntegrationService(adapter,
                _serviceProvider.GetService<ILogger<BankIntegrationService>>());

            return await integrationService.CheckPaymentStatusAsync(transactionId);
        }

        public Task<List<string>> GetSupportedProvidersAsync()
        {
            return Task.FromResult(_adapterTypes.Keys.ToList());
        }

        private IPaymentAdapter GetPaymentAdapter(string provider)
        {
            if (_adapterTypes.TryGetValue(provider, out var adapterType))
            {
                return (IPaymentAdapter)_serviceProvider.GetService(adapterType);
            }

            throw new NotSupportedException($"Payment provider '{provider}' is not supported");
        }
    }
}