using System.Globalization;
using AgriActifs.Web.Data;
using AgriActifs.Web.Services.Interfaces;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace AgriActifs.Web.Middleware;

public class GiseCultureMiddleware(RequestDelegate next)
{
    public const string CultureCookieName = "Gise.Culture";

    public async Task InvokeAsync(
        HttpContext context,
        IAppContextService appContextService,
        ApplicationDbContext dbContext)
    {
        var culture = await ResolveCultureAsync(context, appContextService, dbContext);
        var cultureInfo = CultureInfo.GetCultureInfo(culture);
        CultureInfo.CurrentCulture = cultureInfo;
        CultureInfo.CurrentUICulture = cultureInfo;
        await next(context);
    }

    private static async Task<string> ResolveCultureAsync(
        HttpContext context,
        IAppContextService appContextService,
        ApplicationDbContext dbContext)
    {
        if (context.Request.Query.TryGetValue("culture", out var queryCulture) && !string.IsNullOrWhiteSpace(queryCulture))
        {
            var c = queryCulture.ToString();
            context.Response.Cookies.Append(CultureCookieName, c, new CookieOptions
            {
                HttpOnly = false,
                IsEssential = true,
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                SameSite = SameSiteMode.Lax
            });
            return c;
        }

        if (context.Request.Cookies.TryGetValue(CultureCookieName, out var cookieCulture) && !string.IsNullOrWhiteSpace(cookieCulture))
            return cookieCulture;

        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userId = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrWhiteSpace(userId))
            {
                var profileLang = await dbContext.UserProfiles.AsNoTracking()
                    .Where(p => p.UserId == userId)
                    .Select(p => p.PreferredLanguage)
                    .FirstOrDefaultAsync();
                if (!string.IsNullOrWhiteSpace(profileLang))
                    return profileLang;
            }
        }

        var snapshot = await appContextService.GetSnapshotAsync();
        return snapshot.DefaultCulture;
    }
}

public static class LocalizationExtensions
{
    public static IServiceCollection AddGiseLocalization(this IServiceCollection services)
    {
        services.AddLocalization();
        services.AddSingleton<Localization.IYamlLocalizationProvider, Localization.YamlLocalizationProvider>();
        services.AddSingleton<IStringLocalizerFactory, Localization.YamlStringLocalizerFactory>();
        services.AddSingleton<IHtmlLocalizerFactory, Localization.YamlViewLocalizerFactory>();

        services.Configure<RequestLocalizationOptions>(options =>
        {
            var cultures = new[] { "fr-FR", "en-US" }.Select(c => new CultureInfo(c)).ToList();
            options.DefaultRequestCulture = new RequestCulture("fr-FR");
            options.SupportedCultures = cultures;
            options.SupportedUICultures = cultures;
        });

        return services;
    }

    public static IApplicationBuilder UseGiseLocalization(this IApplicationBuilder app)
    {
        app.UseMiddleware<GiseCultureMiddleware>();
        app.UseRequestLocalization();
        return app;
    }
}
