# Testing Strategy — ZealEducation Backend

> Hướng dẫn viết test cho dự án .NET 8 / EF Core 8 theo Clean Architecture. Đọc trước khi thêm test mới hoặc refactor test hiện có.

---

## Triết lý: Testing Diamond

Project áp dụng **Testing Diamond** thay vì Test Pyramid cổ điển. Lý do: phần lớn code là CRUD-heavy với nhiều orchestration giữa repository + database. Mock IRepository không bắt được các bug ở SQL/EF Core layer (filter sai, projection sai, constraint vi phạm). Real-DB integration tests bắt được nhiều regression hơn unit tests trong những trường hợp này.

```
        [Functional Tests — API layer]      ←  ít, nhưng cover critical user journeys
         (WebApplicationFactory + real DB)
       ↑
[Integration Tests — Infrastructure layer]   ←  TRỌNG TÂM
   (Testcontainers SQL Server + DbContext + Handler)
       ↑
       [Unit Tests — Application layer]      ←  CHỈ cho pure logic
        (Domain helpers, Validators, Behaviors,
         complex command handlers với mock)
```

---

## Quy tắc quyết định: viết loại test nào?

| Loại code | Test layer | Lý do |
|-----------|-----------|-------|
| Domain helper / pure function | **Unit** | Không I/O, dễ test |
| FluentValidation rules | **Unit** | Pure logic, [Theory] với InlineData |
| MediatR Pipeline Behaviors | **Unit** | Pure logic, mock IValidator |
| Command handler có business logic phức tạp (Login, ApplyFine, Approve...) | **Unit + Integration** | Logic phân nhánh ở unit, persistence verify ở integration |
| Command handler CRUD đơn giản (Update, SetActive) | **Integration only** | Mock không đem lại giá trị; test thẳng với real DB |
| Query handler | **Integration only** | Filter / sort / paging chỉ verify được với SQL thật |
| Repository / DbContext / Interceptor | **Integration** | Cần real DB |
| Controller endpoint | **Functional** | E2E qua HTTP, JWT, middleware |
| Auth / Authorization (role-based) | **Functional** | Cần full pipeline để chạy `[Authorize]` |

---

## Cấu trúc test projects

```
backend/tests/
├── ZealEducation.Application.UnitTests/
│   ├── Common/
│   │   ├── Behaviors/         # ValidationBehavior, LoggingBehavior
│   │   ├── Helpers/           # ExamGradeCalculator, CertificateEligibilityChecker, ...
│   │   └── Services/          # PaymentReceiptArchiver, CertificateArchiver
│   ├── Handlers/              # Per-feature command handler tests (mock repos)
│   ├── Validators/            # Per-validator FluentValidation tests
│   └── Helpers/
│       ├── MockRepositoryExtensions.cs   # Fluent Moq helpers
│       └── TestAsyncEnumerable.cs        # IQueryable mock for EF async operators
│
├── ZealEducation.Infrastructure.IntegrationTests/
│   ├── Fixtures/MsSqlContainerFixture.cs # Shared SQL Server container
│   ├── Helpers/EntityBuilders.cs         # Fluent factories for test entities
│   ├── Handlers/                         # Per-feature handler integration tests
│   ├── Queries/                          # Per-feature query handler tests (organized by feature)
│   ├── Repositories/                     # GenericRepository
│   ├── Interceptors/                     # AuditableEntityInterceptor, AuditLogInterceptor
│   └── Services/                         # JwtTokenService, BCryptPasswordHasher
│
└── ZealEducation.API.FunctionalTests/
    ├── Fixtures/ApplicationFactory.cs    # WebApplicationFactory + DB container + fakes
    ├── Helpers/TestDataSeeder.cs         # Seed users/courses/etc.
    ├── Endpoints/                        # Per-controller smoke tests + AuthorizationTests
    └── Journeys/                         # Critical user-journey E2E tests
```

---

## Quy ước

### Đặt tên test methods

Mẫu: `Should_<expected behavior>_when_<condition>` (snake_case_with_underscores).

```csharp
[Fact]
public async Task Should_throw_NotFoundException_when_batch_does_not_exist() { ... }

[Fact]
public async Task Should_increment_failed_count_when_password_wrong() { ... }
```

Không dùng "Test_..." hay "TestX..." vì xUnit đã biết đây là test qua `[Fact]`.

### Tổ chức folder

Mirror cấu trúc của source code. Ví dụ tests cho `src/Application/Common/Helpers/X.cs` đặt ở `tests/Application.UnitTests/Common/Helpers/XTests.cs`.

### Sử dụng `[Theory]` thay vì nhiều `[Fact]`

Khi nhiều test chỉ khác input cho cùng một rule, gộp thành `[Theory]` + `[InlineData]`.

