using AgriActifs.Web.Models.Authorization;
using AgriActifs.Web.Models.ViewModels;

namespace AgriActifs.Web.Services.Interfaces;

public interface IPermissionAdminService
{
    Task<PermissionMatrixViewModel> GetMatrixAsync(CancellationToken cancellationToken = default);
    Task<HabilitationMatrixViewModel> GetHabilitationMatrixAsync(CancellationToken cancellationToken = default);
    Task<ModelPermissionViewModel> GetModelPermissionsAsync(string resource, CancellationToken cancellationToken = default);
    Task SaveRoleGrantsAsync(string roleId, IEnumerable<int> grantedPermissionIds, CancellationToken cancellationToken = default);
    Task SaveHabilitationMatrixAsync(IEnumerable<string> grantTokens, CancellationToken cancellationToken = default);
    Task EnsureSuperAdminGrantsAsync(CancellationToken cancellationToken = default);
    /// <summary>Accorde toutes les permissions actives d'une catégorie à un rôle (ex. User + Ferme).</summary>
    Task EnsureRoleCategoryGrantsAsync(string roleName, string category, CancellationToken cancellationToken = default);
    /// <summary>Accorde une liste explicite de codes permission à un rôle (idempotent).</summary>
    Task EnsureRolePermissionCodesAsync(string roleName, IEnumerable<string> permissionCodes, CancellationToken cancellationToken = default);
}
