namespace AgriActifs.Web.Models;

public class ThemeDefinition
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    /// <summary>Variables CSS sérialisées en JSON (ex. {"--gise-primary":"#1e40af"}).</summary>
    public string CssVariables { get; set; } = "{}";

    public bool IsSystem { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
