# 🎓 ZealEducation – Hướng Dẫn Cài Đặt & Chạy Project

> 📖 Tổng quan dự án: [README.md](README.md) &nbsp;·&nbsp; 🇬🇧 English version: [INSTRUCTION.md](INSTRUCTION.md)

Project gồm 2 phần chính:

- **Backend**: ASP.NET Core 8 + Entity Framework Core + SQL Server
- **Frontend**: Angular 20 (SPA, build tĩnh + Nginx khi deploy)

Có 2 cách chạy:

1. [**Dev (không cần Docker)**](#-dev-không-cần-docker) — chạy trực tiếp `dotnet` + `npm`, dùng SQL Server cài sẵn trên máy. Phù hợp cho phát triển hằng ngày, có hot reload.
2. [**Production (Docker Compose)**](#-production-deploy-với-docker) — build & chạy toàn bộ stack (MSSQL + backend + frontend + MailHog) chỉ bằng `docker compose up`.

---

## 📋 Yêu cầu hệ thống

### Cho dev (không Docker)

- [.NET SDK 8.0+](https://dotnet.microsoft.com/) (kèm `dotnet ef` hoặc cài qua `dotnet tool install --global dotnet-ef`)
- [Node.js 20+ & npm](https://nodejs.org/)
- [Angular CLI](https://angular.io/cli) (`npm i -g @angular/cli`)
- Một SQL Server instance đang chạy (LocalDB / SQL Server Express / SQL Server Developer / Docker container riêng…)
- (Tùy chọn) [Docker](https://www.docker.com/) chỉ để chạy MailHog test mail — xem mục [5. Cấu hình Mailing](#5-cấu-hình-mailing-coravel--mailhog)

### Cho production deploy (Docker)

- [Docker Engine 24+](https://docs.docker.com/engine/install/) và [Docker Compose v2](https://docs.docker.com/compose/install/) (đã tích hợp sẵn `docker compose` plugin)
- Tối thiểu ~4 GB RAM trống cho MSSQL container

---

# 💻 Dev (không cần Docker)

Phần này hướng dẫn chạy thẳng trên máy host bằng .NET SDK + Node.js, không cần build Docker image. Hot reload, debug, sửa code đều nhanh nhất theo hướng này.

## 📦 Backend (dev)

### 1. Cấu hình kết nối database

Mở file `appsettings.json` tại:

```
backend/src/ZealEducation.API/
```

Tìm và sửa giá trị `DefaultConnection` thành connection string của database bạn. Ví dụ:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=ZealEducationDb;User ID=sa;Password=YourStrongPass!;TrustServerCertificate=True;Encrypt=False;"
}
```

> Đối với SQL Server LocalDB (Windows): `Server=(localdb)\\MSSQLLocalDB;Database=ZealEducationDb;Trusted_Connection=True;`

---

### 2. Chạy Migration & Tạo Database

Di chuyển vào thư mục chứa file `.sln`:

```
backend/
```

Chạy lệnh sau để tạo bảng và database:

```bash
dotnet ef database update --project src/ZealEducation.Infrastructure --startup-project src/ZealEducation.API
```

#### 2.1. Sử dụng script migrate (khuyến nghị)

Để tiện cho việc migrate, project có sẵn script tại `backend/scripts/migrate.sh` (Linux/macOS/WSL) và `backend/scripts/migrate.ps1` (Windows). Script tự động trỏ đúng `--project` và `--startup-project`.

Chạy từ thư mục `backend/`:

```bash
# Linux / macOS / WSL
./scripts/migrate.sh <command> [args]

# Windows PowerShell
./scripts/migrate.ps1 <command> [args]
```

**Các lệnh hỗ trợ:**

| Lệnh | Chức năng |
|------|-----------|
| `update [target]` | Apply migration tới `target` (mặc định: migration mới nhất) |
| `add <Name>` | Tạo migration mới với tên `<Name>` |
| `remove` | Xóa migration cuối cùng (chỉ khi chưa apply) |
| `list` | Liệt kê tất cả migration và trạng thái |
| `script [from] [to]` | Sinh SQL script idempotent từ `from` đến `to` |
| `drop` | Xóa toàn bộ database |
| `reset` | Drop database rồi apply lại từ đầu |

**Ví dụ:**

```bash
# Apply tất cả migration mới nhất
./scripts/migrate.sh update

# Tạo migration mới
./scripts/migrate.sh add AddStudentTable

# Rollback về migration cụ thể
./scripts/migrate.sh update 20260421120952_AddUserAccount

# Xuất SQL toàn bộ schema
./scripts/migrate.sh script 0 > migration.sql

# Reset database (drop + update)
./scripts/migrate.sh reset
```

> Nếu lần đầu chạy trên Linux/macOS, cấp quyền thực thi: `chmod +x scripts/migrate.sh`

---

### 3. Seed dữ liệu mẫu

Các script SQL seed nằm tại `backend/scripts/seed/`:

- `seed/demo/` — dataset đầy đủ (courses, staff, candidates, batches, enrollments, exams, fees…) để chạy demo end-to-end. Chạy theo thứ tự `01- → 19-`.
- `seed/chart/` — dataset bổ sung cho các biểu đồ thống kê (registration trend, batch creation trend…). Chạy sau `seed/demo/`.

Xem chi tiết và thứ tự chạy trong `backend/scripts/seed/README.md`.

Có thể chạy bằng `sqlcmd`, Azure Data Studio, SSMS, hoặc bất kỳ SQL client nào trỏ vào database `ZealEducationDb`.

```bash
# Ví dụ chạy toàn bộ demo seed bằng sqlcmd
cd backend/scripts/seed/demo
for f in *.sql; do
  sqlcmd -S localhost -d ZealEducationDb -U sa -P "YourStrongPass!" -C -i "$f"
done
```

---

### 4. Khởi động Backend

Từ thư mục `backend/`, chạy:

```bash
dotnet run --project ./src/ZealEducation.API
```

Mặc định API lắng nghe ở `http://localhost:5085` (xem `launchSettings.json` để đổi port).

#### 4.1. Sử dụng script run (khuyến nghị)

Script khởi động sẵn tại `backend/scripts/run.sh` (Linux/macOS/WSL) và `backend/scripts/run.ps1` (Windows).

Chạy từ thư mục `backend/`:

```bash
# Linux / macOS / WSL
./scripts/run.sh [command] [-- <extra dotnet args>]

# Windows PowerShell
./scripts/run.ps1 [command] [extra dotnet args]
```

**Các lệnh hỗ trợ:**

| Lệnh | Chức năng |
|------|-----------|
| `dev` *(mặc định)* | Chạy API ở môi trường Development |
| `prod` | Chạy API ở môi trường Production (Release) |
| `watch` | Chạy với hot reload (`dotnet watch`) |
| `build` | Build solution (Debug) |
| `publish [dir]` | Publish bản Release (mặc định thư mục: `./publish`) |
| `restore` | Restore NuGet packages |
| `clean` | Xóa build artifacts |

**Ví dụ:**

```bash
# Chạy backend ở Development
./scripts/run.sh

# Chạy với hot reload
./scripts/run.sh watch

# Chạy với launch profile https
./scripts/run.sh dev -- --launch-profile https

# Publish ra thư mục ./out
./scripts/run.sh publish ./out
```

> Nếu lần đầu chạy trên Linux/macOS, cấp quyền thực thi: `chmod +x scripts/run.sh`

---

### 5. Cấu hình Mailing (Coravel + MailHog)

Backend dùng [Coravel.Mailer](https://docs.coravel.net/Mailing/) để gửi email (queue background) khi convert enquiry thành công — email gồm credentials đăng nhập, học phí, các phương án thanh toán (lump-sum + installment).

#### 5.1. Cấu hình `Coravel:Mail` trong `appsettings.json`

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

**Các giá trị `Driver` hỗ trợ:**

| Driver | Mục đích | Ghi chú |
|--------|----------|---------|
| `FileLog` | Dev — ghi mail ra file thay vì gửi | Không cần SMTP server. Mail được log vào console + thư mục output |
| `SMTP` | Gửi qua SMTP server thật | Cần điền `Host`, `Port`, `Username`, `Password` (Username/Password để rỗng nếu server không yêu cầu auth, ví dụ MailHog) |

#### 5.2. Setup MailHog cho local dev (khuyến nghị)

[MailHog](https://github.com/mailhog/MailHog) là SMTP server giả + Web UI để bắt mail trong dev (không gửi thật). Phù hợp cho việc kiểm tra HTML render, layout, và nội dung email.

**Chạy MailHog bằng Docker:**

```bash
docker run -d --name mailhog -p 1025:1025 -p 8025:8025 mailhog/mailhog
```

- Port `1025`: SMTP (app gửi tới)
- Port `8025`: Web UI để xem mail (mở browser http://localhost:8025)

> **Alternative:** [Mailpit](https://github.com/axllent/mailpit) là bản kế nhiệm của MailHog, cùng cách dùng:
> ```bash
> docker run -d --name mailpit -p 1025:1025 -p 8025:8025 axllent/mailpit
> ```

**Cập nhật `appsettings.json` để dùng MailHog:**

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

#### 5.3. App chạy trên WSL + MailHog chạy trên Docker Desktop (Windows)

Tùy WSL integration của Docker Desktop có bật hay không:

**Cách 1 — WSL integration bật** (default trên Docker Desktop 4.x+): dùng `Host: "localhost"` như bình thường.

Verify nhanh từ trong WSL:

```bash
curl -s http://localhost:8025 | head -5    # Truy cập MailHog UI
nc -zv localhost 1025                       # Kiểm tra SMTP port
```

**Cách 2 — WSL integration tắt** hoặc cách 1 không kết nối được: lấy IP gateway của Windows host từ WSL:

```bash
# Lấy IP gateway của Windows host
ip route show | grep -i default | awk '{ print $3}'

# Hoặc dùng special hostname (Docker Desktop mới hỗ trợ)
getent hosts host.docker.internal
```

Đổi `Host` trong `appsettings.json`:

```json
"Host": "host.docker.internal",
"Port": 1025
```

> **Firewall:** nếu Cách 2 vẫn không kết nối, kiểm tra Windows Defender Firewall — có thể cần allow inbound port 1025 từ WSL subnet.

#### 5.4. Test luồng gửi mail

1. Khởi động MailHog (mục 5.2) và backend (mục 4).
2. Convert một enquiry qua API: `POST /api/course-enquiries/{id}/convert` với body chứa `email`.
3. Mở browser **http://localhost:8025** → MailHog UI hiển thị email vừa gửi:
   - Subject: `Enrollment confirmation - {CourseName}`
   - Body HTML render đầy đủ với màu theme khớp frontend
   - Tab `MIME Source` để xem raw HTML/headers nếu cần debug

#### 5.5. Lưu ý production

- Production phải dùng SMTP thật (Gmail App Password, Office365, AWS SES, SendGrid SMTP relay…).
- Đặt `Username`/`Password` vào **User Secrets** hoặc **Environment Variables**, không commit vào `appsettings.json`.

---

## 🌐 Frontend (dev)

### 1. Cài đặt thư viện

Di chuyển vào thư mục frontend:

```
frontend/zeal-edu/
```

Chạy:

```bash
npm install
```

---

### 2. Tạo file Environment

Trong cùng thư mục frontend, sao chép file môi trường mẫu:

```bash
cp .env.example .env
```

Mở file `.env` vừa tạo và **sửa lại `API_URL`** trỏ đúng port của backend đang chạy (mặc định `http://localhost:5085/api`).

---

### 3. Khởi động Frontend

```bash
npm run start
```

Frontend mặc định chạy ở `http://localhost:4200`.

---

# 🐳 Production: Deploy với Docker

Toàn bộ stack (database + backend + frontend + mail catcher) đã được dockerize sẵn. Chỉ cần Docker + Docker Compose, không cần cài .NET / Node trên host.

## Stack thành phần

File `docker-compose.yml` ở **root project** orchestrate 4 service:

| Service | Image | Port (host → container) | Mục đích |
|---------|-------|-------------------------|----------|
| `mssql` | `mcr.microsoft.com/mssql/server:2022-latest` | `1434 → 1433` | Database SQL Server 2022 (Developer edition) |
| `backend` | Build từ `backend/Dockerfile` | `5085 → 8080` | ASP.NET Core API. Tự động chạy EF migrations khi start |
| `frontend` | Build từ `frontend/zeal-edu/Dockerfile` | `8080 → 80` | Angular bundle phục vụ qua Nginx |
| `mailhog` | `mailhog/mailhog:latest` | `1025`, `8025` | SMTP catcher cho test mail (xem UI ở http://localhost:8025) |

> **Backend tự migrate khi start:** entrypoint trong `backend/Dockerfile` sinh sẵn EF migrations bundle và áp khi container khởi động (`RUN_MIGRATIONS_ON_STARTUP=true`). Không cần chạy `dotnet ef` thủ công khi deploy.

> **Frontend bake API URL ở build time:** `API_URL` được truyền qua build arg và inline vào bundle Angular (xem `set-env.ts`). Đổi `FRONTEND_API_URL` ⇒ phải rebuild image frontend.

---

## 1. Cấu hình `.env`

Ở root project, copy file mẫu và chỉnh sửa:

```bash
cp .env.example .env
```

Các biến quan trọng:

| Biến | Mặc định | Ghi chú |
|------|----------|---------|
| `MSSQL_SA_PASSWORD` | `Zeal_StrongPass!2026` | **PHẢI đổi** trước khi deploy thật. Yêu cầu mật khẩu mạnh (>= 8 ký tự, có hoa/thường/số/ký tự đặc biệt) |
| `JWT_SECRET_KEY` | demo key | **PHẢI đổi** sang chuỗi random dài (>= 64 ký tự) cho production |
| `FRONTEND_ORIGIN` | `http://localhost:8080` | Origin của frontend, dùng cho CORS allowlist của backend |
| `FRONTEND_API_URL` | `http://localhost:5085/api` | URL backend mà bundle frontend sẽ gọi (bake build time) |
| `APP_NAME` | `Zeal Education` | Tên hiển thị app, bake vào bundle |
| `R2_*` | rỗng | Cloudflare R2 credentials cho upload (asset, certificate, ID card…). Bỏ trống nếu không dùng feature upload |
| `SMTP_*` | trỏ vào `mailhog` | Production phải đổi sang SMTP thật (SES / SendGrid / Gmail App Password…) |

> ⚠️ Không commit file `.env` (đã trong `.gitignore`). Với production thật, dùng secret manager (Docker secrets, Hashicorp Vault, cloud provider's secret store…).

---

## 2. Build & start toàn bộ stack

Từ root project:

```bash
# Build images + start tất cả services (detached)
docker compose up -d --build

# Theo dõi log realtime
docker compose logs -f

# Theo dõi log của một service cụ thể
docker compose logs -f backend
```

Lần đầu sẽ mất vài phút để pull image MSSQL và build backend/frontend. Lần sau Docker cache layer nên nhanh hơn nhiều.

**Kiểm tra trạng thái:**

```bash
docker compose ps
```

Backend chỉ start sau khi MSSQL pass healthcheck (xem `healthcheck` trong `docker-compose.yml`). Nếu MSSQL chưa healthy, backend sẽ chờ.

---

## 3. Truy cập các service

Sau khi stack lên đầy đủ:

| Endpoint | URL |
|----------|-----|
| Frontend (Angular SPA) | http://localhost:8080 |
| Backend API | http://localhost:5085/api |
| Backend Swagger (nếu enable) | http://localhost:5085/swagger |
| MailHog Web UI | http://localhost:8025 |
| MSSQL (từ host) | `localhost,1434` (user `sa`, password = `MSSQL_SA_PASSWORD`) |

---

## 4. Seed dữ liệu mẫu (tuỳ chọn)

Backend container đã tự apply migration nhưng **không** tự seed. Để load dataset demo, copy các file SQL vào MSSQL container và chạy bằng `sqlcmd`:

```bash
# Copy thư mục seed vào container
docker cp backend/scripts/seed zeal-mssql:/tmp/seed

# Chạy lần lượt các script demo
docker compose exec mssql bash -c '
  for f in /tmp/seed/demo/*.sql; do
    /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa \
      -P "$MSSQL_SA_PASSWORD" -C -d ZealEducationDb -i "$f";
  done
'

# (Tuỳ chọn) seed dữ liệu cho biểu đồ thống kê
docker compose exec mssql bash -c '
  for f in /tmp/seed/chart/*.sql; do
    /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa \
      -P "$MSSQL_SA_PASSWORD" -C -d ZealEducationDb -i "$f";
  done
'
```

> Có thể connect bằng SSMS / Azure Data Studio vào `localhost,1434` rồi run trực tiếp các file `.sql` trong `backend/scripts/seed/`.

---

## 5. Lifecycle commands

Từ root project:

```bash
# Stop stack (giữ lại volume data)
docker compose stop

# Stop + xoá container (volume MSSQL vẫn còn)
docker compose down

# Stop + xoá tất cả + XÓA volume (mất sạch DB!)
docker compose down -v

# Rebuild riêng backend (vd sau khi sửa code)
docker compose up -d --build backend

# Rebuild riêng frontend (vd sau khi đổi FRONTEND_API_URL)
docker compose up -d --build frontend

# Vào shell container
docker compose exec backend bash
docker compose exec mssql bash
```

---

## 6. Update & redeploy

Sau khi pull code mới về server:

```bash
git pull
docker compose up -d --build
```

Compose sẽ rebuild image bị thay đổi và recreate container tương ứng. Backend tự áp migration mới khi container restart.

> Để rollback, dùng tag image cũ hoặc `git checkout <commit>` rồi rebuild lại.

---

## 7. Lưu ý cho production thật

- ✅ Đổi `MSSQL_SA_PASSWORD` và `JWT_SECRET_KEY` sang giá trị mạnh, không commit.
- ✅ Đặt `FRONTEND_API_URL` và `FRONTEND_ORIGIN` về **public URL** (vd `https://api.example.com/api`, `https://app.example.com`).
- ✅ Đặt frontend & backend sau **reverse proxy** (Nginx / Traefik / Caddy) có TLS — Compose hiện expose HTTP plain để dev đơn giản.
- ✅ Dùng SMTP thật cho `SMTP_HOST` (không expose MailHog ra ngoài) — có thể xoá service `mailhog` khỏi compose ở môi trường prod.
- ✅ Backup volume `mssql-data` định kỳ (`docker run --rm -v zealeducation_mssql-data:/data ...`).
- ✅ Cân nhắc dùng managed database (Azure SQL, AWS RDS) thay cho MSSQL container ở scale lớn — chỉ cần đổi `ConnectionStrings__DefaultConnection`.

---

## 🚀 Tóm tắt lệnh

| Tình huống | Lệnh |
|------------|------|
| Dev backend (local) | `cd backend && ./scripts/run.sh watch` |
| Dev frontend (local) | `cd frontend/zeal-edu && npm run start` |
| Dev migrate DB | `cd backend && ./scripts/migrate.sh update` |
| Prod build & start | `docker compose up -d --build` |
| Prod xem log | `docker compose logs -f` |
| Prod stop | `docker compose down` |
| Prod reset DB | `docker compose down -v && docker compose up -d --build` |
