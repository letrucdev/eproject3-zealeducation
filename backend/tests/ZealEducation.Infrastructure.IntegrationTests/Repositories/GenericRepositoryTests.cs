using Microsoft.EntityFrameworkCore;
using ZealEducation.Domain.Entities;
using ZealEducation.Infrastructure.IntegrationTests.Fixtures;
using ZealEducation.Infrastructure.Repositories;

namespace ZealEducation.Infrastructure.IntegrationTests.Repositories;

[Collection(DatabaseCollection.Name)]
public class GenericRepositoryTests : IAsyncLifetime
{
    private readonly MsSqlContainerFixture _fixture;

    public GenericRepositoryTests(MsSqlContainerFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => _fixture.ResetAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    private static Course NewCourse(string name = "Test Course", int weeks = 8, bool active = true) => new()
    {
        Id = Guid.NewGuid(),
        CourseName = name,
        DurationWeeks = weeks,
        IsActive = active,
        BaseFee = 1000m
    };

    [Fact]
    public async Task AddAsync_then_SaveChanges_persists_entity()
    {
        await using var ctx = _fixture.CreateDbContext();
        var repo = new GenericRepository<Course>(ctx);

        var course = NewCourse(name: "Integration Test Course", weeks: 12);
        await repo.AddAsync(course);
        await ctx.SaveChangesAsync();

        await using var ctx2 = _fixture.CreateDbContext();
        var fromDb = await ctx2.Courses.FindAsync(course.Id);
        fromDb.Should().NotBeNull();
        fromDb!.CourseName.Should().Be("Integration Test Course");
        fromDb.DurationWeeks.Should().Be(12);
    }

    [Fact]
    public async Task GetByIdAsync_returns_null_when_entity_missing()
    {
        await using var ctx = _fixture.CreateDbContext();
        var repo = new GenericRepository<Course>(ctx);

        var result = await repo.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task FindAsync_filters_with_predicate()
    {
        await using var ctx = _fixture.CreateDbContext();
        ctx.Courses.AddRange(
            NewCourse(name: "Active Course", active: true),
            NewCourse(name: "Inactive Course", active: false));
        await ctx.SaveChangesAsync();

        var repo = new GenericRepository<Course>(ctx);
        var active = await repo.FindAsync(c => c.IsActive);

        active.Should().HaveCount(1);
        active[0].CourseName.Should().Be("Active Course");
    }

    [Fact]
    public async Task ExistsAsync_true_for_existing_id_false_otherwise()
    {
        await using var ctx = _fixture.CreateDbContext();
        var course = NewCourse();
        ctx.Courses.Add(course);
        await ctx.SaveChangesAsync();

        var repo = new GenericRepository<Course>(ctx);

        (await repo.ExistsAsync(course.Id)).Should().BeTrue();
        (await repo.ExistsAsync(Guid.NewGuid())).Should().BeFalse();
    }

    [Fact]
    public async Task Update_persists_changes()
    {
        await using var ctx = _fixture.CreateDbContext();
        var course = NewCourse(name: "Original");
        ctx.Courses.Add(course);
        await ctx.SaveChangesAsync();

        course.CourseName = "Renamed";
        var repo = new GenericRepository<Course>(ctx);
        repo.Update(course);
        await ctx.SaveChangesAsync();

        await using var ctx2 = _fixture.CreateDbContext();
        var loaded = await ctx2.Courses.FindAsync(course.Id);
        loaded!.CourseName.Should().Be("Renamed");
    }

    [Fact]
    public async Task Delete_removes_entity()
    {
        await using var ctx = _fixture.CreateDbContext();
        var course = NewCourse(name: "Bye");
        ctx.Courses.Add(course);
        await ctx.SaveChangesAsync();

        var repo = new GenericRepository<Course>(ctx);
        repo.Delete(course);
        await ctx.SaveChangesAsync();

        await using var ctx2 = _fixture.CreateDbContext();
        (await ctx2.Courses.FindAsync(course.Id)).Should().BeNull();
    }

    [Fact]
    public async Task Query_returns_no_tracking_queryable()
    {
        await using var ctx = _fixture.CreateDbContext();
        var course = NewCourse(name: "Tracker");
        ctx.Courses.Add(course);
        await ctx.SaveChangesAsync();

        var repo = new GenericRepository<Course>(ctx);
        var loaded = await repo.Query().FirstAsync(c => c.Id == course.Id);

        ctx.Entry(loaded).State.Should().Be(EntityState.Detached);
    }
}
