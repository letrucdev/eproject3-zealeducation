using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Features.Candidates.Queries.GetCandidateDetail;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;
using ZealEducation.Infrastructure.IntegrationTests.Fixtures;
using ZealEducation.Infrastructure.Repositories;

namespace ZealEducation.Infrastructure.IntegrationTests.Queries.Candidates;

[Collection(DatabaseCollection.Name)]
public class GetCandidateDetailQueryHandlerTests : IAsyncLifetime
{
    private readonly MsSqlContainerFixture _fixture;

    public GetCandidateDetailQueryHandlerTests(MsSqlContainerFixture fixture) => _fixture = fixture;

    public Task InitializeAsync() => _fixture.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private GetCandidateDetailQueryHandler CreateHandler()
    {
        var ctx = _fixture.CreateDbContext();
        return new GetCandidateDetailQueryHandler(
            new GenericRepository<Candidate>(ctx),
            new GenericRepository<UserAccount>(ctx),
            new GenericRepository<Enrollment>(ctx),
            new GenericRepository<Course>(ctx),
            new GenericRepository<Batch>(ctx),
            new GenericRepository<FeeStructure>(ctx));
    }

    private static UserAccount NewUser(string suffix) => new()
    {
        Id = Guid.NewGuid(),
        Username = $"detail_{suffix}",
        PasswordHash = "hash",
        FullName = $"Detail User {suffix}",
        Email = $"detail_{suffix}@example.com",
        Phone = $"0922{suffix}",
        Dob = new DateOnly(1995, 6, 15),
        Gender = Gender.Female,
        Role = UserRole.Candidate,
        IsActive = true
    };

    [Fact]
    public async Task Throws_NotFoundException_when_candidate_id_missing()
    {
        var act = async () =>
            await CreateHandler().Handle(new GetCandidateDetailQuery(Guid.NewGuid()), default);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Returns_dto_with_candidate_and_user_fields_populated()
    {
        var user = NewUser("a01");
        var candidate = new Candidate
        {
            Id = Guid.NewGuid(),
            UserAccountId = user.Id,
            CandidateCode = "DET001",
            Address = "123 Main St",
            EmergencyContact = "Mom 0911999000",
            Notes = "Top performer",
            Status = CandidateStatus.Active,
            RegisteredAt = DateTime.UtcNow
        };

        await using (var ctx = _fixture.CreateDbContext())
        {
            ctx.UserAccounts.Add(user);
            await ctx.SaveChangesAsync();
            ctx.Candidates.Add(candidate);
            await ctx.SaveChangesAsync();
        }

        var dto = await CreateHandler().Handle(new GetCandidateDetailQuery(candidate.Id), default);

        dto.CandidateId.Should().Be(candidate.Id);
        dto.CandidateCode.Should().Be("DET001");
        dto.FullName.Should().Be(user.FullName);
        dto.Email.Should().Be(user.Email);
        dto.Phone.Should().Be(user.Phone);
        dto.Dob.Should().Be(user.Dob);
        dto.Gender.Should().Be(Gender.Female);
        dto.IsActive.Should().BeTrue();
        dto.Address.Should().Be("123 Main St");
        dto.EmergencyContact.Should().Be("Mom 0911999000");
        dto.Notes.Should().Be("Top performer");
        dto.Status.Should().Be(CandidateStatus.Active);
        dto.Enrollments.Should().BeEmpty();
        dto.FeeStructures.Should().BeEmpty();
    }

    [Fact]
    public async Task Returns_enrollments_and_fee_structures_for_candidate()
    {
        var user = NewUser("b02");
        var candidate = new Candidate
        {
            Id = Guid.NewGuid(),
            UserAccountId = user.Id,
            CandidateCode = "DET002",
            Status = CandidateStatus.Active,
            RegisteredAt = DateTime.UtcNow
        };
        var course = new Course
        {
            Id = Guid.NewGuid(),
            CourseName = "Detail Course",
            DurationWeeks = 10,
            BaseFee = 1500m,
            IsActive = true
        };
        var batch = new Batch
        {
            Id = Guid.NewGuid(),
            CourseId = course.Id,
            BatchCode = "B-DET",
            StartDate = new DateOnly(2025, 1, 1),
            EndDate = new DateOnly(2025, 3, 1)
        };
        var fee = new FeeStructure
        {
            Id = Guid.NewGuid(),
            CandidateId = candidate.Id,
            TotalFee = 1500m,
            AmountPaid = 500m,
            OutstandingBalance = 1000m,
            FeeType = FeeType.Tuition,
            PaymentStatus = PaymentStatus.Partial,
            PaymentType = PaymentType.FullPayment
        };
        var enrollment = new Enrollment
        {
            Id = Guid.NewGuid(),
            CandidateId = candidate.Id,
            CourseId = course.Id,
            BatchId = batch.Id,
            FeeId = fee.Id,
            EnrollmentDate = new DateOnly(2025, 1, 5),
            Status = EnrollmentStatus.Enrolled
        };

        await using (var ctx = _fixture.CreateDbContext())
        {
            ctx.UserAccounts.Add(user);
            ctx.Courses.Add(course);
            await ctx.SaveChangesAsync();
            ctx.Candidates.Add(candidate);
            ctx.Batches.Add(batch);
            await ctx.SaveChangesAsync();
            ctx.FeeStructures.Add(fee);
            await ctx.SaveChangesAsync();
            ctx.Enrollments.Add(enrollment);
            await ctx.SaveChangesAsync();
        }

        var dto = await CreateHandler().Handle(new GetCandidateDetailQuery(candidate.Id), default);

        dto.Enrollments.Should().HaveCount(1);
        var enrollmentDto = dto.Enrollments.Single();
        enrollmentDto.EnrollmentId.Should().Be(enrollment.Id);
        enrollmentDto.CourseId.Should().Be(course.Id);
        enrollmentDto.CourseName.Should().Be("Detail Course");
        enrollmentDto.DurationWeeks.Should().Be(10);
        enrollmentDto.BatchId.Should().Be(batch.Id);
        enrollmentDto.BatchCode.Should().Be("B-DET");
        enrollmentDto.BatchStartDate.Should().Be(new DateOnly(2025, 1, 1));
        enrollmentDto.BatchEndDate.Should().Be(new DateOnly(2025, 3, 1));
        enrollmentDto.Status.Should().Be(EnrollmentStatus.Enrolled);
        enrollmentDto.FeeId.Should().Be(fee.Id);

        dto.FeeStructures.Should().HaveCount(1);
        var feeDto = dto.FeeStructures.Single();
        feeDto.FeeId.Should().Be(fee.Id);
        feeDto.TotalFee.Should().Be(1500m);
        feeDto.AmountPaid.Should().Be(500m);
        feeDto.OutstandingBalance.Should().Be(1000m);
        feeDto.PaymentStatus.Should().Be(PaymentStatus.Partial);
        feeDto.PaymentType.Should().Be(PaymentType.FullPayment);
        feeDto.CourseTitle.Should().Be("Detail Course");
    }
}
