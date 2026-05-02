using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Features.Courses.Queries.GetCourseById;
using ZealEducation.Domain.Entities;
using ZealEducation.Infrastructure.IntegrationTests.Fixtures;
using ZealEducation.Infrastructure.Repositories;

namespace ZealEducation.Infrastructure.IntegrationTests.Queries.Courses;

[Collection(DatabaseCollection.Name)]
public class GetCourseByIdQueryHandlerTests : IAsyncLifetime
{
    private readonly MsSqlContainerFixture _fixture;

    public GetCourseByIdQueryHandlerTests(MsSqlContainerFixture fixture) => _fixture = fixture;

    public Task InitializeAsync() => _fixture.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private GetCourseByIdQueryHandler CreateHandler()
    {
        var ctx = _fixture.CreateDbContext();
        return new GetCourseByIdQueryHandler(new GenericRepository<Course>(ctx));
    }

    [Fact]
    public async Task Throws_NotFoundException_when_course_does_not_exist()
    {
        var act = async () => await CreateHandler().Handle(new GetCourseByIdQuery(Guid.NewGuid()), default);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Returns_dto_with_all_fields_populated()
    {
        var courseId = Guid.NewGuid();
        await using (var ctx = _fixture.CreateDbContext())
        {
            ctx.Courses.Add(new Course
            {
                Id = courseId,
                CourseName = "Mapped Course",
                Description = "Detailed description",
                DurationWeeks = 16,
                BaseFee = 2500m,
                IsActive = true
            });
            await ctx.SaveChangesAsync();
        }

        var dto = await CreateHandler().Handle(new GetCourseByIdQuery(courseId), default);

        dto.CourseId.Should().Be(courseId);
        dto.CourseName.Should().Be("Mapped Course");
        dto.Description.Should().Be("Detailed description");
        dto.DurationWeeks.Should().Be(16);
        dto.BaseFee.Should().Be(2500m);
        dto.IsActive.Should().BeTrue();
        dto.CreatedAt.Should().NotBe(default);
        dto.UpdatedAt.Should().NotBe(default);
    }

    [Fact]
    public async Task Returns_inactive_course_when_requested_by_id()
    {
        var courseId = Guid.NewGuid();
        await using (var ctx = _fixture.CreateDbContext())
        {
            ctx.Courses.Add(new Course
            {
                Id = courseId,
                CourseName = "Archived",
                DurationWeeks = 4,
                BaseFee = 100m,
                IsActive = false
            });
            await ctx.SaveChangesAsync();
        }

        var dto = await CreateHandler().Handle(new GetCourseByIdQuery(courseId), default);

        dto.IsActive.Should().BeFalse();
        dto.CourseName.Should().Be("Archived");
    }
}
