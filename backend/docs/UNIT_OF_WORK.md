# Unit of Work Pattern trong ZealEducation

## Unit of Work là gì?

Unit of Work là một design pattern quản lý **một "phiên làm việc" với database**. Nó theo dõi tất cả thay đổi (thêm, sửa, xóa) trên các entity trong một business transaction, rồi **commit tất cả cùng lúc** hoặc **rollback tất cả** nếu có lỗi.

> **Ví dụ đời thường:** Bạn đi siêu thị, bỏ nhiều món vào giỏ hàng (add, update, delete). Khi ra quầy thanh toán, bạn trả tiền một lần cho tất cả (SaveChanges). Nếu thẻ bị từ chối, không món nào được mua (rollback).

---

## Vấn đề mà Unit of Work giải quyết

### Không dùng Unit of Work:
```
Thêm Student → SaveChanges() ✅ đã lưu
Thêm Enrollment → SaveChanges() ❌ lỗi!
→ Student đã lưu nhưng Enrollment thì không → DỮ LIỆU KHÔNG NHẤT QUÁN
```

### Dùng Unit of Work:
```
Thêm Student → theo dõi trong bộ nhớ (chưa lưu)
Thêm Enrollment → theo dõi trong bộ nhớ (chưa lưu)
SaveChangesAsync() → lưu cả 2 cùng lúc trong 1 transaction
    ✅ Cả 2 thành công → commit
    ❌ 1 trong 2 lỗi → rollback tất cả → DỮ LIỆU NHẤT QUÁN
```

---

## Cách triển khai trong ZealEducation

### Bước 1: Interface ở Domain Layer

```
📁 Domain/Interfaces/IUnitOfWork.cs
```

```csharp
public interface IUnitOfWork : IDisposable
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
```

- Khai báo ở **Domain** vì đây là contract thuần túy — không phụ thuộc framework nào.
- `SaveChangesAsync` là phương thức duy nhất — gom tất cả thay đổi và lưu một lần.
- `IDisposable` đảm bảo giải phóng tài nguyên (database connection) khi xong.

### Bước 2: IApplicationDbContext ở Application Layer

```
📁 Application/Common/Interfaces/IApplicationDbContext.cs
```

```csharp
public interface IApplicationDbContext
{
    DbSet<Course> Courses { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
```

Đây là interface mà **các Handler sử dụng trực tiếp**. Nó kết hợp:
- **Truy cập dữ liệu** qua `DbSet<T>` (vai trò Repository)
- **Lưu dữ liệu** qua `SaveChangesAsync` (vai trò Unit of Work)

### Bước 3: Implementation ở Infrastructure Layer

```
📁 Infrastructure/Data/ApplicationDbContext.cs
```

```csharp
public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Course> Courses => Set<Course>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}
```

**EF Core DbContext chính là Unit of Work.** Khi bạn gọi `SaveChangesAsync()`, DbContext sẽ:
1. Quét tất cả entity đang được theo dõi (tracked)
2. Tìm những entity có thay đổi (Added, Modified, Deleted)
3. Tạo các câu SQL tương ứng (INSERT, UPDATE, DELETE)
4. Thực thi tất cả trong **một database transaction**
5. Nếu có lỗi → tự động rollback

---

## Luồng hoạt động thực tế

Lấy ví dụ `CreateCourseCommandHandler`:

```csharp
public async Task<Guid> Handle(CreateCourseCommand request, CancellationToken cancellationToken)
{
    // 1. Tạo entity (chưa lưu DB, chỉ ở bộ nhớ)
    var course = new Course
    {
        Id = Guid.NewGuid(),
        Title = request.Title,
        Description = request.Description,
        Price = request.Price,
        IsPublished = false,
        CreatedAt = DateTime.UtcNow
    };

    // 2. Đánh dấu entity cần thêm vào DB (vẫn chưa lưu)
    _context.Courses.Add(course);

    // 3. Unit of Work: commit tất cả thay đổi vào DB
    await _context.SaveChangesAsync(cancellationToken);

    return course.Id;
}
```

