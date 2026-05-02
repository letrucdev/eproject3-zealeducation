using ZealEducation.Application.Features.Feedback.Queries.GetFeedbacks;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;
using ZealEducation.Infrastructure.IntegrationTests.Fixtures;
using ZealEducation.Infrastructure.Repositories;

namespace ZealEducation.Infrastructure.IntegrationTests.Queries.Feedback;

[Collection(DatabaseCollection.Name)]
public class GetFeedbacksQueryHandlerTests : IAsyncLifetime
{
    private readonly MsSqlContainerFixture _fixture;

    public GetFeedbacksQueryHandlerTests(MsSqlContainerFixture fixture) => _fixture = fixture;

    public Task InitializeAsync() => _fixture.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private GetFeedbacksQueryHandler CreateHandler()
    {
        var ctx = _fixture.CreateDbContext();
        return new GetFeedbacksQueryHandler(
            new GenericRepository<Domain.Entities.Feedback>(ctx),
            new GenericRepository<Candidate>(ctx),
            new GenericRepository<UserAccount>(ctx),
            new GenericRepository<Batch>(ctx),
            new GenericRepository<Course>(ctx),
            new GenericRepository<Faculty>(ctx),
            new GenericRepository<Staff>(ctx));
    }

    private static int _phoneSeed = 100000000;
    private static string NextPhone() => $"0{Interlocked.Increment(ref _phoneSeed)}";

    private static UserAccount NewUser(string username, string fullName, UserRole role = UserRole.Candidate) => new()
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

    private static Faculty NewFaculty(Guid staffId, string code) => new()
    {
        Id = Guid.NewGuid(),
        StaffId = staffId,
        FacultyCode = code,
        Qualification = "MSc",
        Specialization = "CS",
        ExperienceYears = 5
    };

    private static Candidate NewCandidate(Guid userId, string code) => new()
    {
        Id = Guid.NewGuid(),
        UserAccountId = userId,
        CandidateCode = code,
        Status = CandidateStatus.Active,
        RegisteredAt = DateTime.UtcNow
    };

