using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using EventSourcingExploration.Application.Identity;

namespace EventSourcingExploration.Infrastructure.Identity;

internal class UserContext(IHttpContextAccessor httpContextAccessor) : IUserContext
{
    public Guid? UserId
    {
        get
        {
            var value = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (Guid.TryParse(value, out var guid))
            {
                return guid;
            }

            return null;
        }
    }

    public string? Email => GetClaimValue(ClaimTypes.Email, x => x);

    public string? UserName => GetClaimValue(ClaimTypes.Name, x => x);

    public bool IsAuthenticated => UserId.HasValue;

    public bool IsInRole(string role)
    {
        return httpContextAccessor.HttpContext?.User.IsInRole(role) ?? false;
    }

    private T? GetClaimValue<T>(string claimType, Func<string, T> converter)
    {
        var value = httpContextAccessor.HttpContext?.User.FindFirst(claimType)?.Value;

        if (value is not null)
        {
            return converter(value);
        }

        return default;
    }
}
