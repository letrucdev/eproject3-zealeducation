# 🎓 ZealEducation – Hướng Dẫn Cài Đặt & Chạy Project

## Yêu cầu hệ thống

- [.NET SDK](https://dotnet.microsoft.com/) (hỗ trợ `dotnet ef`)
- [Node.js & npm](https://nodejs.org/)
- [Angular CLI](https://angular.io/cli) (`ng`)
- Một SQL Server / database instance đang chạy
- (Tùy chọn) [Docker](https://www.docker.com/) để chạy MailHog cho test mail trong dev — xem mục [5. Cấu hình Mailing](#5-cấu-hình-mailing-coravel--mailhog)

---

## 📦 Backend

### 1. Cấu hình kết nối database

Mở file `appsettings.json` tại:

```
eproject3-zealeducation/backend/src/ZealEducation.API/
```

Tìm và sửa giá trị `DefaultConnection` thành connection string của database bạn.

---

### 2. Chạy Migration & Tạo Database

Di chuyển vào thư mục chứa file `.sln`:

```
eproject3-zealeducation/backend/
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

Chạy file SQL để thêm dữ liệu mẫu cho staff, file nằm tại:

```
eproject3-zealeducation/backend/scripts/seed-staffs.sql
```

---

### 4. Khởi động Backend

Từ thư mục `eproject3-zealeducation/backend/`, chạy:

```bash
dotnet run --project ./src/ZealEducation.API
```

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

## 🌐 Frontend

### 1. Cài đặt thư viện

Di chuyển vào thư mục frontend:

```
eproject3-zealeducation/frontend/zeal-edu/
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

Mở file `.env` vừa tạo và **sửa lại port** thành port của backend.

---

### 3. Khởi động Frontend

```bash
npm run start
```

---

## 🚀 Tóm tắt lệnh chạy project

| Thành phần | Lệnh |
|------------|------|
| Backend    | `dotnet run build --project ./src/ZealEducation.API` |
| Frontend   | `npm run start` |
