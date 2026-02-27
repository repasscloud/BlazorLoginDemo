#!/usr/bin/env bash
set -euo pipefail

SITE_DIR="${SITE_DIR:-/site}"

# If site isn't initialized yet, generate a clean Docsy module site.
if [[ ! -f "${SITE_DIR}/hugo.toml" ]]; then
  /docker/docs/init.sh
fi

cd "${SITE_DIR}"

# Install node deps if needed (kept in a named volume)
if [[ -f package.json ]] && [[ ! -d node_modules ]]; then
  npm install
fi

exec hugo server \
  --bind 0.0.0.0 \
  --port 1313 \
  --baseURL "http://localhost:1313/" \
  --disableFastRender \
  -D