```csharp
// BAD - 4 tests cho cùng 1 rule
[Fact] public void Returns_F_for_score_0() => ...
[Fact] public void Returns_F_for_score_25() => ...
// ...

// GOOD
[Theory]
[InlineData(0, 100, 50, "F")]
[InlineData(25, 100, 50, "F")]
[InlineData(50, 100, 50, "D")]
public void Should_return_correct_grade(decimal score, int max, int pass, string expected) => ...
```

### Tránh anti-patterns

1. **Không assertion vào property không liên quan** — mỗi test focus 1 hành vi.
2. **Không mock những thứ không cần** — ở integration test, đừng mock IRepository (hãy dùng real DB).
3. **Không test framework behavior** — không cần test "EF Core lưu được entity" chung chung.
4. **Không gộp setup giữa các test classes qua static state** — dùng `IAsyncLifetime` + `ResetAsync()`.
5. **Không test private methods trực tiếp** — test qua public API.

---

## Unit Tests — `Application.UnitTests`

### Khi nào viết unit test?

Chỉ viết khi:
- Logic thuần, không cần DB
- Có nhiều phân nhánh business rule không trivial
- Cần tốc độ test nhanh (validators, behaviors, helpers)

**Pattern**: Mock dependencies với Moq + FluentAssertions.

### Mẫu unit test cho Validator

```csharp
public class CreateBatchCommandValidatorTests
{
    private readonly CreateBatchCommandValidator _validator = new();

    private static CreateBatchCommand Valid(/* defaults */) => new(...);

    [Fact]
    public void Should_pass_for_valid_command() =>
        _validator.TestValidate(Valid()).ShouldNotHaveAnyValidationErrors();

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Should_fail_when_batch_code_empty(string code) =>
        _validator.TestValidate(Valid() with { BatchCode = code })
                  .ShouldHaveValidationErrorFor(c => c.BatchCode);
}
```

### Mẫu unit test cho Handler (chỉ khi business logic phức tạp)

```csharp
public class LoginCommandHandlerTests
{
    private readonly Mock<IRepository<UserAccount>> _userRepo = new();
    private readonly Mock<IUnitOfWork> _uow = MockRepositoryExtensions.CreateUnitOfWork();
    private readonly Mock<IPasswordHasher> _hasher = new();

    private LoginCommandHandler CreateHandler() => new(_userRepo.Object, ..., _hasher.Object, ...);

    [Fact]
    public async Task Should_throw_when_user_inactive()
    {
        _userRepo.SetupFind(new[] { /* inactive user */ });
        var act = async () => await CreateHandler().Handle(new LoginCommand("x", "y"), default);
        await act.Should().ThrowAsync<UnauthorizedException>();
    }
}
```

Helpers có sẵn:
- `MockRepositoryExtensions.SetupGetById/SetupFind/SetupExists/SetupAdd` — fluent Moq helpers
- `MockRepositoryExtensions.CreateUnitOfWork()` — pre-configured IUnitOfWork mock
- `Helpers/TestAsyncEnumerable<T>` — IQueryable mock supporting EF async operators (FirstOrDefaultAsync, AnyAsync, ToListAsync...)

---

## Integration Tests — `Infrastructure.IntegrationTests`

### Triết lý

**Integration test là xương sống của test suite.** Mỗi handler (Command + Query) **phải có** ít nhất 1 integration test verify:
- Persistence (entity được lưu đúng)
- Filter / sort / paging trên SQL thật
- DTO mapping đúng từ entity sang response

### Yêu cầu môi trường

- Docker chạy local hoặc CI (Testcontainers tự pull image SQL Server 2022)
- Lần đầu chạy mất ~30s để pull image, sau đó cache lại

### Mẫu Query handler integration test

```csharp
[Collection(DatabaseCollection.Name)]
public class GetCoursesQueryHandlerTests : IAsyncLifetime
{
    private readonly MsSqlContainerFixture _fixture;

    public GetCoursesQueryHandlerTests(MsSqlContainerFixture fixture) => _fixture = fixture;

    public Task InitializeAsync() => _fixture.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private GetCoursesQueryHandler CreateHandler()
    {
        var ctx = _fixture.CreateDbContext();
        return new GetCoursesQueryHandler(new GenericRepository<Course>(ctx));
    }

    [Fact]
    public async Task Returns_paginated_courses_filtered_by_search()
    {
        await using (var ctx = _fixture.CreateDbContext())
        {
            ctx.Courses.AddRange(EntityBuilders.NewCourse(name: "Java"), EntityBuilders.NewCourse(name: "Python"));
            await ctx.SaveChangesAsync();
        }

        var result = await CreateHandler().Handle(new GetCoursesQuery(Search: "java"), default);

        result.Items.Should().ContainSingle(i => i.CourseName == "Java");
    }
}
```

### Mẫu Command handler integration test

Xem `Handlers/Auth/LoginCommandHandlerTests.cs` và `Handlers/Payments/ConfirmPaymentCommandHandlerTests.cs` — pattern đầy đủ với seeding qua `EntityBuilders`, mock `ICurrentUser`/`IFileStorageService`, real DB persistence.

### Checklist khi viết integration test cho Query handler

