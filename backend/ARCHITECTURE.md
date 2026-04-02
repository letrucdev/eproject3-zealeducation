# ZealEducation Backend - Kiến trúc Clean Architecture

## Tổng quan

Solution được tổ chức theo **Clean Architecture** với luồng phụ thuộc hướng vào trong:

```
API → Application → Domain
API → Infrastructure → Application → Domain
```

> **Quy tắc vàng:** Lớp bên trong KHÔNG BAO GIỜ biết về lớp bên ngoài.

---

## 1. ZealEducation.Domain

### Ý nghĩa
Đây là **lõi của hệ thống** — chứa business logic thuần túy, không phụ thuộc vào bất kỳ framework hay thư viện ngoài nào (ngoại trừ MediatR.Contracts cho Domain Events). Đây là lớp ổn định nhất, ít thay đổi nhất.

### Được thêm vào
- **Entities** — các đối tượng nghiệp vụ chính (Course, Student, Teacher, Enrollment...)
- **Value Objects** — đối tượng được xác định bằng giá trị, không có identity (Money, Address, Email...)
- **Enums** — các tập giá trị cố định nghiệp vụ (CourseStatus, UserRole, PaymentStatus...)
- **Domain Events** — sự kiện nghiệp vụ (CourseCreatedEvent, StudentEnrolledEvent...)
- **Domain Exceptions** — lỗi nghiệp vụ (InsufficientBalanceException, CourseFullException...)
- **Interfaces** — contract cho Repository, UnitOfWork (chỉ khai báo, không hiện thực)
- **Base classes** — BaseEntity, BaseAuditableEntity, BaseEvent

### KHÔNG được thêm vào
- ❌ Bất kỳ thứ gì liên quan đến database (DbContext, migration, SQL)
- ❌ Thư viện bên ngoài (EF Core, HttpClient, JSON serialization...)
- ❌ DTOs, ViewModels, API models
- ❌ Validation logic dùng FluentValidation (thuộc Application)
- ❌ Logging, caching, configuration
- ❌ Hiện thực (implementation) của interfaces — chỉ khai báo contract

---

## 2. ZealEducation.Application

### Ý nghĩa
Lớp **điều phối nghiệp vụ** — chứa các use case của ứng dụng theo pattern CQRS. Mỗi tính năng được tổ chức thành Command (ghi) hoặc Query (đọc). Lớp này biết "ứng dụng cần làm gì" nhưng không biết "làm bằng cách nào" (cách lưu DB, gọi API ngoài...).

### Được thêm vào
- **Commands** — các hành động thay đổi dữ liệu (CreateCourse, UpdateStudent, DeleteEnrollment...)
- **Queries** — các hành động đọc dữ liệu (GetCourses, GetStudentById, SearchCourses...)
- **Handlers** — xử lý logic cho từng Command/Query
- **Validators** — FluentValidation rules cho Commands/Queries
- **DTOs** — Data Transfer Objects cho input/output của use case
- **Mapping Profiles** — AutoMapper profiles chuyển đổi Entity ↔ DTO
- **Interfaces** — contract cho các service bên ngoài (IApplicationDbContext, IEmailService, IFileStorage...)
- **Pipeline Behaviors** — MediatR behaviors (Validation, Logging, Caching, Transaction...)
- **Common Exceptions** — NotFoundException, ForbiddenException, ConflictException...
- **Common Models** — Result<T>, PaginatedList<T>

### KHÔNG được thêm vào
- ❌ Hiện thực cụ thể của infrastructure (DbContext, EmailService, FileStorage...)
- ❌ Controllers, Middleware, Filters (thuộc API)
- ❌ Entity definitions (thuộc Domain)
- ❌ Kết nối trực tiếp đến database, HTTP client, message queue
- ❌ Cấu hình (appsettings, connection strings)
- ❌ Dependency Injection registration của Infrastructure services

---

## 3. ZealEducation.Infrastructure

### Ý nghĩa
Lớp **hiện thực kỹ thuật** — nơi triển khai tất cả những gì Application layer khai báo bằng interface. Đây là nơi duy nhất biết cách giao tiếp với thế giới bên ngoài (database, file system, email, API bên thứ 3...).

