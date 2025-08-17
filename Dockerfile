# syntax=docker/dockerfile:1.7-labs

# --- Build stage --------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG CSPROJ
WORKDIR /src

# Copy only csproj first for better caching
COPY ${CSPROJ} BlazorLoginDemo.Web.csproj

# Restore with cache mounts (NuGet packages + HTTP v3 cache)
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    --mount=type=cache,id=nuget-http,target=/root/.local/share/NuGet/v3-cache \
    dotnet restore BlazorLoginDemo.Web.csproj

# Copy ONLY the web project sources and publish
COPY ./BlazorLoginDemo.Web/ .
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    --mount=type=cache,id=nuget-http,target=/root/.local/share/NuGet/v3-cache \
    dotnet publish BlazorLoginDemo.Web.csproj -c Release -o /app/publish /p:UseAppHost=false

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

# Run the built DLL provided via APP_DLL (e.g., BlazorLoginDemo.Web.dll)
ENTRYPOINT ["sh","-lc","exec dotnet \"$APP_DLL\""]
