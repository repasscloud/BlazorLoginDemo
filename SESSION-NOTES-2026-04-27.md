# Session Notes - 2026-04-27

## What changed this session

- Updated dev compose Postgres image usage and mount behavior for Postgres 18.
- Fixed Postgres 18 volume mount path in compose:
  - from `/var/lib/postgresql/data`
  - to `/var/lib/postgresql`
- Confirmed API startup is not auto-running EF migrations.
- Updated `dev-start.sh` migration flow to apply generated SQL scripts with `psql` instead of `dotnet ef database update`.

## Current migration policy

- Generate migrations from code changes in `src/Cinturon360.Data`.
- Export ordered/idempotent SQL scripts to `tools/db/migrations`.
- Apply SQL scripts to DB explicitly (managed migration workflow).
- API container should be runtime only (start/stop), not schema mutator.

## Commands for creating a new migration and SQL

Run from project root:

```bash
# 1) Create EF migration from model changes
NNN="003"
MigrationName="YourMigrationName"
Timestamp="$(date +"%Y%m%d%H%M%S")"

dotnet ef migrations add "${MigrationName}" \
  --project src/Cinturon360.Data \
  --startup-project src/Cinturon360.Api

# 2) Generate an idempotent SQL script for that migration
dotnet ef migrations script \
  --project src/Cinturon360.Data \
  --startup-project src/Cinturon360.Api \
  --idempotent \
  --output "tools/db/migrations/${NNN}_${Timestamp}_${MigrationName}.sql"
```

Or regenerate the full ordered set through the dev helper:

```bash
./dev-start.sh
```

(That script now generates SQL files and applies SQL to Postgres via `psql`.)

## Applying SQL to local DB manually

```bash
docker compose -f deploy/compose/compose.dev.yaml up -d postgres

docker compose -f deploy/compose/compose.dev.yaml exec -T postgres \
  psql -v ON_ERROR_STOP=1 -U cinturon -d cinturon360 \
  < tools/db/migrations/<file>.sql
```

Then start API:

```bash
docker compose -f deploy/compose/compose.dev.yaml up -d api
```

## Notes

- The NU190x messages seen during restore are package vulnerability warnings, not startup failures.
- For Postgres 17 -> 18 upgrades, existing persisted volumes may require reset or dump/restore migration.
