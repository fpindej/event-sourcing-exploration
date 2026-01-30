using Microsoft.Extensions.DependencyInjection;
using EventSourcingExploration.Application.Cookies;

namespace EventSourcingExploration.Infrastructure.Cookies.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCookieServices(this IServiceCollection services)
    {
        services.AddScoped<ICookieService, CookieService>();
        return services;
    }
}