Mỗi query nên có các test sau:
- [x] Empty result khi DB rỗng
- [x] Filter theo search term (case-insensitive)
- [x] Filter theo status / date range
- [x] Pagination (default page size, page > total → empty, clamping)
- [x] Sort asc/desc theo từng cột
- [x] Default sort khi không truyền sortBy
- [x] DTO mapping (mọi field từ entity → DTO)

### Checklist khi viết integration test cho Command handler

- [x] Happy path: entity được persist đúng vào DB
- [x] NotFoundException khi referenced entity không tồn tại
- [x] ConflictException khi vi phạm unique/business constraint
- [x] Audit columns (CreatedAt/UpdatedAt) được set tự động qua AuditableEntityInterceptor
- [x] Verify side effects (related entities updated, status flags toggled)

---

## Functional Tests — `API.FunctionalTests`

### Triết lý

Functional tests verify **end-to-end qua HTTP** với JWT, middleware, exception handler. **Không lặp lại** logic đã được test ở integration. Tập trung:
- Routing đúng
- Authentication / Authorization
- Request/response serialization
- Critical user journeys (multi-step flows)

### Mẫu controller smoke test

Xem `Endpoints/AuthEndpointsTests.cs`.

### Mẫu authorization test

Xem `Endpoints/AuthorizationTests.cs` — bao gồm:
- Anonymous → 401
- Invalid token → 401
- Wrong role → 403
- Đúng role → 200
- Inactive user không lấy được token

### Mỗi controller cần ít nhất

- 1 happy path test (đúng role, request hợp lệ → 200/201)
- 1 unauthorized test (anonymous hoặc wrong role → 401/403)
- 1 validation error test (request thiếu field bắt buộc → 400)
- 1 NotFound test (resource không tồn tại → 404)

### Critical user journey tests

Đặt trong `Journeys/`. Mỗi journey test 1 luồng nghiệp vụ E2E quan trọng:
- `AuthJourneyTests` — Login → access protected → must-change-password → ChangePassword → re-login
- `PaymentJourneyTests` — Set payment type → Confirm payment → Get receipt PDF
- `CertificateJourneyTests` — Apply → Approve → Generate → Download
- `EnrollmentJourneyTests` — Create batch → Assign → Mark sessions → Record exam → Check eligibility
- `EnquiryJourneyTests` — Create enquiry → Convert → Verify email queued

---

## Chạy tests

### Local

```bash
# Tất cả
cd backend
dotnet test

# Riêng từng layer
dotnet test tests/ZealEducation.Application.UnitTests        # Nhanh, không cần Docker
dotnet test tests/ZealEducation.Infrastructure.IntegrationTests  # Cần Docker
dotnet test tests/ZealEducation.API.FunctionalTests          # Cần Docker
```

### Coverage

```bash
dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage
reportgenerator -reports:"./coverage/**/coverage.cobertura.xml" -targetdir:./coverage/report -reporttypes:HtmlInline_AzurePipelines
# Mở backend/coverage/report/index.html
```

**Coverage thresholds đề xuất** (chưa enforce trong CI):
- Domain: ≥ 80% line coverage
- Application: ≥ 80% line coverage
- Infrastructure: ≥ 70% line coverage
- API: ≥ 70% line coverage

---

## Quy ước English-only

Theo `src/ZealEducation.Application/CLAUDE.md`, mọi text gửi đến end-user phải bằng tiếng Anh:
- Exception messages
- Validation messages
- Email subject/body
- Error response

**Test phải verify điều này** — khi assert message, kiểm tra không có tiếng Việt:

```csharp
await act.Should().ThrowAsync<ConflictException>()
    .WithMessage("Cannot delete a batch that has an assigned faculty.");
// KHÔNG: .WithMessage("Khong the xoa batch da co faculty.");
```

---

## Khi nào XÓA test?

Xóa test khi:
- Trùng lặp với một test khác cùng độ phủ
- Test framework behavior (e.g., "EF Core lưu được entity")
- Test private implementation detail (e.g., test counter mà không liên quan business)
- Test một CRUD command đơn giản chỉ verify "gọi repo + Save" — chuyển lên integration

**Không** xóa test khi:
- Test có business logic phân nhánh (conflict detection, validation cross-field)
- Test edge case có thể fail trong tương lai
- Test một regression đã từng xảy ra

---

## Khi nào THÊM test?

Thêm test khi:
- Bug fix → viết test reproduce bug trước, fix sau
- Feature mới → viết integration test cho command/query handler trước khi viết code (TDD lite)
- Refactor → bảo đảm test cũ vẫn pass; thêm test cho code path mới

---

## Tài liệu tham khảo

- [Microsoft Docs - Integration tests in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests)
- [Testcontainers for .NET](https://dotnet.testcontainers.org/)
- [xUnit](https://xunit.net/)
- [FluentAssertions](https://fluentassertions.com/)
- [Moq Quickstart](https://github.com/devlooped/moq/wiki/Quickstart)
