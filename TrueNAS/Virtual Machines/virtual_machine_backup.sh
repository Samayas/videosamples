#!/bin/bash
set -euo pipefail

ZFS="/usr/sbin/zfs"
DATASET="Data/Vms/TestWindowsServer2025-z8ig8c"
OUTDIR="/mnt/Data/Files/BackupVMs"     
STAMP="$(date +%F_%H%M%S)"
SNAP="backup-${STAMP}"
SNAPFULL="${DATASET}@${SNAP}"
OUTFILE="${OUTDIR}/$(echo "${DATASET}" | tr '/' '_')@${SNAP}.zfs"

mkdir -p "${OUTDIR}"

cleanup() {
  # If the snapshot exists, remove it.
  if "${ZFS}" list -t snapshot "${SNAPFULL}" >/dev/null 2>&1; then
    echo "Removing snapshot: ${SNAPFULL}"
    sudo "${ZFS}" destroy "${SNAPFULL}"
  fi
}
trap cleanup EXIT

echo "Creating snapshot: ${DATASET}@${SNAP}"
sudo "${ZFS}" snapshot "${DATASET}@${SNAP}"

echo "Snapshot created. Latest snapshots for ${DATASET}:"
"${ZFS}" list -t snapshot -r "${DATASET}" | tail -n 10

echo "Sending snapshot to file: ${OUTFILE}"
sudo "${ZFS}" send "${DATASET}@${SNAP}" > "${OUTFILE}"

echo "Backup completed: ${OUTFILE}"
echo "Snapshot will be removed automatically."