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
}
