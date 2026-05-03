using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Features.Examinations.Queries.GetBatchGradeDistribution;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;
using ZealEducation.Infrastructure.IntegrationTests.Fixtures;
using ZealEducation.Infrastructure.Repositories;

namespace ZealEducation.Infrastructure.IntegrationTests.Queries.Examinations;

[Collection(DatabaseCollection.Name)]
public class GetBatchGradeDistributionQueryHandlerTests(MsSqlContainerFixture fixture) : IAsyncLifetime
{
    private readonly MsSqlContainerFixture _fixture = fixture;

    public Task InitializeAsync() => _fixture.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private GetBatchGradeDistributionQueryHandler CreateHandler()
    {
        var ctx = _fixture.CreateDbContext();
        return new GetBatchGradeDistributionQueryHandler(
            new GenericRepository<Batch>(ctx),
            new GenericRepository<Examination>(ctx),
            new GenericRepository<ExamResult>(ctx));
    }

    private static int _phoneSeed = 400000000;
    private static string NextPhone() => $"0{Interlocked.Increment(ref _phoneSeed)}";

    private static UserAccount NewUser(string username, string fullName, UserRole role = UserRole.Faculty) => new()
    {
        Id = Guid.NewGuid(),
        Username = username,
        PasswordHash = "hash",
        FullName = fullName,
        Email = $"{username}@example.com",
        Phone = NextPhone(),
        Dob = new DateOnly(1990, 1, 1),
        Gender = Gender.Male,
        Role = role,
        IsActive = true
    };

    private static Staff NewStaff(Guid userId) => new()
    {
        Id = Guid.NewGuid(),
        UserAccountId = userId,
        Position = "Lecturer",
        Department = "Engineering",
        JoinedDate = new DateOnly(2020, 1, 1),
        IsActive = true
    };

    private record SeedContext(Batch Batch, Staff Staff, Course Course);

    private async Task<SeedContext> SeedBatchAsync()
    {
        await using var ctx = _fixture.CreateDbContext();
        var course = new Course
        {
            Id = Guid.NewGuid(),
            CourseName = $"Course X{Random.Shared.Next(10000, 99999)}",
            DurationWeeks = 8,
            BaseFee = 1000m,
            IsActive = true
        };
        var batch = new Batch
        {
            Id = Guid.NewGuid(),
            CourseId = course.Id,
            BatchCode = $"B{Random.Shared.Next(10000, 99999)}",
            StartDate = new DateOnly(2024, 1, 1),
            EndDate = new DateOnly(2024, 6, 1),
            Status = BatchStatus.Active
        };
        var user = NewUser($"sched{Random.Shared.Next(10000, 99999)}", "Scheduler");
        var staff = NewStaff(user.Id);
        ctx.Courses.Add(course);
        ctx.Batches.Add(batch);
        ctx.UserAccounts.Add(user);
        ctx.Staff.Add(staff);
        await ctx.SaveChangesAsync();
        return new SeedContext(batch, staff, course);
    }

    private async Task<Examination> AddExamAsync(SeedContext seed, string name, DateOnly? date = null)
    {
        await using var ctx = _fixture.CreateDbContext();
        var exam = new Examination
        {
            Id = Guid.NewGuid(),
            BatchId = seed.Batch.Id,
            ScheduledById = seed.Staff.Id,
            ExamName = name,
            ExamDate = date ?? new DateOnly(2024, 4, 1),
            MaxScore = 100,
            PassScore = 50
        };
        ctx.Examinations.Add(exam);
        await ctx.SaveChangesAsync();
        return exam;
    }

    private async Task<Enrollment> AddEnrollmentAsync(SeedContext seed)
    {
        await using var ctx = _fixture.CreateDbContext();
        var candUser = NewUser($"cand{Random.Shared.Next(100000, 999999)}", "Cand", UserRole.Candidate);
        var candidate = new Candidate
        {
            Id = Guid.NewGuid(),
            UserAccountId = candUser.Id,
            CandidateCode = $"CC{Random.Shared.Next(100000, 999999)}",
            Status = CandidateStatus.Active,
            RegisteredAt = DateTime.UtcNow
        };
        var enrollment = new Enrollment
        {
            Id = Guid.NewGuid(),
            CandidateId = candidate.Id,
            CourseId = seed.Course.Id,
            BatchId = seed.Batch.Id,
            EnrollmentDate = new DateOnly(2024, 1, 5),
            Status = EnrollmentStatus.Enrolled
        };
        ctx.UserAccounts.Add(candUser);
        ctx.Candidates.Add(candidate);
        ctx.Enrollments.Add(enrollment);
        await ctx.SaveChangesAsync();
        return enrollment;
    }

