using AgriActifs.Web.Constants;
using AgriActifs.Web.Models.Identity;
using AgriActifs.Web.Models.ViewModels;
using AgriActifs.Web.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AgriActifs.Web.Services;

public class RoleService(
    RoleManager<ApplicationRole> roleManager,
    UserManager<ApplicationUser> userManager,
    IAuditService auditService) : IRoleService
{
    public async Task<IReadOnlyList<RoleListItemViewModel>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var roles = await roleManager.Roles.AsNoTracking().ToListAsync(cancellationToken);
        var result = new List<RoleListItemViewModel>();

        foreach (var role in roles)
        {
            var usersInRole = await userManager.GetUsersInRoleAsync(role.Name!);
            result.Add(new RoleListItemViewModel
            {
                Id = role.Id,
                Name = role.Name ?? string.Empty,
                Description = role.Description,
                IsSystemRole = role.IsSystemRole,
                UserCount = usersInRole.Count
            });
        }

        return result;
    }

    public async Task<RoleEditViewModel?> GetForEditAsync(string id, CancellationToken cancellationToken = default)
    {
        var role = await roleManager.FindByIdAsync(id);
        if (role is null) return null;

        return new RoleEditViewModel
        {
            Id = role.Id,
            Name = role.Name ?? string.Empty,
            Description = role.Description
        };
    }

    public async Task<(bool Success, string? Error, string? CreatedRoleId)> CreateAsync(RoleEditViewModel model, CancellationToken cancellationToken = default)
    {
        var role = new ApplicationRole
        {
            Name = model.Name,
            Description = model.Description
        };

        var result = await roleManager.CreateAsync(role);
        if (!result.Succeeded)
            return (false, string.Join(", ", result.Errors.Select(e => e.Description)), null);

        await auditService.LogAsync("Create", "Role", role.Id, $"Rôle créé: {role.Name}. Configurez les habilitations dans la matrice.", cancellationToken);

        return (true, null, role.Id);
    }

    public async Task<(bool Success, string? Error)> UpdateAsync(RoleEditViewModel model, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(model.Id))
            return (false, "Identifiant rôle manquant.");

        var role = await roleManager.FindByIdAsync(model.Id);
        if (role is null)
            return (false, "Rôle introuvable.");

        if (role.Name == AppRoles.SuperAdmin && role.Name != model.Name)
            return (false, "Le rôle SuperAdmin ne peut pas être renommé.");

        if (role.IsSystemRole && role.Name != model.Name)
            return (false, "Le nom d'un rôle système ne peut pas être modifié.");

        role.Name = model.Name;
        role.Description = model.Description;

        var result = await roleManager.UpdateAsync(role);
        if (!result.Succeeded)
            return (false, string.Join(", ", result.Errors.Select(e => e.Description)));

        await auditService.LogAsync("Update", "Role", role.Id, cancellationToken: cancellationToken);

        return (true, null);
    }

    public async Task<(bool Success, string? Error)> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var role = await roleManager.FindByIdAsync(id);
        if (role is null)
            return (false, "Rôle introuvable.");

        if (role.IsSystemRole)
            return (false, "Un rôle système ne peut pas être supprimé.");

        var result = await roleManager.DeleteAsync(role);
        if (!result.Succeeded)
            return (false, string.Join(", ", result.Errors.Select(e => e.Description)));

        await auditService.LogAsync("Delete", "Role", role.Id, cancellationToken: cancellationToken);
        return (true, null);
    }

    public async Task<UserRolesViewModel?> GetUserRolesAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null) return null;

        var userRoles = await userManager.GetRolesAsync(user);
        var allRoles = await roleManager.Roles.Select(r => r.Name!).ToListAsync(cancellationToken);

        return new UserRolesViewModel
        {
            UserId = user.Id,
            Email = user.Email ?? string.Empty,
            Roles = allRoles.Select(roleName => new RoleSelectionViewModel
            {
                RoleName = roleName,
                IsSelected = userRoles.Contains(roleName)
            }).ToList()
        };
    }

    public async Task<(bool Success, string? Error)> UpdateUserRolesAsync(string userId, IEnumerable<string> selectedRoles, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
            return (false, "Utilisateur introuvable.");

        var currentRoles = await userManager.GetRolesAsync(user);
        var selected = selectedRoles.ToList();

        var toRemove = currentRoles.Except(selected).ToList();
        var toAdd = selected.Except(currentRoles).ToList();

        var removeResult = await userManager.RemoveFromRolesAsync(user, toRemove);
        if (!removeResult.Succeeded)
            return (false, string.Join(", ", removeResult.Errors.Select(e => e.Description)));

        var addResult = await userManager.AddToRolesAsync(user, toAdd);
        if (!addResult.Succeeded)
            return (false, string.Join(", ", addResult.Errors.Select(e => e.Description)));

        await auditService.LogAsync("UpdateRoles", "User", user.Id, string.Join(", ", selected), cancellationToken);
        return (true, null);
    }

    public Task<int> CountAsync(CancellationToken cancellationToken = default) =>
        roleManager.Roles.CountAsync(cancellationToken);
}
