# ZealEducation.Application - Layer Rules & Guide

## Vai tro

Day la layer **dieu phoi nghiep vu (orchestration)** — chua cac use case cua ung dung theo pattern CQRS (Command Query Responsibility Segregation). Moi tinh nang duoc to chuc thanh **Command** (thay doi du lieu) hoac **Query** (doc du lieu).

Layer nay biet "ung dung can lam gi" nhung KHONG biet "lam bang cach nao" (cach luu DB, goi API ngoai...). No chi lam viec voi **interface** duoc dinh nghia o Domain layer.

---

## Cau truc thu muc

```
ZealEducation.Application/
├── Common/
│   ├── Behaviors/
│   │   ├── LoggingBehavior.cs       # MediatR pipeline — log request name truoc/sau xu ly
│   │   └── ValidationBehavior.cs    # MediatR pipeline — chay FluentValidation truoc Handler
│   ├── Exceptions/
│   │   └── NotFoundException.cs     # Exception khi khong tim thay entity
│   ├── Interfaces/                  # Contract cho cac service ben ngoai
│   ├── Mappings/                    # AutoMapper profiles (Entity <-> DTO)
│   └── Models/
│       ├── Result.cs                # Generic Result<T> wrapper
│       └── PaginatedList.cs         # Pagination model voi async factory method
├── Features/
│   └── Courses/
│       ├── Commands/
│       │   └── CreateCourse/
│       │       ├── CreateCourseCommand.cs          # IRequest<Guid>
│       │       ├── CreateCourseCommandHandler.cs   # IRequestHandler — xu ly logic
│       │       └── CreateCourseCommandValidator.cs # FluentValidation rules
│       └── Queries/
│           └── GetCourses/
│               ├── GetCoursesQuery.cs              # IRequest<List<CourseDto>>
│               └── GetCoursesQueryHandler.cs       # IRequestHandler + CourseDto
├── DependencyInjection.cs  # Dang ky: AutoMapper, FluentValidation, MediatR + Behaviors
└── ZealEducation.Application.csproj
```

---

## Quy tac khi tao Feature moi

### 1. To chuc theo Feature Folder

Moi feature nam trong `Features/{EntityName}/` va chia thanh `Commands/` va `Queries/`:

```
Features/
└── Students/
    ├── Commands/
    │   ├── CreateStudent/
    │   │   ├── CreateStudentCommand.cs
    │   │   ├── CreateStudentCommandHandler.cs
    │   │   └── CreateStudentCommandValidator.cs
    │   └── UpdateStudent/
    │       ├── UpdateStudentCommand.cs
    │       ├── UpdateStudentCommandHandler.cs
    │       └── UpdateStudentCommandValidator.cs
    └── Queries/
        ├── GetStudents/
        │   ├── GetStudentsQuery.cs
        │   ├── GetStudentsQueryHandler.cs
        │   └── StudentDto.cs
        └── GetStudentById/
            ├── GetStudentByIdQuery.cs
            └── GetStudentByIdQueryHandler.cs
```

### 2. Tao Command (ghi du lieu)

**Command** — khai bao du lieu dau vao:
```csharp
using MediatR;

namespace ZealEducation.Application.Features.Students.Commands.CreateStudent;

public record CreateStudentCommand(
    string FullName,
    string Email,
    DateTime DateOfBirth) : IRequest<Guid>;
```

**Handler** — xu ly logic, su dung `IRepository<T>` va `IUnitOfWork`:
```csharp
using MediatR;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.Students.Commands.CreateStudent;

public class CreateStudentCommandHandler(
    IRepository<Student> studentRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateStudentCommand, Guid>
{
    public async Task<Guid> Handle(CreateStudentCommand request, CancellationToken cancellationToken)
    {
        var student = new Student
        {
            Id = Guid.NewGuid(),
            FullName = request.FullName,
            Email = request.Email,
            DateOfBirth = request.DateOfBirth
        };

        await studentRepository.AddAsync(student, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return student.Id;
    }
}
```

**Luu y quan trong:**
- Handler inject `IRepository<T>` de thao tac du lieu va `IUnitOfWork` de luu thay doi
- `IRepository.AddAsync()` chi danh dau entity la "Added" trong EF Core change tracker
- `IUnitOfWork.SaveChangesAsync()` moi thuc su ghi xuong database
- KHONG goi `SaveChangesAsync` tren repository — luon dung `IUnitOfWork`

