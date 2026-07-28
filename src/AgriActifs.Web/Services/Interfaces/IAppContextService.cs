using AgriActifs.Web.Models;

namespace AgriActifs.Web.Services.Interfaces;

public interface IAppContextService
{
    Task<AppContextSnapshot> GetSnapshotAsync(CancellationToken cancellationToken = default);
    void InvalidateCache();
}
