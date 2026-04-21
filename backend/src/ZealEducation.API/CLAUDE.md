# ZealEducation.API - Layer Rules & Guide

## Vai tro

Day la layer **giao tiep voi ben ngoai (presentation)** — diem vao duy nhat cua he thong. Nhiem vu: nhan HTTP request, chuyen thanh Command/Query qua MediatR, tra response. Layer nay cang **"mong" cang tot** — KHONG chua business logic.

---

## Cau truc thu muc

```
ZealEducation.API/
├── Common/
│   └── Models/
│       └── ApiResponse.cs           # Wrapper response chung { message, data }
├── Controllers/
│   └── AuthController.cs            # REST endpoints, goi MediatR
├── Middleware/
│   └── GlobalExceptionHandler.cs    # Bat exception, tra JSON error response
├── Program.cs                       # DI configuration, middleware pipeline
├── Properties/
│   └── launchSettings.json
├── appsettings.json                 # Connection strings, app settings
├── appsettings.Development.json     # Dev-specific config
└── ZealEducation.API.csproj
```

---

## Response format chung — `ApiResponse<T>`

**TAT CA API response (success lan error) deu phai theo format chung:**

```json
{
  "message": "string",
  "data": <T | null>
}
```

Wrapper nam tai `Common/Models/ApiResponse.cs`:

```csharp
public class ApiResponse<T>
{
    public string Message { get; init; } = string.Empty;
    public T? Data { get; init; }

    public static ApiResponse<T> Success(T? data, string message = "Success") => new()
    {
        Message = message,
        Data = data
    };

    public static ApiResponse<T> Error(string message, T? data = default) => new()
    {
        Message = message,
        Data = data
    };
}
```

**Cach dung trong Controller:**
- Success: `Ok(ApiResponse<MyDto>.Success(result, "Message"))`
- Created: `CreatedAtAction(..., ApiResponse<MyDto>.Success(result, "..."))`
- Return type: `Task<ActionResult<ApiResponse<MyDto>>>`

**Error response** tu dong duoc `GlobalExceptionHandler` wrap — Controller KHONG can lo.

**KHONG** tra ve raw DTO/primitive truc tiep (`return Ok(result)`) — phai wrap qua `ApiResponse<T>`.

---

## Quy tac tao Controller moi

### Moi Controller chi lam 3 viec:

1. **Nhan request** (tu route, body, query params)
2. **Goi MediatR** (`sender.Send(command/query)`)
3. **Tra response** wrap trong `ApiResponse<T>` (Ok, Created, NoContent...)

**Vi du:**
```csharp
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ZealEducation.API.Common.Models;
using ZealEducation.Application.Features.Students.Commands.CreateStudent;
using ZealEducation.Application.Features.Students.Queries.GetStudents;

namespace ZealEducation.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StudentsController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<StudentDto>>>> GetAll()
    {
        var result = await sender.Send(new GetStudentsQuery());
        return Ok(ApiResponse<List<StudentDto>>.Success(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<StudentDto>>> GetById(Guid id)
    {
        var result = await sender.Send(new GetStudentByIdQuery(id));
        return Ok(ApiResponse<StudentDto>.Success(result));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<Guid>>> Create([FromBody] CreateStudentCommand command)
    {
        var id = await sender.Send(command);
        return CreatedAtAction(
            nameof(GetById),
            new { id },
            ApiResponse<Guid>.Success(id, "Student created successfully"));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> Update(Guid id, [FromBody] UpdateStudentCommand command)
    {
        if (id != command.Id) return BadRequest(ApiResponse<object>.Error("Id mismatch"));
        await sender.Send(command);
        return Ok(ApiResponse<object>.Success(null, "Student updated successfully"));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id)
    {
        await sender.Send(new DeleteStudentCommand(id));
        return Ok(ApiResponse<object>.Success(null, "Student deleted successfully"));
    }
}
```

### Quy tac bat buoc:

