#!/usr/bin/env bash
set -Eeuo pipefail

DB_HOST="${DB_HOST:-db}"
DB_PORT="${DB_PORT:-5432}"
DB_USER="${DB_USER:-postgres}"
DB_NAME="${DB_NAME:-demodb}"
DB_PASSWORD="${DB_PASSWORD:-YourDbPassword}"
export PGPASSWORD="$DB_PASSWORD"   # handy if you later call psql

echo "⏳ Waiting for Postgres at ${DB_HOST}:${DB_PORT}..."
for i in {1..60}; do
  if pg_isready -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$DB_NAME" >/dev/null 2>&1; then
    echo "✅ Postgres is ready."
    break
  fi
  sleep 2
done

if ! pg_isready -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$DB_NAME" >/dev/null 2>&1; then
  echo "❌ DB not ready in time." >&2
  exit 1
fi

CONNECTION="Host=${DB_HOST};Port=${DB_PORT};Database=${DB_NAME};Username=${DB_USER};Password=${DB_PASSWORD};"
echo "🏗  Applying migrations..."
dotnet ef database update \
  -p ./BlazorLoginDemo.Web/BlazorLoginDemo.Web.csproj \
  -s ./BlazorLoginDemo.Web/BlazorLoginDemo.Web.csproj \
  --connection "$CONNECTION" \
  --no-build

echo "✅ Migrations applied."
