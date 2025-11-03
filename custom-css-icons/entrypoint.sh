#!/bin/sh
set -e
# Ensure mount point exists
mkdir -p /app
# Run the baked tool from /tool while CWD is your mounted /app
exec python /tool/fontawesome-dl/fontawesome-dl.py "$@"
