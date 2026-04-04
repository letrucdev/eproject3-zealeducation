# ZealEducation - Luong Hoat Dong Giua Cac Layer

## Tong quan kien truc

```
┌─────────────────────────────────────────────────────────┐
│                    ZealEducation.API                     │
│  (Controllers, Middleware, Program.cs)                   │
│  Nhiem vu: Nhan HTTP request, tra HTTP response         │
├─────────────────────────────────────────────────────────┤
│                ZealEducation.Application                 │
│  (Commands, Queries, Handlers, Validators, DTOs)        │
│  Nhiem vu: Dieu phoi nghiep vu, CQRS                   │
├─────────────────────────────────────────────────────────┤
│               ZealEducation.Infrastructure               │
│  (DbContext, Repositories, Configurations, Services)     │
│  Nhiem vu: Hien thuc ky thuat, giao tiep DB/API ngoai  │
├─────────────────────────────────────────────────────────┤
│                  ZealEducation.Domain                    │
│  (Entities, Interfaces, Value Objects, Events)           │
│  Nhiem vu: Business logic thuan tuy                     │
└─────────────────────────────────────────────────────────┘
```

**Huong phu thuoc (Dependency Rule):**
```
API → Application → Domain
API → Infrastructure → Application → Domain
```

> Layer ben trong KHONG BAO GIO biet ve layer ben ngoai.

---

## 1. Luong khoi dong ung dung (Application Startup)

```
Program.cs khoi tao
    │
    ├─ builder.Services.AddApplicationServices()
    │   └─ Dang ky: AutoMapper, FluentValidation, MediatR + Pipeline Behaviors
    │
    ├─ builder.Services.AddInfrastructureServices(configuration)
    │   ├─ Dang ky: AuditableEntityInterceptor (Scoped)
    │   ├─ Dang ky: ApplicationDbContext voi SQL Server + Interceptors
    │   ├─ Dang ky: IUnitOfWork → resolve tu ApplicationDbContext
    │   └─ Dang ky: IRepository<> → GenericRepository<> (Open Generic)
    │
    ├─ builder.Services.AddControllers()
    │
    └─ Cau hinh Middleware Pipeline:
        GlobalExceptionHandler → Swagger → HTTPS → Auth → MapControllers
```

**Ket qua:** DI container biet cach resolve moi dependency khi Controller can.

---

## 2. Luong xu ly request COMMAND (Ghi du lieu)

**Vi du: POST /api/courses — Tao Course moi**

```
[1] Client gui HTTP POST /api/courses
    Body: { "title": "C# Basics", "description": "...", "price": 99.99 }
                │
                ▼
[2] API Layer — GlobalExceptionHandler middleware
    Boc toan bo pipeline trong try-catch
                │
                ▼
[3] API Layer — CoursesController.Create()
    Nhan CreateCourseCommand tu [FromBody]
    Goi: sender.Send(command)
                │
                ▼
[4] Application Layer — MediatR Pipeline bat dau
    ┌─────────────────────────────────────────────┐
    │  LoggingBehavior                            │
    │  → Log: "Handling CreateCourseCommand"      │
    │  → Goi next() (chuyen tiep sang behavior ke)│
    └──────────────────┬──────────────────────────┘
                       │
                       ▼
    ┌─────────────────────────────────────────────┐
    │  ValidationBehavior                         │
    │  → Tim CreateCourseCommandValidator         │
    │  → Chay validate:                           │
    │    ✓ Title not empty, max 200 chars         │
    │    ✓ Price >= 0                             │
    │  → Neu FAIL: throw ValidationException      │
    │    → GlobalExceptionHandler bat             │
    │    → Tra 400 Bad Request                    │
    │  → Neu PASS: goi next()                     │
    └──────────────────┬──────────────────────────┘
                       │
                       ▼
    ┌─────────────────────────────────────────────┐
    │  CreateCourseCommandHandler.Handle()        │
    │                                             │
    │  1. Tao Course entity (trong memory):       │
    │     new Course { Id, Title, Description,    │
    │                  Price, IsPublished=false }  │
    │                                             │
    │  2. courseRepository.AddAsync(course)        │
    │     → GenericRepository goi                 │
    │       _dbSet.AddAsync(course)               │
    │     → EF Core danh dau entity: State=Added  │
    │     → CHUA co gi ghi xuong DB               │
    │                                             │
    │  3. unitOfWork.SaveChangesAsync()           │
    │     → Goi ApplicationDbContext              │
    │       .SaveChangesAsync()                   │
    └──────────────────┬──────────────────────────┘
                       │
                       ▼
[5] Infrastructure Layer — SaveChanges Pipeline
    ┌─────────────────────────────────────────────┐
    │  AuditableEntityInterceptor                 │
    │  → Quet tat ca entity co State == Added:    │
    │    Set CreatedAt = DateTime.UtcNow          │
    │    Set UpdatedAt = DateTime.UtcNow          │
    │  → Quet tat ca entity co State == Modified: │
    │    Set UpdatedAt = DateTime.UtcNow          │
    └──────────────────┬──────────────────────────┘
                       │
                       ▼
    ┌─────────────────────────────────────────────┐
    │  EF Core thuc thi                           │
    │  → Generate SQL:                            │
    │    INSERT INTO Courses (Id, Title,          │
    │      Description, Price, IsPublished,       │
    │      CreatedAt, UpdatedAt)                  │
    │    VALUES (@p0, @p1, @p2, @p3, @p4, @p5,   │
    │      @p6)                                   │
    │  → Gui den SQL Server                       │
    │  → Tra ve so row affected                   │
    └──────────────────┬──────────────────────────┘
                       │
                       ▼
[6] Application Layer — Handler tra ve course.Id (Guid)
                │
                ▼
[7] Application Layer — LoggingBehavior
    → Log: "Handled CreateCourseCommand"
                │
                ▼
[8] API Layer — CoursesController
    → Tra ve 201 Created voi Location header va course Id
                │
                ▼
[9] Client nhan response:
    HTTP 201 Created
    Location: /api/courses/{id}
    Body: "3fa85f64-5717-4562-b3fc-2c963f66afa6"
```

