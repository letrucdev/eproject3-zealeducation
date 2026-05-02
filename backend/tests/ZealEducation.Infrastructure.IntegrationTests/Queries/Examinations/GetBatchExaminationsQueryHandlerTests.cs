using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Features.Examinations.Queries.GetBatchExaminations;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;
using ZealEducation.Infrastructure.IntegrationTests.Fixtures;
using ZealEducation.Infrastructure.Repositories;

namespace ZealEducation.Infrastructure.IntegrationTests.Queries.Examinations;

[Collection(DatabaseCollection.Name)]
public class GetBatchExaminationsQueryHandlerTests : IAsyncLifetime
{
    private readonly MsSqlContainerFixture _fixture;

    public GetBatchExaminationsQueryHandlerTests(MsSqlContainerFixture fixture) => _fixture = fixture;

    public Task InitializeAsync() => _fixture.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private GetBatchExaminationsQueryHandler CreateHandler()
    {
        var ctx = _fixture.CreateDbContext();
        return new GetBatchExaminationsQueryHandler(
            new GenericRepository<Batch>(ctx),
            new GenericRepository<Examination>(ctx));
    }

    private static int _phoneSeed = 200000000;
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
        CourseName = $"Course X{Random.Shared.Next(10000, 99999)}",
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

    private static Examination NewExam(
        Guid batchId,
        Guid scheduledById,
        string name,
        DateOnly date,
        string? location = null,
        int max = 100,
        int pass = 50) => new()
        {
            Id = Guid.NewGuid(),
            BatchId = batchId,
            ScheduledById = scheduledById,
            ExamName = name,
            ExamDate = date,
            Location = location,
            MaxScore = max,
            PassScore = pass
        };

    private async Task<(Batch Batch, Staff Staff, UserAccount User)> SeedBatchAsync()
    {
        await using var ctx = _fixture.CreateDbContext();
        var course = NewCourse();
        var batch = NewBatch(course.Id, $"B{Random.Shared.Next(10000, 99999)}");
        var user = NewUser($"sched{Random.Shared.Next(10000, 99999)}", "Scheduler");
        var staff = NewStaff(user.Id);
        ctx.Courses.Add(course);
        ctx.Batches.Add(batch);
        ctx.UserAccounts.Add(user);
        ctx.Staff.Add(staff);
        await ctx.SaveChangesAsync();
        return (batch, staff, user);
    }

