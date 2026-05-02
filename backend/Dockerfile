# syntax=docker/dockerfile:1.7

# ---------- Build stage ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ZealEducation.sln ./
COPY src/ZealEducation.Domain/ZealEducation.Domain.csproj         src/ZealEducation.Domain/
COPY src/ZealEducation.Application/ZealEducation.Application.csproj src/ZealEducation.Application/
COPY src/ZealEducation.Infrastructure/ZealEducation.Infrastructure.csproj src/ZealEducation.Infrastructure/
COPY src/ZealEducation.API/ZealEducation.API.csproj               src/ZealEducation.API/

RUN dotnet restore src/ZealEducation.API/ZealEducation.API.csproj

COPY src/ ./src/

RUN dotnet publish src/ZealEducation.API/ZealEducation.API.csproj \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

# Build a self-contained EF migrations bundle. It's a single binary that
# applies all migrations against whatever connection string we pass at runtime.
RUN dotnet tool install --global dotnet-ef --version 8.0.11
ENV PATH="${PATH}:/root/.dotnet/tools"
RUN dotnet ef migrations bundle \
        --project src/ZealEducation.Infrastructure \
        --startup-project src/ZealEducation.API \
        -o /app/efbundle --force


# ---------- Runtime stage ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# QuestPDF needs basic fonts + libfontconfig on Linux to render PDFs.
RUN apt-get update \
    && apt-get install -y --no-install-recommends \
        libfontconfig1 \
        fontconfig \
        fonts-dejavu \
        fonts-liberation \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish ./
COPY --from=build /app/efbundle /app/efbundle

ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_URLS=http://+:8080 \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    RUN_MIGRATIONS_ON_STARTUP=true

COPY <<'EOF' /app/entrypoint.sh
#!/bin/bash
set -e

if [ "${RUN_MIGRATIONS_ON_STARTUP}" = "true" ]; then
  CONN="${ConnectionStrings__DefaultConnection:-${CONNECTIONSTRINGS__DEFAULTCONNECTION:-}}"
  if [ -z "$CONN" ]; then
    echo "[entrypoint] ConnectionStrings__DefaultConnection is empty — skipping migrations." >&2
  else
    echo "[entrypoint] Applying EF migrations..."
    /app/efbundle --connection "$CONN"
    echo "[entrypoint] Migrations applied."
  fi
fi

exec dotnet ZealEducation.API.dll "$@"
EOF
RUN chmod +x /app/entrypoint.sh

EXPOSE 8080

ENTRYPOINT ["/app/entrypoint.sh"]
