using System.ComponentModel.DataAnnotations;

namespace AgriActifs.Web.Models.ViewModels;

public class RoleListItemViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsSystemRole { get; set; }
    public int UserCount { get; set; }
}

public class RoleEditViewModel
{
    public string? Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
}
