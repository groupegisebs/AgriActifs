# Déploiement serveur Linux

Scripts génériques réutilisables — **chaque projet** configure ses propres paramètres via des fichiers JSON locaux (non commités).

## Configuration par projet

| Fichier | Rôle | Commité ? |
|---------|------|-----------|
| `deploy/project.config.example.json` | Modèle des paramètres applicatifs | ✅ Oui |
| `deploy/project.config.json` | **Votre** app (nom, service, chemins, DLL) | ❌ Non (gitignored) |
| `deploy/deploy-all.config.example.json` | Modèle serveur SSH | ✅ Oui |
| `deploy/deploy-all.config.json` | **Votre** serveur (IP, user, port) | ❌ Non (gitignored) |

### Première configuration

```powershell
copy deploy\project.config.example.json deploy\project.config.json
copy deploy\deploy-all.config.example.json deploy\deploy-all.config.json
```

Éditez **`project.config.json`** pour votre projet :

```json
{
  "appName": "MetaDoc",
  "serviceName": "metadoc",
  "appRoot": "/opt/apps/metadoc",
  "dllName": "MetaDoc.Web.dll",
  "projectPath": "src/MetaDoc.Web/MetaDoc.Web.csproj",
  "healthCheckUrl": "http://localhost:5050",
  "listenPort": 5050,
  "sshIdentityKey": "metadoc_deploy"
}
```

Éditez **`deploy-all.config.json`** pour votre serveur :

```json
{
  "ServerHost": "192.168.1.10",
  "SshUser": "ubuntu",
  "SshPort": 22
}
```

> Lors d'un fork du template GISEBS, renommez le projet .NET **et** mettez à jour `project.config.json` (`dllName`, `projectPath`, `serviceName`, `appRoot`).

## Déploiement en une commande (Windows → Ubuntu)

```bat
deploy\deploy.bat -ServerHost "192.168.1.10"
```

Ou, si `deploy-all.config.json` est configuré :

```bat
deploy\deploy.bat
```

Le script :

1. Lit `project.config.json` + `deploy-all.config.json`
2. Publie le projet (`dotnet publish`)
3. Transfère via SCP
4. Sauvegarde l'ancienne version
5. Installe le service systemd (`serviceName` du config)
6. Vérifie le healthcheck HTTP

| Option | Description |
|--------|-------------|
| `-SkipPublish` | Réutilise `./publish` existant |
| `-ConnectionString` | Surcharge la chaîne lue depuis `appsettings.json` |

### Prérequis une seule fois

1. **Clé SSH** (nom dérivé de `sshIdentityKey` dans `project.config.json`) :
   ```powershell
   ssh-keygen -t ed25519 -C "mon-projet-deploy" -f $env:USERPROFILE\.ssh\gisebs_deploy
   type $env:USERPROFILE\.ssh\gisebs_deploy.pub | ssh ubuntu@<IP> "mkdir -p ~/.ssh && cat >> ~/.ssh/authorized_keys"
   ```

2. **appsettings.json** — configurez `ConnectionStrings:DefaultConnection` et `Database` (Provider, Schema) dans `src/AgriActifs.Web/appsettings.json`. Le script de déploiement lit ces valeurs pour configurer systemd sur le serveur.

3. **Serveur Ubuntu** : runtime **.NET 10** (`dotnet --list-runtimes` → `Microsoft.AspNetCore.App 10.x`).

4. **Sudo sans mot de passe** pour `ubuntu` (adapter `SERVICE` au `serviceName` de votre config) :
   ```
   ubuntu ALL=(ALL) NOPASSWD: /bin/systemctl daemon-reload, /bin/systemctl enable SERVICE, /bin/systemctl restart SERVICE, /bin/systemctl status SERVICE, /bin/mkdir, /bin/cp, /bin/chown, /usr/bin/rsync, /usr/bin/tee
   ```

## Structure sur le serveur

Définie par `appRoot` dans `project.config.json` (exemple GISEBS) :

```
/opt/apps/gisebs-securemvc/
├── app/                  # Application publiée (WorkingDirectory systemd)
├── backups/              # Sauvegardes horodatées
└── staging/              # Publication temporaire (deploy.sh local)
```

## Première installation sur le serveur

```bash
# Adapter APP_ROOT selon project.config.json
sudo mkdir -p /opt/apps/gisebs-securemvc
sudo chown ubuntu:ubuntu /opt/apps/gisebs-securemvc

sudo mkdir -p /opt/apps/gisebs-securemvc/app
sudo cp deploy/appsettings.Production.json.example /opt/apps/gisebs-securemvc/app/appsettings.Production.json
sudo nano /opt/apps/gisebs-securemvc/app/appsettings.Production.json
```

`deploy.sh` **ne remplace jamais** `appsettings.Production.json` s'il existe déjà.

## Déploiement manuel sur le serveur

```bash
git pull
./deploy/deploy.sh
```

Avec publish précompilé :

```bash
./deploy/deploy.sh /tmp/mon-publish
```

## Service systemd

Template générique : `deploy/systemd.service.template`  
Variables substituées depuis `project.config.json` : `${APP_NAME}`, `${APP_PATH}`, `${DLL_NAME}`, `${SERVICE_NAME}`, `${LISTEN_PORT}`.

```bash
sudo systemctl status gisebs-securemvc    # serviceName de votre config
sudo journalctl -u gisebs-securemvc -f
```

## Reverse proxy (Nginx Proxy Manager)

Proxy Host → `http://127.0.0.1:<listenPort>` (défaut `5050`) + SSL Let's Encrypt.

## Variables d'environnement (deploy.sh)

Surchargent `project.config.json` :

| Variable | Description |
|----------|-------------|
| `APP_ROOT` | Racine applicative sur le serveur |
| `SERVICE_NAME` | Nom du service systemd |
| `DLL_NAME` | Assembly à exécuter |
| `SKIP_PUBLISH` | `1` = ne pas publier |
| `SKIP_SYSTEMD` | `1` = ne pas toucher au service |
| `SKIP_HEALTHCHECK` | `1` = ne pas tester HTTP |

## Dépannage

```bash
sudo journalctl -u <serviceName> -e --no-pager
curl -v http://localhost:5050/
/usr/bin/dotnet --list-runtimes

# Restaurer une sauvegarde
sudo systemctl stop <serviceName>
sudo rm -rf /opt/apps/<app>/app
sudo cp -a /opt/apps/<app>/backups/<TIMESTAMP> /opt/apps/<app>/app
sudo systemctl start <serviceName>
```

## Fichiers du dossier deploy/

| Fichier | Rôle |
|---------|------|
| `project.config.example.json` | Modèle config projet |
| `deploy-all.config.example.json` | Modèle config serveur |
| `deploy-all.ps1` / `deploy.bat` | Déploiement Windows → Ubuntu |
| `deploy.sh` | Déploiement sur le serveur Linux |
| `systemd.service.template` | Unit systemd générique |
| `appsettings.Production.json.example` | Config production serveur |
