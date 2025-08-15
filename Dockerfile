# syntax=docker/dockerfile:1.7-labs

# --- Build stage --------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG CSPROJ
WORKDIR /src

# Copy only csproj first for better caching (adjust path as needed)
# If your solution has multiple projects, repeat COPY for each csproj you build.
COPY ${CSPROJ} BlazorLoginDemo.Web.csproj
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    --mount=type=cache,id=nuget-http,target=/root/.local/share/NuGet/v3-cache \
    dotnet restore BlazorLoginDemo.Web.csproj

# Now copy the rest and publish
# Copy ONLY the web project contents (not the whole repo)
COPY ./BlazorLoginDemo.Web/ .

# Publish self-contained=false, no apphost, Release
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    --mount=type=cache,id=nuget-http,target=/root/.local/share/NuGet/v3-cache \
    dotnet publish BlazorLoginDemo.Web.csproj -c Release -o /app/publish /p:UseAppHost=false

# --- Runtime stage ------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:9.0-bookworm-slim AS final
ARG APP_DLL
ENV ASPNETCORE_URLS=http://+:8080 \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    ASPNETCORE_FORWARDEDHEADERS_ENABLED=true \
    # Persist keys here (compose maps a volume)
    ASPNETCORE_DATA_PROTECTION__KEYS__PATH=/home/app/.aspnet/DataProtection-Keys

# Install curl for healthcheck
RUN apt-get update \
 && apt-get install -y --no-install-recommends curl \
 && rm -rf /var/lib/apt/lists/*

# Create non-root user
RUN useradd -u 10001 -m -d /home/app -s /sbin/nologin app
WORKDIR /app

# Copy published output
COPY --from=build /app/publish/ ./

# Ensure DataProtection keys directory exists and is writable
RUN mkdir -p /home/app/.aspnet/DataProtection-Keys \
 && chown -R app:app /home/app /app

USER app
EXPOSE 8080

# APP_DLL must be the main web assembly, e.g., BlazorLoginDemo.Web.dll
ENTRYPOINT ["dotnet", "APP_DLL"]