    private async Task AddResultAsync(Examination exam, Enrollment enrollment, Staff grader, string? grade, decimal score = 80m)
    {
        await using var ctx = _fixture.CreateDbContext();
        ctx.ExamResults.Add(new ExamResult
        {
            Id = Guid.NewGuid(),
            ExamId = exam.Id,
            EnrollmentId = enrollment.Id,
            Score = score,
            Grade = grade,
            IsPassed = score >= 50m,
            IsFinalized = true,
            GradedById = grader.Id,
            GradedAt = DateTime.UtcNow
        });
        await ctx.SaveChangesAsync();
    }

    [Fact]
    public async Task Throws_NotFoundException_when_batch_does_not_exist()
    {
        var act = async () => await CreateHandler().Handle(
            new GetBatchGradeDistributionQuery(Guid.NewGuid()), default);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Returns_zero_distribution_when_no_results_exist()
    {
        var seed = await SeedBatchAsync();

        var dto = await CreateHandler().Handle(new GetBatchGradeDistributionQuery(seed.Batch.Id), default);

        dto.Total.Should().Be(0);
        dto.A.Should().Be(0);
        dto.B.Should().Be(0);
        dto.C.Should().Be(0);
        dto.D.Should().Be(0);
        dto.F.Should().Be(0);
        dto.Ungraded.Should().Be(0);
    }

    [Fact]
    public async Task Counts_grades_correctly_across_results()
    {
        var seed = await SeedBatchAsync();
        var exam = await AddExamAsync(seed, "Exam 1");

        var grades = new[] { "A", "A", "B", "C", "D", "F", null, "X" };
        foreach (var g in grades)
        {
            var enrollment = await AddEnrollmentAsync(seed);
            await AddResultAsync(exam, enrollment, seed.Staff, g);
        }

        var dto = await CreateHandler().Handle(new GetBatchGradeDistributionQuery(seed.Batch.Id), default);

        dto.Total.Should().Be(8);
        dto.A.Should().Be(2);
        dto.B.Should().Be(1);
        dto.C.Should().Be(1);
        dto.D.Should().Be(1);
        dto.F.Should().Be(1);
        dto.Ungraded.Should().Be(2);
    }

    [Fact]
    public async Task Aggregates_results_across_multiple_examinations_in_same_batch()
    {
        var seed = await SeedBatchAsync();
        var exam1 = await AddExamAsync(seed, "Exam 1", new DateOnly(2024, 3, 1));
        var exam2 = await AddExamAsync(seed, "Exam 2", new DateOnly(2024, 4, 1));

        var enr1 = await AddEnrollmentAsync(seed);
        var enr2 = await AddEnrollmentAsync(seed);
        await AddResultAsync(exam1, enr1, seed.Staff, "A");
        await AddResultAsync(exam2, enr1, seed.Staff, "B");
        await AddResultAsync(exam1, enr2, seed.Staff, "C");

        var dto = await CreateHandler().Handle(new GetBatchGradeDistributionQuery(seed.Batch.Id), default);

        dto.Total.Should().Be(3);
        dto.A.Should().Be(1);
        dto.B.Should().Be(1);
        dto.C.Should().Be(1);
    }

    [Fact]
    public async Task Excludes_results_from_other_batches()
    {
        var seedA = await SeedBatchAsync();
        var seedB = await SeedBatchAsync();
        var examA = await AddExamAsync(seedA, "Exam A");
        var examB = await AddExamAsync(seedB, "Exam B");
        var enrA = await AddEnrollmentAsync(seedA);
        var enrB = await AddEnrollmentAsync(seedB);
        await AddResultAsync(examA, enrA, seedA.Staff, "A");
        await AddResultAsync(examB, enrB, seedB.Staff, "F");

        var dto = await CreateHandler().Handle(new GetBatchGradeDistributionQuery(seedA.Batch.Id), default);

        dto.Total.Should().Be(1);
        dto.A.Should().Be(1);
        dto.F.Should().Be(0);
    }
}
