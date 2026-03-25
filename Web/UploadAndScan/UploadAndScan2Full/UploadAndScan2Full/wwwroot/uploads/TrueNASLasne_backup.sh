#!/bin/bash

# ── USER CONFIG ─────────────────────────────────────────────
SERVER_URL="https://localhost"           										# or your TrueNAS IP
API_KEY="1-ymmqsDuaFO7Wa3KWZOvpcaCHRT5US8QJ24yjwfMP1wp3F8NpxyR47tm1TpQZKaJl"	# Key
SEC_SEED=false                            										# include password secret seed
BACKUP_LOCATION="/mnt/Main/Config/TrueNASLasne"									# backup location
MAX_NUMBER_OF_FILES=30                         									# keep last 30 daily backups
# ─────────────────────────────────────────────────────────────

echo "Starting backup..."

# --- 1. Retrieve Version Number ---
VERSION_DIR=$(cat /etc/version | cut -d' ' -f1)
echo "TrueNAS Version $VERSION_DIR..."
BACKUP_VERSION_LOCATION="${BACKUP_LOCATION}/${VERSION_DIR}"
echo "Backing up to $BACKUP_VERSION_LOCATION..."
mkdir -p "$BACKUP_VERSION_LOCATION"

if [ "$SEC_SEED" = true ]; then
    fileExt="tar"
else
    fileExt="db"
fi

fileName=$(hostname)-TrueNAS-$(date +%Y%m%d%H%M%S).$fileExt

curl --no-progress-meter --insecure \
  -X 'POST' \
  "${SERVER_URL}/api/v2.0/config/save" \
  -H "Authorization: Bearer ${API_KEY}" \
  -H "accept: */*" \
  -H "Content-Type: application/json" \
  -d "{\"secretseed\": ${SEC_SEED}}" \
  --output "${BACKUP_VERSION_LOCATION}/${fileName}"

echo "Config saved to ${BACKUP_VERSION_LOCATION}/${fileName}"

# Cleanup old backups
if [ "${MAX_NUMBER_OF_FILES}" -ne 0 ]; then
    nrOfFiles=$(ls -l "${BACKUP_VERSION_LOCATION}" | grep -c "^-.*")
    if [ "${MAX_NUMBER_OF_FILES}" -lt "${nrOfFiles}" ]; then
        nFileToRemove=$((nrOfFiles - MAX_NUMBER_OF_FILES))
        while [ $nFileToRemove -gt 0 ]; do
            fileToRemove=$(ls -t "${BACKUP_VERSION_LOCATION}" | tail -1)
            rm "${BACKUP_VERSION_LOCATION}/${fileToRemove}"
            nFileToRemove=$((nFileToRemove - 1))
        done
    fi
fi

echo "DONE!"