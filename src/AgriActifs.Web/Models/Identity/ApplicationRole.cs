using Microsoft.AspNetCore.Identity;

namespace AgriActifs.Web.Models.Identity;

public class ApplicationRole : IdentityRole
{
    public string? Description { get; set; }
    public bool IsSystemRole { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
