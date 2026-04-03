# ZealEducation.Infrastructure - Layer Rules & Guide

## Vai tro

Day la layer **hien thuc ky thuat (technical implementation)** — noi trien khai tat ca nhung gi Domain va Application layer khai bao bang interface. Day la noi **duy nhat** biet cach giao tiep voi the gioi ben ngoai: database, file system, email, API ben thu 3...

Infrastructure la layer "ban" nhat — chua tat ca chi tiet ky thuat ma cac layer ben trong khong can biet.

---

## Cau truc thu muc

```
ZealEducation.Infrastructure/
├── Data/
│   ├── ApplicationDbContext.cs              # DbContext + implement IUnitOfWork
│   ├── Configurations/
│   │   └── CourseConfiguration.cs           # Fluent API cau hinh bang/cot/index
│   ├── Interceptors/
│   │   └── AuditableEntityInterceptor.cs    # Tu dong set CreatedAt/UpdatedAt khi SaveChanges
│   └── Migrations/                          # EF Core database migrations
├── Repositories/
│   └── GenericRepository.cs                 # Implement IRepository<T> — CRUD operations
├── Services/                                # Implement cac service interface (Email, File...)
├── DependencyInjection.cs                   # Dang ky tat ca infrastructure services vao DI
└── ZealEducation.Infrastructure.csproj
```

---

## ApplicationDbContext — Trung tam cua Infrastructure

```csharp
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options), IUnitOfWork
{
    public DbSet<Course> Courses => Set<Course>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}
```

**Dac diem quan trong:**
- `ApplicationDbContext` vua la `DbContext` (EF Core) vua implement `IUnitOfWork` (Domain interface)
- `SaveChangesAsync()` cua DbContext chinh la implementation cua `IUnitOfWork.SaveChangesAsync()`
- Khi them Entity moi, **BAT BUOC** them `DbSet<T>` vao day
- `OnModelCreating` tu dong load tat ca `IEntityTypeConfiguration<T>` trong assembly

---

## Quy tac khi them Entity moi vao Infrastructure

### 1. Them DbSet vao ApplicationDbContext

```csharp
public DbSet<Student> Students => Set<Student>();
```

### 2. Tao Entity Configuration

Dat trong `Data/Configurations/`:

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZealEducation.Domain.Entities;

namespace ZealEducation.Infrastructure.Data.Configurations;

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.FullName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.Email)
            .IsRequired()
            .HasMaxLength(256);

        // Index
        builder.HasIndex(s => s.Email).IsUnique();

        // Relationships
        builder.HasMany(s => s.Enrollments)
            .WithOne(e => e.Student)
            .HasForeignKey(e => e.StudentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
```

**Quy tac Configuration:**
- Moi Entity co **dung 1 file** Configuration
- Ten file: `{EntityName}Configuration.cs`
- KHONG dung Data Annotations ([Required], [MaxLength]...) tren Entity — dung Fluent API
- Luon khai bao `HasKey`, cac property constraints, indexes, va relationships

### 3. Tao Migration

```bash
cd backend/src/ZealEducation.Infrastructure
dotnet ef migrations add AddStudent --startup-project ../ZealEducation.API
```

---

## GenericRepository — Pattern hien tai

```csharp
public class GenericRepository<T>(ApplicationDbContext context) : IRepository<T>
    where T : BaseEntity
{
    protected readonly DbSet<T> _dbSet = context.Set<T>();
    // ... CRUD methods
}
```

**Luu y:**
- `GenericRepository` da duoc dang ky open generic: `IRepository<>` → `GenericRepository<>`
- Moi Entity tu dong co repository ma KHONG can tao class rieng
- Neu can repository chuyen biet (vd: full-text search), tao class ke thua `GenericRepository<T>` va dang ky override trong `DependencyInjection.cs`

---

## AuditableEntityInterceptor

Interceptor nay tu dong:
- Set `CreatedAt = DateTime.UtcNow` khi entity duoc them moi (State == Added)
- Set `UpdatedAt = DateTime.UtcNow` khi entity duoc cap nhat (State == Modified)

Handler KHONG can tu set cac truong nay — Interceptor xu ly tu dong.

---

## DependencyInjection.cs — Dang ky Services

Tat ca infrastructure services duoc dang ky trong file nay:

```csharp
public static IServiceCollection AddInfrastructureServices(
    this IServiceCollection services, IConfiguration configuration)
{
    // Interceptors
    services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();

    // DbContext
    services.AddDbContext<ApplicationDbContext>((sp, options) =>
    {
        options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
        options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"),
            b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));
    });

    // UnitOfWork — resolve tu ApplicationDbContext
    services.AddScoped<IUnitOfWork>(provider =>
        provider.GetRequiredService<ApplicationDbContext>());

    // Generic Repository
    services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));

    return services;
}
```

**Khi them service moi:** dang ky trong file nay, KHONG dang ky o Program.cs cua API layer.

---

## DUOC THEM VAO LAYER NAY

- DbContext va DbSet declarations
- Entity Configurations (Fluent API: bang, cot, index, relationships)
- Migrations (EF Core database migrations)
- Interceptors (SaveChanges interceptors cho audit, soft delete, domain event dispatch)
- Repository implementations (GenericRepository va cac repository chuyen biet)
- External service implementations (EmailService, FileStorageService, PaymentService...)
- Identity/Auth configuration (Identity, JWT, role/policy)
- Caching implementations (Redis, MemoryCache)
- Message Queue implementations (RabbitMQ, Azure Service Bus)
- DependencyInjection.cs — dang ky tat ca services

## KHONG DUOC THEM VAO LAYER NAY

- **Business logic**: Logic nghiep vu thuoc Domain hoac Application
- **Controllers, Middleware, API filters**: Thuoc API layer
- **Command/Query definitions**: Thuoc Application layer
- **DTOs cho API response**: Thuoc Application layer
- **Validation rules**: FluentValidation thuoc Application layer
- **Entity definitions**: Thuoc Domain layer
- **Cau hinh appsettings**: Thuoc API layer (chi doc qua IConfiguration)

---

## Dependency

```
Infrastructure tham chieu → Domain, Application
Infrastructure KHONG tham chieu → API
```

NuGet packages su dung:
- Microsoft.EntityFrameworkCore (ORM)
- Microsoft.EntityFrameworkCore.SqlServer (SQL Server provider)
- Microsoft.EntityFrameworkCore.Tools (migrations)
