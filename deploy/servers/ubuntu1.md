# Serveur UBUNTU1 — configuration GitHub (AgriActifs)

Convention : **`SSH_*_UBUNTU1`** à l’organisation, **`UBUNTU1_*`** au dépôt.

| Serveur | ID | IP |
|---------|-----|-----|
| Ubuntu principal | `ubuntu1` | `51.79.53.197` |

---

## Organisation BedigaCorps

**https://github.com/organizations/BedigaCorps/settings/secrets/actions**

| Secret | Valeur |
|--------|--------|
| `SSH_PRIVATE_KEY_UBUNTU1` | Clé privée deploy (multiligne) |
| `SSH_HOST_UBUNTU1` | `51.79.53.197` |
| `SSH_USER_UBUNTU1` | `ubuntu` |
| `SSH_PORT_UBUNTU1` | `22` |

**Repository access** → autoriser le dépôt **AgriActifs**.

---

## Dépôt AgriActifs — Secrets

| Secret | Valeur |
|--------|--------|
| `UBUNTU1_CONNECTION_STRING` | Chaîne PostgreSQL (même host BD, schéma `agriactifs`) |
| `UBUNTU1_APP_ROOT` | `/opt/apps/agriactifs` |
| `UBUNTU1_SERVICE_NAME` | `agriactifs` |
| `UBUNTU1_LISTEN_PORT` | `5051` |
| `UBUNTU1_APP_NAME` | `AgriActifs` *(optionnel)* |

---

## Nginx Proxy Manager

| Champ | Valeur |
|-------|--------|
| Scheme | **`http`** |
| Forward Host | `172.17.0.1` |
| Forward Port | `5051` |

---

## Première fois sur le serveur

```bash
ssh ubuntu@51.79.53.197
sudo mkdir -p /opt/apps/agriactifs
sudo chown ubuntu:ubuntu /opt/apps/agriactifs
dotnet --list-runtimes   # Microsoft.AspNetCore.App 10.x
```

## Déploiement

Push sur `main` / `master`, ou **Actions → Deploy Production → Run workflow**.
