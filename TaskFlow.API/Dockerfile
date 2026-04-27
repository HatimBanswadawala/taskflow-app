# syntax=docker/dockerfile:1

# ════════════════════════════════════════════════════
# STAGE 1 — BUILD (big SDK image, used only at build time)
# ════════════════════════════════════════════════════
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj files first — Docker caches the restore layer if these don't change
COPY TaskFlow.Domain/*.csproj         TaskFlow.Domain/
COPY TaskFlow.Application/*.csproj    TaskFlow.Application/
COPY TaskFlow.Infrastructure/*.csproj TaskFlow.Infrastructure/
COPY TaskFlow.API/*.csproj            TaskFlow.API/

RUN dotnet restore TaskFlow.API/TaskFlow.API.csproj

# Copy the rest of the source and publish a Release build
COPY TaskFlow.Domain/         TaskFlow.Domain/
COPY TaskFlow.Application/    TaskFlow.Application/
COPY TaskFlow.Infrastructure/ TaskFlow.Infrastructure/
COPY TaskFlow.API/            TaskFlow.API/

RUN dotnet publish TaskFlow.API/TaskFlow.API.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# ════════════════════════════════════════════════════
# STAGE 2 — RUNTIME (tiny aspnet image, ~200 MB)
# Only this stage ships to production.
# ════════════════════════════════════════════════════
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Copy compiled output from the build stage
COPY --from=build /app/publish .

# Configure ASP.NET to listen on port 8080 inside the container
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "TaskFlow.API.dll"]
