#!/bin/bash

TIMESTAMP=$(date +%Y-%m-%d_%H-%M)
SCAN_DIR="<scandir>"
LOG_DIR="/logs"
LOG_DIR_HOST="<logdir>"   
QUARANTINE_DIR="/quarantine/$TIMESTAMP"          
QUARANTINE_DIR_HOST="<quarantinedir>/$TIMESTAMP"                     
LOG_FILE="$LOG_DIR/clamav-scan-$TIMESTAMP.log"
LOG_FILE_HOST="$LOG_DIR_HOST/clamav-scan-$TIMESTAMP.log"
EMAIL="<email>"
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
    echo "midclt exit: $exit_code"
    return $exit_code
}

mkdir -p "$QUARANTINE_DIR_HOST"
chmod 700 "$QUARANTINE_DIR_HOST"

docker exec ix-clamav-clamav-1 clamscan -zri --no-summary --log="$LOG_FILE" --move="$QUARANTINE_DIR" "$SCAN_DIR"
EXIT_CODE=$?

find "$QUARANTINE_DIR_HOST" -type f -exec chmod 000 {} \;

case $EXIT_CODE in
  0)
	rmdir "$QUARANTINE_DIR_HOST" 2>/dev/null;
    # Clean: no email. [page:1]
    exit 0
    ;;
  1)
    if [ -f "$LOG_FILE_HOST" ] && grep -q "FOUND" "$LOG_FILE_HOST"; then
      INFECTED_FILES=$(grep "FOUND" "$LOG_FILE_HOST")
      LOG_CONTENT=$(cat "$LOG_FILE_HOST")
      send_mail \
        "⚠️ ClamAV: Virus found on $HOSTNAME" \
        "ClamAV detected the following threats on $HOSTNAME at $(date):

$INFECTED_FILES

--- Full scan log ---
$LOG_CONTENT"
    else
      # Exit 1 but no FOUND lines => unexpected state
      send_mail \
        "❌ ClamAV: Scan error (unexpected exit=1) on $HOSTNAME" \
        "ClamAV returned exit code 1 but no 'FOUND' lines were present.
Check container/log path. Expected log: $LOG_FILE_HOST"
    fi
    ;;
  2)
     send_mail \
      "❌ ClamAV: Scan error on $HOSTNAME" \
      "ClamAV encountered an error during the scan on $HOSTNAME at $(date).
Check the log file at: $LOG_FILE_HOST"
    ;;
  *)
    # Docker exec failure or other unexpected exit code
    send_mail \
      "❌ ClamAV: Scan command failed (exit=$EXIT_CODE) on $HOSTNAME" \
      "The scan command failed with exit code $EXIT_CODE on $HOSTNAME at $(date).
Check Docker/container status and paths.
Expected log file: $LOG_FILE_HOST"
    ;;
esac
