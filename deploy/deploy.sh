#!/usr/bin/env bash
# =============================================================================
# deploy/deploy.sh — Déploiement ASP.NET MVC sur serveur Linux (systemd)
#
# Configuration projet : deploy/project.config.json (copiez depuis project.config.example.json)
#
# Modes :
#   1) Sur le serveur après git pull :
#        ./deploy/deploy.sh
#
#   2) Avec un dossier publish déjà compilé :
#        ./deploy/deploy.sh /chemin/vers/publish
#
# Variables d'environnement (surchargent project.config.json) :
#   DLL_NAME, APP_ROOT, SERVICE_NAME, DOTNET, SKIP_PUBLISH, SKIP_SYSTEMD, SKIP_HEALTHCHECK
# =============================================================================

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "${SCRIPT_DIR}/.." && pwd)"
PROJECT_CONFIG="${SCRIPT_DIR}/project.config.json"
SERVICE_TEMPLATE="${SCRIPT_DIR}/systemd.service.template"

log()  { echo "[$(date '+%Y-%m-%d %H:%M:%S')] $*"; }
die()  { log "ERREUR: $*" >&2; exit 1; }

require_cmd() {
  command -v "$1" >/dev/null 2>&1 || die "Commande introuvable : $1"
}

load_project_config() {
  [[ -f "${PROJECT_CONFIG}" ]] || die "Fichier manquant : ${PROJECT_CONFIG}. Copiez deploy/project.config.example.json vers deploy/project.config.json et adaptez-le à votre projet."

  require_cmd python3

  # shellcheck disable=SC1090
  eval "$(python3 - "${PROJECT_CONFIG}" <<'PY'
import json, shlex, sys

path = sys.argv[1]
with open(path, encoding="utf-8") as f:
    cfg = json.load(f)

required = ("appName", "serviceName", "appRoot", "dllName", "projectPath", "healthCheckUrl", "listenPort")
missing = [k for k in required if not cfg.get(k)]
if missing:
    raise SystemExit(f"Clés manquantes dans {path}: {', '.join(missing)}")

exports = {
    "APP_NAME": cfg["appName"],
    "SERVICE_NAME": cfg["serviceName"],
    "APP_ROOT": cfg["appRoot"],
    "DLL_NAME": cfg["dllName"],
    "PROJECT_REL_PATH": cfg["projectPath"],
    "HEALTHCHECK_URL": cfg["healthCheckUrl"],
    "LISTEN_PORT": str(cfg["listenPort"]),
}

for key, value in exports.items():
    print(f"export {key}={shlex.quote(str(value))}")
PY
)"
}

load_project_config

APP_DIR="${APP_ROOT}/app"
BACKUP_DIR="${APP_ROOT}/backups"
STAGING_DIR="${APP_ROOT}/staging"
DOTNET="${DOTNET:-/usr/bin/dotnet}"
SKIP_PUBLISH="${SKIP_PUBLISH:-0}"
SKIP_SYSTEMD="${SKIP_SYSTEMD:-0}"
SKIP_HEALTHCHECK="${SKIP_HEALTHCHECK:-0}"

PROJECT_CSPROJ="${REPO_ROOT}/${PROJECT_REL_PATH}"

