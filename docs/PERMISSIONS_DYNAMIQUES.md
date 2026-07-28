# Permissions dynamiques configurables (UI SuperAdmin)

## Principe

**Aucun rôle n'est codé en dur dans les vues** (pas de `<AuthorizeView Roles="Admin">`).

Toutes les autorisations sont :

1. **Définies** dans le catalogue (`PermissionCatalog` — seed initial uniquement)
2. **Stockées** en base de données (`PermissionDefinitions`, `RolePermissionGrants`)
3. **Configurées** via l'interface graphique SuperAdmin
4. **Évaluées** à l'exécution depuis la BD (avec cache)

```
SuperAdmin (UI) → PermissionDefinitions + RolePermissionGrants (PostgreSQL)
                              ↓
              DynamicPermissionService (runtime)
                              ↓
         Controllers [Authorize(Policy)] + TagHelper <gise-permission>
```

## Tables BD

| Table | Contenu |
|-------|---------|
| `PermissionDefinitions` | Code, Resource, Action, PropertyName, DisplayName |
| `RolePermissionGrants` | RoleId + PermissionId + IsGranted |

## Interface graphique

| URL | Description |
|-----|-------------|
| `/Admin/PermissionMatrix` | Matrice complète + édition par rôle |
| `/Admin/PermissionMatrix/EditRole/{roleId}` | Cocher/décocher toutes les permissions d'un rôle |
| `/Admin/PermissionMatrix/Resources` | Liste des modèles (User, Role, Users…) |
| `/Admin/PermissionMatrix/Model/{resource}` | Vue propriétés du modèle |

Accès : permission `Permissions.Manage` (SuperAdmin + Admin par défaut).

## Niveaux de permission

### Niveau entité (modèle)

```
Users.View, Users.Create, Users.Edit, Users.Delete
Roles.View, Reports.Export, Settings.Manage …
```

### Niveau propriété

```
User.Email.View, User.Email.Edit
User.FirstName.View, UserProfile.Company.Edit …
```

## Usage dans les vues (sans rôle codé en dur)

```html
<gise-permission resource="User" action="Edit" property="Email">
    <input asp-for="Email" class="form-control" />
</gise-permission>

<gise-permission code="Users.Create">
    <a asp-action="Create" class="btn btn-primary">Nouveau</a>
</gise-permission>
```

## Usage dans les controllers

Les controllers gardent des **codes de permission** (pas des rôles) :

```csharp
[Authorize(Policy = AppPolicies.UsersView)]
public class UsersController : AdminControllerBase { }
```

La policy vérifie la BD via `DynamicPermissionService`.

## Ajouter un nouveau modèle

1. Ajouter les entrées dans `PermissionCatalog.All` (seed)
2. Relancer l'app → `EnsureCatalogSeededAsync()` insère les nouvelles permissions
3. Configurer les rôles via `/Admin/PermissionMatrix`
4. Protéger les vues avec `<gise-permission resource="MonModele" …>`

## SuperAdmin — seul rôle par défaut

Au démarrage, **un seul rôle** est créé : `SuperAdmin` (avec toutes les permissions).

Tous les autres rôles (`Admin`, `Manager`, `User`…) sont **créés manuellement** par le SuperAdmin via **Admin > Rôles**, puis configurés via **Admin > Permissions**.
