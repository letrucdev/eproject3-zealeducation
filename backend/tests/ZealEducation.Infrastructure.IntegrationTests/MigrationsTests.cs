using Microsoft.EntityFrameworkCore;
using ZealEducation.Infrastructure.IntegrationTests.Fixtures;

namespace ZealEducation.Infrastructure.IntegrationTests;

[Collection(DatabaseCollection.Name)]
public class MigrationsTests(MsSqlContainerFixture fixture)
{
    private readonly MsSqlContainerFixture _fixture = fixture;

    [Fact]
    public async Task All_migrations_have_been_applied_successfully()
    {
        await using var ctx = _fixture.CreateDbContext();
        var pending = await ctx.Database.GetPendingMigrationsAsync();
        pending.Should().BeEmpty(because: "the fixture runs MigrateAsync at startup");
    }

    [Fact]
    public async Task Database_can_create_a_new_connection_and_query_a_table()
    {
        await using var ctx = _fixture.CreateDbContext();
        // No exceptions = the schema for at least the Courses table exists
        await ctx.Courses.CountAsync();
    }
}
