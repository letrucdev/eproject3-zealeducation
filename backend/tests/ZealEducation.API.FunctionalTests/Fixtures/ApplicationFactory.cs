using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;
using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Infrastructure.Data;

namespace ZealEducation.API.FunctionalTests.Fixtures;

/// <summary>
/// Spins up a SQL Server 2022 container for the test session and a TestServer
/// hosting the real API pointing at it. External I/O (file storage) is replaced
/// with no-op fakes so tests don't hit Cloudflare R2 / SMTP.
/// </summary>
public class ApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MsSqlContainer _mssql = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .WithPassword("Test_StrongPass!2026")
        .Build();

    private string ConnectionString => _mssql.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _mssql.StartAsync();

        // Apply migrations once before the host starts servicing requests.
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(ConnectionString,
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName))
            .Options;
        await using var ctx = new ApplicationDbContext(options);
        await ctx.Database.MigrateAsync();
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
        await _mssql.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");
        builder.ConfigureAppConfiguration((ctx, config) =>
        {
            var dir = Path.GetDirectoryName(typeof(ApplicationFactory).Assembly.Location)!;
            config.AddJsonFile(Path.Combine(dir, "appsettings.Test.json"), optional: false);
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = ConnectionString
            });
        });

        builder.ConfigureServices(services =>
        {
            // Replace Cloudflare R2 with a fake that records uploads but doesn't talk to the network.
            var storageDescriptor = services.Single(d => d.ServiceType == typeof(IFileStorageService));
            services.Remove(storageDescriptor);
            services.AddSingleton<IFileStorageService, FakeFileStorageService>();
        });
    }

    public async Task ResetDatabaseAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(ConnectionString)
            .Options;
        await using var ctx = new ApplicationDbContext(options);
        await ctx.Database.ExecuteSqlRawAsync(ResetScript);
    }

    private const string ResetScript = @"
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

    public ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(ConnectionString)
            .Options;
        return new ApplicationDbContext(options);
    }
}

[CollectionDefinition(Name)]
public class FunctionalCollection : ICollectionFixture<ApplicationFactory>
{
    public const string Name = "FunctionalApi";
}

public class FakeFileStorageService : IFileStorageService
{
    public Task<string> UploadAsync(byte[] content, string objectKey, string contentType, CancellationToken cancellationToken)
        => Task.FromResult($"fake://{objectKey}");

    public Task<byte[]> DownloadAsync(string objectKey, CancellationToken cancellationToken)
        => Task.FromResult(Array.Empty<byte>());

    public Task DeleteAsync(string objectKey, CancellationToken cancellationToken)
        => Task.CompletedTask;
}
