namespace AgriActifs.Web.Models.Authorization;

public enum PermissionAction
{
    View,
    Create,
    Edit,
    Delete,
    Export,
    Manage
}

public class PermissionDefinition
{
    public int Id { get; set; }

    /// <summary>Code unique ex: Users.View, User.Email.Edit</summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>Modèle / ressource ex: Users, User, Role</summary>
    public string Resource { get; set; } = string.Empty;

    public PermissionAction Action { get; set; }

    /// <summary>Propriété du modèle (null = permission au niveau entité)</summary>
    public string? PropertyName { get; set; }

    public string DisplayName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public bool IsSystem { get; set; } = true;
    public bool IsActive { get; set; } = true;

    public ICollection<RolePermissionGrant> RoleGrants { get; set; } = [];
}

public class RolePermissionGrant
{
    public int Id { get; set; }
    public string RoleId { get; set; } = string.Empty;
    public int PermissionDefinitionId { get; set; }
    public bool IsGranted { get; set; } = true;

    public PermissionDefinition? Permission { get; set; }
    public Identity.ApplicationRole? Role { get; set; }
}
