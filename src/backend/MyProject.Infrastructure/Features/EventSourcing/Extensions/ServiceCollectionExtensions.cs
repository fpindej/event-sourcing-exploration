using Microsoft.Extensions.DependencyInjection;
using MyProject.Application.EventSourcing;
using MyProject.Application.Features.EventSourcing;
using MyProject.Infrastructure.Features.EventSourcing.Services;

namespace MyProject.Infrastructure.Features.EventSourcing.Extensions;

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
