using AgriActifs.Web.Models.ViewModels;

namespace AgriActifs.Web.Services.Interfaces;

public interface ISystemSettingsService
{
    Task<SystemSettingsViewModel> GetAsync(CancellationToken cancellationToken = default);
    Task SaveAsync(SystemSettingsViewModel model, CancellationToken cancellationToken = default);
    Task<string?> SaveLogoAsync(IFormFile file, CancellationToken cancellationToken = default);
}
