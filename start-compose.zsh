#!/usr/bin/env zsh
# ──────────────────────────────────────────────────────────────────────────────
# 🎯 Version bump + full local rebuild pipeline (Zsh)
# • Parses vX.Y.Z-aN from line 11 of MainLayout.razor
# • Bumps based on --build/--patch/--minor/--major
# • Rewrites line 11 in place
# • Tears down and rebuilds DB + migrator + pgAdmin + Blazor app
# ──────────────────────────────────────────────────────────────────────────────

set -euo pipefail

# ── 🔧 Config: container names, DB creds, and target file ─────────────────────
pgContainerName='pgsql'
aspContainerName='blazor'
pgadminContainerName='pgadmin'
dbPort=5432
dbUser='demodb'
dbPass='YourAppPassword'
dbName='demodb'
FILE="BlazorLoginDemo.Web/Components/Layout/MainLayout.razor"

# ── 🧭 Usage helper ───────────────────────────────────────────────────────────
usage() {
  echo "Usage: $(basename "$0") [--build | --patch | --minor | --major]"
  exit 1
}

# ── 🏁 Parse action flag ──────────────────────────────────────────────────────
[[ $# -eq 1 ]] || usage
ACTION="$1"

# ── 🔎 Extract current version from line 11 ───────────────────────────────────
# Expected: '            <pre><code>vX.Y.Z-aN</code></pre>'
# Note: using GNU sed explicitly (gsed)
LINE=$(gsed -n '12p' "$FILE")

# Pull out "X.Y.Z-aN" from the code tag
VER=$(echo "$LINE" | gsed -E 's/.*<code>v([0-9]+\.[0-9]+\.[0-9]+-[abr][0-9]+)<\/code>.*/\1/')
if [[ -z "${VER:-}" ]]; then
  echo "❌ Could not parse version on line 11. Found:"
  echo "   $LINE"
  exit 2
fi

# ── 🧩 Split into components: X, Y, Z, letter, N ──────────────────────────────
X=$(echo "$VER" | cut -d. -f1)
Y=$(echo "$VER" | cut -d. -f2)
REST=$(echo "$VER" | cut -d. -f3)       # e.g. '22-a11'

Z="${REST%-[abr]*}"                      # before '-a11' => '22'
SUFFIX="${REST#*-}"                      # after  '-'    => 'a11'
LETTER="${SUFFIX%%[0-9]*}"               # 'a' (or 'b'/'r')
N="${SUFFIX#$LETTER}"                    # '11'

# ── ✅ Sanity checks ──────────────────────────────────────────────────────────
[[ "$LETTER" =~ '^(a|b|r)$' ]] || { echo "❌ Unexpected release letter: $LETTER"; exit 3; }
[[ "$X" =~ '^[0-9]+$' && "$Y" =~ '^[0-9]+$' && "$Z" =~ '^[0-9]+$' && "$N" =~ '^[0-9]+$' ]] || {
  echo "❌ Parsed numbers look wrong: X=$X Y=$Y Z=$Z N=$N"
  exit 4
}

# ── 🧮 Apply bump rules ───────────────────────────────────────────────────────
# --build : N += 1
# --patch : Z += 1, N = 0
# --minor : Y += 1, Z = 0, N = 0
# --major : X += 1, Y = 0, Z = 0, N = 0
case "$ACTION" in
  --build)
    echo "🔧 Bumping build number (N) only…"
    N=$((N + 1))
    ;;
  --patch)
    echo "🩹 Bumping patch (Z) and resetting N…"
    Z=$((Z + 1))
    N=0
    ;;
  --minor)
    echo "📦 Bumping minor (Y) and resetting Z, N…"
    Y=$((Y + 1))
    Z=0
    N=0
    ;;
  --major)
    echo "🚀 Bumping major (X) and resetting Y, Z, N…"
    X=$((X + 1))
    Y=0
    Z=0
    N=0
    ;;
  *)
    usage
    ;;
esac

NEW_VER="${X}.${Y}.${Z}-${LETTER}${N}"
NEW_LINE="            <pre><code>v${NEW_VER}</code></pre>"

# ── ✍️ Replace only line 11 with the new version ──────────────────────────────
awk -v repl="$NEW_LINE" 'NR==11{$0=repl} {print}' "$FILE" > "${FILE}.tmp" && mv "${FILE}.tmp" "$FILE"
echo "✅ Updated version to: v${NEW_VER}"

# ── 🐳 0) Stop all docker containers ──────────────────────────────────────────
echo
echo "🐳 0) Stop all docker containers"
docker compose down -v

# ── 🧹 1) Clean slate: migrations, obj, bin, and blazorlogin* volumes ────────
echo
echo "🧹 1) Cleaning slate: removing Migrations, obj, bin, and blazorlogin* volumes"
rm -rf BlazorLoginDemo.Web/Migrations BlazorLoginDemo.Web/bin BlazorLoginDemo.Web/obj || true

vols="$(docker volume ls -q --filter name=blazorlogin || true)"
if [[ -z "$vols" ]]; then
  echo "   (no blazorlogin* volumes)"
else
  echo "$vols" | xargs -n1 docker volume rm -f
fi

# ── 🐳 2) Start DB only ───────────────────────────────────────────────────────
echo
echo "🐳 2) Starting DB only"
docker compose up -d db

# ── ⏳ 3) Wait for Postgres to be healthy ─────────────────────────────────────
echo
echo "⏳ 3) Waiting for Postgres to be healthy..."
deadline=$((SECONDS + 180))
state=""
while :; do
  # If you don't set container_name in compose, use: pgId=$(docker compose ps -q db) and inspect "$pgId"
  if ! state=$(docker inspect --format '{{.State.Health.Status}}' "$pgContainerName" 2>/dev/null); then
    state="unknown"
  fi
  echo "   - health: $state"
  [[ "$state" == "healthy" ]] && break
  (( SECONDS >= deadline )) && { echo "❌ Postgres did not become healthy in time." >&2; exit 1; }
  sleep 2
done
echo "✅ Postgres is healthy (container: $pgContainerName, port: $dbPort)"

# ── 🏗 4) Run migrations with migrator (blocks, returns exit code) ────────────
echo
echo "🏗  4) Run migrations with migrator (blocks until done, returns exit code)"
docker compose up --build --exit-code-from migrator migrator

# ── 📊 5) Start pgAdmin ──────────────────────────────────────────────────────
echo
echo "📊 5) Start pgAdmin"
docker compose up -d pgadmin

# ── 🌱 6) Seed the DB with additional SQL (after migrations) ─────────────────
echo
echo "🌱 6) Seed the DB with additional SQL (after migrations)"
docker cp .docker/db/sql/01_seed_identity.sql "$pgContainerName":/seed_identity.sql
docker exec -i "$pgContainerName" \
  psql "postgresql://$dbUser:$dbPass@127.0.0.1:$dbPort/$dbName?sslmode=disable" \
  -v ON_ERROR_STOP=1 -f /seed_identity.sql

# ── 🚀 7) Start Blazor app ───────────────────────────────────────────────────
echo
echo "🚀 7) Start Blazor app"
docker compose up -d blazor

# ── 📤 8) Commit & push version bump ──────────────────────────────────────────
echo
echo "📤 8) Commit & push version bump to Git"
git add .
git commit -m "bump v${NEW_VER}"
git push

# ── 🏁 Done ───────────────────────────────────────────────────────────────────
echo
echo "✅ Done."
