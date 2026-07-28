using AgriActifs.Web.Constants;
using AgriActifs.Web.Data;
using AgriActifs.Web.Models;
using AgriActifs.Web.Models.Authorization;
using AgriActifs.Web.Models.Identity;
using AgriActifs.Web.Models.ViewModels;
using AgriActifs.Web.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AgriActifs.Web.Services;

public class PermissionAdminService(
    ApplicationDbContext dbContext,
    RoleManager<ApplicationRole> roleManager,
    IDynamicPermissionService dynamicPermissionService,
    ISecuredEndpointService securedEndpointService,
    IAuditService auditService) : IPermissionAdminService
{
    public async Task EnsureSuperAdminGrantsAsync(CancellationToken cancellationToken = default)
    {
        var superAdminRole = await roleManager.FindByNameAsync(AppRoles.SuperAdmin);
        if (superAdminRole is null)
            return;

        var grantedIds = await dbContext.RolePermissionGrants
            .Where(g => g.RoleId == superAdminRole.Id && g.IsGranted)
            .Select(g => g.PermissionDefinitionId)
            .ToHashSetAsync(cancellationToken);

        var allPermissionIds = await dbContext.PermissionDefinitions
            .Where(p => p.IsActive)
            .Select(p => p.Id)
            .ToListAsync(cancellationToken);

        var added = false;
        foreach (var permissionId in allPermissionIds)
        {
            if (grantedIds.Contains(permissionId))
                continue;

            dbContext.RolePermissionGrants.Add(new RolePermissionGrant
            {
                RoleId = superAdminRole.Id,
                PermissionDefinitionId = permissionId,
                IsGranted = true
            });
            added = true;
        }

        if (added)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            dynamicPermissionService.InvalidateCache();
            securedEndpointService.InvalidateCache();
        }
    }

    public async Task EnsureRoleCategoryGrantsAsync(
        string roleName,
        string category,
        CancellationToken cancellationToken = default)
    {
        var role = await roleManager.FindByNameAsync(roleName);
        if (role is null)
            return;

        var grantedIds = await dbContext.RolePermissionGrants
            .Where(g => g.RoleId == role.Id && g.IsGranted)
            .Select(g => g.PermissionDefinitionId)
            .ToHashSetAsync(cancellationToken);

        var permissionIds = await dbContext.PermissionDefinitions
            .Where(p => p.IsActive && p.Category == category)
            .Select(p => p.Id)
            .ToListAsync(cancellationToken);

        var added = false;
        foreach (var permissionId in permissionIds)
        {
            if (grantedIds.Contains(permissionId))
                continue;

            dbContext.RolePermissionGrants.Add(new RolePermissionGrant
            {
                RoleId = role.Id,
                PermissionDefinitionId = permissionId,
                IsGranted = true
            });
            added = true;
        }

        if (added)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            dynamicPermissionService.InvalidateCache();
            securedEndpointService.InvalidateCache();
        }
    }

    public async Task EnsureRolePermissionCodesAsync(
        string roleName,
        IEnumerable<string> permissionCodes,
        CancellationToken cancellationToken = default)
    {
        var role = await roleManager.FindByNameAsync(roleName);
        if (role is null)
            return;

        var codes = permissionCodes
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Select(c => c.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        if (codes.Count == 0)
            return;

        var permissionIds = await dbContext.PermissionDefinitions
            .Where(p => p.IsActive && codes.Contains(p.Code))
            .Select(p => p.Id)
            .ToListAsync(cancellationToken);

        var grantedIds = await dbContext.RolePermissionGrants
            .Where(g => g.RoleId == role.Id && g.IsGranted)
            .Select(g => g.PermissionDefinitionId)
            .ToHashSetAsync(cancellationToken);

        var added = false;
        foreach (var permissionId in permissionIds)
        {
            if (grantedIds.Contains(permissionId))
                continue;

            dbContext.RolePermissionGrants.Add(new RolePermissionGrant
            {
                RoleId = role.Id,
                PermissionDefinitionId = permissionId,
                IsGranted = true
            });
            added = true;
        }

        if (added)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            dynamicPermissionService.InvalidateCache();
            securedEndpointService.InvalidateCache();
        }
    }

    public async Task<PermissionMatrixViewModel> GetMatrixAsync(CancellationToken cancellationToken = default)
    {
        var roles = await dbContext.Roles
            .AsNoTracking()
            .OrderBy(r => r.Name)
            .Select(r => new RolePermissionColumnViewModel
            {
                RoleId = r.Id,
                RoleName = r.Name ?? string.Empty,
                IsSystemRole = r.IsSystemRole
            })
            .ToListAsync(cancellationToken);

        var permissions = await dbContext.PermissionDefinitions
            .AsNoTracking()
            .Where(p => p.IsActive)
            .OrderBy(p => p.Category)
            .ThenBy(p => p.Resource)
            .ThenBy(p => p.PropertyName)
            .ThenBy(p => p.Action)
            .ToListAsync(cancellationToken);

        var grants = await dbContext.RolePermissionGrants
            .AsNoTracking()
            .Where(g => g.IsGranted)
            .ToListAsync(cancellationToken);

        var rows = permissions.Select(p => new PermissionRowViewModel
        {
            PermissionId = p.Id,
            Code = p.Code,
            Resource = p.Resource,
            PropertyName = p.PropertyName,
            DisplayName = p.DisplayName,
            Category = p.Category,
            GrantsByRoleId = roles.ToDictionary(
                r => r.RoleId,
                r => grants.Any(g => g.RoleId == r.RoleId && g.PermissionDefinitionId == p.Id))
        }).ToList();

        return new PermissionMatrixViewModel
        {
            Roles = roles,
            Permissions = rows,
            Categories = rows
                .GroupBy(r => r.Category)
                .Select(g => new PermissionCategoryGroupViewModel
                {
                    Category = g.Key,
                    Permissions = g.ToList()
                })
                .ToList()
        };
    }

    public async Task<ModelPermissionViewModel> GetModelPermissionsAsync(string resource, CancellationToken cancellationToken = default)
    {
        var matrix = await GetMatrixAsync(cancellationToken);
        var resourcePermissions = matrix.Permissions
            .Where(p => p.Resource.Equals(resource, StringComparison.OrdinalIgnoreCase))
            .ToList();

        return new ModelPermissionViewModel
        {
            Resource = resource,
            Category = resourcePermissions.FirstOrDefault()?.Category ?? resource,
            Roles = matrix.Roles,
            EntityActions = resourcePermissions
                .Where(p => p.IsEntityLevel)
                .Select(p => new ModelPropertyPermissionRowViewModel
                {
                    PermissionId = p.PermissionId,
                    Label = p.DisplayName,
                    Action = p.Code.Split('.').Last(),
                    GrantsByRoleId = p.GrantsByRoleId
                })
                .ToList(),
            Properties = resourcePermissions
                .Where(p => !p.IsEntityLevel)
                .GroupBy(p => p.PropertyName)
                .Select(g => new ModelPropertyPermissionRowViewModel
                {
                    PermissionId = g.First().PermissionId,
                    Label = g.Key ?? string.Empty,
                    Action = "Property",
                    GrantsByRoleId = matrix.Roles.ToDictionary(
                        r => r.RoleId,
                        r => g.Any(p => p.GrantsByRoleId.GetValueOrDefault(r.RoleId)))
                })
                .ToList()
        };
    }

    public async Task SaveRoleGrantsAsync(string roleId, IEnumerable<int> grantedPermissionIds, CancellationToken cancellationToken = default)
    {
        var role = await roleManager.FindByIdAsync(roleId)
            ?? throw new InvalidOperationException("Rôle introuvable.");

        if (role.Name == AppRoles.SuperAdmin)
            throw new InvalidOperationException("Les permissions du SuperAdmin ne peuvent pas être modifiées.");

        var grantedSet = grantedPermissionIds.ToHashSet();
        var allPermissions = await dbContext.PermissionDefinitions
            .Where(p => p.IsActive)
            .Select(p => p.Id)
            .ToListAsync(cancellationToken);

        var existing = await dbContext.RolePermissionGrants
            .Where(g => g.RoleId == roleId)
            .ToListAsync(cancellationToken);

        dbContext.RolePermissionGrants.RemoveRange(existing);

        foreach (var permissionId in allPermissions)
        {
            dbContext.RolePermissionGrants.Add(new RolePermissionGrant
            {
                RoleId = roleId,
                PermissionDefinitionId = permissionId,
                IsGranted = grantedSet.Contains(permissionId)
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        dynamicPermissionService.InvalidateCache();

        await auditService.LogAsync("UpdatePermissions", "Role", roleId, $"Rôle {role.Name} — {grantedSet.Count} permissions", cancellationToken);
    }

    public async Task<HabilitationMatrixViewModel> GetHabilitationMatrixAsync(CancellationToken cancellationToken = default)
    {
        var matrix = await GetMatrixAsync(cancellationToken);
        var allRoles = SortRolesForMatrix(matrix.Roles);
        var editableRoles = allRoles
            .Where(r => r.RoleName != AppRoles.SuperAdmin)
            .ToList();

        var modules = matrix.Permissions
            .Where(p => p.IsEntityLevel)
            .GroupBy(p => p.Category, StringComparer.OrdinalIgnoreCase)
            .OrderBy(g => CategorySortOrder(g.Key))
            .Select(g =>
            {
                var (title, subtitle) = ModuleLabels.GetValueOrDefault(g.Key, (g.Key.ToUpperInvariant(), string.Empty));
                return new HabilitationModuleGroupViewModel
                {
                    Category = g.Key,
                    ModuleTitle = title,
                    ModuleSubtitle = subtitle,
                    Actions = g
                        .OrderBy(p => ResourceSortOrder(p.Resource))
                        .ThenBy(p => ActionSortOrder(p.Code.Split('.').Last()))
                        .Select(p =>
                        {
                            var row = ToActionRow(p);
                            row.ActionLabel = EntityActionLabel(p.Resource, p.Code.Split('.').Last(), p.DisplayName);
                            return row;
                        })
                        .ToList()
                };
            })
            .ToList();

        var controllerResources = matrix.Permissions
            .Where(p => p.IsEntityLevel)
            .GroupBy(p => p.Resource, StringComparer.OrdinalIgnoreCase)
            .OrderBy(g => g.First().Category)
            .ThenBy(g => g.Key)
            .Select(g => new ControllerHabilitationGroupViewModel
            {
                Resource = g.Key,
                Category = g.First().Category,
                Actions = g
                    .OrderBy(p => ActionSortOrder(p.Code.Split('.').Last()))
                    .Select(p => ToActionRow(p))
                    .ToList()
            })
            .ToList();

        var modelResources = matrix.Permissions
            .Where(p => !p.IsEntityLevel)
            .GroupBy(p => p.Resource, StringComparer.OrdinalIgnoreCase)
            .OrderBy(g => g.First().Category)
            .ThenBy(g => g.Key)
            .Select(g => new ModelHabilitationGroupViewModel
            {
                Resource = g.Key,
                Category = g.First().Category,
                Properties = g
                    .GroupBy(p => p.PropertyName ?? string.Empty)
                    .OrderBy(pg => pg.Key)
                    .Select(pg => new ModelPropertyHabilitationRowViewModel
                    {
                        PropertyName = pg.Key,
                        View = pg.Where(p => p.Code.EndsWith(".View", StringComparison.OrdinalIgnoreCase))
                            .Select(ToActionRow).FirstOrDefault(),
                        Edit = pg.Where(p => p.Code.EndsWith(".Edit", StringComparison.OrdinalIgnoreCase))
                            .Select(ToActionRow).FirstOrDefault()
                    })
                    .ToList()
            })
            .ToList();

        return new HabilitationMatrixViewModel
        {
            AllRoles = allRoles,
            EditableRoles = editableRoles,
            Modules = modules,
            Controllers = controllerResources,
            Models = modelResources
        };
    }

    private static readonly Dictionary<string, (string Title, string Subtitle)> ModuleLabels =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["Utilisateurs"] = ("UTILISATEURS", "Gestion des utilisateurs"),
            ["Rôles"] = ("RÔLES & PERMISSIONS", "Gestion des rôles et des habilitations"),
            ["Rapports"] = ("RAPPORTS", "Consultation des rapports"),
            ["Administration"] = ("PARAMÈTRES & SÉCURITÉ", "Configuration système et audit"),
            ["Utilisateur"] = ("DÉTAIL UTILISATEUR", "Propriétés du modèle utilisateur"),
            ["Profil"] = ("DÉTAIL PROFIL", "Propriétés du profil utilisateur"),
            ["Rôle"] = ("DÉTAIL RÔLE", "Propriétés du modèle rôle")
        };

    private static IReadOnlyList<RolePermissionColumnViewModel> SortRolesForMatrix(
        IReadOnlyList<RolePermissionColumnViewModel> roles)
    {
        var order = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            [AppRoles.SuperAdmin] = 0,
            ["Admin"] = 1,
            ["AdminSysteme"] = 1,
            ["Admin Système"] = 1,
            ["Manager"] = 2,
            ["Collaborateur"] = 3,
            ["Lecteur"] = 4,
            ["Invite"] = 5,
            ["Invité"] = 5,
            ["Guest"] = 5
        };

        return roles
            .OrderBy(r => order.GetValueOrDefault(r.RoleName, 100))
            .ThenBy(r => r.RoleName, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static int CategorySortOrder(string category) => category switch
    {
        "Utilisateurs" => 1,
        "Rôles" => 2,
        "Rapports" => 3,
        "Administration" => 4,
        "Utilisateur" => 5,
        "Profil" => 6,
        "Rôle" => 7,
        _ => 99
    };

    private static int ResourceSortOrder(string resource) => resource switch
    {
        "Users" => 1,
        "Roles" => 2,
        "Permissions" => 3,
        "Reports" => 4,
        "Settings" => 5,
        "Audit" => 6,
        "Dashboard" => 7,
        "SecuredEndpoints" => 8,
        _ => 99
    };

    private static string EntityActionLabel(string resource, string action, string displayName)
    {
        if (action.Equals("View", StringComparison.OrdinalIgnoreCase)
            && resource.Equals("Audit", StringComparison.OrdinalIgnoreCase))
            return "Voir journal d'audit";

        return action switch
        {
            "View" => "Lire / Consulter",
            "Create" => "Créer",
            "Edit" => "Modifier",
            "Delete" => "Désactiver / Supprimer",
            "Export" => "Exporter",
            "Manage" when resource.Equals("Permissions", StringComparison.OrdinalIgnoreCase) => "Attribuer permissions",
            "Manage" when resource.Equals("Roles", StringComparison.OrdinalIgnoreCase) => "Créer / Modifier rôles",
            "Manage" when resource.Equals("Settings", StringComparison.OrdinalIgnoreCase) => "Modifier paramètres",
            "Manage" when resource.Equals("SecuredEndpoints", StringComparison.OrdinalIgnoreCase) => "Gérer sécurité",
            "Manage" => "Gérer",
            _ => displayName
        };
    }

    public async Task SaveHabilitationMatrixAsync(IEnumerable<string> grantTokens, CancellationToken cancellationToken = default)
    {
        var parsed = grantTokens
            .Select(ParseGrantToken)
            .Where(x => x is not null)
            .Cast<(string RoleId, int PermissionId)>()
            .ToList();

        var matrix = await GetHabilitationMatrixAsync(cancellationToken);

        foreach (var role in matrix.EditableRoles)
        {
            var grantedForRole = parsed
                .Where(g => g.RoleId == role.RoleId)
                .Select(g => g.PermissionId)
                .ToHashSet();

            await SaveRoleGrantsAsync(role.RoleId, grantedForRole, cancellationToken);
        }
    }

    private static HabilitationActionRowViewModel ToActionRow(PermissionRowViewModel p) => new()
    {
        PermissionId = p.PermissionId,
        Resource = p.Resource,
        Action = p.Code.Split('.').Last(),
        ActionLabel = ActionLabel(p.Code.Split('.').Last()),
        GrantsByRoleId = p.GrantsByRoleId
    };

    private static int ActionSortOrder(string action) => action switch
    {
        "View" => 1,
        "Create" => 2,
        "Edit" => 3,
        "Delete" => 4,
        "Export" => 5,
        "Manage" => 6,
        _ => 99
    };

    private static string ActionLabel(string action) => action switch
    {
        "View" => "Voir",
        "Create" => "Créer",
        "Edit" => "Modifier",
        "Delete" => "Supprimer",
        "Export" => "Exporter",
        "Manage" => "Gérer",
        _ => action
    };

    private static (string RoleId, int PermissionId)? ParseGrantToken(string token)
    {
        var parts = token.Split('|', 2);
        if (parts.Length != 2 || !int.TryParse(parts[1], out var permissionId))
            return null;
        return (parts[0], permissionId);
    }
}
