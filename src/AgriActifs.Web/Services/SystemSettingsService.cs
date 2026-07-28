using AgriActifs.Web.Configuration;
using AgriActifs.Web.Models.ViewModels;
using AgriActifs.Web.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AgriActifs.Web.Services;

public class SystemSettingsService(
    Data.ApplicationDbContext dbContext,
    IAuditService auditService,
    IAppContextService appContextService,
    IConfiguration configuration) : ISystemSettingsService
{
    public async Task<SystemSettingsViewModel> GetAsync(CancellationToken cancellationToken = default)
    {
        var settings = await dbContext.SystemSettings.AsNoTracking().FirstAsync(cancellationToken);
        var themes = await dbContext.ThemeDefinitions.AsNoTracking()
            .Where(t => t.IsActive)
            .OrderBy(t => t.Name)
            .Select(t => new ThemeListItemViewModel { Id = t.Id, Name = t.Name, Code = t.Code, IsSystem = t.IsSystem })
            .ToListAsync(cancellationToken);

        var dbOptions = configuration.GetSection(DatabaseOptions.SectionName).Get<DatabaseOptions>() ?? new DatabaseOptions();
        var conn = configuration.GetConnectionString("DefaultConnection") ?? string.Empty;

        return new SystemSettingsViewModel
        {
            AppName = settings.AppName,
            Tagline = settings.Tagline,
            LogoUrl = settings.LogoUrl,
            ActiveThemeId = settings.ActiveThemeId,
            DefaultCulture = settings.DefaultCulture,
            AvailableThemes = themes,
            AvailableCultures = ["fr-FR", "en-US"],
            DatabaseProvider = dbOptions.Provider,
            DatabaseSchema = dbOptions.Schema,
            DatabaseHost = MaskConnectionHost(conn),
            SmtpHost = settings.SmtpHost,
            SmtpPort = settings.SmtpPort,
            SmtpUser = settings.SmtpUser,
            SmtpUseSsl = settings.SmtpUseSsl,
            RequireConfirmedEmail = settings.RequireConfirmedEmail,
            RequireTwoFactor = settings.RequireTwoFactor,
            SessionTimeoutMinutes = settings.SessionTimeoutMinutes,
            MaxFailedAccessAttempts = settings.MaxFailedAccessAttempts,
            LockoutMinutes = settings.LockoutMinutes
        };
    }

    public async Task SaveAsync(SystemSettingsViewModel model, CancellationToken cancellationToken = default)
    {
        var settings = await dbContext.SystemSettings.FirstAsync(cancellationToken);

        settings.AppName = model.AppName;
        settings.Tagline = model.Tagline;
        settings.LogoUrl = model.LogoUrl;
        settings.ActiveThemeId = model.ActiveThemeId;
        settings.DefaultCulture = model.DefaultCulture;
        settings.SmtpHost = model.SmtpHost;
        settings.SmtpPort = model.SmtpPort;
        settings.SmtpUser = model.SmtpUser;
        settings.SmtpUseSsl = model.SmtpUseSsl;
        settings.RequireConfirmedEmail = model.RequireConfirmedEmail;
        settings.RequireTwoFactor = model.RequireTwoFactor;
        settings.SessionTimeoutMinutes = model.SessionTimeoutMinutes;
        settings.MaxFailedAccessAttempts = model.MaxFailedAccessAttempts;
        settings.LockoutMinutes = model.LockoutMinutes;
        settings.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        appContextService.InvalidateCache();
        await auditService.LogAsync("Update", "SystemSettings", settings.Id.ToString(), cancellationToken: cancellationToken);
    }

    public async Task<string?> SaveLogoAsync(IFormFile file, CancellationToken cancellationToken = default)
    {
        if (file.Length == 0)
            return null;

        var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".png", ".jpg", ".jpeg", ".svg", ".webp" };
        var ext = Path.GetExtension(file.FileName);
        if (!allowed.Contains(ext))
            throw new InvalidOperationException("Format de logo non supporté.");

        var dir = Path.Combine("wwwroot", "uploads", "branding");
        Directory.CreateDirectory(dir);
        var fileName = $"logo{ext.ToLowerInvariant()}";
        var path = Path.Combine(dir, fileName);

        await using var stream = File.Create(path);
        await file.CopyToAsync(stream, cancellationToken);

        var url = $"/uploads/branding/{fileName}";
        var settings = await dbContext.SystemSettings.FirstAsync(cancellationToken);
        settings.LogoUrl = url;
        settings.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        appContextService.InvalidateCache();
        return url;
    }

    private static string MaskConnectionHost(string connectionString)
    {
        var parts = connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries);
        foreach (var part in parts)
        {
            if (part.StartsWith("Host=", StringComparison.OrdinalIgnoreCase) ||
                part.StartsWith("Server=", StringComparison.OrdinalIgnoreCase))
                return part.Split('=')[1];
        }
        return "(configuré)";
    }
}
