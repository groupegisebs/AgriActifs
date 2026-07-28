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
git add -A
git commit -m "Initial commit: AgriActifs MVP (ferme agricole)"
# Créer le repo vide sur github.com puis :
git remote add origin https://github.com/VOTRE_ORG/AgriActifs.git
git push -u origin main
```

## Déploiement serveur (GitHub Actions)

Même pipeline que ComptaDoc. Voir [`deploy/servers/ubuntu1.md`](deploy/servers/ubuntu1.md).

Sur le serveur une fois :

```bash
sudo mkdir -p /opt/apps/agriactifs && sudo chown ubuntu:ubuntu /opt/apps/agriactifs
```

Secrets dépôt : `UBUNTU1_APP_ROOT=/opt/apps/agriactifs`, `UBUNTU1_SERVICE_NAME=agriactifs`, `UBUNTU1_LISTEN_PORT=5051`, `UBUNTU1_CONNECTION_STRING=...`

## Modules

Area `Ferme` : Dashboard, Exploitations, Parcelles, Actifs (export CSV), Stocks, Maintenance, Fournisseurs.

## Documentation

- [Cahier des charges](docs/CAHIER_DES_CHARGES.md)
- [Vision famille FermeActifs](../FERMEACTIFS_VISION.md)
