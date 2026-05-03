using Microsoft.EntityFrameworkCore;
using ZealEducation.Domain.Entities;
using ZealEducation.Infrastructure.Data;
using ZealEducation.Infrastructure.Data.Interceptors;
using ZealEducation.Infrastructure.IntegrationTests.Fixtures;

namespace ZealEducation.Infrastructure.IntegrationTests.Interceptors;

[Collection(DatabaseCollection.Name)]
public class AuditableEntityInterceptorTests(MsSqlContainerFixture fixture) : IAsyncLifetime
{
    private readonly MsSqlContainerFixture _fixture = fixture;

    public Task InitializeAsync() => _fixture.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private ApplicationDbContext CreateContextWithInterceptor()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(_fixture.ConnectionString,
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName))
            .AddInterceptors(new AuditableEntityInterceptor())
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task Sets_CreatedAt_and_UpdatedAt_on_insert()
    {
        await using var ctx = CreateContextWithInterceptor();
        var course = new Course
        {
            Id = Guid.NewGuid(),
            CourseName = "Audit-Insert",
            DurationWeeks = 6,
            BaseFee = 500,
            IsActive = true
        };

        var before = DateTime.UtcNow.AddSeconds(-1);
        ctx.Courses.Add(course);
        await ctx.SaveChangesAsync();
        var after = DateTime.UtcNow.AddSeconds(1);

        course.CreatedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
        course.UpdatedAt.Should().NotBeNull().And.Subject.Should().BeOnOrAfter(before);
    }

    [Fact]
    public async Task Updates_UpdatedAt_but_keeps_CreatedAt_on_update()
    {
        await using var ctx = CreateContextWithInterceptor();
        var course = new Course
        {
            Id = Guid.NewGuid(),
            CourseName = "Audit-Update",
            DurationWeeks = 6,
            BaseFee = 500,
            IsActive = true
        };
        ctx.Courses.Add(course);
        await ctx.SaveChangesAsync();
        var originalCreated = course.CreatedAt;

        await Task.Delay(50);

        await using var ctx2 = CreateContextWithInterceptor();
        var loaded = await ctx2.Courses.FindAsync(course.Id);
        loaded!.CourseName = "Renamed";
        await ctx2.SaveChangesAsync();

        loaded.CreatedAt.Should().Be(originalCreated);
        loaded.UpdatedAt.Should().NotBeNull();
        loaded.UpdatedAt!.Value.Should().BeOnOrAfter(originalCreated);
    }
}
