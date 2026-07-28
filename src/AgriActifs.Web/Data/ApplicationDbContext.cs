using AgriActifs.Web.Configuration;
using AgriActifs.Web.Extensions;
using AgriActifs.Web.Models;
using AgriActifs.Web.Models.Authorization;
using AgriActifs.Web.Models.Ferme;
using AgriActifs.Web.Models.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AgriActifs.Web.Data;

public class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options,
    IOptions<DatabaseOptions> databaseOptions)
    : IdentityDbContext<ApplicationUser, ApplicationRole, string>(options)
{
    private readonly string _schema = DatabaseExtensions.NormalizeSchema(databaseOptions.Value.Schema);

    public string Schema => _schema;
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<SystemSettings> SystemSettings => Set<SystemSettings>();
    public DbSet<ReportDefinition> ReportDefinitions => Set<ReportDefinition>();
    public DbSet<PermissionDefinition> PermissionDefinitions => Set<PermissionDefinition>();
    public DbSet<RolePermissionGrant> RolePermissionGrants => Set<RolePermissionGrant>();
    public DbSet<SecuredEndpoint> SecuredEndpoints => Set<SecuredEndpoint>();
    public DbSet<ThemeDefinition> ThemeDefinitions => Set<ThemeDefinition>();

    public DbSet<Exploitation> Exploitations => Set<Exploitation>();
    public DbSet<ExploitationUser> ExploitationUsers => Set<ExploitationUser>();
    public DbSet<Parcelle> Parcelles => Set<Parcelle>();
    public DbSet<Assolement> Assolements => Set<Assolement>();
    public DbSet<ActifAgricole> ActifsAgricoles => Set<ActifAgricole>();
    public DbSet<StockArticle> StockArticles => Set<StockArticle>();
    public DbSet<StockMouvement> StockMouvements => Set<StockMouvement>();
    public DbSet<InterventionMaintenance> Interventions => Set<InterventionMaintenance>();
    public DbSet<InterventionPiece> InterventionPieces => Set<InterventionPiece>();
    public DbSet<Fournisseur> Fournisseurs => Set<Fournisseur>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        if (UseExplicitSchema())
            builder.HasDefaultSchema(_schema);

        base.OnModelCreating(builder);

        builder.Entity<UserProfile>(entity =>
        {
            entity.HasIndex(x => x.UserId).IsUnique();
            entity.HasOne(x => x.User)
                .WithOne(x => x.Profile)
                .HasForeignKey<UserProfile>(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<AuditLog>(entity =>
        {
            entity.HasIndex(x => x.CreatedAt);
            entity.HasIndex(x => x.UserId);
        });

        builder.Entity<ReportDefinition>(entity =>
        {
            entity.HasIndex(x => x.Code).IsUnique();
            entity.HasIndex(x => x.RequiredPermissionCode);
            entity.HasOne(x => x.RequiredPermission)
                .WithMany()
                .HasForeignKey(x => x.RequiredPermissionCode)
                .HasPrincipalKey(p => p.Code)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<PermissionDefinition>(entity =>
        {
            entity.HasIndex(x => x.Code).IsUnique();
            entity.HasIndex(x => new { x.Resource, x.Action, x.PropertyName });
        });

        builder.Entity<RolePermissionGrant>(entity =>
        {
            entity.HasIndex(x => new { x.RoleId, x.PermissionDefinitionId }).IsUnique();
            entity.HasOne(x => x.Permission)
                .WithMany(x => x.RoleGrants)
                .HasForeignKey(x => x.PermissionDefinitionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<SecuredEndpoint>(entity =>
        {
            entity.HasIndex(x => new { x.Area, x.Controller, x.Action, x.HttpMethod }).IsUnique();
            entity.HasOne(x => x.Permission)
                .WithMany()
                .HasForeignKey(x => x.PermissionDefinitionId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<SystemSettings>(entity =>
        {
            entity.HasOne<ThemeDefinition>()
                .WithMany()
                .HasForeignKey(x => x.ActiveThemeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasData(new SystemSettings
            {
                Id = 1,
                ActiveThemeId = 1,
                DefaultCulture = "fr-FR",
                UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            });
        });

        builder.Entity<ThemeDefinition>(entity =>
        {
            entity.HasIndex(x => x.Code).IsUnique();
            foreach (var theme in ThemeDefaults.SeedThemes)
            {
                entity.HasData(new ThemeDefinition
                {
                    Id = theme.Id,
                    Code = theme.Code,
                    Name = theme.Name,
                    Description = theme.Description,
                    CssVariables = theme.CssVariables,
                    IsSystem = true,
                    IsActive = true,
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                });
            }
        });

        builder.Entity<ExploitationUser>(entity =>
        {
            entity.HasIndex(x => new { x.ExploitationId, x.UserId }).IsUnique();
            entity.HasOne(x => x.Exploitation)
                .WithMany(x => x.Members)
                .HasForeignKey(x => x.ExploitationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Parcelle>(entity =>
        {
            entity.HasIndex(x => new { x.ExploitationId, x.Code }).IsUnique();
            entity.Property(x => x.AreaHa).HasPrecision(18, 2);
            entity.Property(x => x.EstimatedYieldPerHa).HasPrecision(18, 2);
            entity.Property(x => x.ActualYieldPerHa).HasPrecision(18, 2);
            entity.HasOne(x => x.Exploitation)
                .WithMany(x => x.Parcelles)
                .HasForeignKey(x => x.ExploitationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Assolement>(entity =>
        {
            entity.Property(x => x.YieldPerHa).HasPrecision(18, 2);
            entity.HasOne(x => x.Parcelle)
                .WithMany(x => x.Assolements)
                .HasForeignKey(x => x.ParcelleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ActifAgricole>(entity =>
        {
            entity.HasIndex(x => new { x.ExploitationId, x.InternalCode }).IsUnique();
            entity.Property(x => x.AcquisitionValue).HasPrecision(18, 2);
            entity.Property(x => x.ResidualValue).HasPrecision(18, 2);
            entity.Property(x => x.EngineHours).HasPrecision(18, 1);
            entity.Property(x => x.OdometerKm).HasPrecision(18, 1);
            entity.Property(x => x.NextServiceHours).HasPrecision(18, 1);
            entity.HasOne(x => x.Exploitation)
                .WithMany(x => x.Actifs)
                .HasForeignKey(x => x.ExploitationId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Parcelle)
                .WithMany(x => x.Actifs)
                .HasForeignKey(x => x.ParcelleId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(x => x.Fournisseur)
                .WithMany(x => x.Actifs)
                .HasForeignKey(x => x.FournisseurId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<StockArticle>(entity =>
        {
            entity.HasIndex(x => new { x.ExploitationId, x.Sku }).IsUnique();
            entity.Property(x => x.QuantityOnHand).HasPrecision(18, 3);
            entity.Property(x => x.ReorderLevel).HasPrecision(18, 3);
            entity.Property(x => x.UnitCost).HasPrecision(18, 2);
            entity.HasOne(x => x.Exploitation)
                .WithMany(x => x.Stocks)
                .HasForeignKey(x => x.ExploitationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<StockMouvement>(entity =>
        {
            entity.Property(x => x.Quantity).HasPrecision(18, 3);
            entity.HasOne(x => x.StockArticle)
                .WithMany(x => x.Mouvements)
                .HasForeignKey(x => x.StockArticleId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Intervention)
                .WithMany()
                .HasForeignKey(x => x.InterventionMaintenanceId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<InterventionMaintenance>(entity =>
        {
            entity.Property(x => x.LaborCost).HasPrecision(18, 2);
            entity.Property(x => x.PartsCost).HasPrecision(18, 2);
            entity.HasOne(x => x.Exploitation)
                .WithMany(x => x.Interventions)
                .HasForeignKey(x => x.ExploitationId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Actif)
                .WithMany(x => x.Interventions)
                .HasForeignKey(x => x.ActifAgricoleId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<InterventionPiece>(entity =>
        {
            entity.Property(x => x.Quantity).HasPrecision(18, 3);
            entity.HasOne(x => x.Intervention)
                .WithMany(x => x.Pieces)
                .HasForeignKey(x => x.InterventionMaintenanceId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.StockArticle)
                .WithMany()
                .HasForeignKey(x => x.StockArticleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Fournisseur>(entity =>
        {
            entity.HasOne(x => x.Exploitation)
                .WithMany(x => x.Fournisseurs)
                .HasForeignKey(x => x.ExploitationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Exploitation>(entity =>
        {
            entity.Property(x => x.TotalAreaHa).HasPrecision(18, 2);
        });
    }

    private bool UseExplicitSchema() =>
        !string.Equals(_schema, "public", StringComparison.OrdinalIgnoreCase)
        && !string.Equals(_schema, "dbo", StringComparison.OrdinalIgnoreCase);
}
