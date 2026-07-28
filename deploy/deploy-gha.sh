#!/usr/bin/env bash
# Déploiement depuis GitHub Actions vers Ubuntu (systemd).
# Variables requises : voir deploy/servers/ubuntu1.md

set -euo pipefail

: "${SSH_HOST:?SSH_HOST requis}"
: "${SSH_USER:?SSH_USER requis}"
: "${APP_ROOT:?APP_ROOT requis}"
: "${SERVICE_NAME:?SERVICE_NAME requis}"
: "${DLL_NAME:=AgriActifs.Web.dll}"
: "${LISTEN_PORT:=5071}"
: "${CONNECTION_STRING:?CONNECTION_STRING requis}"
: "${DATABASE_PROVIDER:=PostgreSQL}"
: "${DATABASE_SCHEMA:=agriactifs}"
: "${PUBLISH_DIR:=publish}"
: "${SSH_PORT:=22}"
: "${APP_NAME:=AgriActifs}"

sanitize() {
  printf '%s' "$1" | tr -d '\r\n\t' | sed -e 's/^[[:space:]]*//' -e 's/[[:space:]]*$//' -e 's/^"//' -e 's/"$//' -e "s/^'//" -e "s/'$//"
}

SSH_HOST=$(sanitize "${SSH_HOST}")
SSH_HOST="${SSH_HOST#http://}"
SSH_HOST="${SSH_HOST#https://}"
SSH_HOST="${SSH_HOST%%/*}"
SSH_USER=$(sanitize "${SSH_USER}")
SSH_PORT=$(sanitize "${SSH_PORT}")
APP_ROOT=$(sanitize "${APP_ROOT}")
SERVICE_NAME=$(sanitize "${SERVICE_NAME}")
LISTEN_PORT=$(sanitize "${LISTEN_PORT}")
CONNECTION_STRING=$(sanitize "${CONNECTION_STRING}")
APP_NAME=$(sanitize "${APP_NAME}")
DLL_NAME=$(sanitize "${DLL_NAME}")
DATABASE_SCHEMA=$(sanitize "${DATABASE_SCHEMA}")
DATABASE_PROVIDER=$(sanitize "${DATABASE_PROVIDER}")

if [[ ! "$SSH_HOST" =~ ^[0-9a-zA-Z.-]+$ ]]; then
  echo "SSH_HOST invalide — IP ou hostname seul (ex. 51.79.53.197)" >&2
  exit 1
fi
if [[ ! "$SSH_PORT" =~ ^[0-9]+$ ]]; then
  echo "SSH_PORT invalide (ex. 22)" >&2
  exit 1
fi
if [[ ! "$SSH_USER" =~ ^[a-zA-Z0-9._-]+$ ]]; then
  echo "SSH_USER invalide (ex. ubuntu)" >&2
  exit 1
fi

APP_DIR="${APP_ROOT}/app"
BACKUP_DIR="${APP_ROOT}/backups"
STAGING_REMOTE="/tmp/${SERVICE_NAME}-gha-$(date +%Y%m%d-%H%M%S)"
SSH_OPTS=(-p "${SSH_PORT}" -o BatchMode=yes -o StrictHostKeyChecking=yes)
SCP_OPTS=(-P "${SSH_PORT}" -o BatchMode=yes -o StrictHostKeyChecking=yes)
SSH_TARGET="${SSH_USER}@${SSH_HOST}"

if [[ -n "${SSH_KEY_PATH:-}" ]]; then
  SSH_OPTS+=(-i "${SSH_KEY_PATH}")
  SCP_OPTS+=(-i "${SSH_KEY_PATH}")
fi

[[ -f "${PUBLISH_DIR}/${DLL_NAME}" ]] || { echo "Assembly introuvable : ${PUBLISH_DIR}/${DLL_NAME}" >&2; exit 1; }

ssh "${SSH_OPTS[@]}" "${SSH_TARGET}" "sudo mkdir -p '${APP_DIR}' '${BACKUP_DIR}' && sudo chown -R ${SSH_USER}:${SSH_USER} '${APP_ROOT}'"

ssh "${SSH_OPTS[@]}" "${SSH_TARGET}" bash -s <<REMOTE_BACKUP
set -eu
TIMESTAMP=\$(date +%Y%m%d-%H%M%S)
if [[ -d '${APP_DIR}' ]] && [[ -n "\$(ls -A '${APP_DIR}' 2>/dev/null || true)" ]]; then
  sudo cp -a '${APP_DIR}' '${BACKUP_DIR}/'\${TIMESTAMP}
  echo "Sauvegarde : ${BACKUP_DIR}/\${TIMESTAMP}"
else
  echo "Aucune version precedente."
fi
REMOTE_BACKUP

ssh "${SSH_OPTS[@]}" "${SSH_TARGET}" "mkdir -p '${STAGING_REMOTE}'"
scp "${SCP_OPTS[@]}" -r "${PUBLISH_DIR}/." "${SSH_TARGET}:${STAGING_REMOTE}/"

ENV_FILE="$(mktemp)"
{
  printf 'ConnectionStrings__DefaultConnection=%s\n' "${CONNECTION_STRING}"
  printf 'Database__Provider=%s\n' "${DATABASE_PROVIDER}"
  printf 'Database__Schema=%s\n' "${DATABASE_SCHEMA}"
} > "${ENV_FILE}"
scp "${SCP_OPTS[@]}" "${ENV_FILE}" "${SSH_TARGET}:/tmp/${SERVICE_NAME}.app.env"
rm -f "${ENV_FILE}"

