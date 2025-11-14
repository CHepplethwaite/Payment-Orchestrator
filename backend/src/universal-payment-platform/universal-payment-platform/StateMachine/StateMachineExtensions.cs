using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using universal_payment_platform.StateMachine.Core;
using universal_payment_platform.StateMachine.Services;

namespace universal_payment_platform.StateMachine
{
    public static class StateMachineExtensions
    {
        public static IServiceCollection AddPaymentStateMachine(this IServiceCollection services)
        {
            services.TryAddScoped<IPaymentStateService, PaymentStateService>();

            // Register state machine factory if needed
            services.TryAddSingleton<IStateMachineFactory, StateMachineFactory>();

            return services;
        }
    }

    public interface IStateMachineFactory
    {
        IStateMachine<T> CreateStateMachine<T>(string name) where T : class;
    }

    public class StateMachineFactory : IStateMachineFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public StateMachineFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IStateMachine<T> CreateStateMachine<T>(string name) where T : class
        {
            return new StateMachine<T>(name);
        }
    }
}