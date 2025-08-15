#!/usr/bin/env zsh

set -euo pipefail

pgContainerName='pgsql'
aspContainerName='blazor'
pgadminContainerName='pgadmin'
dbPort=5432
dbUser='postgres'
dbPass='YourDbPassword'
dbName='demodb'


echo
echo "🐳 0) Stop all docker containers"
docker compose down

echo
echo "🧹 0) Cleaning slate: removing Migrations, obj, and bin directories"
if (Test-Path BlazorLoginDemo.Web/Migrations) { rm -rf BlazorLoginDemo.Web/Migrations }
if (Test-Path BlazorLoginDemo.Web/bin) { rm -rf BlazorLoginDemo.Web/bin }
if (Test-Path BlazorLoginDemo.Web/obj) { rm -rf BlazorLoginDemo.Web/obj }

echo
echo "🐳 0) Restarting Docker stack (db only)"
docker compose up -d db

echo
echo "⏳ Waiting for Postgres to be healthy..."
deadline=$((SECONDS + 180))
state=""
while :; do
  if ! state=$(docker inspect --format '{{.State.Health.Status}}' "$pgContainerName" 2>/dev/null); then
    state="unknown"
  fi
  echo "   - health: $state"
  if [[ "$state" == "healthy" ]]; then
    break
  fi
  if (( SECONDS >= deadline )); then
    echo "❌ Postgres did not become healthy in time." >&2
    exit 1
  fi
  sleep 2
done
echo "✅ Postgres is healthy (container: $pgContainerName, port: $dbPort)"



echo
echo "Run migrations with docker.migrator tool"
docker compose up migrator -d

# wait for this to exit (sleep 10 seconds, don't now how to test it?)


echo
echo "start pgadmin"
docker compose up pgadmin -d

echo
echo "Seed the DB with additional sql stuff, now db has been migrated"
docker cp .docker/db/sql/01_seed_identity.sql pgsql:/seed_identity.sql

docker exec -e PGPASSWORD=$dbPass -it $dbName \
  psql -U $dbUser -d $dbName -v "ON_ERROR_STOP=1" -f /seed_identity.sql