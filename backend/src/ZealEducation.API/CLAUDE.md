# ZealEducation.API - Layer Rules & Guide

## Vai tro

Day la layer **giao tiep voi ben ngoai (presentation)** — diem vao duy nhat cua he thong. Nhiem vu: nhan HTTP request, chuyen thanh Command/Query qua MediatR, tra response. Layer nay cang **"mong" cang tot** — KHONG chua business logic.

---

## Cau truc thu muc

```
ZealEducation.API/
├── Controllers/
│   └── CoursesController.cs         # REST endpoints, goi MediatR
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

## Quy tac tao Controller moi

### Moi Controller chi lam 3 viec:

1. **Nhan request** (tu route, body, query params)
2. **Goi MediatR** (`sender.Send(command/query)`)
3. **Tra response** (Ok, Created, NoContent...)

**Vi du:**
```csharp
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ZealEducation.Application.Features.Students.Commands.CreateStudent;
using ZealEducation.Application.Features.Students.Queries.GetStudents;

namespace ZealEducation.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StudentsController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<StudentDto>>> GetAll()
    {
        var result = await sender.Send(new GetStudentsQuery());
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<StudentDto>> GetById(Guid id)
    {
        var result = await sender.Send(new GetStudentByIdQuery(id));
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create([FromBody] CreateStudentCommand command)
    {
        var id = await sender.Send(command);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateStudentCommand command)
    {
        if (id != command.Id) return BadRequest();
        await sender.Send(command);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await sender.Send(new DeleteStudentCommand(id));
        return NoContent();
    }
}
```

### Quy tac bat buoc:

- **KHONG** viet business logic trong Controller
- **KHONG** inject Repository hay DbContext truc tiep — chi dung `ISender` (MediatR)
- **KHONG** try-catch trong Controller — exception duoc xu ly boi `GlobalExceptionHandler`
- Dung `[ApiController]` attribute de tu dong model validation va 400 response
- Dung `[Route("api/[controller]")]` cho consistent URL pattern
- Inject `ISender` (khong phai `IMediator`) — `ISender` la interface nho gon hon, chi co `Send()`
- Primary constructor: `StudentsController(ISender sender)` — khong can field/property rieng

---

## GlobalExceptionHandler — Xu ly loi tap trung

Middleware nay bat tat ca exception va chuyen thanh HTTP response phu hop:

| Exception Type | HTTP Status | Response |
|---|---|---|
| `ValidationException` (FluentValidation) | 400 Bad Request | `{ errors: ["..."] }` |
| `NotFoundException` (Application) | 404 Not Found | `{ error: "..." }` |
| Moi exception khac | 500 Internal Server Error | `{ error: "An internal server error occurred." }` |

**Khi them exception type moi:** cap nhat `switch` expression trong `HandleExceptionAsync`.

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
