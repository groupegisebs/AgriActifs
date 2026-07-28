namespace AgriActifs.Web.Services.Interfaces;

public interface ISecuredEndpointService
{
    Task<string?> GetRequiredPermissionCodeAsync(string? area, string controller, string action, string httpMethod, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SecuredEndpointListItem>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<SecuredEndpointEditModel?> GetForEditAsync(int id, CancellationToken cancellationToken = default);
    Task SaveAsync(SecuredEndpointEditModel model, CancellationToken cancellationToken = default);
    Task CreateAsync(SecuredEndpointEditModel model, CancellationToken cancellationToken = default);
    void InvalidateCache();
}

public record SecuredEndpointListItem(
    int Id,
    string? Area,
    string Controller,
    string Action,
    string? HttpMethod,
    string PermissionCode,
    string PermissionDisplayName,
    bool IsActive);

public class SecuredEndpointEditModel
{
    public int Id { get; set; }
    public string? Area { get; set; }
    public string Controller { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string? HttpMethod { get; set; }
    public int PermissionDefinitionId { get; set; }
    public bool IsActive { get; set; } = true;
}
