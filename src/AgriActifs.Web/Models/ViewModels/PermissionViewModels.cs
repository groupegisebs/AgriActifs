namespace AgriActifs.Web.Models.ViewModels;

public class PermissionMatrixViewModel
{
    public IReadOnlyList<RolePermissionColumnViewModel> Roles { get; set; } = [];
    public IReadOnlyList<PermissionRowViewModel> Permissions { get; set; } = [];
    public IReadOnlyList<PermissionCategoryGroupViewModel> Categories { get; set; } = [];
}

public class RolePermissionColumnViewModel
{
    public string RoleId { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public bool IsSystemRole { get; set; }
}

public class PermissionRowViewModel
{
    public int PermissionId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Resource { get; set; } = string.Empty;
    public string? PropertyName { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public bool IsEntityLevel => PropertyName is null;
    public Dictionary<string, bool> GrantsByRoleId { get; set; } = new();
}

public class PermissionCategoryGroupViewModel
{
    public string Category { get; set; } = string.Empty;
    public IReadOnlyList<PermissionRowViewModel> Permissions { get; set; } = [];
}

public class ModelPermissionViewModel
{
    public string Resource { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public IReadOnlyList<RolePermissionColumnViewModel> Roles { get; set; } = [];
    public IReadOnlyList<ModelPropertyPermissionRowViewModel> EntityActions { get; set; } = [];
    public IReadOnlyList<ModelPropertyPermissionRowViewModel> Properties { get; set; } = [];
}

public class ModelPropertyPermissionRowViewModel
{
    public int PermissionId { get; set; }
    public string Label { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public Dictionary<string, bool> GrantsByRoleId { get; set; } = new();
}

public class SaveRolePermissionsViewModel
{
    public string RoleId { get; set; } = string.Empty;
    public List<int> GrantedPermissionIds { get; set; } = [];
}

public class HabilitationMatrixViewModel
{
    public IReadOnlyList<RolePermissionColumnViewModel> AllRoles { get; set; } = [];
    public IReadOnlyList<RolePermissionColumnViewModel> EditableRoles { get; set; } = [];
    public IReadOnlyList<HabilitationModuleGroupViewModel> Modules { get; set; } = [];
    public IReadOnlyList<ControllerHabilitationGroupViewModel> Controllers { get; set; } = [];
    public IReadOnlyList<ModelHabilitationGroupViewModel> Models { get; set; } = [];
}

public class HabilitationModuleGroupViewModel
{
    public string Category { get; set; } = string.Empty;
    public string ModuleTitle { get; set; } = string.Empty;
    public string ModuleSubtitle { get; set; } = string.Empty;
    public IReadOnlyList<HabilitationActionRowViewModel> Actions { get; set; } = [];
}

public class ControllerHabilitationGroupViewModel
{
    public string Resource { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public IReadOnlyList<HabilitationActionRowViewModel> Actions { get; set; } = [];
}

public class ModelHabilitationGroupViewModel
{
    public string Resource { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public IReadOnlyList<ModelPropertyHabilitationRowViewModel> Properties { get; set; } = [];
}

public class HabilitationActionRowViewModel
{
    public int PermissionId { get; set; }
    public string Resource { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string ActionLabel { get; set; } = string.Empty;
    public Dictionary<string, bool> GrantsByRoleId { get; set; } = new();
}

public class ModelPropertyHabilitationRowViewModel
{
    public string PropertyName { get; set; } = string.Empty;
    public HabilitationActionRowViewModel? View { get; set; }
    public HabilitationActionRowViewModel? Edit { get; set; }
}

public class SaveHabilitationMatrixViewModel
{
    /// <summary>Format: roleId|permissionId</summary>
    public List<string> Grants { get; set; } = [];
}
