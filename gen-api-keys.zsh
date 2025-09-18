#!/usr/bin/env zsh
# gen-api-keys.zsh — generate header-safe API keys for X-Ava-ApiKey
# Defaults: 5 keys, 32 random bytes each, Base64URL format.

set -euo pipefail

# --- config defaults ---
COUNT=5        # how many keys
BYTES=32       # entropy per key (32 bytes ≈ 256 bits)
FORMAT="b64url"  # b64url | hex | both

usage() {
  cat <<EOF
Usage: $0 [-n COUNT] [-b BYTES] [-f FORMAT]
  -n COUNT   number of keys to generate (default: $COUNT)
  -b BYTES   random bytes per key (default: $BYTES)
  -f FORMAT  b64url | hex | both (default: $FORMAT)
Examples:
  $0                  # 5 Base64URL keys, 32 bytes each
  $0 -n 10 -b 48      # 10 Base64URL keys, 48 bytes each
  $0 -f hex           # 5 hex keys (64 chars for 32 bytes)
  $0 -f both -n 3     # 3 lines, showing both formats
EOF
}

# parse flags
while getopts ":n:b:f:h" opt; do
  case "$opt" in
    n) COUNT="$OPTARG" ;;
    b) BYTES="$OPTARG" ;;
    f) FORMAT="$OPTARG" ;;
    h) usage; exit 0 ;;
    \?) echo "Unknown option: -$OPTARG" >&2; usage; exit 1 ;;
    :)  echo "Missing argument for -$OPTARG" >&2; usage; exit 1 ;;
  esac
done

# deps check
command -v openssl >/dev/null 2>&1 || { echo "openssl not found"; exit 1; }

rand_b64url() { openssl rand -base64 "$1" | tr '+/' '-_' | tr -d '='; }
rand_hex()    { openssl rand -hex    "$1"; }

case "$FORMAT" in
  b64url|B64URL|b64|B64)
    for i in {1..$COUNT}; do
      print -- "$(rand_b64url "$BYTES")"
    done
    ;;
  hex|HEX)
    for i in {1..$COUNT}; do
      print -- "$(rand_hex "$BYTES")"
    done
    ;;
  both|BOTH)
    printf "%-56s  |  %s\n" "Base64URL" "Hex"
    printf "%-56s--+--%s\n" ${(l:56::-:)} ${(l:64::-:)}
    for i in {1..$COUNT}; do
      local k1="$(rand_b64url "$BYTES")"
      local k2="$(rand_hex "$BYTES")"
      printf "%-56s  |  %s\n" "$k1" "$k2"
    done
    ;;
  *)
    echo "Invalid -f FORMAT: $FORMAT (use b64url|hex|both)"; exit 1 ;;
esac
