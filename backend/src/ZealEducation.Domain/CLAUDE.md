# ZealEducation.Domain - Layer Rules & Guide

## Vai tro

Day la **loi (core)** cua he thong — chua business logic thuan tuy, khong phu thuoc bat ky framework hay thu vien ngoai nao. Day la layer on dinh nhat, it thay doi nhat trong toan bo solution.

Domain layer dinh nghia "the gioi thuc" cua he thong: Entity la gi, quan he giua chung ra sao, quy tac nghiep vu nao can tuan thu. Moi thu o day phai co y nghia nghiep vu, khong phai ky thuat.

---

## Cau truc thu muc

```
ZealEducation.Domain/
├── Common/
│   ├── BaseEntity.cs           # Base class: Id (Guid) + DomainEvents collection
│   ├── BaseAuditableEntity.cs  # Ke thua BaseEntity, them CreatedAt/UpdatedAt/CreatedBy/UpdatedBy
│   └── BaseEvent.cs            # Base cho Domain Events, implement MediatR.INotification
├── Entities/
│   └── Course.cs               # Entity ke thua BaseAuditableEntity
├── Interfaces/
│   ├── IRepository.cs          # Generic Repository contract (CRUD + Find + Exists)
│   └── IUnitOfWork.cs          # Unit of Work contract (SaveChangesAsync)
├── Enums/                      # Cac enum nghiep vu (CourseStatus, UserRole...)
├── Exceptions/                 # Domain exceptions (CourseFullException...)
└── ZealEducation.Domain.csproj
```

---

## Quy tac khi tao Entity moi

1. Moi Entity **BAT BUOC** ke thua `BaseAuditableEntity` (hoac `BaseEntity` neu khong can audit)
2. Entity chi chua **properties** va **domain logic** (validate, tinh toan nghiep vu)
3. Property `Id` da co san tu `BaseEntity` — khong khai bao lai
4. Dat file trong thu muc `Entities/`

**Vi du tao entity:**
```csharp
using ZealEducation.Domain.Common;

namespace ZealEducation.Domain.Entities;

public class Student : BaseAuditableEntity
{
    public string FullName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public DateTime DateOfBirth { get; set; }

    // Navigation properties
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}
```

---

## Quy tac khi tao Interface

1. Dat trong thu muc `Interfaces/`
2. Interface chi khai bao **contract** — KHONG chua implementation
3. Generic constraint phai la `BaseEntity` hoac derived class
4. `IRepository<T>` da co san cac method: `GetByIdAsync`, `GetAllAsync`, `FindAsync`, `AddAsync`, `Update`, `Delete`, `ExistsAsync`
5. Neu entity can repository dac biet (vd: `ICourseRepository`), ke thua tu `IRepository<Course>` va them method rieng

---

## Quy tac khi tao Domain Event

1. Ke thua `BaseEvent` (implements `INotification` cua MediatR)
2. Dat trong thu muc `Events/` (tao neu chua co)
3. Ten event theo format: `{Entity}{Action}Event` (vd: `CourseCreatedEvent`, `StudentEnrolledEvent`)
4. Raise event trong Entity bang `AddDomainEvent(new CourseCreatedEvent(this))`

---

## DUOC THEM VAO LAYER NAY

- Entities (doi tuong nghiep vu chinh)
- Value Objects (doi tuong xac dinh bang gia tri, vd: Money, Email, Address)
- Enums nghiep vu (CourseStatus, PaymentStatus, UserRole...)
- Domain Events (su kien nghiep vu)
- Domain Exceptions (loi nghiep vu dac thu)
- Interfaces (IRepository, IUnitOfWork, va cac interface repository chuyen biet)
- Base classes trong `Common/`

## KHONG DUOC THEM VAO LAYER NAY

- **Database**: DbContext, Migration, SQL, connection string, EF Core attributes ([Table], [Column]...)
- **Thu vien ngoai**: EF Core, HttpClient, JSON serialization, FluentValidation, AutoMapper
- **DTOs/ViewModels**: Cac model dung cho API request/response
- **Validation logic**: FluentValidation rules (thuoc Application layer)
- **Logging/Caching/Configuration**: Bat ky infrastructure concern nao
- **Implementation**: KHONG hien thuc interface o day — chi khai bao contract
- **NuGet packages** (ngoai tru MediatR.Contracts cho BaseEvent)

---

## Dependency

```
Domain KHONG tham chieu bat ky project nao khac trong solution
Domain chi dung: MediatR.Contracts (cho INotification)
```

> **Nguyen tac vang:** Neu doi database tu SQL Server sang PostgreSQL, Domain KHONG thay doi bat ky dong code nao.
