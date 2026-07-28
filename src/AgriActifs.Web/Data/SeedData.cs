using AgriActifs.Web.Configuration;
using AgriActifs.Web.Constants;
using AgriActifs.Web.Extensions;
using AgriActifs.Web.Models.Authorization;
using AgriActifs.Web.Models.Ferme;
using AgriActifs.Web.Models.Identity;
using AgriActifs.Web.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AgriActifs.Web.Data;

public static class SeedData
{
    private const string SuperAdminEmail = "superadmin@agriactifs.local";
    private const string SuperAdminPassword = "Agri@Secure2026!";
    private const string AdminEmail = "admin@agriactifs.local";
    private const string AdminPassword = "Agri@Admin2026!";
    private const string DemoPassword = "Demo@Agri2026!";

    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var provider = scope.ServiceProvider;

        var context = provider.GetRequiredService<ApplicationDbContext>();
        var dbOptions = provider.GetRequiredService<IOptions<DatabaseOptions>>().Value;
        var configuration = provider.GetRequiredService<IConfiguration>();

        await EnsureSchemaAsync(context, dbOptions);
        await context.Database.MigrateAsync();

        var roleManager = provider.GetRequiredService<RoleManager<ApplicationRole>>();
        var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();
        var permissionAdmin = provider.GetRequiredService<IPermissionAdminService>();

        await SeedRolesAsync(roleManager);
        await EnsureCatalogAsync(context);
        await SeedSuperAdminAsync(userManager);
        await EnsureUserAsync(userManager, AdminEmail, AdminPassword, "Admin", "Agri", AppRoles.Admin);
        await permissionAdmin.EnsureSuperAdminGrantsAsync();