    [Fact]
    public async Task Throws_NotFoundException_when_batch_does_not_exist()
    {
        var act = async () => await CreateHandler().Handle(
            new GetBatchExaminationsQuery(Guid.NewGuid()), default);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Returns_empty_when_batch_has_no_examinations()
    {
        var (batch, _, _) = await SeedBatchAsync();

        var result = await CreateHandler().Handle(new GetBatchExaminationsQuery(batch.Id), default);

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Returns_paginated_examinations_with_default_paging()
    {
        var (batch, staff, _) = await SeedBatchAsync();
        await using (var ctx = _fixture.CreateDbContext())
        {
            for (var i = 0; i < 15; i++)
                ctx.Examinations.Add(NewExam(batch.Id, staff.Id, $"Exam {i:D2}", new DateOnly(2024, 3, 1).AddDays(i)));
            await ctx.SaveChangesAsync();
        }

        var result = await CreateHandler().Handle(new GetBatchExaminationsQuery(batch.Id), default);

        result.Items.Should().HaveCount(10);
        result.TotalCount.Should().Be(15);
        result.HasNextPage.Should().BeTrue();
        result.HasPreviousPage.Should().BeFalse();
    }

    [Fact]
    public async Task Filters_examinations_by_batch()
    {
        var (batchA, staffA, _) = await SeedBatchAsync();
        var (batchB, staffB, _) = await SeedBatchAsync();
        await using (var ctx = _fixture.CreateDbContext())
        {
            ctx.Examinations.Add(NewExam(batchA.Id, staffA.Id, "ExamA", new DateOnly(2024, 4, 1)));
            ctx.Examinations.Add(NewExam(batchB.Id, staffB.Id, "ExamB", new DateOnly(2024, 4, 1)));
            await ctx.SaveChangesAsync();
        }

        var result = await CreateHandler().Handle(new GetBatchExaminationsQuery(batchA.Id), default);

        result.Items.Should().HaveCount(1);
        result.Items.Single().ExamName.Should().Be("ExamA");
        result.Items.Single().BatchId.Should().Be(batchA.Id);
    }

    [Fact]
    public async Task Filters_by_search_in_exam_name_case_insensitive()
    {
        var (batch, staff, _) = await SeedBatchAsync();
        await using (var ctx = _fixture.CreateDbContext())
        {
            ctx.Examinations.AddRange(
                NewExam(batch.Id, staff.Id, "Midterm Algebra", new DateOnly(2024, 3, 1)),
                NewExam(batch.Id, staff.Id, "Final Calculus", new DateOnly(2024, 5, 1)),
                NewExam(batch.Id, staff.Id, "Algebra Quiz", new DateOnly(2024, 4, 1)));
            await ctx.SaveChangesAsync();
        }

        var result = await CreateHandler().Handle(
            new GetBatchExaminationsQuery(batch.Id, Search: "ALGEBRA"), default);

        result.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task Filters_by_search_in_location()
    {
        var (batch, staff, _) = await SeedBatchAsync();
        await using (var ctx = _fixture.CreateDbContext())
        {
            ctx.Examinations.AddRange(
                NewExam(batch.Id, staff.Id, "Exam 1", new DateOnly(2024, 3, 1), location: "Room 101"),
                NewExam(batch.Id, staff.Id, "Exam 2", new DateOnly(2024, 4, 1), location: "Hall A"));
            await ctx.SaveChangesAsync();
        }

        var result = await CreateHandler().Handle(
            new GetBatchExaminationsQuery(batch.Id, Search: "room"), default);

        result.Items.Should().HaveCount(1);
        result.Items.Single().Location.Should().Be("Room 101");
    }

    [Fact]
    public async Task Sorts_by_exam_name_ascending()
    {
        var (batch, staff, _) = await SeedBatchAsync();
        await using (var ctx = _fixture.CreateDbContext())
        {
            ctx.Examinations.AddRange(
                NewExam(batch.Id, staff.Id, "Charlie", new DateOnly(2024, 3, 1)),
                NewExam(batch.Id, staff.Id, "Alpha", new DateOnly(2024, 3, 2)),
                NewExam(batch.Id, staff.Id, "Bravo", new DateOnly(2024, 3, 3)));
            await ctx.SaveChangesAsync();
        }

        var result = await CreateHandler().Handle(
            new GetBatchExaminationsQuery(batch.Id, SortBy: "examName", SortDirection: "asc"), default);

        result.Items.Select(e => e.ExamName).Should().ContainInOrder("Alpha", "Bravo", "Charlie");
    }

    [Fact]
    public async Task Sorts_by_max_score_descending()
    {
        var (batch, staff, _) = await SeedBatchAsync();
        await using (var ctx = _fixture.CreateDbContext())
        {
            ctx.Examinations.AddRange(
                NewExam(batch.Id, staff.Id, "Low", new DateOnly(2024, 3, 1), max: 50),
                NewExam(batch.Id, staff.Id, "High", new DateOnly(2024, 3, 2), max: 200),
                NewExam(batch.Id, staff.Id, "Mid", new DateOnly(2024, 3, 3), max: 100));
            await ctx.SaveChangesAsync();
        }

        var result = await CreateHandler().Handle(
            new GetBatchExaminationsQuery(batch.Id, SortBy: "maxScore", SortDirection: "desc"), default);

        result.Items.Select(e => e.ExamName).Should().ContainInOrder("High", "Mid", "Low");
    }

    [Fact]
    public async Task Defaults_to_exam_date_descending_when_sort_unspecified()
    {
        var (batch, staff, _) = await SeedBatchAsync();
        await using (var ctx = _fixture.CreateDbContext())
        {
            ctx.Examinations.AddRange(
                NewExam(batch.Id, staff.Id, "Old", new DateOnly(2024, 1, 1)),
                NewExam(batch.Id, staff.Id, "Newest", new DateOnly(2024, 12, 1)),
                NewExam(batch.Id, staff.Id, "Mid", new DateOnly(2024, 6, 1)));
            await ctx.SaveChangesAsync();
        }

        var result = await CreateHandler().Handle(new GetBatchExaminationsQuery(batch.Id), default);

        result.Items.Select(e => e.ExamName).Should().ContainInOrder("Newest", "Mid", "Old");
    }

    [Fact]
    public async Task Clamps_page_below_1_to_first_page()
    {
        var (batch, staff, _) = await SeedBatchAsync();
        await using (var ctx = _fixture.CreateDbContext())
        {
            ctx.Examinations.Add(NewExam(batch.Id, staff.Id, "Solo", new DateOnly(2024, 3, 1)));
            await ctx.SaveChangesAsync();
        }

        var result = await CreateHandler().Handle(
            new GetBatchExaminationsQuery(batch.Id, Page: -1), default);

        result.PageNumber.Should().Be(1);
    }

    [Fact]
    public async Task Clamps_page_size_above_100_to_100()
    {
        var (batch, staff, _) = await SeedBatchAsync();
        await using (var ctx = _fixture.CreateDbContext())
        {
            ctx.Examinations.Add(NewExam(batch.Id, staff.Id, "Solo", new DateOnly(2024, 3, 1)));
            await ctx.SaveChangesAsync();
        }

        var result = await CreateHandler().Handle(
            new GetBatchExaminationsQuery(batch.Id, PageSize: 500), default);

        result.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task Returns_second_page_correctly()
    {
        var (batch, staff, _) = await SeedBatchAsync();
        await using (var ctx = _fixture.CreateDbContext())
        {
            for (var i = 0; i < 12; i++)
                ctx.Examinations.Add(NewExam(batch.Id, staff.Id, $"Exam {i:D2}", new DateOnly(2024, 3, 1).AddDays(i)));
            await ctx.SaveChangesAsync();
        }

        var result = await CreateHandler().Handle(
            new GetBatchExaminationsQuery(batch.Id, Page: 2, PageSize: 10), default);

        result.Items.Should().HaveCount(2);
        result.PageNumber.Should().Be(2);
        result.HasPreviousPage.Should().BeTrue();
        result.HasNextPage.Should().BeFalse();
    }

    [Fact]
    public async Task Maps_all_dto_fields_correctly_with_has_results()
    {
        var (batch, staff, user) = await SeedBatchAsync();
        var examId = Guid.NewGuid();
        await using (var ctx = _fixture.CreateDbContext())
        {
            ctx.Examinations.Add(new Examination
            {
                Id = examId,
                BatchId = batch.Id,
                ExamName = "Mapped Exam",
                ExamDate = new DateOnly(2024, 4, 15),
                Location = "Lab 5",
                MaxScore = 150,
                PassScore = 75,
                ScheduledById = staff.Id
            });
            await ctx.SaveChangesAsync();
        }

        var result = await CreateHandler().Handle(new GetBatchExaminationsQuery(batch.Id), default);

        var dto = result.Items.Single();
        dto.ExaminationId.Should().Be(examId);
        dto.BatchId.Should().Be(batch.Id);
        dto.ExamName.Should().Be("Mapped Exam");
        dto.ExamDate.Should().Be(new DateOnly(2024, 4, 15));
        dto.Location.Should().Be("Lab 5");
        dto.MaxScore.Should().Be(150);
        dto.PassScore.Should().Be(75);
        dto.ScheduledById.Should().Be(staff.Id);
        dto.ScheduledByName.Should().Be(user.FullName);
        dto.HasResults.Should().BeFalse();
        dto.CreatedAt.Should().NotBe(default);
    }
}
