using MediatR;
using universal_payment_platform.Services.Interfaces;

namespace universal_payment_platform.CQRS.Queries
{
    public class GetSupportedProvidersQueryHandler
        : IRequestHandler<GetSupportedProvidersQuery, IEnumerable<string>>
    {
        private readonly IEnumerable<IPaymentAdapter> _adapters;

        public GetSupportedProvidersQueryHandler(IEnumerable<IPaymentAdapter> adapters)
        {
            _adapters = adapters;
        }

        public Task<IEnumerable<string>> Handle(GetSupportedProvidersQuery request, CancellationToken cancellationToken)
        {
            var providers = _adapters.Select(a => a.GetAdapterName());
            return Task.FromResult(providers);
        }
    }
}
