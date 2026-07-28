using AgriActifs.Web.Constants;
using AgriActifs.Web.Data;
using AgriActifs.Web.Models.Authorization;
using AgriActifs.Web.Models.Ferme;

namespace AgriActifs.Tests.Constants;

public class CatalogSeedDataTests
{
    [Fact]
    public void Catalog_HasPermissionsEndpointsAndReports()
    {
        Assert.NotEmpty(CatalogSeedData.Permissions);
        Assert.NotEmpty(CatalogSeedData.Endpoints);
        Assert.NotEmpty(CatalogSeedData.Reports);
    }

    [Fact]
    public void Catalog_PermissionCodesAreUnique()
    {
        var codes = CatalogSeedData.Permissions.Select(p => p.Code).ToList();
        Assert.Equal(codes.Count, codes.Distinct(StringComparer.OrdinalIgnoreCase).Count());
    }

    [Fact]
    public void Catalog_EndpointPermissionCodesExistInCatalog()
    {
        var permissionCodes = CatalogSeedData.Permissions.Select(p => p.Code).ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var endpoint in CatalogSeedData.Endpoints)
        {
            Assert.True(
                permissionCodes.Contains(endpoint.PermissionCode),
                $"Permission '{endpoint.PermissionCode}' manquante pour {endpoint.Controller}.{endpoint.Action}");
        }
    }

    [Fact]
    public void Catalog_ContainsPropertyLevelPermissions()
    {
        Assert.Contains(CatalogSeedData.Permissions, p => p.PropertyName == "Email" && p.Resource == "User");
    }

    [Fact]
    public void DefaultSeed_IncludesSuperAdminAndPlatformRoles()
    {
        Assert.Contains(AppRoles.SuperAdmin, AppRoles.DefaultSeedRoles);
        Assert.Contains(AppRoles.Admin, AppRoles.DefaultSeedRoles);
        Assert.Contains(AppRoles.User, AppRoles.DefaultSeedRoles);
    }

    [Fact]
    public void Catalog_ContainsFermePermissions()
    {
        Assert.Contains(CatalogSeedData.Permissions, p => p.Code == "Actifs.View");
        Assert.Contains(CatalogSeedData.Permissions, p => p.Code == "FermeDashboard.View");
        Assert.Contains(CatalogSeedData.Endpoints, e => e.Area == "Ferme" && e.Controller == "Dashboard");
    }

    [Fact]
    public void Catalog_AllActionsAreValidEnum()
    {
        foreach (var permission in CatalogSeedData.Permissions)
        {
            Assert.True(Enum.IsDefined(permission.Action));
        }
    }
}

public class ExploitationScopingTests
{
    [Fact]
    public void ActifAgricole_RequiresExploitationId()
    {
        var actif = new ActifAgricole
        {
            ExploitationId = 7,
            InternalCode = "TR-01",
            Name = "Tracteur"
        };
        Assert.Equal(7, actif.ExploitationId);
        Assert.True(actif.IsActive);
        Assert.Equal(ActifStatut.EnService, actif.Statut);
    }

    [Fact]
    public void StockArticle_LowStock_WhenAtOrBelowReorder()
    {
        var stock = new StockArticle
        {
            ExploitationId = 1,
            Sku = "SEM-1",
            Name = "Semences",
            QuantityOnHand = 2,
            ReorderLevel = 5
        };
        Assert.True(stock.QuantityOnHand <= stock.ReorderLevel);
    }
}
