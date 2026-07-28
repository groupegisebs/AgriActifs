using AgriActifs.Web.Models.Identity;
using AgriActifs.Web.Services.Email;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace AgriActifs.Web.Extensions;

public static class IdentityExtensions
{
    public static IServiceCollection AddSecureIdentity(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
        {
            options.SignIn.RequireConfirmedAccount = configuration.GetValue("Security:RequireConfirmedEmail", true);
            options.User.RequireUniqueEmail = true;

            options.Password.RequiredLength = 12;
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;

            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(
                configuration.GetValue("Security:LockoutMinutes", 15));
            options.Lockout.MaxFailedAccessAttempts = configuration.GetValue("Security:MaxFailedAccessAttempts", 5);
            options.Lockout.AllowedForNewUsers = true;
        })
        .AddEntityFrameworkStores<Data.ApplicationDbContext>()
        .AddDefaultTokenProviders()
        .AddDefaultUI();

        services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/Identity/Account/Login";
            options.AccessDeniedPath = "/Identity/Account/AccessDenied";
            options.SlidingExpiration = true;
            options.ExpireTimeSpan = TimeSpan.FromMinutes(
                configuration.GetValue("Security:SessionTimeoutMinutes", 30));
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.Strict;
        });

        services.Configure<DataProtectionTokenProviderOptions>(options =>
        {
            options.TokenLifespan = TimeSpan.FromHours(
                configuration.GetValue("Security:PasswordResetTokenHours", 24));
        });

        return services;
    }

    public static IServiceCollection AddEmailServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AuthMessageSenderOptions>(configuration.GetSection(AuthMessageSenderOptions.SectionName));
        services.AddTransient<IEmailSender, EmailSender>();
        return services;
    }
}

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddHttpContextAccessor();
        services.AddScoped<Services.Interfaces.IAuditService, Services.AuditService>();
        services.AddScoped<Services.Interfaces.IUserService, Services.UserService>();
        services.AddScoped<Services.Interfaces.IRoleService, Services.RoleService>();
        services.AddScoped<Services.Interfaces.IReportService, Services.ReportService>();
        services.AddScoped<Services.Interfaces.IDynamicPermissionService, Services.DynamicPermissionService>();
        services.AddScoped<Services.Interfaces.IPermissionAdminService, Services.PermissionAdminService>();
        services.AddScoped<Services.Interfaces.ISecuredEndpointService, Services.SecuredEndpointService>();
        services.AddScoped<Services.Interfaces.IMvcEndpointScanner, Services.MvcEndpointScanner>();
        services.AddScoped<Services.Interfaces.ISystemSettingsService, Services.SystemSettingsService>();
        services.AddScoped<Services.Interfaces.IAppContextService, Services.AppContextService>();
        services.AddScoped<Services.Interfaces.IThemeService, Services.ThemeService>();
        services.AddScoped<Services.Interfaces.IExploitationContextService, Services.ExploitationContextService>();

        return services;
    }
}
