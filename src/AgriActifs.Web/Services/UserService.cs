using AgriActifs.Web.Data;
using AgriActifs.Web.Models.Identity;
using AgriActifs.Web.Models.ViewModels;
using AgriActifs.Web.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AgriActifs.Web.Services;

public class UserService(
    UserManager<ApplicationUser> userManager,
    ApplicationDbContext dbContext,
    IAuditService auditService) : IUserService
{
    public async Task<IReadOnlyList<UserListItemViewModel>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var users = await userManager.Users.AsNoTracking().ToListAsync(cancellationToken);
        var result = new List<UserListItemViewModel>();

        foreach (var user in users)
        {
            var roles = await userManager.GetRolesAsync(user);
            result.Add(new UserListItemViewModel
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FullName = $"{user.FirstName} {user.LastName}".Trim(),
                IsActive = user.IsActive,
                IsLockedOut = user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow,
                Roles = roles.ToList()
            });
        }

        return result;
    }

    public async Task<UserEditViewModel?> GetForEditAsync(string id, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user is null) return null;

        return new UserEditViewModel
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            IsActive = user.IsActive
        };
    }

    public async Task<(bool Success, string? Error)> CreateAsync(UserEditViewModel model, CancellationToken cancellationToken = default)
    {
        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FirstName = model.FirstName,
            LastName = model.LastName,
            PhoneNumber = model.PhoneNumber,
            IsActive = model.IsActive,
            EmailConfirmed = false
        };

        var result = string.IsNullOrWhiteSpace(model.Password)
            ? await userManager.CreateAsync(user)
            : await userManager.CreateAsync(user, model.Password);

        if (!result.Succeeded)
            return (false, string.Join(", ", result.Errors.Select(e => e.Description)));

        dbContext.UserProfiles.Add(new Models.UserProfile { UserId = user.Id });
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditService.LogAsync("Create", "User", user.Id, $"Utilisateur créé: {user.Email}", cancellationToken);

        return (true, null);
    }

    public async Task<(bool Success, string? Error)> UpdateAsync(UserEditViewModel model, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(model.Id))
            return (false, "Identifiant utilisateur manquant.");

        var user = await userManager.FindByIdAsync(model.Id);
        if (user is null)
            return (false, "Utilisateur introuvable.");

        user.Email = model.Email;
        user.UserName = model.Email;
        user.FirstName = model.FirstName;
        user.LastName = model.LastName;
        user.PhoneNumber = model.PhoneNumber;
        user.IsActive = model.IsActive;

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return (false, string.Join(", ", result.Errors.Select(e => e.Description)));

        await auditService.LogAsync("Update", "User", user.Id, $"Utilisateur modifié: {user.Email}", cancellationToken);
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> DeactivateAsync(string id, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user is null)
            return (false, "Utilisateur introuvable.");

        user.IsActive = false;
        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return (false, string.Join(", ", result.Errors.Select(e => e.Description)));

        await auditService.LogAsync("Deactivate", "User", user.Id, cancellationToken: cancellationToken);
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> UnlockAsync(string id, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user is null)
            return (false, "Utilisateur introuvable.");

        var result = await userManager.SetLockoutEndDateAsync(user, null);
        if (!result.Succeeded)
            return (false, string.Join(", ", result.Errors.Select(e => e.Description)));

        await auditService.LogAsync("Unlock", "User", user.Id, cancellationToken: cancellationToken);
        return (true, null);
    }

    public Task<int> CountAsync(CancellationToken cancellationToken = default) =>
        userManager.Users.CountAsync(cancellationToken);

    public Task<int> CountLockedAsync(CancellationToken cancellationToken = default) =>
        userManager.Users.CountAsync(u => u.LockoutEnd.HasValue && u.LockoutEnd > DateTimeOffset.UtcNow, cancellationToken);
}
