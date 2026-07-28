namespace AgriActifs.Web.Models;

public class ReportDefinition
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? Description { get; set; }

    /// <summary>Code de la permission requise — FK vers PermissionDefinitions.Code.</summary>
    public string RequiredPermissionCode { get; set; } = string.Empty;

    public Models.Authorization.PermissionDefinition? RequiredPermission { get; set; }
    public bool IsActive { get; set; } = true;
}
