using AgriActifs.Web.Data;
using AgriActifs.Web.Middleware;
using AgriActifs.Web.Models.Identity;
using AgriActifs.Web.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgriActifs.Web.Controllers;

[Authorize]
public class ProfileController(
    UserManager<ApplicationUser> userManager,
    ApplicationDbContext dbContext) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
            return Challenge();

        var profile = await dbContext.UserProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == user.Id, cancellationToken);

        var model = new ProfileViewModel
        {
            UserId = user.Id,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            Company = profile?.Company,
            JobTitle = profile?.JobTitle,
            PreferredLanguage = profile?.PreferredLanguage ?? "fr-FR",
            Theme = profile?.Theme ?? "light",
            PhotoUrl = profile?.PhotoUrl,
            EmailNotifications = profile?.EmailNotifications ?? true
        };

        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(ProfileViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await userManager.GetUserAsync(User);
        if (user is null || user.Id != model.UserId)
            return Forbid();

        user.FirstName = model.FirstName;
        user.LastName = model.LastName;
        user.PhoneNumber = model.PhoneNumber;

        await userManager.UpdateAsync(user);

        var profile = await dbContext.UserProfiles.FirstOrDefaultAsync(x => x.UserId == user.Id, cancellationToken);
        if (profile is null)
        {
            profile = new Models.UserProfile { UserId = user.Id };
            dbContext.UserProfiles.Add(profile);
        }

        profile.Company = model.Company;
        profile.JobTitle = model.JobTitle;
        profile.PreferredLanguage = model.PreferredLanguage;
        profile.Theme = model.Theme;
        profile.PhotoUrl = model.PhotoUrl;
        profile.EmailNotifications = model.EmailNotifications;

        await dbContext.SaveChangesAsync(cancellationToken);

        Response.Cookies.Append(GiseCultureMiddleware.CultureCookieName, model.PreferredLanguage, new CookieOptions
        {
            HttpOnly = false,
            IsEssential = true,
            Expires = DateTimeOffset.UtcNow.AddYears(1),
            SameSite = SameSiteMode.Lax
        });

        TempData["Success"] = "Profil mis à jour.";

        return RedirectToAction(nameof(Index));
    }
}
