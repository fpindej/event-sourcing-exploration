using Microsoft.Extensions.DependencyInjection;
using EventSourcingExploration.Application.EventSourcing;
using EventSourcingExploration.Application.Features.EventSourcing;
using EventSourcingExploration.Infrastructure.Features.EventSourcing.Services;

namespace EventSourcingExploration.Infrastructure.Features.EventSourcing.Extensions;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddEventSourcing()
        {
            services.AddScoped<IEventStore, EventStore>();
            services.AddScoped<IBankAccountService, BankAccountService>();
            return services;
        }
    }
}