# --- Analyse des arguments ---
PUBLISH_SOURCE=""
if [[ $# -gt 0 ]]; then
  if [[ "$1" == "--help" || "$1" == "-h" ]]; then
    echo "Usage: $0 [chemin/publish]"
    echo "Config projet : deploy/project.config.json"
    exit 0
  fi
  PUBLISH_SOURCE="$1"
  [[ -d "${PUBLISH_SOURCE}" ]] || die "Répertoire publish introuvable : ${PUBLISH_SOURCE}"
fi

# --- Vérifications préalables ---
require_cmd rsync
require_cmd curl
[[ -x "${DOTNET}" || -n "${PUBLISH_SOURCE}" ]] || die "${DOTNET} introuvable. Installez le runtime .NET ou fournissez un répertoire publish."

log "Projet : ${APP_NAME} (service ${SERVICE_NAME})"

# --- Publication (mode dépôt local uniquement) ---
if [[ -z "${PUBLISH_SOURCE}" ]]; then
  [[ -f "${PROJECT_CSPROJ}" ]] || die "Projet introuvable : ${PROJECT_CSPROJ}"
  if [[ "${SKIP_PUBLISH}" != "1" ]]; then
    log "Publication Release depuis ${PROJECT_CSPROJ}..."
    mkdir -p "${STAGING_DIR}"
    PUBLISH_DIR="${STAGING_DIR}/publish-$$"
    rm -rf "${PUBLISH_DIR}"
    "${DOTNET}" publish "${PROJECT_CSPROJ}" \
      -c Release \
      -o "${PUBLISH_DIR}" \
      --no-self-contained
    PUBLISH_SOURCE="${PUBLISH_DIR}"
    log "Publication terminée : ${PUBLISH_SOURCE}"
  else
    die "SKIP_PUBLISH=1 sans chemin publish fourni en argument."
  fi
fi

[[ -f "${PUBLISH_SOURCE}/${DLL_NAME}" ]] || die "Assembly introuvable dans publish : ${PUBLISH_SOURCE}/${DLL_NAME}"

# --- Création des répertoires système ---
log "Création des répertoires sous ${APP_ROOT}..."
sudo mkdir -p "${APP_DIR}" "${BACKUP_DIR}" "${STAGING_DIR}"

# --- Sauvegarde de la version courante ---
TIMESTAMP="$(date '+%Y%m%d-%H%M%S')"
if [[ -d "${APP_DIR}" ]] && [[ -n "$(ls -A "${APP_DIR}" 2>/dev/null || true)" ]]; then
  BACKUP_PATH="${BACKUP_DIR}/${TIMESTAMP}"
  log "Sauvegarde de l'application actuelle vers ${BACKUP_PATH}..."
  sudo cp -a "${APP_DIR}" "${BACKUP_PATH}"
  log "Sauvegarde créée."
else
  log "Aucune version précédente à sauvegarder."
fi

# --- Préservation de appsettings.Production.json ---
PROD_SETTINGS="${APP_DIR}/appsettings.Production.json"
SAVED_SETTINGS=""
if [[ -f "${PROD_SETTINGS}" ]]; then
  SAVED_SETTINGS="$(mktemp)"
  sudo cp "${PROD_SETTINGS}" "${SAVED_SETTINGS}"
  log "appsettings.Production.json existant préservé (sera restauré après copie)."
fi

# --- Déploiement des fichiers publiés ---
log "Copie des fichiers publiés vers ${APP_DIR}..."
sudo rsync -a --delete \
  --exclude 'appsettings.Production.json' \
  "${PUBLISH_SOURCE}/" "${APP_DIR}/"

if [[ -n "${SAVED_SETTINGS}" ]]; then
  sudo cp "${SAVED_SETTINGS}" "${PROD_SETTINGS}"
  rm -f "${SAVED_SETTINGS}"
  log "appsettings.Production.json restauré."
fi

if id ubuntu &>/dev/null; then
  sudo chown -R ubuntu:ubuntu "${APP_DIR}"
fi

if [[ "${PUBLISH_SOURCE}" == "${STAGING_DIR}/publish-"* ]]; then
  rm -rf "${PUBLISH_SOURCE}"
fi

# --- Installation / mise à jour du service systemd ---
if [[ "${SKIP_SYSTEMD}" != "1" ]]; then
  APP_SETTINGS="${REPO_ROOT}/$(dirname "${PROJECT_REL_PATH}")/appsettings.json"
  [[ -f "${APP_SETTINGS}" ]] || die "appsettings.json introuvable : ${APP_SETTINGS}"

  read -r DB_CONN DB_PROVIDER DB_SCHEMA < <(python3 - "${APP_SETTINGS}" <<'PY'
import json, sys

with open(sys.argv[1], encoding="utf-8") as f:
    cfg = json.load(f)

conn = cfg.get("ConnectionStrings", {}).get("DefaultConnection", "").strip()
provider = cfg.get("Database", {}).get("Provider", "").strip()
schema = cfg.get("Database", {}).get("Schema", "").strip()

if not conn:
    raise SystemExit("ConnectionStrings:DefaultConnection manquant dans appsettings.json")
if not provider or not schema:
    raise SystemExit("Database:Provider ou Database:Schema manquant dans appsettings.json")

print(conn, provider, schema)
PY
)

  log "Paramètres BD depuis appsettings.json : Provider=${DB_PROVIDER}, Schema=${DB_SCHEMA}"

  log "Installation du service systemd ${SERVICE_NAME}..."
  cat <<EOF | sudo tee "/etc/systemd/system/${SERVICE_NAME}.service" >/dev/null
[Unit]
Description=${APP_NAME}
After=network.target

[Service]
Type=simple
User=ubuntu
Group=ubuntu
WorkingDirectory=${APP_DIR}
ExecStart=/usr/bin/dotnet ${APP_DIR}/${DLL_NAME}
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://localhost:${LISTEN_PORT}
Environment=ConnectionStrings__DefaultConnection=${DB_CONN}
Environment=Database__Provider=${DB_PROVIDER}
Environment=Database__Schema=${DB_SCHEMA}
Restart=always
RestartSec=5
KillSignal=SIGINT
SyslogIdentifier=${SERVICE_NAME}
NoNewPrivileges=true
PrivateTmp=true

[Install]
WantedBy=multi-user.target
EOF

  sudo systemctl daemon-reload
  sudo systemctl enable "${SERVICE_NAME}"
  sudo systemctl restart "${SERVICE_NAME}"

  log "Statut du service ${SERVICE_NAME} :"
  sudo systemctl status "${SERVICE_NAME}" --no-pager || true
else
  log "SKIP_SYSTEMD=1 — service systemd non modifié."
fi

# --- Test de santé HTTP ---
if [[ "${SKIP_HEALTHCHECK}" != "1" ]]; then
  log "Test HTTP : ${HEALTHCHECK_URL}"
  sleep 2
  if curl -fsS -o /dev/null -w "HTTP %{http_code}\n" "${HEALTHCHECK_URL}"; then
    log "Healthcheck réussi."
  else
    log "AVERTISSEMENT : healthcheck échoué sur ${HEALTHCHECK_URL}. Vérifiez : journalctl -u ${SERVICE_NAME} -e"
    exit 1
  fi
fi

log "Déploiement terminé avec succès."