- **KHONG** viet business logic trong Controller
- **KHONG** inject Repository hay DbContext truc tiep — chi dung `ISender` (MediatR)
- **KHONG** try-catch trong Controller — exception duoc xu ly boi `GlobalExceptionHandler`
- **PHAI** wrap response qua `ApiResponse<T>` — khong tra ve raw DTO
- Dung `[ApiController]` attribute de tu dong model validation va 400 response
- Dung `[Route("api/[controller]")]` cho consistent URL pattern
- Inject `ISender` (khong phai `IMediator`) — `ISender` la interface nho gon hon, chi co `Send()`
- Primary constructor: `StudentsController(ISender sender)` — khong can field/property rieng

---

## GlobalExceptionHandler — Xu ly loi tap trung

Middleware nay bat tat ca exception va chuyen thanh HTTP response **cung format `ApiResponse<T>`** voi success response:

| Exception Type | HTTP Status | Response |
|---|---|---|
| `ValidationException` (FluentValidation) | 400 Bad Request | `{ message: "Validation failed", data: { errors: ["..."] } }` |
| `NotFoundException` (Application) | 404 Not Found | `{ message: "<ex.Message>", data: null }` |
| `UnauthorizedException` (Application) | 401 Unauthorized | `{ message: "<ex.Message>", data: null }` |
| `ConflictException` (Application) | 409 Conflict | `{ message: "<ex.Message>", data: null }` |
| Moi exception khac | 500 Internal Server Error | `{ message: "An internal server error occurred.", data: null }` |

Response JSON dung `camelCase` (thong qua `JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase`).

**Khi them exception type moi:**
1. Tao exception class trong `Application/Common/Exceptions/`
2. Cap nhat `switch` expression trong `HandleExceptionAsync` — tra ve `ApiResponse<object>.Error(...)` voi HTTP status phu hop

---

## Program.cs — Cau hinh DI va Pipeline

```csharp
var builder = WebApplication.CreateBuilder(args);

// 1. Dang ky services theo layer
builder.Services.AddApplicationServices();                         // Application layer
builder.Services.AddInfrastructureServices(builder.Configuration); // Infrastructure layer

// 2. Dang ky API services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 3. Cau hinh middleware pipeline (thu tu QUAN TRONG)
app.UseMiddleware<GlobalExceptionHandler>();  // Bat exception dau tien
app.UseSwagger();                            // Swagger UI (dev only)
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

**Luu y:**
- `AddApplicationServices()` va `AddInfrastructureServices()` la extension methods tu cac layer tuong ung
- API layer **KHONG** dang ky service cua layer khac truc tiep — goi extension method
- Thu tu middleware anh huong den xu ly request — `GlobalExceptionHandler` phai o dau pipeline

---

## DUOC THEM VAO LAYER NAY

- Controllers (nhan request, goi MediatR, tra response)
- Middleware (GlobalExceptionHandler, RequestLogging, Correlation ID...)
- Filters (Authorization filters, Action filters)
- Response wrappers (`ApiResponse<T>`) va cac presentation model dung chung
- Program.cs configuration (DI, pipeline, Swagger, CORS, Authentication)
- appsettings.json (connection strings, JWT settings, app settings)
- Health Checks endpoints
- API Versioning configuration
- SignalR Hubs (real-time communication)

## KHONG DUOC THEM VAO LAYER NAY

- **Business logic**: Bat ky logic nghiep vu nao (thuoc Domain hoac Application)
- **Truy van database truc tiep**: Phai qua MediatR → Handler, KHONG inject DbContext/Repository
- **Entity definitions**: Thuoc Domain layer
- **DbContext, Migrations**: Thuoc Infrastructure layer
- **Repository implementations**: Thuoc Infrastructure layer
- **Logic phuc tap trong Controller**: Controller chi nhan request → goi MediatR → tra response

---

## Dependency

```
API tham chieu → Application, Infrastructure
API KHONG tham chieu truc tiep → Domain (chi gian tiep qua Application)
```

> **Luu y:** API tham chieu Infrastructure chi de goi `AddInfrastructureServices()` trong Program.cs.
> Controller chi lam viec voi cac type tu Application layer (Commands, Queries, DTOs).
