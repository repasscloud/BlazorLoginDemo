#!/usr/bin/env zsh

set -euo pipefail

pgContainerName='pgsql'
aspContainerName='blazor'
pgadminContainerName='pgadmin'
dbPort=5432
dbUser='demodb'
dbPass='YourAppPassword'
dbName='demodb'

echo
echo "🐳 0) Stop all docker containers"
docker compose down -v

echo
echo "🧹 1) Cleaning slate: removing Migrations, obj, bin, and blazorlogin* volumes"
rm -rf BlazorLoginDemo.Web/Migrations BlazorLoginDemo.Web/bin BlazorLoginDemo.Web/obj || true

vols="$(docker volume ls -q --filter name=blazorlogin || true)"
if [[ -z "$vols" ]]; then
  echo "   (no blazorlogin* volumes)"
else
  echo "$vols" | xargs -n1 docker volume rm -f
fi


echo
echo "🐳 2) Starting DB only"
docker compose up -d db

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

echo
echo "🏗 4) Run migrations with migrator (blocks until done, returns exit code)"
# Brings up migrator, waits for it to finish, exits with its code
docker compose up --build --exit-code-from migrator migrator

echo
echo "📊 5) Start pgAdmin"
docker compose up -d pgadmin

echo
echo "🌱 6) Seed the DB with additional SQL (after migrations)"
docker cp .docker/db/sql/01_seed_identity.sql "$pgContainerName":/seed_identity.sql
docker exec -i "$pgContainerName" \
  psql "postgresql://$dbUser:$dbPass@127.0.0.1:$dbPort/$dbName?sslmode=disable" \
  -v ON_ERROR_STOP=1 -f /seed_identity.sql

# Optional: start app
echo
echo "🚀 7) Start Blazor app"
docker compose up -d blazor

echo
echo "✅ Done."