**Luồng chi tiết:**
```
_context.Courses.Add(course)
    → DbContext theo dõi course với trạng thái "Added"
    → Chưa có gì xảy ra ở database

await _context.SaveChangesAsync()
    → DbContext kiểm tra tất cả entity đang theo dõi
    → Phát hiện course có trạng thái "Added"
    → Sinh câu SQL: INSERT INTO Courses (...) VALUES (...)
    → Mở transaction → Thực thi SQL → Commit transaction
    → Trả về số dòng bị ảnh hưởng (1)
```

---

## Kết hợp với Repository Pattern

Trong project, `GenericRepository<T>` xử lý các thao tác CRUD trên từng entity:

```
📁 Infrastructure/Repositories/GenericRepository.cs
```

```csharp
public class GenericRepository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
        return entity;  // Chưa lưu DB! Chỉ đánh dấu "Added"
    }

    public void Update(T entity) => _dbSet.Update(entity);  // Đánh dấu "Modified"
    public void Delete(T entity) => _dbSet.Remove(entity);   // Đánh dấu "Deleted"
}
```

**Quan hệ Repository vs Unit of Work:**

| Vai trò | Nhiệm vụ | Trong project |
|---------|-----------|---------------|
| **Repository** | Thao tác CRUD trên **một loại entity** | `IRepository<T>`, `GenericRepository<T>` |
| **Unit of Work** | Gom tất cả thay đổi, commit **một lần** | `IUnitOfWork`, `IApplicationDbContext`, `ApplicationDbContext` |

```
Repository.AddAsync(student)      → đánh dấu "cần thêm Student"
Repository.AddAsync(enrollment)   → đánh dấu "cần thêm Enrollment"
Repository.Update(course)         → đánh dấu "cần sửa Course"
UnitOfWork.SaveChangesAsync()     → INSERT Student + INSERT Enrollment + UPDATE Course
                                    trong 1 transaction
```

---

## Tại sao EF Core DbContext là Unit of Work?

EF Core DbContext **đã tích hợp sẵn** cả 2 pattern:

| Pattern | DbContext cung cấp |
|---------|-------------------|
| **Repository** | `DbSet<T>` — Add, Remove, Find, Where... |
| **Unit of Work** | `SaveChangesAsync()` — commit tất cả thay đổi |
| **Change Tracking** | Tự động theo dõi trạng thái entity (Added, Modified, Deleted, Unchanged) |
| **Transaction** | `SaveChangesAsync()` mặc định bọc trong transaction |

Vì vậy, trong project này `ApplicationDbContext` đóng vai trò **cả Repository lẫn Unit of Work**, và các Handler có thể dùng trực tiếp qua `IApplicationDbContext`.

---

## Tóm tắt

```
                    ┌─────────────────────────────┐
                    │     IUnitOfWork (Domain)     │
                    │  SaveChangesAsync()          │
                    └──────────────┬──────────────┘
                                   │ kế thừa concept
                    ┌──────────────▼──────────────┐
                    │ IApplicationDbContext (App)   │
                    │  DbSet<Course> Courses       │
                    │  SaveChangesAsync()           │
                    └──────────────┬──────────────┘
                                   │ implement
                    ┌──────────────▼──────────────┐
                    │ ApplicationDbContext (Infra)  │
                    │  : DbContext                  │
                    │  : IApplicationDbContext      │
                    │                              │
                    │  EF Core Change Tracking     │
                    │  + Transaction Management    │
                    └─────────────────────────────┘
```

**3 điều cần nhớ:**
1. **Unit of Work = gom thay đổi, lưu một lần** — đảm bảo tính nhất quán dữ liệu
2. **EF Core DbContext đã là Unit of Work** — không cần tự viết từ đầu
3. **Trong project:** `IApplicationDbContext` là interface chính, `ApplicationDbContext` là implementation, các Handler gọi `SaveChangesAsync()` để commit
