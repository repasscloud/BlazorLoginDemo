---
title: Migrator
description: One-shot database migration container used by Compose
weight: 30
---

## Purpose

This Dockerfile defines the **migrator container**, a short-lived, one-shot container
used to apply database migrations during environment startup.

The migrator is **not** a long-running service.

It exists to:
- Apply schema migrations deterministically
- Fail fast on migration errors
- Block application startup until the database is ready

Compose uses the migrator’s **exit code** as a control signal.

---

## Why a dedicated migrator container

Separating migrations into their own container provides:

- Clear lifecycle boundaries (run once, then exit)
- Predictable failure handling
- No accidental re‑execution on app restarts
- Clean dependency ordering in Compose

This avoids common problems such as:
- Migrations running on every app restart
- Race conditions between API instances
- Hidden migration failures

---

## Build inputs

This Dockerfile is parameterised via build arguments supplied by Compose:

- `CSPROJ_WEB`
- `CSPROJ_SHARED`

The migrator intentionally reuses shared project code without hosting a web server.

---

## Dockerfile

```dockerfile
# syntax=docker/dockerfile:1.19.0
FROM mcr.microsoft.com/dotnet/sdk:9.0-bookworm-slim
LABEL org.opencontainers.image.title="cinturon360-migrator"
LABEL org.opencontainers.image.description="Dedicated sql migrator container"
LABEL org.opencontainers.image.version="1.0.0"
LABEL org.opencontainers.image.created="2025-10-24"
LABEL org.opencontainers.image.authors="Cinturon360 Platform Team"
LABEL org.opencontainers.image.source="https://github.com/repasscloud/cinturon360"
LABEL org.opencontainers.image.documentation="https://docs.cinturon360.com/docs/migrator/dockerfile"
LABEL org.opencontainers.image.licenses="Apache-2.0"
ARG CSPROJ_WEB
ARG CSPROJ_SHARED
WORKDIR /src

# dotnet-ef + pg client for pg_isready
RUN dotnet tool install --global dotnet-ef --version 9.0.11 \
 && ln -s /root/.dotnet/tools/dotnet-ef /usr/local/bin/dotnet-ef \
 && apt-get update && apt-get install -y --no-install-recommends postgresql-client \
 && rm -rf /var/lib/apt/lists/*
ENV PATH="$PATH:/root/.dotnet/tools" DOTNET_CLI_TELEMETRY_OPTOUT=1

# restore cache
COPY ${CSPROJ_WEB} Cinturon360.Web/Cinturon360.Web.csproj
COPY ${CSPROJ_SHARED} Cinturon360.Shared/Cinturon360.Shared.csproj


RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    --mount=type=cache,id=nuget-http,target=/root/.local/share/NuGet/v3-cache \
    dotnet restore Cinturon360.Web/Cinturon360.Web.csproj

# copy sources & build Release
COPY ./Cinturon360.Shared/ ./Cinturon360.Shared/
COPY ./Cinturon360.Web/ ./Cinturon360.Web/
RUN rm -rf ./Cinturon360.Web/bin ./Cinturon360.Web/obj
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet build Cinturon360.Web/Cinturon360.Web.csproj
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet build Cinturon360.Web/Cinturon360.Web.csproj -c Release

# runner
COPY ./.docker/migrator/migrate.sh /usr/local/bin/migrate.sh
RUN chmod +x /usr/local/bin/migrate.sh
ENTRYPOINT ["/usr/local/bin/migrate.sh"]
```

---

## Runtime characteristics

- Runs once and exits
- Returns a non‑zero exit code on failure
- Produces no exposed ports
- No persistent volumes
- Safe to re‑run if the database is already migrated

---

## Relationship to Compose

- Service name: `migrator`
- Startup order: after `db` becomes healthy
- Compose flag: `--exit-code-from migrator`
- Restart policy: `no`

Compose blocks further startup until this container completes successfully.

---

## Related documentation

- API container: `dockerfiles/api`
- Webapp container: `dockerfiles/webapp`
- Compose topology: `compose/topology`
- Health & lifecycle: `compose/health`
