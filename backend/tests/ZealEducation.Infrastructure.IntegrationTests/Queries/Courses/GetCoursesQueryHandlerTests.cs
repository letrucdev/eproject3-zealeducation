using ZealEducation.Application.Features.Courses.Queries.GetCourses;
using ZealEducation.Domain.Entities;
using ZealEducation.Infrastructure.IntegrationTests.Fixtures;
using ZealEducation.Infrastructure.Repositories;

namespace ZealEducation.Infrastructure.IntegrationTests.Queries.Courses;

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

    private static Course NewCourse(string name = "Course", bool active = true, int weeks = 8, decimal fee = 1000m, string? description = null) => new()
    {
        Id = Guid.NewGuid(),
        CourseName = name,
        DurationWeeks = weeks,
        BaseFee = fee,
        IsActive = active,
        Description = description
    };

    [Fact]
    public async Task Returns_empty_list_when_no_courses_exist()
    {
        var result = await CreateHandler().Handle(new GetCoursesQuery(), default);

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.PageNumber.Should().Be(1);
    }

    [Fact]
    public async Task Returns_paginated_courses_with_default_paging()
    {
        await using (var ctx = _fixture.CreateDbContext())
        {
            for (var i = 1; i <= 15; i++)
                ctx.Courses.Add(NewCourse(name: $"Course {i:D2}"));
            await ctx.SaveChangesAsync();
        }

        var result = await CreateHandler().Handle(new GetCoursesQuery(), default);

        result.Items.Should().HaveCount(10);
        result.TotalCount.Should().Be(15);
        result.PageNumber.Should().Be(1);
        result.TotalPages.Should().Be(2);
        result.HasNextPage.Should().BeTrue();
        result.HasPreviousPage.Should().BeFalse();
    }

    [Fact]
    public async Task Filters_by_search_in_course_name_case_insensitive()
    {
        await using (var ctx = _fixture.CreateDbContext())
        {
            ctx.Courses.AddRange(
                NewCourse(name: "Java Programming"),
                NewCourse(name: "Python Basics"),
                NewCourse(name: "Advanced JavaScript"));
            await ctx.SaveChangesAsync();
        }

        var result = await CreateHandler().Handle(new GetCoursesQuery(Search: "JAVA"), default);

        result.Items.Should().HaveCount(2);
        result.Items.Select(i => i.CourseName).Should().BeEquivalentTo(new[] { "Java Programming", "Advanced JavaScript" });
    }

    [Fact]
    public async Task Filters_by_is_active_when_specified()
    {
        await using (var ctx = _fixture.CreateDbContext())
        {
            ctx.Courses.AddRange(
                NewCourse(name: "Active 1", active: true),
                NewCourse(name: "Active 2", active: true),
                NewCourse(name: "Inactive 1", active: false));
            await ctx.SaveChangesAsync();
        }

        var result = await CreateHandler().Handle(new GetCoursesQuery(IsActive: false), default);

        result.Items.Should().HaveCount(1);
        result.Items.Single().CourseName.Should().Be("Inactive 1");
    }

    [Fact]
    public async Task Sorts_by_course_name_ascending_when_requested()
    {
        await using (var ctx = _fixture.CreateDbContext())
        {
            ctx.Courses.AddRange(
                NewCourse(name: "Charlie"),
                NewCourse(name: "Alpha"),
                NewCourse(name: "Bravo"));
            await ctx.SaveChangesAsync();
        }

        var result = await CreateHandler().Handle(new GetCoursesQuery(SortBy: "courseName", SortDirection: "asc"), default);

        result.Items.Select(i => i.CourseName).Should().ContainInOrder("Alpha", "Bravo", "Charlie");
    }

    [Fact]
    public async Task Sorts_by_base_fee_descending_when_requested()
    {
        await using (var ctx = _fixture.CreateDbContext())
        {
            ctx.Courses.AddRange(
                NewCourse(name: "Cheap", fee: 500m),
                NewCourse(name: "Premium", fee: 5000m),
                NewCourse(name: "Mid", fee: 2000m));
            await ctx.SaveChangesAsync();
        }

        var result = await CreateHandler().Handle(new GetCoursesQuery(SortBy: "baseFee", SortDirection: "desc"), default);

        result.Items.Select(i => i.CourseName).Should().ContainInOrder("Premium", "Mid", "Cheap");
    }

    [Fact]
    public async Task Defaults_to_created_at_descending_when_sort_unspecified()
    {
        await using (var ctx = _fixture.CreateDbContext())
        {
            for (var i = 1; i <= 3; i++)
            {
                ctx.Courses.Add(NewCourse(name: $"Course {i}"));
                await ctx.SaveChangesAsync();
                await Task.Delay(50);
            }
        }

        var result = await CreateHandler().Handle(new GetCoursesQuery(), default);

        result.Items.Select(i => i.CourseName).Should().ContainInOrder("Course 3", "Course 2", "Course 1");
    }

    [Fact]
    public async Task Clamps_page_below_1_to_first_page()
    {
        await using (var ctx = _fixture.CreateDbContext())
        {
            for (var i = 1; i <= 5; i++)
                ctx.Courses.Add(NewCourse(name: $"Course {i:D2}"));
            await ctx.SaveChangesAsync();
        }

        var result = await CreateHandler().Handle(new GetCoursesQuery(Page: -1), default);

        result.PageNumber.Should().Be(1);
        result.Items.Should().HaveCount(5);
    }

    [Fact]
    public async Task Clamps_page_size_above_100_to_100()
    {
        await using (var ctx = _fixture.CreateDbContext())
        {
            ctx.Courses.Add(NewCourse());
            await ctx.SaveChangesAsync();
        }

        var result = await CreateHandler().Handle(new GetCoursesQuery(PageSize: 500), default);

        result.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task Returns_second_page_correctly()
    {
        await using (var ctx = _fixture.CreateDbContext())
        {
            for (var i = 1; i <= 12; i++)
                ctx.Courses.Add(NewCourse(name: $"Course {i:D2}"));
            await ctx.SaveChangesAsync();
        }

        var result = await CreateHandler().Handle(new GetCoursesQuery(Page: 2, PageSize: 10), default);

        result.Items.Should().HaveCount(2);
        result.PageNumber.Should().Be(2);
        result.HasPreviousPage.Should().BeTrue();
        result.HasNextPage.Should().BeFalse();
    }

    [Fact]
    public async Task Maps_all_dto_fields_correctly()
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

        var result = await CreateHandler().Handle(new GetCoursesQuery(), default);

        var dto = result.Items.Single();
        dto.CourseId.Should().Be(courseId);
        dto.CourseName.Should().Be("Mapped Course");
        dto.Description.Should().Be("Detailed description");
        dto.DurationWeeks.Should().Be(16);
        dto.BaseFee.Should().Be(2500m);
        dto.IsActive.Should().BeTrue();
        dto.CreatedAt.Should().NotBe(default);
    }
}
