# 🎓 ZealEducation – Hướng Dẫn Cài Đặt & Chạy Project

## Yêu cầu hệ thống

- [.NET SDK](https://dotnet.microsoft.com/) (hỗ trợ `dotnet ef`)
- [Node.js & npm](https://nodejs.org/)
- [Angular CLI](https://angular.io/cli) (`ng`)
- Một SQL Server / database instance đang chạy

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
