---
title: Webapp (Blazor)
description: Build definition for the Cinturon360 web application container
weight: 20
---

## Naming note

This Dockerfile is currently named simply `Dockerfile`.

It builds the **web application container** (historically referred to as *Blazor*).
The intention is to rename this file to:

```
Dockerfile.webapp
```

and to standardise terminology on **webapp** going forward.

This documentation already uses the **webapp** name to avoid further churn.

---

## Purpose

This Dockerfile defines the **web application runtime container** used by Compose.

It is responsible for:
- Building the web UI application
- Producing a slim, production-like runtime image
- Hosting the user-facing HTTP endpoint

The resulting image is consumed by the `blazor` (future: `webapp`) service in Compose.

---

## Build inputs

This Dockerfile is parameterised via build arguments supplied by Compose:

- `CSPROJ_WEB`
- `CSPROJ_SHARED`
- `APP_DLL`

These allow:
- Reuse across projects
- Future renaming without changing build logic

---

## Dockerfile

```dockerfile
# syntax=docker/dockerfile:1.19.0

# --- Build stage --------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
LABEL org.opencontainers.image.title="cinturon360-blazor"
LABEL org.opencontainers.image.description="Dedicated Blazor container"
LABEL org.opencontainers.image.version="1.0.0"
LABEL org.opencontainers.image.created="2025-10-24"
LABEL org.opencontainers.image.authors="Cinturon360 Platform Team"
LABEL org.opencontainers.image.source="https://github.com/repasscloud/cinturon360"
LABEL org.opencontainers.image.documentation="https://docs.cinturon360.com/docs/blazor/dockerfile"
LABEL org.opencontainers.image.licenses="Apache-2.0"
ARG CSPROJ_WEB
ARG CSPROJ_SHARED
WORKDIR /src

# Copy only csproj first for better caching
COPY ${CSPROJ_WEB} Cinturon360.Web/Cinturon360.Web.csproj
COPY ${CSPROJ_SHARED} Cinturon360.Shared/Cinturon360.Shared.csproj

# Restore with cache mounts (NuGet packages + HTTP v3 cache)
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    --mount=type=cache,id=nuget-http,target=/root/.local/share/NuGet/v3-cache \
    dotnet restore Cinturon360.Shared/Cinturon360.Shared.csproj
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    --mount=type=cache,id=nuget-http,target=/root/.local/share/NuGet/v3-cache \
    dotnet restore Cinturon360.Web/Cinturon360.Web.csproj

# Copy ONLY the web project sources and publish
COPY ./Cinturon360.Shared/ ./Cinturon360.Shared/
COPY ./Cinturon360.Web/ ./Cinturon360.Web/
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    --mount=type=cache,id=nuget-http,target=/root/.local/share/NuGet/v3-cache \
    dotnet publish Cinturon360.Web/Cinturon360.Web.csproj -c Release -o /app/publish /p:UseAppHost=false

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

# Ensure DataProtection keys directory exists and is writable
RUN mkdir -p /home/app/.aspnet/DataProtection-Keys \
 && chown -R app:app /home/app /app

USER app
EXPOSE 8080

# Run the built DLL provided via APP_DLL (e.g., Cinturon360.Web.dll)
ENTRYPOINT ["sh","-lc","exec dotnet \"$APP_DLL\""]
```

---

## Runtime characteristics

- Runs as a non-root application user
- Exposes a single HTTP endpoint
- Persists ASP.NET DataProtection keys via a mounted volume
- Designed for rapid rebuild during local development

---

## Relationship to Compose

- Service name today: `blazor`
- Intended future service name: `webapp`
- Restart policy: `unless-stopped`
- Health is validated via HTTP probe

---

## Related documentation

- Compose service: `services/application/webapp`
- API container: `dockerfiles/api`
- Environment variables: `compose/environment`
- Health & lifecycle: `compose/health`
