#!/usr/bin/env bash
set -Eeuo pipefail

# --- Config ---
containerName='webshop-db'
dbName='webshop'
dbUser='webshop'
dbPass='webshop'
dbPort=5432
pgImage='postgres:15'
# --------------

echo "▶️  Ensuring Postgres container '$containerName' is running..."

# Remove old container if present (running or exited)
if docker ps -a --format '{{.Names}}' | grep -Fxq "$containerName"; then
  docker rm -f "$containerName" >/dev/null 2>&1 || true
fi

# Run postgres with a healthcheck so we can wait properly
docker run -d --rm \
  --name "$containerName" \
  -e POSTGRES_DB="$dbName" \
  -e POSTGRES_USER="$dbUser" \
  -e POSTGRES_PASSWORD="$dbPass" \
  -p "${dbPort}:5432" \
  --health-cmd="pg_isready -h 127.0.0.1 -U $dbUser -d $dbName -q || exit 1" \
  --health-interval=2s \
  --health-timeout=3s \
  --health-retries=60 \
  "$pgImage" >/dev/null

echo "⏳ Waiting for Postgres to be healthy..."
deadline=$((SECONDS + 180))
state=""
while :; do
  if ! state=$(docker inspect --format '{{.State.Health.Status}}' "$containerName" 2>/dev/null); then
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

echo "✅ Postgres is healthy (container: $containerName, port: $dbPort)"

docker run -d --name pgadmin \
  --restart unless-stopped \
  -p 5050:80 \
  -e PGADMIN_DEFAULT_EMAIL=admin@localhost.com \
  -e PGADMIN_DEFAULT_PASSWORD=admin \
  dpage/pgadmin4

docker exec -i "$containerName" psql -U "$dbUser" -d "$dbName" -v ON_ERROR_STOP=1 -q <<'SQL'
CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
  "MigrationId"   varchar(150) NOT NULL,
  "ProductVersion" varchar(32) NOT NULL,
  CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);
SQL

echo "✅ __EFMigrationsHistory ensured in database '$dbName'."

rm -rf /Users/danijeljw/Developer/Ava-Warren-Tests/BlazorLoginDemo/BlazorLoginDemo.Web/Migrations

dotnet ef migrations add initDb \
  --project "/Users/danijeljw/Developer/Ava-Warren-Tests/BlazorLoginDemo/BlazorLoginDemo.Web/BlazorLoginDemo.Web.csproj" \
  --startup-project "/Users/danijeljw/Developer/Ava-Warren-Tests/BlazorLoginDemo/BlazorLoginDemo.Web/BlazorLoginDemo.Web.csproj"

dotnet ef database update \
  --project "/Users/danijeljw/Developer/Ava-Warren-Tests/BlazorLoginDemo/BlazorLoginDemo.Web/BlazorLoginDemo.Web.csproj" \
  --startup-project "/Users/danijeljw/Developer/Ava-Warren-Tests/BlazorLoginDemo/BlazorLoginDemo.Web/BlazorLoginDemo.Web.csproj"


docker cp /Users/danijeljw/Developer/Ava-Warren-Tests/BlazorLoginDemo/HashGen/seed_identity.sql webshop-db:/seed_identity.sql

docker exec -e PGPASSWORD=webshop -it webshop-db \
  psql -U webshop -d webshop -v "ON_ERROR_STOP=1" -f /seed_identity.sql