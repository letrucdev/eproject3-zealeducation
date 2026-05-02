using ZealEducation.Application.Features.Courses.Queries.GetCourseStatistics;
using ZealEducation.Domain.Entities;
using ZealEducation.Infrastructure.IntegrationTests.Fixtures;
using ZealEducation.Infrastructure.Repositories;

namespace ZealEducation.Infrastructure.IntegrationTests.Queries.Courses;

[Collection(DatabaseCollection.Name)]
public class GetCourseStatisticsQueryHandlerTests : IAsyncLifetime
{
    private readonly MsSqlContainerFixture _fixture;

    public GetCourseStatisticsQueryHandlerTests(MsSqlContainerFixture fixture) => _fixture = fixture;

    public Task InitializeAsync() => _fixture.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private GetCourseStatisticsQueryHandler CreateHandler()
    {
        var ctx = _fixture.CreateDbContext();
        return new GetCourseStatisticsQueryHandler(new GenericRepository<Course>(ctx));
    }

    private static Course NewCourse(string name, bool active) => new()
    {
        Id = Guid.NewGuid(),
        CourseName = name,
        DurationWeeks = 8,
        BaseFee = 1000m,
        IsActive = active
    };

    [Fact]
    public async Task Returns_zero_counts_when_no_courses_exist()
    {
        var dto = await CreateHandler().Handle(new GetCourseStatisticsQuery(), default);

        dto.Total.Should().Be(0);
        dto.Active.Should().Be(0);
        dto.Inactive.Should().Be(0);
    }

    [Fact]
    public async Task Counts_active_and_inactive_courses_correctly()
    {
        await using (var ctx = _fixture.CreateDbContext())
        {
            ctx.Courses.AddRange(
                NewCourse("Active 1", true),
                NewCourse("Active 2", true),
                NewCourse("Active 3", true),
                NewCourse("Inactive 1", false),
                NewCourse("Inactive 2", false));
            await ctx.SaveChangesAsync();
        }

        var dto = await CreateHandler().Handle(new GetCourseStatisticsQuery(), default);

        dto.Total.Should().Be(5);
        dto.Active.Should().Be(3);
        dto.Inactive.Should().Be(2);
    }

    [Fact]
    public async Task Counts_all_active_when_no_inactive_present()
    {
        await using (var ctx = _fixture.CreateDbContext())
        {
            ctx.Courses.AddRange(
                NewCourse("A", true),
                NewCourse("B", true));
            await ctx.SaveChangesAsync();
        }

        var dto = await CreateHandler().Handle(new GetCourseStatisticsQuery(), default);

        dto.Total.Should().Be(2);
        dto.Active.Should().Be(2);
        dto.Inactive.Should().Be(0);
    }

    [Fact]
    public async Task Counts_all_inactive_when_no_active_present()
    {
        await using (var ctx = _fixture.CreateDbContext())
        {
            ctx.Courses.AddRange(
                NewCourse("A", false),
                NewCourse("B", false),
                NewCourse("C", false));
            await ctx.SaveChangesAsync();
        }

        var dto = await CreateHandler().Handle(new GetCourseStatisticsQuery(), default);

        dto.Total.Should().Be(3);
        dto.Active.Should().Be(0);
        dto.Inactive.Should().Be(3);
    }
}
