# ── Build stage ──────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /repo

COPY ThingsBooksy.slnx global.json ./
COPY src/ src/

RUN dotnet restore ThingsBooksy.slnx

RUN dotnet publish src/Bootstrapper/ThingsBooksy.Bootstrapper/ThingsBooksy.Bootstrapper.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore

# ── Runtime stage ─────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Docker

EXPOSE 8080
ENTRYPOINT ["dotnet", "ThingsBooksy.Bootstrapper.dll"]