**Validator** — FluentValidation rules:
```csharp
using FluentValidation;

namespace ZealEducation.Application.Features.Students.Commands.CreateStudent;

public class CreateStudentCommandValidator : AbstractValidator<CreateStudentCommand>
{
    public CreateStudentCommandValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required")
            .MaximumLength(200);

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress();
    }
}
```

### 3. Tao Query (doc du lieu)

**Query + Handler + DTO** cung nằm trong 1 folder:
```csharp
using MediatR;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.Students.Queries.GetStudents;

// Query
public record GetStudentsQuery() : IRequest<List<StudentDto>>;

// DTO
public class StudentDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = default!;
    public string Email { get; set; } = default!;
}

// Handler
public class GetStudentsQueryHandler(IRepository<Student> studentRepository)
    : IRequestHandler<GetStudentsQuery, List<StudentDto>>
{
    public async Task<List<StudentDto>> Handle(GetStudentsQuery request, CancellationToken cancellationToken)
    {
        var students = await studentRepository.GetAllAsync(cancellationToken);
        return [.. students.Select(s => new StudentDto
        {
            Id = s.Id,
            FullName = s.FullName,
            Email = s.Email
        })];
    }
}
```

---

## MediatR Pipeline Behaviors

Request di qua pipeline theo thu tu duoc dang ky trong `DependencyInjection.cs`:

```
Request → LoggingBehavior → ValidationBehavior → Handler → Response
```

1. **LoggingBehavior** — Log ten request truoc va sau khi xu ly
2. **ValidationBehavior** — Tim tat ca `IValidator<TRequest>`, chay validate. Neu co loi → throw `ValidationException` (duoc bat boi GlobalExceptionHandler o API layer)

Khi them Behavior moi:
- Tao trong `Common/Behaviors/`
- Implement `IPipelineBehavior<TRequest, TResponse>`
- Dang ky trong `DependencyInjection.cs` voi `cfg.AddBehavior(...)`

---

## Ngon ngu cho message

**TAT CA text tra ve cho client phai bang tieng Anh** — bao gom:
- Message trong exception (`NotFoundException`, `ConflictException`, `ForbiddenException`, custom domain exception...)
- Message cua FluentValidation (`.WithMessage("...")`)
- Bat ky string nao xuat hien trong response body (loi, thong bao, status...)

KHONG dung tieng Viet (co dau hay khong dau) trong cac message tra ve API. Comment trong code va tai lieu noi bo (CLAUDE.md...) van co the dung tieng Viet.

```csharp
// DUNG
throw new ConflictException("Faculty already has another batch scheduled within this date range.");
RuleFor(x => x.Email).EmailAddress().WithMessage("Email is invalid.");

// SAI
throw new ConflictException("Faculty da co lich o batch khac.");
RuleFor(x => x.Email).EmailAddress().WithMessage("Email khong hop le.");
```

---

## DUOC THEM VAO LAYER NAY

- Commands + Handlers (hanh dong thay doi du lieu)
- Queries + Handlers (hanh dong doc du lieu)
- Validators (FluentValidation rules cho Command/Query)
- DTOs (Data Transfer Objects cho input/output)
- Mapping Profiles (AutoMapper profiles chuyen doi Entity <-> DTO)
- Interfaces cho service ben ngoai (IEmailService, IFileStorage...)
- Pipeline Behaviors (Validation, Logging, Caching, Transaction...)
- Common Exceptions (NotFoundException, ForbiddenException, ConflictException...)
- Common Models (Result<T>, PaginatedList<T>)

## KHONG DUOC THEM VAO LAYER NAY

- **Implementation cua infrastructure**: DbContext, EmailService, FileStorage... (thuoc Infrastructure)
- **Controllers, Middleware, Filters**: Thuoc API layer
- **Entity definitions**: Thuoc Domain layer
- **Ket noi truc tiep den database**: Khong dung DbContext, SqlConnection... truc tiep
- **Cau hinh**: appsettings, connection strings
- **DI registration cua Infrastructure services**: Thuoc Infrastructure layer
- **HttpClient, Message Queue**: Cac external client chi duoc dung qua interface

---

## Dependency

```
Application tham chieu → Domain
Application KHONG tham chieu → Infrastructure, API
```

NuGet packages su dung:
- MediatR (CQRS dispatcher)
- FluentValidation (validation rules)
- AutoMapper (object mapping)
- Microsoft.Extensions.DependencyInjection.Abstractions (DI registration)
- Microsoft.Extensions.Logging.Abstractions (ILogger)
