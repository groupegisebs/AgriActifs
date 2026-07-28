using AgriActifs.Web.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgriActifs.Web.Extensions;

public static class AuthorizationExtensions
{
    public static IServiceCollection AddSecureAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization();
        services.AddScoped<EndpointAuthorizationFilter>();
        services.Configure<MvcOptions>(options =>
        {
            options.Filters.AddService<EndpointAuthorizationFilter>();
        });

        return services;
    }
}