    private static Course NewCourse(string name) => new()
    {
        Id = Guid.NewGuid(),
        CourseName = name,
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

    private record SeedContext(
        Course Course,
        Batch Batch,
        Candidate Candidate,
        UserAccount CandidateUser,
        Faculty Faculty,
        Staff FacultyStaff,
        UserAccount FacultyUser);

    private async Task<SeedContext> SeedBaseAsync(string suffix = "")
    {
        await using var ctx = _fixture.CreateDbContext();
        var course = NewCourse($"Course{suffix}");
        var batch = NewBatch(course.Id, $"BATCH{suffix}{Random.Shared.Next(1000, 9999)}");

        var candidateUser = NewUser($"cand{suffix}{Random.Shared.Next(1000, 9999)}", $"Candidate {suffix}");
        var candidate = NewCandidate(candidateUser.Id, $"C{suffix}{Random.Shared.Next(10000, 99999)}");

        var facultyUser = NewUser($"fac{suffix}{Random.Shared.Next(1000, 9999)}", $"Faculty {suffix}", UserRole.Faculty);
        var facultyStaff = NewStaff(facultyUser.Id);
        var faculty = NewFaculty(facultyStaff.Id, $"FAC{suffix}{Random.Shared.Next(1000, 9999)}");

        ctx.Courses.Add(course);
        ctx.Batches.Add(batch);
        ctx.UserAccounts.Add(candidateUser);
        ctx.Candidates.Add(candidate);
        ctx.UserAccounts.Add(facultyUser);
        ctx.Staff.Add(facultyStaff);
        ctx.Faculties.Add(faculty);
        await ctx.SaveChangesAsync();

        return new SeedContext(course, batch, candidate, candidateUser, faculty, facultyStaff, facultyUser);
    }

    private static Domain.Entities.Feedback NewFeedback(
        Guid candidateId,
        Guid batchId,
        FeedbackType type = FeedbackType.Course,
        Guid? targetFacultyId = null,
        int rating = 5,
        string? comment = null,
        bool isProcessed = false,
        Guid? processedById = null,
        DateTime? processedAt = null) => new()
        {
            Id = Guid.NewGuid(),
            CandidateId = candidateId,
            BatchId = batchId,
            Type = type,
            TargetFacultyId = targetFacultyId,
            Rating = rating,
            Comment = comment,
            IsProcessed = isProcessed,
            ProcessedById = processedById,
            ProcessedAt = processedAt
        };

    [Fact]
    public async Task Returns_empty_list_when_no_feedbacks_exist()
    {
        var result = await CreateHandler().Handle(new GetFeedbacksQuery(), default);

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.PageNumber.Should().Be(1);
    }

    [Fact]
    public async Task Returns_paginated_feedbacks_with_default_paging()
    {
        var seed = await SeedBaseAsync();
        await using (var ctx = _fixture.CreateDbContext())
        {
            for (var i = 0; i < 15; i++)
            {
                var u = NewUser($"x{i:D2}", $"X {i:D2}");
                var c = NewCandidate(u.Id, $"CX{i:D3}");
                ctx.UserAccounts.Add(u);
                ctx.Candidates.Add(c);
                ctx.Feedbacks.Add(NewFeedback(c.Id, seed.Batch.Id, type: FeedbackType.Course, rating: 5));
            }
            await ctx.SaveChangesAsync();
        }

        var result = await CreateHandler().Handle(new GetFeedbacksQuery(), default);

        result.Items.Should().HaveCount(10);
        result.TotalCount.Should().Be(15);
        result.PageNumber.Should().Be(1);
        result.TotalPages.Should().Be(2);
        result.HasNextPage.Should().BeTrue();
    }

    [Fact]
    public async Task Filters_by_is_processed()
    {
        var seed = await SeedBaseAsync();
        await using (var ctx = _fixture.CreateDbContext())
        {
            ctx.Feedbacks.AddRange(
                NewFeedback(seed.Candidate.Id, seed.Batch.Id, type: FeedbackType.Course, isProcessed: true),
                NewFeedback(seed.Candidate.Id, seed.Batch.Id, type: FeedbackType.General, isProcessed: false));
            await ctx.SaveChangesAsync();
        }

        var processed = await CreateHandler().Handle(new GetFeedbacksQuery(IsProcessed: true), default);
        processed.Items.Should().HaveCount(1);
        processed.Items.Single().IsProcessed.Should().BeTrue();

        var unprocessed = await CreateHandler().Handle(new GetFeedbacksQuery(IsProcessed: false), default);
        unprocessed.Items.Should().HaveCount(1);
        unprocessed.Items.Single().IsProcessed.Should().BeFalse();
    }

    [Fact]
    public async Task Filters_by_type()
    {
        var seed = await SeedBaseAsync();
        await using (var ctx = _fixture.CreateDbContext())
        {
            ctx.Feedbacks.AddRange(
                NewFeedback(seed.Candidate.Id, seed.Batch.Id, type: FeedbackType.Faculty, targetFacultyId: seed.Faculty.Id),
                NewFeedback(seed.Candidate.Id, seed.Batch.Id, type: FeedbackType.Course),
                NewFeedback(seed.Candidate.Id, seed.Batch.Id, type: FeedbackType.General));
            await ctx.SaveChangesAsync();
        }

        var result = await CreateHandler().Handle(new GetFeedbacksQuery(Type: FeedbackType.Course), default);

        result.Items.Should().HaveCount(1);
        result.Items.Single().Type.Should().Be(FeedbackType.Course);
    }

    [Fact]
    public async Task Filters_by_batch_id()
    {
        var seed1 = await SeedBaseAsync("a");
        var seed2 = await SeedBaseAsync("b");
        await using (var ctx = _fixture.CreateDbContext())
        {
            ctx.Feedbacks.AddRange(
                NewFeedback(seed1.Candidate.Id, seed1.Batch.Id, type: FeedbackType.Course),
                NewFeedback(seed2.Candidate.Id, seed2.Batch.Id, type: FeedbackType.Course));
            await ctx.SaveChangesAsync();
        }

        var result = await CreateHandler().Handle(new GetFeedbacksQuery(BatchId: seed1.Batch.Id), default);

        result.Items.Should().HaveCount(1);
        result.Items.Single().BatchId.Should().Be(seed1.Batch.Id);
    }

    [Fact]
    public async Task Filters_by_rating()
    {
        var seed = await SeedBaseAsync();
        await using (var ctx = _fixture.CreateDbContext())
        {
            ctx.Feedbacks.AddRange(
                NewFeedback(seed.Candidate.Id, seed.Batch.Id, type: FeedbackType.Course, rating: 5),
                NewFeedback(seed.Candidate.Id, seed.Batch.Id, type: FeedbackType.General, rating: 3));
            await ctx.SaveChangesAsync();
        }

        var result = await CreateHandler().Handle(new GetFeedbacksQuery(Rating: 5), default);

        result.Items.Should().HaveCount(1);
        result.Items.Single().Rating.Should().Be(5);
    }

    [Fact]
    public async Task Filters_by_search_in_comment_case_insensitive()
    {
        var seed = await SeedBaseAsync();
        await using (var ctx = _fixture.CreateDbContext())
        {
            ctx.Feedbacks.AddRange(
                NewFeedback(seed.Candidate.Id, seed.Batch.Id, type: FeedbackType.Course, comment: "Excellent course"),
                NewFeedback(seed.Candidate.Id, seed.Batch.Id, type: FeedbackType.General, comment: "Average experience"));
            await ctx.SaveChangesAsync();
        }

        var result = await CreateHandler().Handle(new GetFeedbacksQuery(Search: "EXCELLENT"), default);

        result.Items.Should().HaveCount(1);
        result.Items.Single().Comment.Should().Be("Excellent course");
    }

    [Fact]
    public async Task Sorts_by_rating_ascending()
    {
        var seed = await SeedBaseAsync();
        await using (var ctx = _fixture.CreateDbContext())
        {
            ctx.Feedbacks.AddRange(
                NewFeedback(seed.Candidate.Id, seed.Batch.Id, type: FeedbackType.Course, rating: 5),
                NewFeedback(seed.Candidate.Id, seed.Batch.Id, type: FeedbackType.General, rating: 1),
                NewFeedback(seed.Candidate.Id, seed.Batch.Id, type: FeedbackType.Faculty, targetFacultyId: seed.Faculty.Id, rating: 3));
            await ctx.SaveChangesAsync();
        }

        var result = await CreateHandler().Handle(new GetFeedbacksQuery(SortBy: "rating", SortDirection: "asc"), default);

        result.Items.Select(f => f.Rating).Should().ContainInOrder(1, 3, 5);
    }

    [Fact]
    public async Task Sorts_by_rating_descending()
    {
        var seed = await SeedBaseAsync();
        await using (var ctx = _fixture.CreateDbContext())
        {
            ctx.Feedbacks.AddRange(
                NewFeedback(seed.Candidate.Id, seed.Batch.Id, type: FeedbackType.Course, rating: 2),
                NewFeedback(seed.Candidate.Id, seed.Batch.Id, type: FeedbackType.General, rating: 5),
                NewFeedback(seed.Candidate.Id, seed.Batch.Id, type: FeedbackType.Faculty, targetFacultyId: seed.Faculty.Id, rating: 4));
            await ctx.SaveChangesAsync();
        }

        var result = await CreateHandler().Handle(new GetFeedbacksQuery(SortBy: "rating", SortDirection: "desc"), default);

        result.Items.Select(f => f.Rating).Should().ContainInOrder(5, 4, 2);
    }

    [Fact]
    public async Task Defaults_to_created_at_descending_when_sort_unspecified()
    {
        var seed = await SeedBaseAsync();
        var firstId = Guid.NewGuid();
        var secondId = Guid.NewGuid();
        var thirdId = Guid.NewGuid();
        await using (var ctx = _fixture.CreateDbContext())
        {
            ctx.Feedbacks.Add(new Domain.Entities.Feedback
            {
                Id = firstId,
                CandidateId = seed.Candidate.Id,
                BatchId = seed.Batch.Id,
                Type = FeedbackType.Course,
                Rating = 5
            });
            await ctx.SaveChangesAsync();
            await Task.Delay(50);
            ctx.Feedbacks.Add(new Domain.Entities.Feedback
            {
                Id = secondId,
                CandidateId = seed.Candidate.Id,
                BatchId = seed.Batch.Id,
                Type = FeedbackType.General,
                Rating = 4
            });
            await ctx.SaveChangesAsync();
            await Task.Delay(50);
            ctx.Feedbacks.Add(new Domain.Entities.Feedback
            {
                Id = thirdId,
                CandidateId = seed.Candidate.Id,
                BatchId = seed.Batch.Id,
                Type = FeedbackType.Faculty,
                TargetFacultyId = seed.Faculty.Id,
                Rating = 3
            });
            await ctx.SaveChangesAsync();
        }

        var result = await CreateHandler().Handle(new GetFeedbacksQuery(), default);

        result.Items.Select(f => f.FeedbackId).Should().ContainInOrder(thirdId, secondId, firstId);
    }

    [Fact]
    public async Task Clamps_page_size_above_100_to_100()
    {
        var seed = await SeedBaseAsync();
        await using (var ctx = _fixture.CreateDbContext())
        {
            ctx.Feedbacks.Add(NewFeedback(seed.Candidate.Id, seed.Batch.Id, type: FeedbackType.Course));
            await ctx.SaveChangesAsync();
        }

        var result = await CreateHandler().Handle(new GetFeedbacksQuery(PageSize: 500), default);

        result.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task Clamps_page_below_1_to_first_page()
    {
        var seed = await SeedBaseAsync();
        await using (var ctx = _fixture.CreateDbContext())
        {
            ctx.Feedbacks.Add(NewFeedback(seed.Candidate.Id, seed.Batch.Id, type: FeedbackType.Course));
            await ctx.SaveChangesAsync();
        }

        var result = await CreateHandler().Handle(new GetFeedbacksQuery(Page: -3), default);

        result.PageNumber.Should().Be(1);
    }

    [Fact]
    public async Task Maps_all_dto_fields_correctly_with_target_faculty()
    {
        var seed = await SeedBaseAsync();
        var processedById = Guid.NewGuid();
        var processedAt = new DateTime(2024, 6, 1, 12, 0, 0, DateTimeKind.Utc);
        var feedbackId = Guid.NewGuid();

        await using (var ctx = _fixture.CreateDbContext())
        {
            var processor = NewUser("processor", "Processor User", UserRole.Incharge);
            processor.Id = processedById;
            ctx.UserAccounts.Add(processor);
            ctx.Feedbacks.Add(new Domain.Entities.Feedback
            {
                Id = feedbackId,
                CandidateId = seed.Candidate.Id,
                BatchId = seed.Batch.Id,
                Type = FeedbackType.Faculty,
                TargetFacultyId = seed.Faculty.Id,
                Rating = 4,
                Comment = "Helpful",
                IsProcessed = true,
                ProcessedById = processedById,
                ProcessedAt = processedAt
            });
            await ctx.SaveChangesAsync();
        }

        var result = await CreateHandler().Handle(new GetFeedbacksQuery(), default);

        var dto = result.Items.Single();
        dto.FeedbackId.Should().Be(feedbackId);
        dto.CandidateId.Should().Be(seed.Candidate.Id);
        dto.CandidateCode.Should().Be(seed.Candidate.CandidateCode);
        dto.CandidateName.Should().Be(seed.CandidateUser.FullName);
        dto.BatchId.Should().Be(seed.Batch.Id);
        dto.BatchCode.Should().Be(seed.Batch.BatchCode);
        dto.CourseName.Should().Be(seed.Course.CourseName);
        dto.Type.Should().Be(FeedbackType.Faculty);
        dto.TargetFacultyId.Should().Be(seed.Faculty.Id);
        dto.TargetFacultyName.Should().Be(seed.FacultyUser.FullName);
        dto.Rating.Should().Be(4);
        dto.Comment.Should().Be("Helpful");
        dto.IsProcessed.Should().BeTrue();
        dto.ProcessedById.Should().Be(processedById);
        dto.ProcessedByName.Should().Be("Processor User");
        dto.ProcessedAt.Should().BeCloseTo(processedAt, TimeSpan.FromSeconds(1));
        dto.CreatedAt.Should().NotBe(default);
    }
}