---

## 3. Luong xu ly request QUERY (Doc du lieu)

**Vi du: GET /api/courses — Lay danh sach Courses**

```
[1] Client gui HTTP GET /api/courses
                │
                ▼
[2] API Layer — GlobalExceptionHandler middleware
                │
                ▼
[3] API Layer — CoursesController.GetAll()
    Goi: sender.Send(new GetCoursesQuery())
                │
                ▼
[4] Application Layer — MediatR Pipeline
    LoggingBehavior → Log "Handling GetCoursesQuery"
    ValidationBehavior → Khong co Validator → bo qua
                │
                ▼
[5] Application Layer — GetCoursesQueryHandler.Handle()
    │
    │  courseRepository.GetAllAsync()
    │  → GenericRepository goi _dbSet.ToListAsync()
    │
    ├───────────────────────────────────────────┐
    │                                           ▼
    │  [Infrastructure] EF Core generate:
    │  SELECT [c].[Id], [c].[Title],
    │    [c].[Description], [c].[Price],
    │    [c].[IsPublished], [c].[CreatedAt], ...
    │  FROM [Courses] AS [c]
    │                                           │
    │◄──────────────────────────────────────────┘
    │
    │  Map Entity → DTO:
    │  courses.Select(c => new CourseDto { ... })
    │
    │  Tra ve List<CourseDto>
                │
                ▼
[6] API Layer — CoursesController
    → Tra ve 200 OK voi JSON body
                │
                ▼
[7] Client nhan response:
    HTTP 200 OK
    Body: [{ "id": "...", "title": "C# Basics", ... }]
```

---

## 4. Luong xu ly loi (Error Flow)

### 4a. Validation Error (400)

```
Client gui POST /api/courses voi title = ""
    │
    ▼
Controller → MediatR → LoggingBehavior → ValidationBehavior
    │
    ▼
ValidationBehavior:
    CreateCourseCommandValidator phat hien Title empty
    → throw ValidationException([{ "Title is required" }])
    │
    ▼
GlobalExceptionHandler bat ValidationException
    → Response: 400 Bad Request
    → Body: { "errors": ["Title is required"] }
```

### 4b. Not Found Error (404)

```
Handler goi repository.GetByIdAsync(id)
    → Tra ve null
    → Handler throw new NotFoundException("Course", id)
    │
    ▼
GlobalExceptionHandler bat NotFoundException
    → Response: 404 Not Found
    → Body: { "error": "Entity \"Course\" (abc-123) was not found." }
```

### 4c. Unhandled Error (500)

