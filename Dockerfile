# ── Base runtime (ASP.NET Core 9) ──────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# ── Build stage (SDK 9) ───────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution + package metadata early for caching
COPY *.sln ./
# COPY Directory.Packages.props ./     # if you use CPM; safe if absent
# COPY NuGet.config ./                 # if you use private feeds; safe if absent

# Copy project files referenced by the solution (repeat per project if needed)
COPY BlazorLoginDemo.Web/*.csproj BlazorLoginDemo.Web/

# Restore on project file to prime cache
RUN dotnet restore BlazorLoginDemo.Web/BlazorLoginDemo.Web.csproj

# Now copy the rest of the source
COPY . .

# Full restore again AFTER all sources are present (bullet-proof)
RUN dotnet restore BlazorLoginDemo.sln || dotnet restore BlazorLoginDemo.Web/BlazorLoginDemo.Web.csproj

# Build & publish
# If analyzers keep failing in container, TEMPORARILY add:
#   -p:RunAnalyzersDuringBuild=false -p:RunAnalyzersDuringCompilation=false
RUN dotnet build BlazorLoginDemo.Web -c Release -o /app/build
RUN dotnet publish BlazorLoginDemo.Web -c Release -o /app/publish /p:UseAppHost=false

# ── Final image ───────────────────────────────────────────────────────────────
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish ./
ENTRYPOINT ["dotnet","BlazorLoginDemo.Web.dll"]

