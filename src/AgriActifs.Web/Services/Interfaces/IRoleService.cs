using AgriActifs.Web.Models.ViewModels;

namespace AgriActifs.Web.Services.Interfaces;

public interface IRoleService
{
    Task<IReadOnlyList<RoleListItemViewModel>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<RoleEditViewModel?> GetForEditAsync(string id, CancellationToken cancellationToken = default);
    Task<(bool Success, string? Error, string? CreatedRoleId)> CreateAsync(RoleEditViewModel model, CancellationToken cancellationToken = default);
    Task<(bool Success, string? Error)> UpdateAsync(RoleEditViewModel model, CancellationToken cancellationToken = default);
    Task<(bool Success, string? Error)> DeleteAsync(string id, CancellationToken cancellationToken = default);
    Task<UserRolesViewModel?> GetUserRolesAsync(string userId, CancellationToken cancellationToken = default);
    Task<(bool Success, string? Error)> UpdateUserRolesAsync(string userId, IEnumerable<string> selectedRoles, CancellationToken cancellationToken = default);
    Task<int> CountAsync(CancellationToken cancellationToken = default);
}
