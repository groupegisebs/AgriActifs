namespace AgriActifs.Web.Configuration;

public class DatabaseOptions
{
    public const string SectionName = "Database";

    /// <summary>PostgreSQL (défaut) ou SqlServer.</summary>
    public string Provider { get; set; } = "PostgreSQL";

    /// <summary>Schéma PostgreSQL / SQL Server (ex. agriactifs, dbo).</summary>
    public string Schema { get; set; } = "public";
}
