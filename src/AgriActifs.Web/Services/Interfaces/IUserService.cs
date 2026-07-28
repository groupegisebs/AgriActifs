using AgriActifs.Web.Models.ViewModels;

namespace AgriActifs.Web.Services.Interfaces;

public interface IUserService
{
    Task<IReadOnlyList<UserListItemViewModel>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<UserEditViewModel?> GetForEditAsync(string id, CancellationToken cancellationToken = default);
    Task<(bool Success, string? Error)> CreateAsync(UserEditViewModel model, CancellationToken cancellationToken = default);
    Task<(bool Success, string? Error)> UpdateAsync(UserEditViewModel model, CancellationToken cancellationToken = default);
    Task<(bool Success, string? Error)> DeactivateAsync(string id, CancellationToken cancellationToken = default);
    Task<(bool Success, string? Error)> UnlockAsync(string id, CancellationToken cancellationToken = default);
    Task<int> CountAsync(CancellationToken cancellationToken = default);
    Task<int> CountLockedAsync(CancellationToken cancellationToken = default);
}
