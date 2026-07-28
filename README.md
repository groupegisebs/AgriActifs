# AgriActifs

Gestion des **actifs d’une ferme agricole** (parcelles, matériel, stocks, maintenance) — écosystème GISEBS.

## Stack

ASP.NET Core 10 MVC · Identity RBAC · EF Core · PostgreSQL (schéma `agriactifs`) · Bootstrap 5

## Démarrage

```powershell
cd AgriActifs
dotnet ef database update --project src/AgriActifs.Web
dotnet run --project src/AgriActifs.Web --launch-profile https
```

URL : https://localhost:7121 (http://localhost:5051)

## Comptes démo

| Email | Mot de passe | Rôle |
|-------|--------------|------|
| `superadmin@agriactifs.local` | `Agri@Secure2026!` | SuperAdmin |
| `admin@agriactifs.local` | `Agri@Admin2026!` | Admin |
| `gerant@fermedeserables.demo` | `Demo@Agri2026!` | Gérant — Ferme des Érables |
| `tech@fermedeserables.demo` | `Demo@Agri2026!` | Technicien |
| `ouvrier@fermedeserables.demo` | `Demo@Agri2026!` | Ouvrier |
| `lecture@fermedeserables.demo` | `Demo@Agri2026!` | Observateur |

Désactiver le seed démo : `"Seed": { "IncludeDemoData": false }` dans `appsettings.json`.

## Configuration locale (secrets)

Ne pas committer les mots de passe. Copier :

```powershell
copy src\AgriActifs.Web\appsettings.Development.local.json.example src\AgriActifs.Web\appsettings.Development.local.json
```

Puis renseigner la chaîne PostgreSQL. Le fichier `*.local.json` est ignoré par Git.

## Dépôt GitHub (2 repos séparés)

```powershell
cd AgriActifs
git init -b main   # déjà fait si vous avez suivi la prep
git add -A
git commit -m "Initial commit: AgriActifs MVP (ferme agricole)"
# Créer le repo vide sur github.com puis :
git remote add origin https://github.com/VOTRE_ORG/AgriActifs.git
git push -u origin main
```

## Modules

Area `Ferme` : Dashboard, Exploitations, Parcelles, Actifs (export CSV), Stocks, Maintenance, Fournisseurs.

## Documentation

- [Cahier des charges](docs/CAHIER_DES_CHARGES.md)
- [Vision famille FermeActifs](../FERMEACTIFS_VISION.md)
