# Cinturon360 v5

Cinturon360 v5 is a .NET 10 multi-project platform for travel operations, approvals, policy, billing, background jobs, and a Blazor web client.

## Repository Structure

- src: application projects (API, domain, infrastructure, web, jobs, contracts, data)
- tests: test projects by layer
- tools: operational tooling, including the system account admin CLI
- deploy: Dockerfiles, compose files, and deployment assets
- docs: architecture, policies, runbooks, and API docs

Main solution file:

- Cinturon360.slnx

## Tech Stack

- .NET SDK: 10.0.105 (from global.json)
- Database: PostgreSQL 17
- Container orchestration (dev): Docker Compose
- Web UI: Blazor Server

## Prerequisites

Install the following:

1. .NET SDK 10.0.105 or compatible patch
2. Docker Desktop (or Docker Engine + Compose)
3. PostgreSQL client tooling (optional)
4. Entity Framework CLI (if not already installed):

```bash
dotnet tool install --global dotnet-ef
```

## Local Development Quick Start

Start the full local stack (Postgres + migrations + services):

```bash
./dev-start.sh
```

This script will:

1. Start local Postgres from deploy/compose/compose.dev.yaml
2. Generate ordered migration SQL into tools/db/migrations
3. Apply EF migrations to local Postgres
4. Build and start the compose services

## Compose Services

Dev compose file:

- deploy/compose/compose.dev.yaml

Default local ports:

- Postgres: 5432
- API container: 5100
- Web container: 5001

Default local database credentials:

- Database: cinturon360
- Username: cinturon
- Password: cinturon_dev_password

## Build

Build the whole solution:

```bash
dotnet build Cinturon360.slnx
```

Build the system account tool project:

```bash
dotnet build tools/db/system-account/Cinturon360.SystemAccountTool.csproj
```

## Test

Run all tests:

```bash
dotnet test Cinturon360.slnx
```

Run a single test project example:

```bash
dotnet test tests/Cinturon360.Application.Tests/Cinturon360.Application.Tests.csproj
```

## System Account Tool

The system account tool is a standalone CLI for creating and managing highest-privilege platform accounts in PostgreSQL.

Tool location:

- tools/db/system-account

Detailed tool documentation:

- tools/db/system-account/README.md

### Build the Native Binary Used by the Run Command

To build the exact osx-arm64 binary that is run from:

- tools/db/system-account/bin/Release/net10.0/osx-arm64/publish/cinturon-system-account

use:

```bash
dotnet publish tools/db/system-account/Cinturon360.SystemAccountTool.csproj -c Release -r osx-arm64
```

### Run the Binary

After publishing, run:

```bash
tools/db/system-account/bin/Release/net10.0/osx-arm64/publish/cinturon-system-account create \
  --connection-string "Host=localhost;Port=5432;Database=cinturon360;Username=cinturon;Password=cinturon_dev_password" \
  --email sudo@example.com \
  --first-name Sudo \
  --last-name Admin \
  --password ChangeMeNow! \
  --email-verified
```

Other useful commands:

```bash
tools/db/system-account/bin/Release/net10.0/osx-arm64/publish/cinturon-system-account status --connection-string "Host=localhost;Port=5432;Database=cinturon360;Username=cinturon;Password=cinturon_dev_password" --email sudo@example.com
tools/db/system-account/bin/Release/net10.0/osx-arm64/publish/cinturon-system-account disable --connection-string "Host=localhost;Port=5432;Database=cinturon360;Username=cinturon;Password=cinturon_dev_password" --email sudo@example.com
tools/db/system-account/bin/Release/net10.0/osx-arm64/publish/cinturon-system-account set-password --connection-string "Host=localhost;Port=5432;Database=cinturon360;Username=cinturon;Password=cinturon_dev_password" --email sudo@example.com --password "NewPassword123!"
tools/db/system-account/bin/Release/net10.0/osx-arm64/publish/cinturon-system-account clear-password --connection-string "Host=localhost;Port=5432;Database=cinturon360;Username=cinturon;Password=cinturon_dev_password" --email sudo@example.com
```

## Notes

- The system account tool is intentionally manual and is not part of compose startup.
- For production support workflows, publish for the target runtime and pass connection strings securely.
- See tools/db/system-account/README.md for full operational details.