        if (configuration.GetValue("Seed:IncludeDemoData", true))
        {
            await SeedDemoAsync(context, userManager);
            await EnsureDemoPatrimoineAsync(context);
            await EnsureDemoPhase2Async(context);
            await EnsureDemoPhase34Async(context);
        }
    }

    private static async Task SeedRolesAsync(RoleManager<ApplicationRole> roleManager)
    {
        foreach (var roleName in AppRoles.DefaultSeedRoles)
        {
            if (await roleManager.RoleExistsAsync(roleName))
                continue;

            await roleManager.CreateAsync(new ApplicationRole
            {
                Name = roleName,
                Description = "Rôle fondateur — seul rôle créé automatiquement. Les autres rôles se créent via Admin > Rôles.",
                IsSystemRole = true
            });
        }
    }

    private static async Task SeedSuperAdminAsync(UserManager<ApplicationUser> userManager) =>
        await EnsureUserAsync(userManager, SuperAdminEmail, SuperAdminPassword, "Super", "Admin", AppRoles.SuperAdmin);

    private static async Task EnsureUserAsync(
        UserManager<ApplicationUser> userManager,
        string email,
        string password,
        string firstName,
        string lastName,
        string role)
    {
        if (await userManager.FindByEmailAsync(email) is not null)
            return;

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FirstName = firstName,
            LastName = lastName,
            IsActive = true
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));

        await userManager.AddToRoleAsync(user, role);
    }

    public static async Task EnsureCatalogAsync(ApplicationDbContext context)
    {
        var schema = SqlIdent(context.Schema);
        var permissions = $"{schema}.\"PermissionDefinitions\"";
        var endpoints = $"{schema}.\"SecuredEndpoints\"";
        var reports = $"{schema}.\"ReportDefinitions\"";

        // INSERT … WHERE NOT EXISTS : idempotent, évite 23505 même avec doublons catalogue.
        foreach (var permission in CatalogSeedData.Permissions)
        {
            var propertyName = permission.PropertyName is null
                ? "NULL"
                : $"'{SqlLiteral(permission.PropertyName)}'";

#pragma warning disable EF1002
            await context.Database.ExecuteSqlRawAsync($"""
                INSERT INTO {permissions} ("Code", "Resource", "Action", "PropertyName", "DisplayName", "Category", "IsSystem", "IsActive")
                SELECT '{SqlLiteral(permission.Code)}', '{SqlLiteral(permission.Resource)}', {(int)permission.Action}, {propertyName}, '{SqlLiteral(permission.DisplayName)}', '{SqlLiteral(permission.Category)}', TRUE, TRUE
                WHERE NOT EXISTS (SELECT 1 FROM {permissions} WHERE "Code" = '{SqlLiteral(permission.Code)}');
                """);
#pragma warning restore EF1002
        }

        var catalogEndpoints = CatalogSeedData.Endpoints
            .GroupBy(e => $"{e.Area}|{e.Controller}|{e.Action}|{e.HttpMethod}", StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First());

        foreach (var endpoint in catalogEndpoints)
        {
            var area = endpoint.Area is null ? "NULL" : $"'{SqlLiteral(endpoint.Area)}'";
            var httpMethod = endpoint.HttpMethod is null ? "NULL" : $"'{SqlLiteral(endpoint.HttpMethod)}'";
            var areaMatch = endpoint.Area is null
                ? "e.\"Area\" IS NULL"
                : $"e.\"Area\" = '{SqlLiteral(endpoint.Area)}'";
            var httpMatch = endpoint.HttpMethod is null
                ? "e.\"HttpMethod\" IS NULL"
                : $"e.\"HttpMethod\" = '{SqlLiteral(endpoint.HttpMethod)}'";

#pragma warning disable EF1002
            await context.Database.ExecuteSqlRawAsync($"""
                INSERT INTO {endpoints} ("Area", "Controller", "Action", "HttpMethod", "PermissionDefinitionId", "IsActive")
                SELECT {area}, '{SqlLiteral(endpoint.Controller)}', '{SqlLiteral(endpoint.Action)}', {httpMethod}, p."Id", TRUE
                FROM {permissions} p
                WHERE p."Code" = '{SqlLiteral(endpoint.PermissionCode)}'
                AND NOT EXISTS (
                    SELECT 1 FROM {endpoints} e
                    WHERE {areaMatch}
                      AND e."Controller" = '{SqlLiteral(endpoint.Controller)}'
                      AND e."Action" = '{SqlLiteral(endpoint.Action)}'
                      AND {httpMatch});
                """);
#pragma warning restore EF1002
        }

        foreach (var report in CatalogSeedData.Reports)
        {
#pragma warning disable EF1002
            await context.Database.ExecuteSqlRawAsync($"""
                INSERT INTO {reports} ("Code", "Name", "Category", "RequiredPermissionCode", "IsActive")
                SELECT '{SqlLiteral(report.Code)}', '{SqlLiteral(report.Name)}', '{SqlLiteral(report.Category)}', '{SqlLiteral(report.RequiredPermissionCode)}', TRUE
                WHERE NOT EXISTS (SELECT 1 FROM {reports} WHERE "Code" = '{SqlLiteral(report.Code)}');
                """);
#pragma warning restore EF1002
        }
    }

    private static string SqlIdent(string value) => $"\"{value.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";

    private static string SqlLiteral(string value) => value.Replace("'", "''", StringComparison.Ordinal);

    private static async Task SeedDemoAsync(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        if (await db.Exploitations.AnyAsync(e => e.Name == "Ferme des Érables"))
            return;

        var exploitation = new Exploitation
        {
            Name = "Ferme des Érables",
            Address = "120 Rang des Érables",
            City = "Saint-Hyacinthe",
            Province = "QC",
            PostalCode = "J2S 0A1",
            TotalAreaHa = 80,
            ProductionType = "Grandes cultures",
            Email = "info@fermedeserables.demo"
        };
        db.Exploitations.Add(exploitation);
        await db.SaveChangesAsync();

        var demoUsers = new (string Email, string First, string Last, ExploitationUserRole Role)[]
        {
            ("gerant@fermedeserables.demo", "Marie", "Gérant", ExploitationUserRole.Gerant),
            ("tech@fermedeserables.demo", "Luc", "Technicien", ExploitationUserRole.Technicien),
            ("ouvrier@fermedeserables.demo", "Jean", "Ouvrier", ExploitationUserRole.Ouvrier),
            ("lecture@fermedeserables.demo", "Claire", "Lecture", ExploitationUserRole.Observateur)
        };

        foreach (var (email, first, last, role) in demoUsers)
        {
            await EnsureUserAsync(userManager, email, DemoPassword, first, last, AppRoles.User);
            var user = await userManager.FindByEmailAsync(email);
            if (user is null) continue;
            db.ExploitationUsers.Add(new ExploitationUser
            {
                ExploitationId = exploitation.Id,
                UserId = user.Id,
                Role = role
            });
        }

        var super = await userManager.FindByEmailAsync(SuperAdminEmail);
        if (super is not null)
        {
            db.ExploitationUsers.Add(new ExploitationUser
            {
                ExploitationId = exploitation.Id,
                UserId = super.Id,
                Role = ExploitationUserRole.Proprietaire
            });
        }

        var parcelles = new[]
        {
            new Parcelle
            {
                ExploitationId = exploitation.Id, Code = "P01", Name = "Champ Nord", AreaHa = 18, SoilType = "Loam",
                HasIrrigation = true, CurrentCulture = "Maïs", PreviousCulture = "Soya", PlannedCulture = "Blé",
                EstimatedYieldPerHa = 8.5m, ActualYieldPerHa = 8.2m, ResponsibleName = "Jean",
                Etat = ParcelleEtat.EnProduction, SowingDate = new DateTime(2026, 5, 10)
            },
            new Parcelle
            {
                ExploitationId = exploitation.Id, Code = "P02", Name = "Champ Sud", AreaHa = 22, SoilType = "Argile",
                HasIrrigation = false, CurrentCulture = "Soya", PreviousCulture = "Maïs",
                EstimatedYieldPerHa = 3.2m, ResponsibleName = "Marie", Etat = ParcelleEtat.EnProduction,
                SowingDate = new DateTime(2026, 5, 18)
            },
            new Parcelle
            {
                ExploitationId = exploitation.Id, Code = "P03", Name = "Vallée", AreaHa = 15, SoilType = "Sableux",
                HasIrrigation = true, CurrentCulture = "Blé", EstimatedYieldPerHa = 4.5m,
                ResponsibleName = "Luc", Etat = ParcelleEtat.EnProduction
            },
            new Parcelle
            {
                ExploitationId = exploitation.Id, Code = "P04", Name = "Plateau", AreaHa = 12, SoilType = "Loam",
                HasIrrigation = false, CurrentCulture = null, PlannedCulture = "Maïs",
                Etat = ParcelleEtat.EnPreparation, ResponsibleName = "Jean"
            },
            new Parcelle
            {
                ExploitationId = exploitation.Id, Code = "P05", Name = "Bordure", AreaHa = 13, SoilType = "Limoneux",
                HasIrrigation = true, CurrentCulture = "Fourrage", Etat = ParcelleEtat.EnProduction,
                ResponsibleName = "Claire"
            }
        };
        db.Parcelles.AddRange(parcelles);
        await db.SaveChangesAsync();

        db.Assolements.AddRange(
            new Assolement { ParcelleId = parcelles[0].Id, Season = "2026", Culture = "Maïs", YieldPerHa = 9.2m },
            new Assolement { ParcelleId = parcelles[1].Id, Season = "2026", Culture = "Soya", YieldPerHa = 3.1m },
            new Assolement { ParcelleId = parcelles[2].Id, Season = "2026", Culture = "Blé", YieldPerHa = 4.5m });

        await SeedActifsForExploitationAsync(db, exploitation.Id, parcelles);

        db.StockArticles.AddRange(
            new StockArticle { ExploitationId = exploitation.Id, Sku = "SEM-MAIS", Name = "Semences maïs P9870", Categorie = StockCategorie.Semences, Unit = "sac", QuantityOnHand = 12, ReorderLevel = 5, UnitCost = 320 },
            new StockArticle { ExploitationId = exploitation.Id, Sku = "SEM-SOYA", Name = "Semences soya", Categorie = StockCategorie.Semences, Unit = "sac", QuantityOnHand = 8, ReorderLevel = 4, UnitCost = 85 },
            new StockArticle { ExploitationId = exploitation.Id, Sku = "ENG-UREE", Name = "Urée 46-0-0", Categorie = StockCategorie.Engrais, Unit = "t", QuantityOnHand = 4, ReorderLevel = 2, UnitCost = 780 },
            new StockArticle { ExploitationId = exploitation.Id, Sku = "ENG-NPK", Name = "NPK 15-15-15", Categorie = StockCategorie.Engrais, Unit = "t", QuantityOnHand = 1.5m, ReorderLevel = 2, UnitCost = 920 },
            new StockArticle { ExploitationId = exploitation.Id, Sku = "PHY-GLY", Name = "Glyphosate", Categorie = StockCategorie.Phyto, Unit = "L", QuantityOnHand = 40, ReorderLevel = 20, UnitCost = 12, ExpirationDate = new DateTime(2027, 6, 1) },
            new StockArticle { ExploitationId = exploitation.Id, Sku = "PHY-FON", Name = "Fongicide", Categorie = StockCategorie.Phyto, Unit = "L", QuantityOnHand = 8, ReorderLevel = 10, UnitCost = 45 },
            new StockArticle { ExploitationId = exploitation.Id, Sku = "REC-MAIS", Name = "Maïs récolté", Categorie = StockCategorie.Recolte, Unit = "t", QuantityOnHand = 120, ReorderLevel = 0, UnitCost = 220 },
            new StockArticle { ExploitationId = exploitation.Id, Sku = "PIE-FIL", Name = "Filtres hydrauliques", Categorie = StockCategorie.Pieces, Unit = "u", QuantityOnHand = 6, ReorderLevel = 4, UnitCost = 35 },
            new StockArticle { ExploitationId = exploitation.Id, Sku = "PIE-HUI", Name = "Huile moteur 15W40", Categorie = StockCategorie.Pieces, Unit = "L", QuantityOnHand = 60, ReorderLevel = 25, UnitCost = 8 },
            new StockArticle { ExploitationId = exploitation.Id, Sku = "SEM-BLE", Name = "Semences blé", Categorie = StockCategorie.Semences, Unit = "sac", QuantityOnHand = 3, ReorderLevel = 5, UnitCost = 28 },
            new StockArticle { ExploitationId = exploitation.Id, Sku = "ENG-POT", Name = "Potasse", Categorie = StockCategorie.Engrais, Unit = "t", QuantityOnHand = 2, ReorderLevel = 1, UnitCost = 650 },
            new StockArticle { ExploitationId = exploitation.Id, Sku = "PIE-COUR", Name = "Courroies", Categorie = StockCategorie.Pieces, Unit = "u", QuantityOnHand = 2, ReorderLevel = 3, UnitCost = 55 }
        );

        var actifs = await db.ActifsAgricoles.Where(a => a.ExploitationId == exploitation.Id).OrderBy(a => a.InternalCode).ToListAsync();
        if (actifs.Count >= 6)
        {
            db.Interventions.AddRange(
                new InterventionMaintenance { ExploitationId = exploitation.Id, ActifAgricoleId = actifs[0].Id, Title = "Entretien 500h", Type = InterventionType.Preventif, Statut = InterventionStatut.Ouverte, PlannedDate = DateTime.UtcNow.Date.AddDays(3), LaborCost = 350, Description = "Vidange et filtres" },
                new InterventionMaintenance { ExploitationId = exploitation.Id, ActifAgricoleId = actifs[2].Id, Title = "Réparation convoyeur", Type = InterventionType.Correctif, Statut = InterventionStatut.EnCours, PlannedDate = DateTime.UtcNow.Date.AddDays(-1), LaborCost = 800, PartsCost = 450 },
                new InterventionMaintenance { ExploitationId = exploitation.Id, ActifAgricoleId = actifs[1].Id, Title = "Contrôle annuel", Type = InterventionType.Preventif, Statut = InterventionStatut.Cloturee, PlannedDate = DateTime.UtcNow.Date.AddDays(-30), CompletedDate = DateTime.UtcNow.Date.AddDays(-28), LaborCost = 200, Report = "OK" },
                new InterventionMaintenance { ExploitationId = exploitation.Id, ActifAgricoleId = actifs[5].Id, Title = "Inspection pivot", Type = InterventionType.Preventif, Statut = InterventionStatut.Cloturee, PlannedDate = DateTime.UtcNow.Date.AddDays(-60), CompletedDate = DateTime.UtcNow.Date.AddDays(-59), LaborCost = 150 }
            );
            actifs[2].Statut = ActifStatut.EnMaintenance;
        }

        db.Fournisseurs.AddRange(
            new Fournisseur { ExploitationId = exploitation.Id, Name = "Machinerie Centre-du-Québec", ContactName = "Paul Tremblay", Email = "ventes@mcq.demo", Phone = "450-555-0101" },
            new Fournisseur { ExploitationId = exploitation.Id, Name = "AgriIntrants Plus", ContactName = "Sophie Roy", Email = "commande@aip.demo", Phone = "450-555-0102" }
        );

        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Si une exploitation existe sans actifs (seed partiel), complète le parc démo.
    /// </summary>
    private static async Task EnsureDemoPatrimoineAsync(ApplicationDbContext db)
    {
        var exploitation = await db.Exploitations
            .FirstOrDefaultAsync(e => e.Name == "Ferme des Érables");
        if (exploitation is null) return;

        if (await db.ActifsAgricoles.AnyAsync(a => a.ExploitationId == exploitation.Id))
            return;

        var parcelles = await db.Parcelles
            .Where(p => p.ExploitationId == exploitation.Id)
            .OrderBy(p => p.Code)
            .ToListAsync();

        if (parcelles.Count == 0)
        {
            parcelles =
            [
                new Parcelle
                {
                    ExploitationId = exploitation.Id, Code = "P01", Name = "Champ Nord", AreaHa = 18,
                    CurrentCulture = "Maïs", EstimatedYieldPerHa = 8.2m, ResponsibleName = "Jean",
                    Etat = ParcelleEtat.EnProduction, HasIrrigation = true
                }
            ];
            db.Parcelles.AddRange(parcelles);
            await db.SaveChangesAsync();
        }
        else
        {
            foreach (var p in parcelles.Where(x => string.IsNullOrEmpty(x.CurrentCulture)))
            {
                var last = await db.Assolements.AsNoTracking()
                    .Where(a => a.ParcelleId == p.Id)
                    .OrderByDescending(a => a.Season)
                    .FirstOrDefaultAsync();
                if (last is null) continue;
                p.CurrentCulture = last.Culture;
                p.EstimatedYieldPerHa ??= last.YieldPerHa;
                if (p.Etat == ParcelleEtat.EnJachere)
                    p.Etat = ParcelleEtat.EnProduction;
            }
        }

        await SeedActifsForExploitationAsync(db, exploitation.Id, parcelles.ToArray());

        if (!await db.Interventions.AnyAsync(i => i.ExploitationId == exploitation.Id))
        {
            var actifs = await db.ActifsAgricoles
                .Where(a => a.ExploitationId == exploitation.Id)
                .OrderBy(a => a.InternalCode)
                .ToListAsync();
            if (actifs.Count >= 3)
            {
                db.Interventions.AddRange(
                    new InterventionMaintenance
                    {
                        ExploitationId = exploitation.Id,
                        ActifAgricoleId = actifs[0].Id,
                        Title = "Entretien 500h",
                        Type = InterventionType.Preventif,
                        Statut = InterventionStatut.Ouverte,
                        PlannedDate = DateTime.UtcNow.Date.AddDays(3),
                        LaborCost = 350,
                        Description = "Vidange et filtres"
                    },
                    new InterventionMaintenance
                    {
                        ExploitationId = exploitation.Id,
                        ActifAgricoleId = actifs[Math.Min(2, actifs.Count - 1)].Id,
                        Title = "Réparation convoyeur",
                        Type = InterventionType.Correctif,
                        Statut = InterventionStatut.EnCours,
                        PlannedDate = DateTime.UtcNow.Date.AddDays(-1),
                        LaborCost = 800,
                        PartsCost = 450
                    });
                actifs[Math.Min(2, actifs.Count - 1)].Statut = ActifStatut.EnMaintenance;
            }
        }

        await db.SaveChangesAsync();
    }

    private static async Task SeedActifsForExploitationAsync(
        ApplicationDbContext db,
        int exploitationId,
        Parcelle[] parcelles)
    {
        var p0 = parcelles.ElementAtOrDefault(0)?.Id;
        var actifs = new[]
        {
            new ActifAgricole
            {
                ExploitationId = exploitationId, InternalCode = "TR-01", Name = "Tracteur John Deere 6155M",
                Categorie = ActifCategorie.Machinerie, SubCategory = "Tracteur", Brand = "John Deere", Model = "6155M",
                Year = 2019, AcquisitionValue = 185000, AcquisitionDate = new DateTime(2019, 4, 12),
                ParcelleId = p0, Building = "Hangar A", EngineHours = 4820, NextServiceHours = 5000,
                NextServiceDate = DateTime.UtcNow.Date.AddDays(14), WarrantyEndDate = DateTime.UtcNow.Date.AddDays(20),
                QrPayload = "TR-01", GpsLat = 45.6308, GpsLng = -72.9569, LocationNote = "Hangar A"
            },
            new ActifAgricole
            {
                ExploitationId = exploitationId, InternalCode = "TR-02", Name = "Tracteur New Holland T6",
                Categorie = ActifCategorie.Machinerie, Brand = "New Holland", Model = "T6.180", Year = 2016,
                AcquisitionValue = 95000, AcquisitionDate = new DateTime(2016, 6, 1),
                EngineHours = 7100, NextServiceHours = 7000, NextServiceDate = DateTime.UtcNow.Date.AddDays(-5),
                Building = "Hangar A", QrPayload = "TR-02", Statut = ActifStatut.EnService
            },
            new ActifAgricole
            {
                ExploitationId = exploitationId, InternalCode = "MO-01", Name = "Moissonneuse Case IH",
                Categorie = ActifCategorie.Machinerie, Brand = "Case IH", Model = "6140", Year = 2018,
                AcquisitionValue = 320000, AcquisitionDate = new DateTime(2018, 8, 20),
                EngineHours = 2100, NextServiceHours = 2500, Building = "Hangar B", QrPayload = "MO-01"
            },
            new ActifAgricole
            {
                ExploitationId = exploitationId, InternalCode = "VH-01", Name = "Camion grain 10 roues",
                Categorie = ActifCategorie.Vehicules, Brand = "Freightliner", Year = 2015,
                AcquisitionValue = 78000, OdometerKm = 245000, NextServiceDate = DateTime.UtcNow.Date.AddDays(40),
                Building = "Garage", QrPayload = "VH-01"
            },
            new ActifAgricole
            {
                ExploitationId = exploitationId, InternalCode = "OU-01", Name = "Semoir de précision",
                Categorie = ActifCategorie.Outillage, Brand = "Kinze", Model = "3600", Year = 2020,
                AcquisitionValue = 78000, Building = "Hangar A", QrPayload = "OU-01"
            },
            new ActifAgricole
            {
                ExploitationId = exploitationId, InternalCode = "IR-01", Name = "Pompe irrigation principale",
                Categorie = ActifCategorie.Irrigation, Brand = "Valley", Year = 2015, AcquisitionValue = 110000,
                ParcelleId = p0, EngineHours = 3200, NextServiceHours = 3500, QrPayload = "IR-01",
                LocationNote = "Réservoir principal"
            },
            new ActifAgricole
            {
                ExploitationId = exploitationId, InternalCode = "EN-01", Name = "Génératrice diesel 50 kW",
                Categorie = ActifCategorie.Energie, Brand = "Generac", Year = 2021, AcquisitionValue = 18500,
                Building = "Atelier", EngineHours = 410, NextServiceHours = 500, QrPayload = "EN-01"
            },
            new ActifAgricole
            {
                ExploitationId = exploitationId, InternalCode = "BA-01", Name = "Hangar matériel",
                Categorie = ActifCategorie.Installations, AcquisitionValue = 250000,
                AcquisitionDate = new DateTime(2012, 1, 1), Building = "Hangar A", LocationNote = "Siège",
                QrPayload = "BA-01"
            }
        };
        db.ActifsAgricoles.AddRange(actifs);
        await db.SaveChangesAsync();
    }

    private static async Task EnsureDemoPhase2Async(ApplicationDbContext db)
    {
        var exploitation = await db.Exploitations.FirstOrDefaultAsync(e => e.Name == "Ferme des Érables");
        if (exploitation is null) return;

        var parcelles = await db.Parcelles.Where(p => p.ExploitationId == exploitation.Id).OrderBy(p => p.Code).ToListAsync();
        var actifs = await db.ActifsAgricoles.Where(a => a.ExploitationId == exploitation.Id).OrderBy(a => a.InternalCode).ToListAsync();
        var pompe = actifs.FirstOrDefault(a => a.Categorie == ActifCategorie.Irrigation);
        var gen = actifs.FirstOrDefault(a => a.Categorie == ActifCategorie.Energie);
        var tracteur = actifs.FirstOrDefault(a => a.InternalCode == "TR-01") ?? actifs.FirstOrDefault();

        if (!await db.ActivitesAgricoles.AnyAsync(a => a.ExploitationId == exploitation.Id))
        {
            db.ActivitesAgricoles.AddRange(
                new ActiviteAgricole
                {
                    ExploitationId = exploitation.Id,
                    Title = "Semis maïs Champ Nord",
                    Type = ActiviteType.Semis,
                    Statut = ActiviteStatut.Planifiee,
                    PlannedDate = DateTime.UtcNow.Date.AddDays(2),
                    ParcelleId = parcelles.ElementAtOrDefault(0)?.Id,
                    ActifAgricoleId = tracteur?.Id,
                    AssignedTo = "Jean",
                    Cost = 1200
                },
                new ActiviteAgricole
                {
                    ExploitationId = exploitation.Id,
                    Title = "Irrigation Secteur A",
                    Type = ActiviteType.Irrigation,
                    Statut = ActiviteStatut.EnCours,
                    PlannedDate = DateTime.UtcNow.Date,
                    ParcelleId = parcelles.ElementAtOrDefault(0)?.Id,
                    ActifAgricoleId = pompe?.Id,
                    AssignedTo = "Luc"
                },
                new ActiviteAgricole
                {
                    ExploitationId = exploitation.Id,
                    Title = "Traitement fongicide Sud",
                    Type = ActiviteType.Traitement,
                    Statut = ActiviteStatut.Planifiee,
                    PlannedDate = DateTime.UtcNow.Date.AddDays(5),
                    ParcelleId = parcelles.ElementAtOrDefault(1)?.Id
                });
        }

        if (!await db.IrrigationSecteurs.AnyAsync(s => s.ExploitationId == exploitation.Id))
        {
            db.IrrigationSecteurs.AddRange(
                new IrrigationSecteur
                {
                    ExploitationId = exploitation.Id,
                    Code = "S-A",
                    Name = "Secteur A",
                    ParcelleId = parcelles.ElementAtOrDefault(0)?.Id,
                    PompeActifId = pompe?.Id,
                    ReservoirNote = "Réservoir principal",
                    DebitM3H = 45,
                    PressionBar = 3.2m,
                    LastServiceDate = DateTime.UtcNow.Date.AddDays(-40)
                },
                new IrrigationSecteur
                {
                    ExploitationId = exploitation.Id,
                    Code = "S-B",
                    Name = "Secteur B",
                    ParcelleId = parcelles.ElementAtOrDefault(2)?.Id,
                    PompeActifId = pompe?.Id,
                    DebitM3H = 30,
                    PressionBar = 2.8m
                });
        }

        if (gen is not null && !await db.EnergieReleves.AnyAsync(r => r.ExploitationId == exploitation.Id))
        {
            db.EnergieReleves.AddRange(
                new EnergieReleve
                {
                    ExploitationId = exploitation.Id,
                    ActifAgricoleId = gen.Id,
                    ReadingDate = DateTime.UtcNow.Date.AddDays(-7),
                    Kwh = 420,
                    Cost = 95,
                    Source = "Diesel"
                },
                new EnergieReleve
                {
                    ExploitationId = exploitation.Id,
                    ActifAgricoleId = gen.Id,
                    ReadingDate = DateTime.UtcNow.Date.AddDays(-1),
                    Kwh = 85,
                    Cost = 22,
                    Source = "Diesel"
                });
        }

        if (tracteur is not null && !await db.DocumentsFerme.AnyAsync(d => d.ExploitationId == exploitation.Id))
        {
            db.DocumentsFerme.AddRange(
                new DocumentFerme
                {
                    ExploitationId = exploitation.Id,
                    Title = "Garantie John Deere 6155M",
                    Categorie = DocumentCategorie.Garantie,
                    ActifAgricoleId = tracteur.Id,
                    DocumentDate = new DateTime(2019, 4, 12),
                    Tags = "garantie,tracteur",
                    FileUrl = "#"
                },
                new DocumentFerme
                {
                    ExploitationId = exploitation.Id,
                    Title = "Police assurance flotte 2026",
                    Categorie = DocumentCategorie.Assurance,
                    DocumentDate = new DateTime(2026, 1, 1),
                    Tags = "assurance"
                });
        }

        var fournisseurs = await db.Fournisseurs.Where(f => f.ExploitationId == exploitation.Id).ToListAsync();
        foreach (var f in fournisseurs.Where(x => x.Rating == 0 && string.IsNullOrEmpty(x.ContractRef)))
        {
            if (f.Name.Contains("Machinerie", StringComparison.OrdinalIgnoreCase))
            {
                f.Categorie = FournisseurCategorie.Machinerie;
                f.Rating = 4;
                f.ContractRef = "CTR-MCQ-2025";
                f.ContractEndDate = DateTime.UtcNow.Date.AddMonths(8);
            }
            else
            {
                f.Categorie = FournisseurCategorie.Intrants;
                f.Rating = 5;
                f.ContractRef = "CTR-AIP-2026";
                f.ContractEndDate = DateTime.UtcNow.Date.AddMonths(14);
            }
        }

        await db.SaveChangesAsync();
    }

    private static async Task EnsureDemoPhase34Async(ApplicationDbContext db)
    {
        var exploitation = await db.Exploitations.FirstOrDefaultAsync(e => e.Name == "Ferme des Érables");
        if (exploitation is null) return;

        exploitation.MapCenterLat ??= 45.6308;
        exploitation.MapCenterLng ??= -72.9569;

        var parcelles = await db.Parcelles.Where(p => p.ExploitationId == exploitation.Id).OrderBy(p => p.Code).ToListAsync();
        double[,] coords =
        {
            { 45.6330, -72.9600 },
            { 45.6285, -72.9550 },
            { 45.6315, -72.9480 },
            { 45.6260, -72.9620 },
            { 45.6350, -72.9520 }
        };
        for (var i = 0; i < parcelles.Count && i < coords.GetLength(0); i++)
        {
            parcelles[i].GpsLat ??= coords[i, 0];
            parcelles[i].GpsLng ??= coords[i, 1];
        }

        var actifs = await db.ActifsAgricoles.Where(a => a.ExploitationId == exploitation.Id).ToListAsync();
        var offsets = new (double Lat, double Lng)[]
        {
            (45.6322, -72.9585), (45.6310, -72.9570), (45.6298, -72.9545),
            (45.6305, -72.9610), (45.6328, -72.9530), (45.6335, -72.9595),
            (45.6290, -72.9500), (45.6280, -72.9565)
        };
        for (var i = 0; i < actifs.Count && i < offsets.Length; i++)
        {
            actifs[i].GpsLat ??= offsets[i].Lat;
            actifs[i].GpsLng ??= offsets[i].Lng;
        }

        if (!await db.CapteursIoT.AnyAsync(c => c.ExploitationId == exploitation.Id))
        {
            var p0 = parcelles.ElementAtOrDefault(0);
            var pompe = actifs.FirstOrDefault(a => a.Categorie == ActifCategorie.Irrigation);
            var capteurs = new[]
            {
                new CapteurIoT
                {
                    ExploitationId = exploitation.Id, Code = "IOT-HUM-01", Name = "Humidité Champ Nord",
                    Type = CapteurType.HumiditeSol, Unit = "%", LastValue = 28.5m, LastReadingAt = DateTime.UtcNow.AddHours(-2),
                    AlertMin = 20m, AlertMax = 45m, ParcelleId = p0?.Id,
                    GpsLat = p0?.GpsLat ?? 45.633, GpsLng = p0?.GpsLng ?? -72.96
                },
                new CapteurIoT
                {
                    ExploitationId = exploitation.Id, Code = "IOT-TEMP-01", Name = "Température sol Nord",
                    Type = CapteurType.Temperature, Unit = "°C", LastValue = 17.2m, LastReadingAt = DateTime.UtcNow.AddHours(-1),
                    AlertMin = 5m, AlertMax = 32m, ParcelleId = p0?.Id,
                    GpsLat = (p0?.GpsLat ?? 45.633) + 0.001, GpsLng = (p0?.GpsLng ?? -72.96) + 0.001
                },
                new CapteurIoT
                {
                    ExploitationId = exploitation.Id, Code = "IOT-PRESS-01", Name = "Pression pompe",
                    Type = CapteurType.Pression, Unit = "bar", LastValue = 3.1m, LastReadingAt = DateTime.UtcNow.AddMinutes(-40),
                    AlertMin = 2m, AlertMax = 4.5m, ActifAgricoleId = pompe?.Id,
                    GpsLat = pompe?.GpsLat ?? 45.6335, GpsLng = pompe?.GpsLng ?? -72.9595, Statut = CapteurStatut.EnLigne
                }
            };
            db.CapteursIoT.AddRange(capteurs);
            await db.SaveChangesAsync();

            foreach (var c in capteurs)
            {
                db.CapteurLectures.AddRange(
                    new CapteurLecture { CapteurIoTId = c.Id, Value = c.LastValue ?? 0, RecordedAt = DateTime.UtcNow.AddDays(-1) },
                    new CapteurLecture { CapteurIoTId = c.Id, Value = (c.LastValue ?? 0) - 1.2m, RecordedAt = DateTime.UtcNow.AddHours(-6) },
                    new CapteurLecture { CapteurIoTId = c.Id, Value = c.LastValue ?? 0, RecordedAt = c.LastReadingAt ?? DateTime.UtcNow });
            }
        }

        await db.SaveChangesAsync();
    }

    private static async Task EnsureSchemaAsync(ApplicationDbContext context, DatabaseOptions dbOptions)
    {
        var provider = dbOptions.Provider.Trim().ToUpperInvariant();
        if (provider is not ("POSTGRESQL" or "POSTGRES" or "NPGSQL"))
            return;

        var schema = DatabaseExtensions.NormalizeSchema(dbOptions.Schema);
        if (string.Equals(schema, "public", StringComparison.OrdinalIgnoreCase))
            return;

#pragma warning disable EF1002
        await context.Database.ExecuteSqlRawAsync($"CREATE SCHEMA IF NOT EXISTS \"{schema.Replace("\"", "\"\"")}\"");
#pragma warning restore EF1002
    }
}
