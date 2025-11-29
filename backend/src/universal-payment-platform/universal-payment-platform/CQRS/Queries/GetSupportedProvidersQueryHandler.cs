using MediatR;
using universal_payment_platform.Services.Interfaces;

namespace universal_payment_platform.CQRS.Queries
{
    public class GetSupportedProvidersQueryHandler(IEnumerable<IPaymentAdapter> adapters)
                : IRequestHandler<GetSupportedProvidersQuery, IEnumerable<string>>
    {
        public Task<IEnumerable<string>> Handle(GetSupportedProvidersQuery request, CancellationToken cancellationToken)
        {
            var providers = adapters.Select(a => a.GetAdapterName());
            return Task.FromResult(providers);
        }
    }
}
