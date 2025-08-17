#!/usr/bin/env bash
set -Eeuo pipefail

# creds (prefer DB_*, else POSTGRES_*, else hardcoded)
DB_HOST="${DB_HOST:-${POSTGRES_HOST:-db}}"
DB_PORT="${DB_PORT:-${POSTGRES_PORT:-5432}}"
DB_USER="${DB_USER:-${APP_DB_USER:-demodb}}"
DB_NAME="${DB_NAME:-${APP_DB_NAME:-demodb}}"
DB_PASSWORD="${DB_PASSWORD:-${APP_DB_PASSWORD:-YourAppPassword}}"
export PGPASSWORD="$DB_PASSWORD"

echo "⏳ Waiting for Postgres at ${DB_HOST}:${DB_PORT}..."
for i in {1..60}; do
  if pg_isready -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$DB_NAME" >/dev/null 2>&1; then
    echo "✅ Postgres is accepting connections."
    break
  fi
  sleep 2
done
pg_isready -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$DB_NAME" >/dev/null 2>&1 || { echo "❌ DB not ready in time."; exit 1; }

echo "🔐 Verifying login/DB access (SELECT 1)..."
until psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$DB_NAME" -v ON_ERROR_STOP=1 -q -c "SELECT 1;" >/dev/null 2>&1; do
  sleep 1
done
echo "✅ DB is queryable."

CONNECTION="Host=${DB_HOST};Port=${DB_PORT};Database=${DB_NAME};Username=${DB_USER};Password=${DB_PASSWORD};"

echo "🧱  Adding migration: initDb (if missing)..."
if ! ls ./Migrations/*initDb*.cs >/dev/null 2>&1; then
  echo "   - creating initDb..."
  dotnet ef migrations add initDb \
    -p ./BlazorLoginDemo.Web.csproj \
    --configuration Release \
    --framework net9.0 \
    --no-build
else
  echo "   - initDb already exists, skipping."
fi

echo "🏗  Applying migrations..."
dotnet ef database update \
  --connection "$CONNECTION" \

echo "✅ Migrations applied."
