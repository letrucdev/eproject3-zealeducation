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
dotnet run build --project ./src/ZealEducation.API
```

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
