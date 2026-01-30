using Microsoft.Extensions.DependencyInjection;
using EventSourcingExploration.Application.Identity;
using EventSourcingExploration.Infrastructure.Identity.Services;

namespace EventSourcingExploration.Infrastructure.Identity.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUserContext(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<IUserContext, UserContext>();
        services.AddScoped<IUserService, UserService>();
        return services;
    }
}