ssh "${SSH_OPTS[@]}" "${SSH_TARGET}" bash -s <<REMOTE_DEPLOY
set -eu
PROD_SETTINGS='${APP_DIR}/appsettings.Production.json'
SAVED_SETTINGS=''
if [[ -f "\${PROD_SETTINGS}" ]]; then
  SAVED_SETTINGS=\$(mktemp)
  cp "\${PROD_SETTINGS}" "\${SAVED_SETTINGS}"
fi

echo "Arret ${SERVICE_NAME} avant remplacement des fichiers..."
sudo systemctl stop '${SERVICE_NAME}' || true
sleep 1

sudo rsync -a --delete \
  --exclude 'appsettings.Production.json' \
  --exclude 'app.env' \
  '${STAGING_REMOTE}/' '${APP_DIR}/'
if [[ -n "\${SAVED_SETTINGS}" ]] && [[ -f "\${SAVED_SETTINGS}" ]]; then
  sudo cp "\${SAVED_SETTINGS}" "\${PROD_SETTINGS}"
  rm -f "\${SAVED_SETTINGS}"
fi
if [[ -f "\${PROD_SETTINGS}" ]]; then
  python3 - <<'PY' "\${PROD_SETTINGS}" '${DATABASE_SCHEMA}' '${DATABASE_PROVIDER}'
import json, sys
path, expected, provider = sys.argv[1], sys.argv[2], sys.argv[3]
with open(path, encoding='utf-8') as f:
    data = json.load(f)
db = data.setdefault('Database', {})
current = (db.get('Schema') or '').strip()
if current != expected:
    db['Schema'] = expected
    db.setdefault('Provider', provider)
    with open(path, 'w', encoding='utf-8') as f:
        json.dump(data, f, indent=2, ensure_ascii=False)
        f.write('\n')
    print(f"appsettings.Production.json : Database.Schema corrige ({current or 'vide'} -> {expected})")
PY
fi
sudo mv "/tmp/${SERVICE_NAME}.app.env" "${APP_DIR}/app.env"
sudo chmod 600 "${APP_DIR}/app.env"
sudo chown ${SSH_USER}:${SSH_USER} "${APP_DIR}/app.env"
sudo chown -R ${SSH_USER}:${SSH_USER} '${APP_DIR}'
rm -rf '${STAGING_REMOTE}'
REMOTE_DEPLOY

SERVICE_FILE="/tmp/${SERVICE_NAME}.service"
cat > "${SERVICE_FILE}" <<EOF
[Unit]
Description=${APP_NAME}
After=network.target

[Service]
Type=simple
User=${SSH_USER}
Group=${SSH_USER}
WorkingDirectory=${APP_DIR}
ExecStart=/usr/bin/dotnet ${APP_DIR}/${DLL_NAME}
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://0.0.0.0:${LISTEN_PORT}
Environment=Database__Provider=${DATABASE_PROVIDER}
Environment=Database__Schema=${DATABASE_SCHEMA}
EnvironmentFile=-${APP_DIR}/app.env
Restart=always
RestartSec=5
TimeoutStartSec=120
KillSignal=SIGINT
TimeoutStopSec=30
SyslogIdentifier=${SERVICE_NAME}
NoNewPrivileges=true
PrivateTmp=true

[Install]
WantedBy=multi-user.target
EOF

scp "${SCP_OPTS[@]}" "${SERVICE_FILE}" "${SSH_TARGET}:/tmp/${SERVICE_NAME}.service"
rm -f "${SERVICE_FILE}"

ssh "${SSH_OPTS[@]}" "${SSH_TARGET}" "sudo cp '/tmp/${SERVICE_NAME}.service' '/etc/systemd/system/${SERVICE_NAME}.service' && rm -f '/tmp/${SERVICE_NAME}.service'"
ssh "${SSH_OPTS[@]}" "${SSH_TARGET}" "sudo systemctl daemon-reload && sudo systemctl enable ${SERVICE_NAME} && sudo systemctl start ${SERVICE_NAME}"

echo "Attente démarrage ${SERVICE_NAME} sur le port ${LISTEN_PORT}..."
ssh "${SSH_OPTS[@]}" "${SSH_TARGET}" bash -s <<REMOTE_HEALTH
set -eu
PORT='${LISTEN_PORT}'
for i in \$(seq 1 45); do
  if curl -fsS -o /dev/null "http://127.0.0.1:\${PORT}/" 2>/dev/null; then
    echo "HTTP 200 après \${i} tentative(s)"
    exit 0
  fi
  if ! systemctl is-active --quiet '${SERVICE_NAME}'; then
    echo "Service inactif — logs récents :"
    journalctl -u '${SERVICE_NAME}' -n 15 --no-pager || true
  fi
  sleep 2
done
echo "::error::${SERVICE_NAME} ne répond pas après 90 s"
journalctl -u '${SERVICE_NAME}' -n 30 --no-pager || true
exit 1
REMOTE_HEALTH

echo "Deploiement reussi sur ${SSH_HOST} (${SERVICE_NAME})."
