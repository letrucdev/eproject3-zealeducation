using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Features.Examinations.Queries.GetExaminationById;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;
using ZealEducation.Infrastructure.IntegrationTests.Fixtures;
using ZealEducation.Infrastructure.Repositories;

namespace ZealEducation.Infrastructure.IntegrationTests.Queries.Examinations;

[Collection(DatabaseCollection.Name)]
public class GetExaminationByIdQueryHandlerTests : IAsyncLifetime
{
    private readonly MsSqlContainerFixture _fixture;

    public GetExaminationByIdQueryHandlerTests(MsSqlContainerFixture fixture) => _fixture = fixture;

    public Task InitializeAsync() => _fixture.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private GetExaminationByIdQueryHandler CreateHandler()
    {
        var ctx = _fixture.CreateDbContext();
        return new GetExaminationByIdQueryHandler(new GenericRepository<Examination>(ctx));
    }

    private static int _phoneSeed = 300000000;
    private static string NextPhone() => $"0{Interlocked.Increment(ref _phoneSeed)}";

    private static UserAccount NewUser(string username, string fullName) => new()
    {
        Id = Guid.NewGuid(),
        Username = username,
        PasswordHash = "hash",
        FullName = fullName,
        Email = $"{username}@example.com",
        Phone = NextPhone(),
        Dob = new DateOnly(1990, 1, 1),
        Gender = Gender.Male,
        Role = UserRole.Faculty,
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

    private static Course NewCourse() => new()
    {
        Id = Guid.NewGuid(),
        CourseName = "Course X",
        DurationWeeks = 8,
        BaseFee = 1000m,
        IsActive = true
    };

    private static Batch NewBatch(Guid courseId, string code) => new()
    {
        Id = Guid.NewGuid(),
        CourseId = courseId,
        BatchCode = code,
        StartDate = new DateOnly(2024, 1, 1),
        EndDate = new DateOnly(2024, 6, 1),
        MaxCapacity = 30,
        Status = BatchStatus.Active
    };

    private async Task<(Batch Batch, Staff Staff, UserAccount User)> SeedBatchAsync()
    {
        await using var ctx = _fixture.CreateDbContext();
        var course = NewCourse();
        var batch = NewBatch(course.Id, $"B{Random.Shared.Next(10000, 99999)}");
        var user = NewUser($"sched{Random.Shared.Next(10000, 99999)}", "Scheduler User");
        var staff = NewStaff(user.Id);
        ctx.Courses.Add(course);
        ctx.Batches.Add(batch);
        ctx.UserAccounts.Add(user);
        ctx.Staff.Add(staff);
        await ctx.SaveChangesAsync();
        return (batch, staff, user);
    }

    [Fact]
    public async Task Throws_NotFoundException_when_examination_does_not_exist()
    {
        var act = async () => await CreateHandler().Handle(
            new GetExaminationByIdQuery(Guid.NewGuid()), default);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Returns_dto_with_all_fields_populated()
    {
        var (batch, staff, user) = await SeedBatchAsync();
        var examId = Guid.NewGuid();
        await using (var ctx = _fixture.CreateDbContext())
        {
            ctx.Examinations.Add(new Examination
            {
                Id = examId,
                BatchId = batch.Id,
                ExamName = "Final Exam",
                ExamDate = new DateOnly(2024, 5, 20),
                Location = "Hall 3",
                MaxScore = 200,
                PassScore = 100,
                ScheduledById = staff.Id
            });
            await ctx.SaveChangesAsync();
        }

        var dto = await CreateHandler().Handle(new GetExaminationByIdQuery(examId), default);

        dto.ExaminationId.Should().Be(examId);
        dto.BatchId.Should().Be(batch.Id);
        dto.ExamName.Should().Be("Final Exam");
        dto.ExamDate.Should().Be(new DateOnly(2024, 5, 20));
        dto.Location.Should().Be("Hall 3");
        dto.MaxScore.Should().Be(200);
        dto.PassScore.Should().Be(100);
        dto.ScheduledById.Should().Be(staff.Id);
        dto.ScheduledByName.Should().Be(user.FullName);
        dto.HasResults.Should().BeFalse();
        dto.CreatedAt.Should().NotBe(default);
    }

    [Fact]
    public async Task HasResults_true_when_exam_has_at_least_one_result()
    {
        var (batch, staff, _) = await SeedBatchAsync();
        var examId = Guid.NewGuid();

        await using (var ctx = _fixture.CreateDbContext())
        {
            var candidateUser = NewUser($"cand{Random.Shared.Next(10000, 99999)}", "Cand");
            candidateUser.Role = UserRole.Candidate;
            var candidate = new Candidate
            {
                Id = Guid.NewGuid(),
                UserAccountId = candidateUser.Id,
                CandidateCode = $"CC{Random.Shared.Next(10000, 99999)}",
                Status = CandidateStatus.Active,
                RegisteredAt = DateTime.UtcNow
            };
            var enrollment = new Enrollment
            {
                Id = Guid.NewGuid(),
                CandidateId = candidate.Id,
                CourseId = batch.CourseId,
                BatchId = batch.Id,
                EnrollmentDate = new DateOnly(2024, 1, 5),
                Status = EnrollmentStatus.Enrolled
            };
            ctx.UserAccounts.Add(candidateUser);
            ctx.Candidates.Add(candidate);
            ctx.Enrollments.Add(enrollment);
            ctx.Examinations.Add(new Examination
            {
                Id = examId,
                BatchId = batch.Id,
                ExamName = "Quiz",
                ExamDate = new DateOnly(2024, 4, 1),
                MaxScore = 100,
                PassScore = 50,
                ScheduledById = staff.Id
            });
            ctx.ExamResults.Add(new ExamResult
            {
                Id = Guid.NewGuid(),
                ExamId = examId,
                EnrollmentId = enrollment.Id,
                Score = 85m,
                Grade = "A",
                IsPassed = true,
                IsFinalized = true,
                GradedById = staff.Id,
                GradedAt = DateTime.UtcNow
            });
            await ctx.SaveChangesAsync();
        }

        var dto = await CreateHandler().Handle(new GetExaminationByIdQuery(examId), default);

        dto.HasResults.Should().BeTrue();
    }
}
