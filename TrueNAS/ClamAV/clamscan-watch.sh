#!/bin/bash

WATCH_DIR="/mnt/Data/Files"
SCAN_DIR="/scandir/files"
LOG_DIR="/logs"
LOG_DIR_HOST="/mnt/Data/Apps/ClamAVLogs"
QUARANTINE_BASE="/quarantine"  
QUARANTINE_BASE_HOST="/mnt/Data/Apps/ClamAVQuarantine"
EMAIL="stef+clamav@samayas.eu"
HOSTNAME=$(hostname)

send_mail() {
    local subject="$1"
    local body="$2"
    python3 -c "
import json, subprocess, sys
data = {'subject': sys.argv[1], 'text': sys.argv[2], 'to': [sys.argv[3]]}
r = subprocess.run(['midclt', 'call', 'mail.send', json.dumps(data)])
sys.exit(r.returncode)
" "$subject" "$body" "$EMAIL"
    local exit_code=$?
    echo "$(date): midclt exit: $exit_code" >> "$LOG_DIR_HOST/watcher.log"
    return $exit_code
}

rm -f /tmp/clamav-scan-*.lock

mkdir -p "$LOG_DIR_HOST"
mkdir -p "$QUARANTINE_BASE_HOST"
chmod 700 "$QUARANTINE_BASE_HOST"

echo "$(date): ────────────────────────────────────────" >> "$LOG_DIR_HOST/watcher.log"
echo "$(date): Watcher started on $WATCH_DIR"           >> "$LOG_DIR_HOST/watcher.log"

# ─── inotifywait real-time loop ───────────────────────────────────────
inotifywait -m -r \
    -e close_write \
    -e moved_to \
    --format '%w%f' \
    "$WATCH_DIR" | while read NEWFILE
do
    # Skip directories
    [ -d "$NEWFILE" ] && continue

    # Skip if file no longer exists
    [ ! -f "$NEWFILE" ] && continue
	
    # Skip incomplete/temp files
    [[ "$NEWFILE" == *.tmp ]]            && continue
    [[ "$NEWFILE" == *.part ]]           && continue
    [[ "$NEWFILE" == *.!qb ]]            && continue
    [[ "$NEWFILE" == *.crdownload ]]     && continue

	# Dedup lock via temp file
    LOCKFILE="/tmp/clamav-scan-$(echo "$NEWFILE" | md5sum | cut -d' ' -f1).lock"
    [ -f "$LOCKFILE" ] && continue
    touch "$LOCKFILE"
	
    TIMESTAMP=$(date +%Y-%m-%d_%H-%M-%S)
    LOG_FILE="$LOG_DIR/clamav-scan-$TIMESTAMP.log"
    LOG_FILE_HOST="$LOG_DIR_HOST/clamav-scan-$TIMESTAMP.log"
    QUARANTINE_DIR="$QUARANTINE_BASE/$TIMESTAMP"
    QUARANTINE_DIR_HOST="$QUARANTINE_BASE_HOST/$TIMESTAMP"

    # Map host path to container path
    CONTAINER_FILE="${NEWFILE/$WATCH_DIR/$SCAN_DIR}"

    echo "$(date): Scanning: $NEWFILE" >> "$LOG_DIR_HOST/watcher.log"

    # Pre-create quarantine dir on host (container will write into it)
    mkdir -p "$QUARANTINE_DIR_HOST"
    chmod 700 "$QUARANTINE_DIR_HOST"

    # Run scan: infected files are moved to quarantine inside container
    docker exec ix-clamav-clamav-1 clamscan -zri --no-summary \
        --log="$LOG_FILE" \
        --move="$QUARANTINE_DIR" \
        "$CONTAINER_FILE"
    EXIT_CODE=$?

    # Lock down any quarantined files immediately
    find "$QUARANTINE_DIR_HOST" -type f -exec chmod 000 {} \;

    case $EXIT_CODE in
        0)
            # Clean scan — remove empty quarantine dir, no email
            rmdir "$QUARANTINE_DIR_HOST" 2>/dev/null
            echo "$(date): Clean: $NEWFILE" >> "$LOG_DIR_HOST/watcher.log"
            ;;
        1)
            if [ -f "$LOG_FILE_HOST" ] && grep -q "FOUND" "$LOG_FILE_HOST"; then
                INFECTED_FILES=$(grep "FOUND" "$LOG_FILE_HOST")
                LOG_CONTENT=$(cat "$LOG_FILE_HOST")
                echo "$(date): ⚠️ VIRUS FOUND in $NEWFILE" >> "$LOG_DIR_HOST/watcher.log"
                echo "$(date): $INFECTED_FILES"            >> "$LOG_DIR_HOST/watcher.log"

                send_mail \
                    "⚠️ ClamAV: Virus found on $HOSTNAME" \
                    "ClamAV detected threats on $HOSTNAME at $(date).

File scanned  : $NEWFILE
Quarantined to: $QUARANTINE_DIR_HOST

Threats found:
$INFECTED_FILES

--- Full scan log ---
$LOG_CONTENT"
            else
                # Exit 1 but no FOUND lines — unexpected state
                echo "$(date): ⚠️ Exit 1 but no FOUND lines for $NEWFILE" >> "$LOG_DIR_HOST/watcher.log"
                send_mail \
                    "❌ ClamAV: Scan error (unexpected exit=1) on $HOSTNAME" \
                    "ClamAV returned exit code 1 but no FOUND lines were present.
File: $NEWFILE
Check container/log path. Expected log: $LOG_FILE_HOST"
            fi
            ;;
        2)      
            ;;
    esac
	
	# ── Release lock ──────────────────────────  ← HERE, after case, before done
    rm -f "$LOCKFILE"
done