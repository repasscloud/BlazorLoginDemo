#!/usr/bin/env zsh
# ──────────────────────────────────────────────────────────────────────────────
# 🎯 Version bump + full local rebuild pipeline (Zsh)
# • Parses vX.Y.Z-aN from **line 12** of MainLayout.razor
# • Bumps based on --build/--patch/--minor/--major
# • Rewrites only the version inside <code>…</code> on line 12 (preserves quotes/indent)
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
POSTGRES_USER=postgres
POSTGRES_PASSWORD=YourAdminPassword
LINE_NUMBER=12  # keep this in one place

# ── 🧭 Usage helper ───────────────────────────────────────────────────────────
usage() {
  echo "Usage: $(basename "$0") [--build | --patch | --minor | --major]"
  exit 1
}

# ── 🏁 Parse action flag ──────────────────────────────────────────────────────
[[ $# -eq 1 ]] || usage
ACTION="$1"

# ── 🔎 Extract current version from line 12 ───────────────────────────────────
# Expected to find: <code>vX.Y.Z-aN</code> (quotes/indent may surround it)
# Uses GNU sed (gsed).
LINE="$(gsed -n "${LINE_NUMBER}p" "$FILE")"

# Pull out "X.Y.Z-aN" from the code tag
VER="$(echo "$LINE" | gsed -nE 's~.*<code>v([0-9]+\.[0-9]+\.[0-9]+-[abr][0-9]+)</code>.*~\1~p')"
if [[ -z ${VER:-} ]]; then
  echo "❌ Could not parse version on line ${LINE_NUMBER}. Found:"
  echo "   $LINE"
  exit 2
fi

# ── 🧩 Split into components: X, Y, Z, letter, N ──────────────────────────────
X="$(echo "$VER" | cut -d. -f1)"
Y="$(echo "$VER" | cut -d. -f2)"
REST="$(echo "$VER" | cut -d. -f3)"      # e.g. '22-a11'

Z="${REST%-[abr]*}"                      # before '-a11' => '22'
SUFFIX="${REST#*-}"                      # after  '-'    => 'a11'
LETTER="${SUFFIX%%[0-9]*}"               # 'a' (or 'b'/'r')
N="${SUFFIX#$LETTER}"                    # '11'

# ── ✅ Sanity checks ──────────────────────────────────────────────────────────
[[ "$LETTER" =~ ^(a|b|r)$ ]] || { echo "❌ Unexpected release letter: $LETTER"; exit 3; }
[[ "$X" =~ ^[0-9]+$ && "$Y" =~ ^[0-9]+$ && "$Z" =~ ^[0-9]+$ && "$N" =~ ^[0-9]+$ ]] || {
  echo "❌ Parsed numbers look wrong: X=$X Y=$Y Z=$Z N=$N"
  exit 4
}

# ── 🧮 Apply bump rules ───────────────────────────────────────────────────────
# --build : N += 1
# --patch : Z += 1, N = 0
# --minor : Y += 1, Z = 0, N = 0
# --major : X += 1, Y = 0, Z = 0, N = 0
case "$ACTION" in
  --web)
    echo "🔧 Bumping build number (N) only…"
    N=$((N + 1))
    ;;
  --build)
    echo "🔧 Bumping build number (N) only…"
    N=$((N + 1))
    ;;
  --patch)
    echo "🩹 Bumping patch (Z) and resetting N…"
    Z=$((Z + 1)); N=0
    ;;
  --minor)
    echo "📦 Bumping minor (Y) and resetting Z, N…"
    Y=$((Y + 1)); Z=0; N=0
    ;;
  --major)
    echo "🚀 Bumping major (X) and resetting Y, Z, N…"
    X=$((X + 1)); Y=0; Z=0; N=0
    ;;
  *)
    usage
    ;;
esac

NEW_VER="${X}.${Y}.${Z}-${LETTER}${N}"

# ── ✍️ Replace only the version inside <code>…</code> on line 12 ──────────────
# This keeps leading/trailing quotes, indentation, and does NOT touch <pre>.
gsed -i -E "${LINE_NUMBER}s~(<code>)v[0-9]+\.[0-9]+\.[0-9]+-[abr][0-9]+(</code>)~\1v${NEW_VER}\2~" "$FILE"
echo "✅ Updated version to: v${NEW_VER}"

# # ── 🐳 0) Build only ────────────----------------──────────────────────────────
case "$ACTION" in
  --web)
    c_TIME=$(date +"%Y-%m-%d_%H-%M-%S")
    git add .
    git commit -m "$c_TIME"
    docker compose up -d --build "$aspContainerName"
    exit 0
    ;;
  *)
      ;;
esac


# ── 🐳 0) Stop all docker containers ──────────────────────────────────────────
echo
echo "🐳 0) Stop all docker containers"
docker compose down -v --remove-orphans # --rmi all
docker buildx prune --force

