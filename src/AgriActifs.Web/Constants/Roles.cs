namespace AgriActifs.Web.Constants;

/// <summary>
/// Rôles Identity de la plateforme. SuperAdmin est toujours seedé ;
/// les autres rôles métier plateforme sont aussi créés pour les comptes démo.
/// </summary>
public static class AppRoles
{
    public const string SuperAdmin = "SuperAdmin";
    public const string Admin = "Admin";
    public const string Manager = "Manager";
    public const string User = "User";
    public const string Auditor = "Auditor";
    public const string ReportViewer = "ReportViewer";

    public static readonly IReadOnlyList<string> DefaultSeedRoles =
    [
        SuperAdmin,
        Admin,
        Manager,
        User,
        Auditor,
        ReportViewer
    ];
}
