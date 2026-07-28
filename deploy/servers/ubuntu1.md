# Serveur UBUNTU1 — configuration GitHub (AgriActifs)

> **Org GitHub :** [`groupegisebs`](https://github.com/groupegisebs)  
> (ComptaDoc est sous `BedigaCorps` — ses secrets org **ne s’appliquent pas** ici.)

Convention : **`SSH_*_UBUNTU1`** à l’organisation **groupegisebs** (ou en secret dépôt), **`UBUNTU1_*`** au dépôt.

| Serveur | ID | IP |
|---------|-----|-----|
| Ubuntu principal | `ubuntu1` | `51.79.53.197` |

---

## 1. Secrets SSH (obligatoire)

### Option A — Organisation groupegisebs (recommandé)

1. Ouvrir https://github.com/organizations/groupegisebs/settings/secrets/actions  
2. Créer / vérifier :

| Secret | Valeur |
|--------|--------|
| `SSH_PRIVATE_KEY_UBUNTU1` | Même clé privée que pour ComptaDoc/CogniDoc (multiligne) |
| `SSH_HOST_UBUNTU1` | `51.79.53.197` |
| `SSH_USER_UBUNTU1` | `ubuntu` |
| `SSH_PORT_UBUNTU1` | `22` *(secret ou variable)* |

3. Sur le secret `SSH_PRIVATE_KEY_UBUNTU1` → **Repository access** → cocher **AgriActifs** (et **ElevageActifs**).

### Option B — Secrets au niveau du dépôt

https://github.com/groupegisebs/AgriActifs/settings/secrets/actions  

Créer les mêmes noms (`SSH_PRIVATE_KEY_UBUNTU1`, `SSH_HOST_UBUNTU1`, `SSH_USER_UBUNTU1`, …) en collant les valeurs depuis BedigaCorps / votre gestionnaire de secrets.

---

## 2. Secrets applicatifs (dépôt AgriActifs)

| Secret | Valeur |
|--------|--------|
| `UBUNTU1_CONNECTION_STRING` | Chaîne PostgreSQL (schéma `agriactifs`) |
| `UBUNTU1_APP_ROOT` | `/opt/apps/agriactifs` |
| `UBUNTU1_SERVICE_NAME` | `agriactifs` |
| `UBUNTU1_LISTEN_PORT` | `5071` |
| `UBUNTU1_APP_NAME` | `AgriActifs` *(optionnel)* |

---

## Nginx Proxy Manager

| Champ | Valeur |
|-------|--------|
| Scheme | **`http`** |
| Forward Host | `172.17.0.1` |
| Forward Port | `5071` |

---

## Première fois sur le serveur

```bash
ssh ubuntu@51.79.53.197
sudo mkdir -p /opt/apps/agriactifs
sudo chown ubuntu:ubuntu /opt/apps/agriactifs
dotnet --list-runtimes   # Microsoft.AspNetCore.App 10.x
```

## Déploiement

**Actions → Deploy Production → Run workflow**
