#!/usr/bin/env bash
set -euo pipefail

COMPOSE_FILE="deploy/compose/compose.dev.yaml"
DATA_PROJECT="src/Cinturon360.Data"
API_PROJECT="src/Cinturon360.Api"
SQL_OUT="tools/db/migrations"
PG_CONN="Host=localhost;Port=5432;Database=cinturon360;Username=cinturon;Password=cinturon_dev_password"

# ── Step 1: Start Postgres ─────────────────────────────────────────────────
echo "▶ Starting Postgres..."
docker compose -f "$COMPOSE_FILE" up -d postgres

echo "  Waiting for Postgres to be healthy..."
until docker compose -f "$COMPOSE_FILE" exec -T postgres \
    pg_isready -U cinturon -d cinturon360 &>/dev/null; do
  sleep 1
done
echo "  Postgres ready."

# ── Step 2: Generate numbered SQL migration scripts ────────────────────────
echo "▶ Generating SQL migration scripts → $SQL_OUT"
mkdir -p "$SQL_OUT"

# Collect ordered migration names from EF into a temp file
MIGRATIONS_TMP=$(mktemp)
dotnet ef migrations list \
  --project "$DATA_PROJECT" \
  --startup-project "$API_PROJECT" \
  --no-color 2>/dev/null \
  | grep -E '^[0-9]{14}_' \
  | awk '{print $1}' > "$MIGRATIONS_TMP"

PREV="0"
IDX=1
while IFS= read -r MIGRATION; do
  PADDED=$(printf "%03d" "$IDX")
  OUTFILE="$SQL_OUT/${PADDED}_${MIGRATION}.sql"

  if [[ ! -f "$OUTFILE" ]]; then
    echo "  Generating $OUTFILE"
    dotnet ef migrations script "$PREV" "$MIGRATION" \
      --project "$DATA_PROJECT" \
      --startup-project "$API_PROJECT" \
      --output "$OUTFILE" \
      --idempotent \
      --no-build 2>/dev/null
  else
    echo "  Skipping $OUTFILE (already exists)"
  fi

  PREV="$MIGRATION"
  IDX=$((IDX + 1))
done < "$MIGRATIONS_TMP"
rm -f "$MIGRATIONS_TMP"
echo "  SQL scripts up to date."

# ── Step 3: Apply migrations to local Postgres ─────────────────────────────
echo "▶ Applying EF migrations to local database..."
dotnet ef database update \
  --project "$DATA_PROJECT" \
  --startup-project "$API_PROJECT" \
  --connection "$PG_CONN"

# ── Step 4: Start remaining services ──────────────────────────────────────
echo "▶ Building and starting all services..."
docker compose -f "$COMPOSE_FILE" up --build

