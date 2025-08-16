#!/usr/bin/env bash
set -Eeuo pipefail

DB_HOST="${DB_HOST:-db}"
DB_PORT="${DB_PORT:-5432}"
DB_USER="${DB_USER:-demodb}"
DB_NAME="${DB_NAME:-demodb}"
DB_PASSWORD="${DB_PASSWORD:-YourDbPassword}"
export PGPASSWORD="$DB_PASSWORD"

echo "⏳ Waiting for Postgres at ${DB_HOST}:${DB_PORT}..."
for i in {1..60}; do
  pg_isready -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$DB_NAME" >/dev/null 2>&1 && { echo "✅ Postgres is ready."; break; }
  sleep 2
done
pg_isready -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$DB_NAME" >/dev/null 2>&1 || { echo "❌ DB not ready in time."; exit 1; }

CONNECTION="Host=${DB_HOST};Port=${DB_PORT};Database=${DB_NAME};Username=${DB_USER};Password=${DB_PASSWORD};"

echo "🧱  Adding migration: initDb (if missing)..."
if ! ls ./Migrations/*initDb*.cs >/dev/null 2>&1; then
  dotnet ef migrations add initDb \
    -p ./BlazorLoginDemo.Web.csproj
else
  echo "   - initDb already exists, skipping."
fi

echo "🏗  Applying migrations..."
dotnet ef database update \
  -p ./BlazorLoginDemo.Web.csproj \
  --connection "$CONNECTION"

echo "✅ Migrations applied."
