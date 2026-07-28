namespace AgriActifs.Web.Models.Authorization;

/// <summary>
/// Mapping configurable en BD : endpoint MVC → permission requise.
/// </summary>
public class SecuredEndpoint
{
    public int Id { get; set; }

    public string? Area { get; set; }
    public string Controller { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;

    /// <summary>Null = toute méthode HTTP.</summary>
    public string? HttpMethod { get; set; }

    public int PermissionDefinitionId { get; set; }
    public bool IsActive { get; set; } = true;

    public PermissionDefinition? Permission { get; set; }
}
