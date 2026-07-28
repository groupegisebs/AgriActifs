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
            await SeedDemoAsync(context, userManager);
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
        var existingCodes = await context.PermissionDefinitions.Select(p => p.Code).ToHashSetAsync();
        foreach (var permission in CatalogSeedData.Permissions)
        {
            if (existingCodes.Contains(permission.Code))
                continue;

            context.PermissionDefinitions.Add(new PermissionDefinition
            {
                Code = permission.Code,
                Resource = permission.Resource,
                Action = permission.Action,
                PropertyName = permission.PropertyName,
                DisplayName = permission.DisplayName,
                Category = permission.Category,
                IsSystem = true,
                IsActive = true
            });
        }
        await context.SaveChangesAsync();

        var permissionMap = await context.PermissionDefinitions.ToDictionaryAsync(p => p.Code, p => p.Id);
        var endpoints = await context.SecuredEndpoints.AsNoTracking().ToListAsync();
        foreach (var endpoint in CatalogSeedData.Endpoints)
        {
            if (!permissionMap.TryGetValue(endpoint.PermissionCode, out var permissionId))
                continue;

            var exists = endpoints.Any(e =>
                e.Area == endpoint.Area &&
                e.Controller == endpoint.Controller &&
                e.Action == endpoint.Action &&
                e.HttpMethod == endpoint.HttpMethod);
            if (exists) continue;

            context.SecuredEndpoints.Add(new SecuredEndpoint
            {
                Area = endpoint.Area,
                Controller = endpoint.Controller,
                Action = endpoint.Action,
                HttpMethod = endpoint.HttpMethod,
                PermissionDefinitionId = permissionId,
                IsActive = true
            });
        }

        var reportCodes = await context.ReportDefinitions.Select(r => r.Code).ToHashSetAsync();
        foreach (var report in CatalogSeedData.Reports)
        {
            if (reportCodes.Contains(report.Code)) continue;
            context.ReportDefinitions.Add(new Models.ReportDefinition
            {
                Code = report.Code,
                Name = report.Name,
                Category = report.Category,
                RequiredPermissionCode = report.RequiredPermissionCode,
                IsActive = true
            });
        }

        await context.SaveChangesAsync();
    }

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
            new Parcelle { ExploitationId = exploitation.Id, Code = "P01", Name = "Champ Nord", AreaHa = 18, SoilType = "Loam", HasIrrigation = true },
            new Parcelle { ExploitationId = exploitation.Id, Code = "P02", Name = "Champ Sud", AreaHa = 22, SoilType = "Argile", HasIrrigation = false },
            new Parcelle { ExploitationId = exploitation.Id, Code = "P03", Name = "Vallée", AreaHa = 15, SoilType = "Sableux", HasIrrigation = true },
            new Parcelle { ExploitationId = exploitation.Id, Code = "P04", Name = "Plateau", AreaHa = 12, SoilType = "Loam", HasIrrigation = false },
            new Parcelle { ExploitationId = exploitation.Id, Code = "P05", Name = "Bordure", AreaHa = 13, SoilType = "Limoneux", HasIrrigation = true }
        };
        db.Parcelles.AddRange(parcelles);
        await db.SaveChangesAsync();

        db.Assolements.AddRange(
            new Assolement { ParcelleId = parcelles[0].Id, Season = "2026", Culture = "Maïs", YieldPerHa = 9.2m },
            new Assolement { ParcelleId = parcelles[1].Id, Season = "2026", Culture = "Soya", YieldPerHa = 3.1m },
            new Assolement { ParcelleId = parcelles[2].Id, Season = "2026", Culture = "Blé", YieldPerHa = 4.5m });

        var actifs = new[]
        {
            new ActifAgricole { ExploitationId = exploitation.Id, InternalCode = "TR-01", Name = "Tracteur John Deere 6155R", Categorie = ActifCategorie.MaterielRoulant, Brand = "John Deere", Model = "6155R", Year = 2019, AcquisitionValue = 185000, AcquisitionDate = new DateTime(2019, 4, 12), ParcelleId = parcelles[0].Id },
            new ActifAgricole { ExploitationId = exploitation.Id, InternalCode = "TR-02", Name = "Tracteur New Holland T6", Categorie = ActifCategorie.MaterielRoulant, Brand = "New Holland", Model = "T6.180", Year = 2016, AcquisitionValue = 95000, AcquisitionDate = new DateTime(2016, 6, 1) },
            new ActifAgricole { ExploitationId = exploitation.Id, InternalCode = "MO-01", Name = "Moissonneuse Case IH", Categorie = ActifCategorie.MaterielRoulant, Brand = "Case IH", Model = "6140", Year = 2018, AcquisitionValue = 320000, AcquisitionDate = new DateTime(2018, 8, 20) },
            new ActifAgricole { ExploitationId = exploitation.Id, InternalCode = "OU-01", Name = "Semoir de précision", Categorie = ActifCategorie.Outillage, Brand = "Kinze", Model = "3600", Year = 2020, AcquisitionValue = 78000 },
            new ActifAgricole { ExploitationId = exploitation.Id, InternalCode = "OU-02", Name = "Pulvérisateur traîné", Categorie = ActifCategorie.Outillage, Brand = "Hardi", Model = "Navigator", Year = 2017, AcquisitionValue = 42000 },
            new ActifAgricole { ExploitationId = exploitation.Id, InternalCode = "IR-01", Name = "Pivot d'irrigation", Categorie = ActifCategorie.Irrigation, Brand = "Valley", Year = 2015, AcquisitionValue = 110000, ParcelleId = parcelles[0].Id },
            new ActifAgricole { ExploitationId = exploitation.Id, InternalCode = "BA-01", Name = "Hangar matériel", Categorie = ActifCategorie.Batiment, AcquisitionValue = 250000, AcquisitionDate = new DateTime(2012, 1, 1), LocationNote = "Siège" },
            new ActifAgricole { ExploitationId = exploitation.Id, InternalCode = "BA-02", Name = "Silo à grains", Categorie = ActifCategorie.Batiment, AcquisitionValue = 65000, Year = 2014 }
        };
        db.ActifsAgricoles.AddRange(actifs);
        await db.SaveChangesAsync();

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

        db.Interventions.AddRange(
            new InterventionMaintenance { ExploitationId = exploitation.Id, ActifAgricoleId = actifs[0].Id, Title = "Entretien 500h", Type = InterventionType.Preventif, Statut = InterventionStatut.Ouverte, PlannedDate = DateTime.UtcNow.Date.AddDays(3), LaborCost = 350, Description = "Vidange et filtres" },
            new InterventionMaintenance { ExploitationId = exploitation.Id, ActifAgricoleId = actifs[2].Id, Title = "Réparation convoyeur", Type = InterventionType.Correctif, Statut = InterventionStatut.EnCours, PlannedDate = DateTime.UtcNow.Date.AddDays(-1), LaborCost = 800, PartsCost = 450 },
            new InterventionMaintenance { ExploitationId = exploitation.Id, ActifAgricoleId = actifs[1].Id, Title = "Contrôle annuel", Type = InterventionType.Preventif, Statut = InterventionStatut.Cloturee, PlannedDate = DateTime.UtcNow.Date.AddDays(-30), CompletedDate = DateTime.UtcNow.Date.AddDays(-28), LaborCost = 200, Report = "OK" },
            new InterventionMaintenance { ExploitationId = exploitation.Id, ActifAgricoleId = actifs[5].Id, Title = "Inspection pivot", Type = InterventionType.Preventif, Statut = InterventionStatut.Cloturee, PlannedDate = DateTime.UtcNow.Date.AddDays(-60), CompletedDate = DateTime.UtcNow.Date.AddDays(-59), LaborCost = 150 }
        );

        db.Fournisseurs.AddRange(
            new Fournisseur { ExploitationId = exploitation.Id, Name = "Machinerie Centre-du-Québec", ContactName = "Paul Tremblay", Email = "ventes@mcq.demo", Phone = "450-555-0101" },
            new Fournisseur { ExploitationId = exploitation.Id, Name = "AgriIntrants Plus", ContactName = "Sophie Roy", Email = "commande@aip.demo", Phone = "450-555-0102" }
        );

        actifs[2].Statut = ActifStatut.EnMaintenance;
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
