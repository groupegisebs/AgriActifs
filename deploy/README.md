# Déploiement AgriActifs (serveur Linux)

Même modèle que ComptaDoc / GISERBAC-TEMPLATE.

## Fichiers

| Fichier | Commité ? | Rôle |
|---------|-----------|------|
| `project.config.example.json` | Oui | Modèle app |
| `project.config.json` | Non | Config locale deploy-all |
| `deploy-all.config.example.json` | Oui | Modèle SSH |
| `deploy-all.config.json` | Non | IP / user SSH |
| `deploy-gha.sh` | Oui | Déploiement GitHub Actions |
| `gha-env.sh` | Oui | Sanitisation secrets GHA |
| `.github/workflows/deploy-production.yml` | Oui | Pipeline CI/CD |

## GitHub Actions (recommandé)

Guide détaillé : [`servers/ubuntu1.md`](servers/ubuntu1.md)

Secrets dépôt à créer :

- `UBUNTU1_APP_ROOT` = `/opt/apps/agriactifs`
- `UBUNTU1_SERVICE_NAME` = `agriactifs`
- `UBUNTU1_LISTEN_PORT` = `5071`
- `UBUNTU1_CONNECTION_STRING` = chaîne PostgreSQL

Secrets org (partagés) : `SSH_PRIVATE_KEY_UBUNTU1`, `SSH_HOST_UBUNTU1`, `SSH_USER_UBUNTU1`.

## Déploiement local Windows → Ubuntu

```powershell
copy deploy\project.config.example.json deploy\project.config.json
copy deploy\deploy-all.config.example.json deploy\deploy-all.config.json
# Éditer deploy-all.config.json (ServerHost, SshUser)
.\deploy\deploy-all.ps1
```

## Cibles serveur

| Paramètre | Valeur |
|-----------|--------|
| Service systemd | `agriactifs` |
| Répertoire | `/opt/apps/agriactifs` |
| Port | `5071` |
| Schéma PostgreSQL | `agriactifs` |
| DLL | `AgriActifs.Web.dll` |
