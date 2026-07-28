namespace AgriActifs.Web.Constants;

/// <summary>
/// Rôles Identity plateforme + rôles métier ferme (démo / exploitation).
/// </summary>
public static class AppRoles
{
    public const string SuperAdmin = "SuperAdmin";
    public const string Admin = "Admin";
    public const string Manager = "Manager";
    public const string User = "User";
    public const string Auditor = "Auditor";
    public const string ReportViewer = "ReportViewer";

    // Rôles métier ferme (comptes démo distincts)
    public const string Gerant = "Gerant";
    public const string Technicien = "Technicien";
    public const string Ouvrier = "Ouvrier";
    public const string Observateur = "Observateur";

    public static readonly IReadOnlyList<string> DefaultSeedRoles =
    [
        SuperAdmin,
        Admin,
        Manager,
        User,
        Auditor,
        ReportViewer,
        Gerant,
        Technicien,
        Ouvrier,
        Observateur
    ];
}
