#!/usr/bin/env zsh

set -euo pipefail

pgContainerName='pgsql'
dbPort=5432
dbUser='webshop'
dbPass='webshop'
dbName='webshop'



echo
echo "🐳 0) Stop all docker containers"
docker compose down

echo
echo "🧹 0) Cleaning slate: removing Migrations, obj, and bin directories"
rm -rf BlazorLoginDemo.Web/Migrations BlazorLoginDemo.Web/bin BlazorLoginDemo.Web/obj

echo
echo "Create fresh migrations"
dotnet ef migrations add initDb \
  --project "BlazorLoginDemo.Web/BlazorLoginDemo.Web.csproj" \
  --startup-project "BlazorLoginDemo.Web/BlazorLoginDemo.Web.csproj"

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
echo "Run migrations with docker.cli tool"



dotnet ef database update \
  --project "BlazorLoginDemo.Web.csproj" \
  --startup-project "BlazorLoginDemo.Web.csproj"