# ── 🧹 1) Clean slate: migrations, obj, bin, and blazorlogin* volumes ────────
echo
echo "🧹 1) Cleaning slate: removing Migrations, obj, bin, and blazorlogin* volumes"
rm -rf BlazorLoginDemo.Web/bin BlazorLoginDemo.Web/obj BlazorLoginDemo.Web/Migrations || true
rm -rf BlazorLoginDemo.Api/bin BlazorLoginDemo.Api/obj || true
rm -rf BlazorLoginDemo.Shared/bin BlazorLoginDemo.Shared/obj || true

vols="$(docker volume ls -q --filter name=blazorlogindemo_postgresql || true)"
if [[ -z "$vols" ]]; then
  echo "   (no blazorlogindemo_postgresql* volumes)"
else
  echo "$vols" | xargs -n1 docker volume rm -f
fi

# ── 🐳 2a) Pull containers ───────────────────────────────────────────────────────
echo
echo "🐳 2a) Pull containers"
docker pull postgres:18.0-alpine3.22
docker pull dpage/pgadmin4
docker pull mcr.microsoft.com/dotnet/sdk:9.0
docker pull mcr.microsoft.com/dotnet/aspnet:9.0-bookworm-slim

# ── 🐳 2b) Start DB only ───────────────────────────────────────────────────────
echo
echo "🐳 2b) Starting DB only"
docker compose up -d db

# ── ⏳ 3) Wait for Postgres to be healthy ─────────────────────────────────────
echo
echo "⏳ 3) Waiting for Postgres to be healthy..."
deadline=$((SECONDS + 180))
state=""
while :; do
  if ! state="$(docker inspect --format '{{.State.Health.Status}}' "$pgContainerName" 2>/dev/null)"; then
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
docker cp .docker/db/sql/01_seed_serilog.sql "$pgContainerName":/seed_serilog.sql
docker exec -i "$pgContainerName" \
  psql "postgresql://$POSTGRES_USER:$POSTGRES_PASSWORD@127.0.0.1:$dbPort/$dbName?sslmode=disable" \
  -v ON_ERROR_STOP=1 -f /seed_serilog.sql

# ── 🚀 7) Start Blazor app ───────────────────────────────────────────────────
echo
echo "🚀 7) Start Blazor app"
docker compose up -d "$aspContainerName"

# ── 🚀 8) Start Api app ───────────────────────────────────────────────────
echo
echo "🚀 8) Start Api app"
docker compose up -d api

# ── 🌱 9) Seed the DB with airport data ──────────────────────────────────
echo
echo "🌱 9) Seed the DB with airport data"
pwsh -File .scripts/import-airports.ps1 -CsvPath .scripts/data/airports.csv -Batch 500

# ── 📤 10) Commit & push version bump ──────────────────────────────────────────
echo
echo "📤 10) Commit & push version bump to Git"
git add .
git commit -m "bump v${NEW_VER}"
git push

# ── 🌱 11) Seed the DB with additional data ──────────────────────────────────
echo
echo "🌱 11) Seed the DB with additional data"
curl -X 'GET' \
  'http://localhost:8090/api/v1/test/create-org-data' \
  -H 'accept: */*'
docker cp .scripts/sql/update_organizations_from_csv_v2.sql "$pgContainerName":/update_organizations_from_csv_v2.sql
docker exec -i "$pgContainerName" \
  psql "postgresql://$dbUser:$dbPass@127.0.0.1:$dbPort/$dbName?sslmode=disable" \
  -v ON_ERROR_STOP=1 -f /update_organizations_from_csv_v2.sql
docker cp .scripts/sql/link_organizations_hierarchy.sql "$pgContainerName":/link_organizations_hierarchy.sql
docker exec -i "$pgContainerName" \
  psql "postgresql://$dbUser:$dbPass@127.0.0.1:$dbPort/$dbName?sslmode=disable" \
  -v ON_ERROR_STOP=1 -f /link_organizations_hierarchy.sql
curl -X 'GET' \
  'http://localhost:8090/api/v1/test/create-user-data' \
  -H 'accept: */*'
curl -X 'GET' \
  'http://localhost:8090/api/v1/test/create-org-license' \
  -H 'accept: */*'
pwsh -File .scripts/import-error-codes-seed.v3.ps1
region_country_data_import_log=".docker/db/pwsh/import.log"
[[ -e "$region_country_data_import_log" ]] && rm -f -- "$region_country_data_import_log"
pwsh -File .docker/db/pwsh/01-import-regions.ps1
pwsh -File .docker/db/pwsh/02-import-continents.ps1
pwsh -File .docker/db/pwsh/03-import-countries.ps1

# ── 🐳 12) Start crontab and pgweb ──────────────────────────────────────────
echo
echo "🐳 12) Start crontab and pgweb"
docker compose up -d --build crontab
docker compose up -d pgweb

# ── 🏁 Done ───────────────────────────────────────────────────────────────────
echo
echo "✅ Done."
