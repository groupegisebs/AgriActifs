using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgriActifs.Web.Areas.Admin.Controllers;

/// <summary>
/// Contrôleur de base Admin — accès contrôlé par permissions (BD), pas par liste de rôles codée en dur.
/// </summary>
[Area("Admin")]
[Authorize]
public abstract class AdminControllerBase : Controller;
