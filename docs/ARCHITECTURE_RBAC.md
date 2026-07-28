# Architecture RBAC — GISEBS Identity Framework

## Terminologie

| Terme | Signification |
|-------|---------------|
| **Identity-Based Application** | Application construite sur ASP.NET Core Identity |
| **RBAC** | Role-Based Access Control — contrôle d'accès basé sur les rôles |
| **Claims** | Attributs portés par l'utilisateur (via ses rôles) |
| **Policies** | Règles d'autorisation déclaratives dans ASP.NET Core |
| **Resources** | Fonctionnalités protégées (controllers, actions, API) |

## Chaîne d'autorisation

```
┌─────────────┐
│  Utilisateur │  ApplicationUser (Identity)
└──────┬──────┘
       │ appartient à
       ▼
┌─────────────┐
│    Rôles     │  SuperAdmin, Admin, Manager, User…
└──────┬──────┘
       │ portent des claims
       ▼
┌─────────────┐
│ Permissions  │  Users.View, Roles.Edit, Audit.View…
└──────┬──────┘
       │ protègent
       ▼
┌─────────────┐
│  Ressources  │  UsersController, ReportsController…
└─────────────┘
```

## Implémentation dans le template

### 1. Utilisateurs → Rôles (Identity natif)

```csharp
await userManager.AddToRoleAsync(user, AppRoles.Admin);
var roles = await userManager.GetRolesAsync(user);
```

Tables Identity : `AspNetUsers`, `AspNetRoles`, `AspNetUserRoles`

### 2. Rôles → Permissions (Claims)

Chaque rôle reçoit des claims `permission` au seed :

```csharp
await roleManager.AddClaimAsync(role, new Claim("permission", Permissions.UsersView));
```

Service : `PermissionService.EnsureRolePermissionsAsync()`

### 3. Permissions → Ressources (Policies)

```csharp
// Enregistrement (Program.cs / Extensions)
options.AddPolicy(AppPolicies.UsersView, policy =>
    policy.Requirements.Add(new PermissionRequirement(Permissions.UsersView)));

// Utilisation sur les controllers
[Authorize(Policy = AppPolicies.UsersView)]
public class UsersController : AdminControllerBase { }
```

Handler : `PermissionAuthorizationHandler` — SuperAdmin bypass total.

### 4. Accès module par rôle (RBAC classique)

Pour les zones entières, le pattern `[Authorize(Roles = "...")]` reste le standard :

```csharp
[Authorize(Roles = AppRoles.AdminPanelRoles)]  // "SuperAdmin,Admin,Manager,Auditor,ReportViewer"
public class DashboardController : AdminControllerBase { }

[Authorize(Roles = AppRoles.FullAdminRoles)]   // "SuperAdmin,Admin"
public class SettingsController : AdminControllerBase { }
```

Constantes : `Constants/AppRoles.cs`

## Matrice simplifiée

| Rôle | Admin module | Users | Roles | Reports | Settings | Audit |
|------|:------------:|:-----:|:-----:|:-------:|:--------:|:-----:|
| SuperAdmin | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| Admin | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| Manager | ✅ | 👁️✏️ | 👁️ | 👁️📤 | ❌ | ✅ |
| Auditor | ✅ | 👁️ | 👁️ | 👁️ | ❌ | ✅ |
| ReportViewer | ✅ | ❌ | ❌ | 👁️📤 | ❌ | ❌ |
| User | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ |

👁️ = View · ✏️ = Edit · 📤 = Export

## Fichiers clés

| Fichier | Rôle |
|---------|------|
| `Constants/AppRoles.cs` | Définition des rôles RBAC |
| `Constants/Permissions.cs` | Permissions + matrice rôle→permission |
| `Constants/Resources.cs` | Mapping permission → ressource |
| `Constants/Policies.cs` | Noms des policies ASP.NET Core |
| `Authorization/PermissionAuthorizationHandler.cs` | Vérification des claims |
| `Areas/Admin/Controllers/AdminControllerBase.cs` | Base RBAC niveau 1 (rôles) |
| `Data/SeedData.cs` | Seed rôles, permissions, SuperAdmin |

## Pourquoi RBAC + Permissions (et pas rôles seuls) ?

| Approche | Avantage | Limite |
|----------|----------|--------|
| Rôles seuls `[Authorize(Roles="Admin")]` | Simple, lisible | Rigide, explosion de rôles |
| RBAC + Permissions (claims/policies) | Granulaire, évolutif | Légèrement plus complexe |

**Recommandation GISEBS** : les deux niveaux combinés — rôles pour les modules, permissions pour les actions.

## Réutilisation (MetaDoc, WarrantySafe…)

1. Cloner le template `AgriActifs.Web`
2. Renommer le projet (`GISEBS.MetaDoc.Web`, etc.)
3. Ajouter vos permissions métier dans `Constants/Permissions.cs`
4. Étendre la matrice dans `Permissions.RolePermissions`
5. Protéger vos controllers avec `[Authorize(Policy = ...)]`
6. Conserver Identity + RBAC inchangés

C'est la fondation standard des applications ASP.NET Core d'entreprise modernes.
