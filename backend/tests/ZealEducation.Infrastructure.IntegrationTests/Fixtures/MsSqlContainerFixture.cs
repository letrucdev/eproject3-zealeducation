using Microsoft.EntityFrameworkCore;
using Testcontainers.MsSql;
using ZealEducation.Infrastructure.Data;
using ZealEducation.Infrastructure.Data.Interceptors;

namespace ZealEducation.Infrastructure.IntegrationTests.Fixtures;

/// <summary>
/// Spins up an isolated SQL Server 2022 container for the test run, applies all
/// EF Core migrations once, and tears the container down at the end.
/// </summary>
public class MsSqlContainerFixture : IAsyncLifetime
{
    private readonly MsSqlContainer _container = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .WithPassword("Test_StrongPass!2026")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        await using var ctx = CreateDbContext();
        await ctx.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }

    public ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(ConnectionString,
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName))
            .AddInterceptors(new AuditableEntityInterceptor())
            .Options;
        return new ApplicationDbContext(options);
    }

    /// <summary>
    /// Wipes user-data tables so each test class starts on a clean slate.
    /// Uses dynamic SQL inside the caller's batch so SET QUOTED_IDENTIFIER ON
    /// applies to the DELETEs (some tables have filtered indexes / computed
    /// columns that require it). Migrations / __EFMigrationsHistory are left untouched.
    /// </summary>
    public async Task ResetAsync()
    {
        await using var ctx = CreateDbContext();
        await ctx.Database.ExecuteSqlRawAsync(ResetScript);
    }

    internal const string ResetScript = @"
        SET QUOTED_IDENTIFIER ON;
        SET ANSI_NULLS ON;

        DECLARE @sql NVARCHAR(MAX) = N'';

        SELECT @sql += 'ALTER TABLE ' + QUOTENAME(s.name) + '.' + QUOTENAME(t.name) + ' NOCHECK CONSTRAINT ALL;' + CHAR(10)
        FROM sys.tables t
        JOIN sys.schemas s ON s.schema_id = t.schema_id
        WHERE t.name <> '__EFMigrationsHistory';
        EXEC sp_executesql @sql;

        SET @sql = N'';
        SELECT @sql += 'DELETE FROM ' + QUOTENAME(s.name) + '.' + QUOTENAME(t.name) + ';' + CHAR(10)
        FROM sys.tables t
        JOIN sys.schemas s ON s.schema_id = t.schema_id
        WHERE t.name <> '__EFMigrationsHistory';
        EXEC sp_executesql @sql;

        SET @sql = N'';
        SELECT @sql += 'ALTER TABLE ' + QUOTENAME(s.name) + '.' + QUOTENAME(t.name) + ' WITH CHECK CHECK CONSTRAINT ALL;' + CHAR(10)
        FROM sys.tables t
        JOIN sys.schemas s ON s.schema_id = t.schema_id
        WHERE t.name <> '__EFMigrationsHistory';
        EXEC sp_executesql @sql;
    ";
}

[CollectionDefinition(Name)]
public class DatabaseCollection : ICollectionFixture<MsSqlContainerFixture>
{
    public const string Name = "MsSqlDatabase";
}
