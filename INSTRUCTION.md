# 🎓 ZealEducation – Setup & Run Guide

> 📖 Project overview: [README.md](README.md) &nbsp;·&nbsp; 🇻🇳 Phiên bản tiếng Việt: [HUONGDAN.md](HUONGDAN.md)

The project has two main parts:

- **Backend**: ASP.NET Core 8 + Entity Framework Core + SQL Server
- **Frontend**: Angular 20 (SPA, static build + Nginx when deployed)

There are two ways to run it:

1. [**Dev (no Docker required)**](#-dev-no-docker-required) — run `dotnet` + `npm` directly against a SQL Server instance on your host. Best for day-to-day development with hot reload.
2. [**Production (Docker Compose)**](#-production-deploy-with-docker) — build & run the whole stack (MSSQL + backend + frontend + MailHog) with a single `docker compose up`.

---

## 📋 System requirements

### For dev (no Docker)

- [.NET SDK 8.0+](https://dotnet.microsoft.com/) (with `dotnet ef`, or install via `dotnet tool install --global dotnet-ef`)
- [Node.js 20+ & npm](https://nodejs.org/)
- [Angular CLI](https://angular.io/cli) (`npm i -g @angular/cli`)
- A running SQL Server instance (LocalDB / SQL Server Express / SQL Server Developer / standalone Docker container…)
- (Optional) [Docker](https://www.docker.com/) just to run MailHog for local mail testing — see [5. Mailing setup](#5-mailing-setup-coravel--mailhog)

### For production deploy (Docker)

- [Docker Engine 24+](https://docs.docker.com/engine/install/) and [Docker Compose v2](https://docs.docker.com/compose/install/) (`docker compose` plugin bundled by default)
- At least ~4 GB of free RAM for the MSSQL container

---

# 💻 Dev (no Docker required)

This section covers running directly on the host machine using the .NET SDK + Node.js — no Docker images to build. Hot reload, debugging, and code edits are fastest this way.

## 📦 Backend (dev)

### 1. Configure the database connection

Open `appsettings.json` at:

```
backend/src/ZealEducation.API/
```

Find the `DefaultConnection` value and replace it with your database connection string. Example:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=ZealEducationDb;User ID=sa;Password=YourStrongPass!;TrustServerCertificate=True;Encrypt=False;"
}
```

> For SQL Server LocalDB (Windows): `Server=(localdb)\\MSSQLLocalDB;Database=ZealEducationDb;Trusted_Connection=True;`

---

### 2. Run migrations & create the database

Move into the directory containing the `.sln` file:

```
backend/
```

Run the following command to create the database and tables:

```bash
dotnet ef database update --project src/ZealEducation.Infrastructure --startup-project src/ZealEducation.API
```

#### 2.1. Use the migrate script (recommended)

For convenience, the project ships with helper scripts at `backend/scripts/migrate.sh` (Linux/macOS/WSL) and `backend/scripts/migrate.ps1` (Windows). They wire up the correct `--project` and `--startup-project` for you.

Run from the `backend/` directory:

```bash
# Linux / macOS / WSL
./scripts/migrate.sh <command> [args]

# Windows PowerShell
./scripts/migrate.ps1 <command> [args]
```

**Supported commands:**

| Command | Description |
|---------|-------------|
| `update [target]` | Apply migrations up to `target` (default: latest) |
| `add <Name>` | Create a new migration named `<Name>` |
| `remove` | Remove the last migration (only if not yet applied) |
| `list` | List all migrations and their status |
| `script [from] [to]` | Generate an idempotent SQL script from `from` to `to` |
| `drop` | Drop the entire database |
| `reset` | Drop the database and re-apply migrations from scratch |

**Examples:**

```bash
# Apply all latest migrations
./scripts/migrate.sh update

# Create a new migration
./scripts/migrate.sh add AddStudentTable

# Roll back to a specific migration
./scripts/migrate.sh update 20260421120952_AddUserAccount

# Export the full schema as SQL
./scripts/migrate.sh script 0 > migration.sql

# Reset the database (drop + update)
./scripts/migrate.sh reset
```

> First time on Linux/macOS, grant execute permission: `chmod +x scripts/migrate.sh`

---

### 3. Seed sample data

Seed SQL scripts live in `backend/scripts/seed/`:

- `seed/demo/` — full dataset (courses, staff, candidates, batches, enrollments, exams, fees…) for end-to-end demos. Run in order `01- → 19-`.
- `seed/chart/` — extra dataset for the analytics charts (registration trend, batch creation trend…). Run after `seed/demo/`.

See `backend/scripts/seed/README.md` for details and exact ordering.

You can run them with `sqlcmd`, Azure Data Studio, SSMS, or any SQL client pointed at the `ZealEducationDb` database.

```bash
# Example: run the entire demo seed via sqlcmd
cd backend/scripts/seed/demo
for f in *.sql; do
  sqlcmd -S localhost -d ZealEducationDb -U sa -P "YourStrongPass!" -C -i "$f"
done
```

---

### 4. Start the backend

From the `backend/` directory, run:

```bash
dotnet run --project ./src/ZealEducation.API
```

The API listens on `http://localhost:5085` by default (see `launchSettings.json` to change the port).

#### 4.1. Use the run script (recommended)

Helper scripts ship at `backend/scripts/run.sh` (Linux/macOS/WSL) and `backend/scripts/run.ps1` (Windows).

Run from the `backend/` directory:

```bash
# Linux / macOS / WSL
./scripts/run.sh [command] [-- <extra dotnet args>]

# Windows PowerShell
./scripts/run.ps1 [command] [extra dotnet args]
```

**Supported commands:**

| Command | Description |
|---------|-------------|
| `dev` *(default)* | Run the API in Development environment |
| `prod` | Run the API in Production (Release) environment |
| `watch` | Run with hot reload (`dotnet watch`) |
| `build` | Build the solution (Debug) |
| `publish [dir]` | Publish a Release build (default dir: `./publish`) |
| `restore` | Restore NuGet packages |
| `clean` | Remove build artifacts |

**Examples:**

```bash
# Run backend in Development
./scripts/run.sh

# Run with hot reload
./scripts/run.sh watch

# Run with the https launch profile
./scripts/run.sh dev -- --launch-profile https

# Publish to ./out
./scripts/run.sh publish ./out
```

> First time on Linux/macOS, grant execute permission: `chmod +x scripts/run.sh`

---

### 5. Mailing setup (Coravel + MailHog)

The backend uses [Coravel.Mailer](https://docs.coravel.net/Mailing/) to queue and send emails in the background. When an enquiry is converted, the email contains login credentials, the course fee, and the available payment options (lump-sum + installment).

#### 5.1. Configure `Coravel:Mail` in `appsettings.json`

File: `backend/src/ZealEducation.API/appsettings.json`

```json
"Coravel": {
  "Mail": {
    "Driver": "FileLog",
    "Host": "",
    "Port": 587,
    "Username": "",
    "Password": "",
    "From": {
      "Address": "noreply@zealeducation.local",
      "Name": "Zeal Education"
    }
  }
}
```

**Supported `Driver` values:**

| Driver | Use case | Notes |
|--------|----------|-------|
| `FileLog` | Dev — write mail to a file instead of sending | No SMTP server needed. Mail is logged to the console + an output folder |
| `SMTP` | Send via a real SMTP server | Fill in `Host`, `Port`, `Username`, `Password` (leave Username/Password empty if the server doesn't require auth, e.g. MailHog) |

#### 5.2. Set up MailHog for local dev (recommended)

[MailHog](https://github.com/mailhog/MailHog) is a fake SMTP server + Web UI that captures mail in dev (nothing is delivered). Great for inspecting HTML rendering, layout, and email content.

**Run MailHog with Docker:**

```bash
docker run -d --name mailhog -p 1025:1025 -p 8025:8025 mailhog/mailhog
```

- Port `1025`: SMTP (where the app sends to)
- Port `8025`: Web UI to view captured mail (open http://localhost:8025)

> **Alternative:** [Mailpit](https://github.com/axllent/mailpit) is the spiritual successor to MailHog with the same usage:
> ```bash
> docker run -d --name mailpit -p 1025:1025 -p 8025:8025 axllent/mailpit
> ```

**Update `appsettings.json` to use MailHog:**

```json
"Coravel": {
  "Mail": {
    "Driver": "SMTP",
    "Host": "localhost",
    "Port": 1025,
    "Username": "",
    "Password": "",
    "From": {
      "Address": "noreply@zealeducation.local",
      "Name": "Zeal Education"
    }
  }
}
```

#### 5.3. App on WSL + MailHog on Docker Desktop (Windows)

It depends on whether Docker Desktop's WSL integration is enabled:

**Option 1 — WSL integration enabled** (the default on Docker Desktop 4.x+): use `Host: "localhost"` as usual.

Quick verification from inside WSL:

```bash
curl -s http://localhost:8025 | head -5    # Reach the MailHog UI
nc -zv localhost 1025                       # Check the SMTP port
```

**Option 2 — WSL integration disabled** or Option 1 doesn't connect: get the Windows host's gateway IP from WSL:

```bash
# Windows host gateway IP
ip route show | grep -i default | awk '{ print $3}'

# Or use the special hostname (newer Docker Desktop versions support it)
getent hosts host.docker.internal
```

Change `Host` in `appsettings.json`:

```json
"Host": "host.docker.internal",
"Port": 1025
```

> **Firewall:** if Option 2 still fails, check Windows Defender Firewall — you may need to allow inbound port 1025 from the WSL subnet.

#### 5.4. Test the mail flow

1. Start MailHog (5.2) and the backend (4).
2. Convert an enquiry via the API: `POST /api/course-enquiries/{id}/convert` with an `email` in the body.
3. Open **http://localhost:8025** → the MailHog UI shows the email:
   - Subject: `Enrollment confirmation - {CourseName}`
   - HTML body fully rendered with theme colors matching the frontend
   - The `MIME Source` tab shows raw HTML/headers if you need to debug

#### 5.5. Production notes

- Production must use a real SMTP provider (Gmail App Password, Office365, AWS SES, SendGrid SMTP relay…).
- Put `Username`/`Password` in **User Secrets** or **environment variables** — do NOT commit them to `appsettings.json`.

---

## 🌐 Frontend (dev)

### 1. Install dependencies

Move into the frontend directory:

```
frontend/zeal-edu/
```

Run:

```bash
npm install
```

---

### 2. Create the environment file

In the same frontend directory, copy the env template:

```bash
cp .env.example .env
```

Open the new `.env` and **update `API_URL`** to match the running backend's port (default `http://localhost:5085/api`).

---

### 3. Start the frontend

```bash
npm run start
```

The frontend runs at `http://localhost:4200` by default.

---

# 🐳 Production: deploy with Docker

The whole stack (database + backend + frontend + mail catcher) is already dockerized. You only need Docker + Docker Compose — no .NET / Node installation on the host.

## Stack components

The `docker-compose.yml` at the **project root** orchestrates 4 services:

| Service | Image | Port (host → container) | Purpose |
|---------|-------|-------------------------|---------|
| `mssql` | `mcr.microsoft.com/mssql/server:2022-latest` | `1434 → 1433` | SQL Server 2022 database (Developer edition) |
| `backend` | Built from `backend/Dockerfile` | `5085 → 8080` | ASP.NET Core API. Auto-applies EF migrations on startup |
| `frontend` | Built from `frontend/zeal-edu/Dockerfile` | `8080 → 80` | Angular bundle served via Nginx |
| `mailhog` | `mailhog/mailhog:latest` | `1025`, `8025` | SMTP catcher for mail testing (UI at http://localhost:8025) |

> **Backend auto-migrates on startup:** the entrypoint in `backend/Dockerfile` ships an EF migrations bundle and applies it whenever the container starts (`RUN_MIGRATIONS_ON_STARTUP=true`). No manual `dotnet ef` step needed at deploy time.

> **Frontend bakes the API URL at build time:** `API_URL` is passed in as a build arg and inlined into the Angular bundle (see `set-env.ts`). Changing `FRONTEND_API_URL` ⇒ you must rebuild the frontend image.

---

## 1. Configure `.env`

At the project root, copy the template and edit:

```bash
cp .env.example .env
```

Important variables:

| Variable | Default | Notes |
|----------|---------|-------|
| `MSSQL_SA_PASSWORD` | `Zeal_StrongPass!2026` | **MUST change** before a real deploy. Strong password required (≥ 8 chars, with upper/lowercase/digits/symbols) |
| `JWT_SECRET_KEY` | demo key | **MUST change** to a long random string (≥ 64 chars) for production |
| `FRONTEND_ORIGIN` | `http://localhost:8080` | Frontend origin used by the backend's CORS allowlist |
| `FRONTEND_API_URL` | `http://localhost:5085/api` | Backend URL that the frontend bundle calls (baked at build time) |
| `APP_NAME` | `Zeal Education` | App display name, baked into the bundle |
| `R2_*` | empty | Cloudflare R2 credentials for uploads (assets, certificates, ID cards…). Leave empty if not using upload features |
| `SMTP_*` | points at `mailhog` | Production must point at a real SMTP provider (SES / SendGrid / Gmail App Password…) |

> ⚠️ Do not commit `.env` (it's in `.gitignore`). For real production, use a secret manager (Docker secrets, Hashicorp Vault, your cloud provider's secret store…).

---

## 2. Build & start the full stack

From the project root:

```bash
# Build images + start all services (detached)
docker compose up -d --build

# Tail logs in real time
docker compose logs -f

# Tail logs for a specific service
docker compose logs -f backend
```

The first run takes a few minutes to pull the MSSQL image and build the backend/frontend. Subsequent runs are much faster thanks to Docker layer caching.

**Check status:**

```bash
docker compose ps
```

The backend only starts after MSSQL passes its healthcheck (see `healthcheck` in `docker-compose.yml`). If MSSQL is not yet healthy, the backend waits.

---

## 3. Access the services

Once the stack is up:

| Endpoint | URL |
|----------|-----|
| Frontend (Angular SPA) | http://localhost:8080 |
| Backend API | http://localhost:5085/api |
| Backend Swagger (if enabled) | http://localhost:5085/swagger |
| MailHog Web UI | http://localhost:8025 |
| MSSQL (from host) | `localhost,1434` (user `sa`, password = `MSSQL_SA_PASSWORD`) |

---

## 4. Seed sample data (optional)

The backend container auto-applies migrations but does **not** auto-seed. To load the demo dataset, copy the SQL files into the MSSQL container and run them with `sqlcmd`:

```bash
# Copy the seed folder into the container
docker cp backend/scripts/seed zeal-mssql:/tmp/seed

# Run all demo scripts in order
docker compose exec mssql bash -c '
  for f in /tmp/seed/demo/*.sql; do
    /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa \
      -P "$MSSQL_SA_PASSWORD" -C -d ZealEducationDb -i "$f";
  done
'

# (Optional) seed analytics chart data
docker compose exec mssql bash -c '
  for f in /tmp/seed/chart/*.sql; do
    /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa \
      -P "$MSSQL_SA_PASSWORD" -C -d ZealEducationDb -i "$f";
  done
'
```

> You can also connect with SSMS / Azure Data Studio to `localhost,1434` and run the `.sql` files in `backend/scripts/seed/` directly.

---

## 5. Lifecycle commands

From the project root:

```bash
# Stop the stack (volume data preserved)
docker compose stop

# Stop + remove containers (MSSQL volume preserved)
docker compose down

# Stop + remove everything + DELETE the volume (full DB wipe!)
docker compose down -v

# Rebuild only the backend (e.g. after code changes)
docker compose up -d --build backend

# Rebuild only the frontend (e.g. after changing FRONTEND_API_URL)
docker compose up -d --build frontend

# Open a shell inside a container
docker compose exec backend bash
docker compose exec mssql bash
```

---

## 6. Update & redeploy

After pulling new code on the server:

```bash
git pull
docker compose up -d --build
```

Compose rebuilds changed images and recreates the corresponding containers. The backend auto-applies any new migrations on restart.

> To roll back, deploy the previous image tag, or `git checkout <commit>` and rebuild.

---

## 7. Real-production checklist

- ✅ Change `MSSQL_SA_PASSWORD` and `JWT_SECRET_KEY` to strong, uncommitted values.
- ✅ Set `FRONTEND_API_URL` and `FRONTEND_ORIGIN` to **public URLs** (e.g. `https://api.example.com/api`, `https://app.example.com`).
- ✅ Put the frontend & backend behind a **reverse proxy** (Nginx / Traefik / Caddy) with TLS — Compose currently exposes plain HTTP for dev simplicity.
- ✅ Use a real SMTP provider for `SMTP_HOST` (don't expose MailHog publicly) — you can remove the `mailhog` service from compose in production.
- ✅ Back up the `mssql-data` volume regularly (`docker run --rm -v zealeducation_mssql-data:/data ...`).
- ✅ Consider a managed database (Azure SQL, AWS RDS) instead of the MSSQL container at scale — only `ConnectionStrings__DefaultConnection` needs to change.

---

## 🚀 Command cheat sheet

| Scenario | Command |
|----------|---------|
| Dev backend (local) | `cd backend && ./scripts/run.sh watch` |
| Dev frontend (local) | `cd frontend/zeal-edu && npm run start` |
| Dev migrate DB | `cd backend && ./scripts/migrate.sh update` |
| Prod build & start | `docker compose up -d --build` |
| Prod view logs | `docker compose logs -f` |
| Prod stop | `docker compose down` |
| Prod reset DB | `docker compose down -v && docker compose up -d --build` |