### Được thêm vào
- **DbContext** — ApplicationDbContext, cấu hình EF Core
- **Entity Configurations** — Fluent API configurations (bảng, cột, index, relationships...)
- **Migrations** — EF Core database migrations
- **Interceptors** — SaveChanges interceptors (audit, soft delete, domain event dispatch...)
- **Repository implementations** — GenericRepository<T> và các repository chuyên biệt
- **External service implementations** — EmailService, FileStorageService, PaymentService...
- **Identity/Auth** — cấu hình Identity, JWT, role/policy
- **Caching** — Redis, MemoryCache implementations
- **Message Queue** — RabbitMQ, Azure Service Bus producers/consumers
- **DependencyInjection.cs** — đăng ký tất cả infrastructure services

### KHÔNG được thêm vào
- ❌ Business logic (thuộc Domain hoặc Application)
- ❌ Controllers, Middleware, API filters (thuộc API)
- ❌ Command/Query definitions (thuộc Application)
- ❌ DTOs cho API response (thuộc Application)
- ❌ Validation rules (thuộc Application)
- ❌ Entity definitions (thuộc Domain)

---

## 4. ZealEducation.API

### Ý nghĩa
Lớp **giao tiếp với bên ngoài** — điểm vào duy nhất của hệ thống. Nhận HTTP request, chuyển thành Command/Query qua MediatR, trả response. Lớp này càng "mỏng" càng tốt — không chứa business logic.

### Được thêm vào
- **Controllers** — nhận request, gọi MediatR, trả response
- **Middleware** — GlobalExceptionHandler, RequestLogging, Correlation ID...
- **Filters** — Authorization filters, Action filters
- **Program.cs** — cấu hình DI, pipeline, Swagger, CORS, Authentication
- **appsettings.json** — cấu hình connection strings, JWT settings, app settings
- **Health Checks** — endpoint kiểm tra sức khỏe hệ thống
- **API Versioning** — cấu hình versioning cho endpoints
- **SignalR Hubs** — real-time communication hubs (nếu cần)

### KHÔNG được thêm vào
- ❌ Business logic (thuộc Domain hoặc Application)
- ❌ Truy vấn database trực tiếp (phải qua MediatR → Handler)
- ❌ Entity definitions (thuộc Domain)
- ❌ DbContext, Migrations (thuộc Infrastructure)
- ❌ Repository implementations (thuộc Infrastructure)
- ❌ Logic phức tạp trong Controller — Controller chỉ làm 3 việc: nhận request → gọi MediatR → trả response

---

## Luồng xử lý một request

```
[Client] → HTTP Request
    ↓
[API] Controller nhận request, tạo Command/Query
    ↓
[Application] MediatR Pipeline: Logging → Validation → Handler
    ↓
[Application] Handler sử dụng IApplicationDbContext (interface)
    ↓
[Infrastructure] ApplicationDbContext thực thi truy vấn SQL Server
    ↓
[Application] Handler trả về DTO
    ↓
[API] Controller trả HTTP Response
    ↓
[Client] ← JSON Response
```

---

## Quy tắc thêm tính năng mới

Khi thêm một tính năng (ví dụ: quản lý Student), tạo theo thứ tự:

1. **Domain** — Tạo Entity `Student` trong `Entities/`
2. **Application** — Tạo thư mục `Features/Students/` với:
   - `Commands/CreateStudent/` → Command + Validator + Handler
   - `Queries/GetStudents/` → Query + DTO + Handler
3. **Infrastructure** — Tạo `StudentConfiguration` trong `Data/Configurations/`
4. **Infrastructure** — Thêm `DbSet<Student>` vào `ApplicationDbContext`
5. **Application** — Thêm `DbSet<Student>` vào `IApplicationDbContext`
6. **API** — Tạo `StudentsController` trong `Controllers/`
7. **Infrastructure** — Tạo Migration: `dotnet ef migrations add AddStudent`

> **Nguyên tắc:** Domain và Application KHÔNG BAO GIỜ thay đổi khi đổi database từ SQL Server sang PostgreSQL. Chỉ Infrastructure thay đổi.
