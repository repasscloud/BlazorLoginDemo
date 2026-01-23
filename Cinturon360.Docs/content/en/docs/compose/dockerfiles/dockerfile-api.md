---
title: API
description: Build definition for the Cinturon360 API container
weight: 10
---

_Last updated: 24 January 2026_

## Purpose

This Dockerfile defines the **API runtime container** used by Compose.

It is responsible for:
- Building the Cinturon360 API
- Producing a hardened runtime image
- Exposing health endpoints for orchestration

This container is consumed by the `api` service in Compose.

## Build inputs

This Dockerfile is parameterised via build arguments supplied by Compose:

- `CSPROJ_API`
- `CSPROJ_SHARED`
- `APP_DLL`

These allow the Dockerfile to remain reusable without hardcoding project paths.

## Dockerfile

```dockerfile
# syntax=docker/dockerfile:1.19.0

# --- Build stage --------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
LABEL org.opencontainers.image.title="cinturon360-api"
LABEL org.opencontainers.image.description="Dedicated API container"
LABEL org.opencontainers.image.version="1.0.0"
LABEL org.opencontainers.image.created="2025-10-24"
LABEL org.opencontainers.image.authors="Cinturon360 Platform Team"
LABEL org.opencontainers.image.source="https://github.com/repasscloud/cinturon360"
LABEL org.opencontainers.image.documentation="https://docs.cinturon360.com/docs/api/dockerfile"
LABEL org.opencontainers.image.licenses="Apache-2.0"
ARG CSPROJ_API
ARG CSPROJ_SHARED
WORKDIR /src

# Copy only csproj first for better caching
COPY ${CSPROJ_API} Cinturon360.Api/Cinturon360.Api.csproj
COPY ${CSPROJ_SHARED} Cinturon360.Shared/Cinturon360.Shared.csproj

# Restore with cache mounts (NuGet packages + HTTP v3 cache)
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    --mount=type=cache,id=nuget-http,target=/root/.local/share/NuGet/v3-cache \
    dotnet restore Cinturon360.Shared/Cinturon360.Shared.csproj
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    --mount=type=cache,id=nuget-http,target=/root/.local/share/NuGet/v3-cache \
    dotnet restore Cinturon360.Api/Cinturon360.Api.csproj

# Copy ONLY the api project sources and publish
COPY ./Cinturon360.Shared/ ./Cinturon360.Shared/
COPY ./Cinturon360.Api/ ./Cinturon360.Api/
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    --mount=type=cache,id=nuget-http,target=/root/.local/share/NuGet/v3-cache \
    dotnet publish Cinturon360.Api/Cinturon360.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

# --- Runtime stage ------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:9.0-bookworm-slim AS final
ARG APP_DLL
ENV APP_DLL=${APP_DLL}
ENV ASPNETCORE_URLS=http://+:8080 \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    ASPNETCORE_FORWARDEDHEADERS_ENABLED=true \
    ASPNETCORE_DATA_PROTECTION__KEYS__PATH=/home/app/.aspnet/DataProtection-Keys

# Install curl for healthcheck
RUN apt-get update \
 && apt-get install -y --no-install-recommends curl \
 && rm -rf /var/lib/apt/lists/*

# Use the preexisting non-root 'app' user in the base image
WORKDIR /app
COPY --from=build /app/publish/ ./

# Ensure ASP.NET DataProtection keys and shared temp directories exist,
# are owned by the non-root app user, and have restrictive permissions
RUN mkdir -p /home/app/.aspnet/DataProtection-Keys /sharedtmp \
 && chown -R app:app /home/app /sharedtmp \
 && chmod 700 /home/app/.aspnet/DataProtection-Keys \
 && chmod 770 /sharedtmp

USER app
EXPOSE 5050

# Run the built DLL provided via APP_DLL (e.g., Cinturon360.Api.dll)
ENTRYPOINT ["sh","-lc","exec dotnet \"$APP_DLL\""]
```

## Runtime characteristics

- Runs as a non-root user (where applicable)
- Exposes HTTP endpoints only
- Health is validated via the `/api/v1/healthz/check` endpoint
- Designed to be restarted safely

## Related documentation

- Compose service: `services/application/api`
- Environment variables: `compose/environment`
- Health & lifecycle: `compose/health`
