using MediatR;
using System.Collections.Generic;

namespace universal_payment_platform.CQRS.Queries
{
    public class GetSupportedProvidersQuery : IRequest<IEnumerable<string>>
    {
    }
}
