using System.ComponentModel.DataAnnotations;

namespace AgriActifs.Web.Models.ViewModels;

public class UserListItemViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public bool IsActive { get; set; }
    public bool IsLockedOut { get; set; }
    public IReadOnlyList<string> Roles { get; set; } = [];
}

public class UserEditViewModel
{
    public string? Id { get; set; }

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Prénom")]
    public string? FirstName { get; set; }

    [Display(Name = "Nom")]
    public string? LastName { get; set; }

    [Phone]
    public string? PhoneNumber { get; set; }

    public bool IsActive { get; set; } = true;

    [DataType(DataType.Password)]
    public string? Password { get; set; }
}

public class UserRolesViewModel
{
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public IList<RoleSelectionViewModel> Roles { get; set; } = [];
}

public class RoleSelectionViewModel
{
    public string RoleName { get; set; } = string.Empty;
    public bool IsSelected { get; set; }
}