```
Bat ky exception nao khong duoc xu ly cu the
    │
    ▼
GlobalExceptionHandler bat Exception
    → Log error (ILogger)
    → Response: 500 Internal Server Error
    → Body: { "error": "An internal server error occurred." }
    (KHONG lo thong tin nhay cam cho client)
```

---

## 5. Dependency Injection — Ai inject gi?

```
CoursesController
    └─ ISender (MediatR) ← duoc dang ky boi AddApplicationServices()

CreateCourseCommandHandler
    ├─ IRepository<Course> ← GenericRepository<Course> (Infrastructure)
    └─ IUnitOfWork ← ApplicationDbContext (Infrastructure)

GetCoursesQueryHandler
    └─ IRepository<Course> ← GenericRepository<Course> (Infrastructure)

GenericRepository<Course>
    └─ ApplicationDbContext ← EF Core DbContext (Infrastructure)

ApplicationDbContext
    └─ ISaveChangesInterceptor ← AuditableEntityInterceptor (Infrastructure)
```

**Quy tac DI:**
- Controller chi inject `ISender`
- Handler inject `IRepository<T>` va `IUnitOfWork` (interfaces tu Domain)
- Repository inject `ApplicationDbContext` (concrete class, internal cua Infrastructure)
- Tat ca dang ky Scoped (1 instance per HTTP request)

---

## 6. Tong ket luong du lieu qua cac layer

```
           REQUEST                              RESPONSE
              │                                    ▲
              ▼                                    │
    ┌─────────────────┐                  ┌─────────────────┐
    │   API Layer     │                  │   API Layer     │
    │   (Controller)  │                  │   (Controller)  │
    │                 │                  │                 │
    │ Input:          │                  │ Output:         │
    │ Command/Query   │                  │ ActionResult    │
    │ (from HTTP)     │                  │ (to HTTP)       │
    └────────┬────────┘                  └────────▲────────┘
             │ sender.Send(command)               │ DTO / Guid
             ▼                                    │
    ┌─────────────────┐                  ┌─────────────────┐
    │ Application     │                  │ Application     │
    │ (Pipeline +     │                  │ (Handler)       │
    │  Handler)       │                  │                 │
    │                 │                  │ Output:         │
    │ Validate →      │                  │ DTO hoac       │
    │ Log →           │                  │ primitive type  │
    │ Execute Handler │                  │ (Guid, bool...) │
    └────────┬────────┘                  └────────▲────────┘
             │ repository.AddAsync()              │ Entity → DTO
             │ unitOfWork.SaveChangesAsync()      │ mapping
             ▼                                    │
    ┌─────────────────┐                  ┌─────────────────┐
    │ Infrastructure  │                  │ Infrastructure  │
    │ (Repository +   │                  │ (Repository +   │
    │  DbContext)     │                  │  DbContext)     │
    │                 │                  │                 │
    │ EF Core:        │                  │ EF Core:        │
    │ Track changes → │                  │ Query DB →      │
    │ Interceptor →   │                  │ Materialize     │
    │ Generate SQL →  │                  │ entities        │
    │ Execute         │                  │                 │
    └────────┬────────┘                  └────────▲────────┘
             │ SQL INSERT/UPDATE/DELETE           │ SQL SELECT
             ▼                                    │
    ┌─────────────────────────────────────────────────────┐
    │                    SQL Server                        │
    └─────────────────────────────────────────────────────┘
```

---

## 7. Checklist khi them tinh nang moi

| Buoc | Layer | Hanh dong | File |
|------|-------|-----------|------|
| 1 | Domain | Tao Entity moi | `Entities/{Name}.cs` |
| 2 | Domain | Tao interface repository chuyen biet (neu can) | `Interfaces/I{Name}Repository.cs` |
| 3 | Application | Tao Command + Handler + Validator | `Features/{Name}/Commands/...` |
| 4 | Application | Tao Query + Handler + DTO | `Features/{Name}/Queries/...` |
| 5 | Infrastructure | Them DbSet vao ApplicationDbContext | `Data/ApplicationDbContext.cs` |
| 6 | Infrastructure | Tao EntityConfiguration | `Data/Configurations/{Name}Configuration.cs` |
| 7 | Infrastructure | Tao Migration | `dotnet ef migrations add Add{Name}` |
| 8 | API | Tao Controller | `Controllers/{Name}Controller.cs` |

> **Thu tu quan trong:** Luon bat dau tu Domain (ben trong) ra API (ben ngoai).
