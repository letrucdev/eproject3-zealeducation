using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;

namespace ZealEducation.Infrastructure.Data;

public class ApplicationDbContextInitialiser(
    ILogger<ApplicationDbContextInitialiser> logger,
    ApplicationDbContext context,
    IPasswordHasher passwordHasher,
    IConfiguration configuration)
{
    public async Task InitialiseAsync()
    {
        try
        {
            if (context.Database.IsRelational())
            {
                await context.Database.MigrateAsync();
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while migrating the database.");
            throw;
        }
    }

    public async Task SeedAsync()
    {
        try
        {
            await SeedDefaultAdminAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    private async Task SeedDefaultAdminAsync()
    {
        var section = configuration.GetSection("DefaultAdmin");
        var username = section["Username"] ?? "admin";
        var password = section["Password"] ?? "Admin@123";
        var email = section["Email"] ?? "admin@zealeducation.local";
        var fullName = section["FullName"] ?? "System Administrator";
        var phone = section["Phone"] ?? "0000000000";
        var resetPassword = section.GetValue<bool>("ResetPasswordOnStartup");

        var existingAdmin = await context.UserAccounts
            .FirstOrDefaultAsync(u => u.Role == UserRole.SystemAdmin);

        if (existingAdmin is not null)
        {
            if (resetPassword)
            {
                existingAdmin.PasswordHash = passwordHasher.Hash(password);
                existingAdmin.FailedLoginCount = 0;
                existingAdmin.IsActive = true;
                await context.SaveChangesAsync();
                logger.LogWarning("Default admin password was reset for '{Username}' (ResetPasswordOnStartup=true).", existingAdmin.Username);
            }
            return;
        }

        if (await context.UserAccounts.AnyAsync(u => u.Username == username || u.Email == email || u.Phone == phone))
        {
            logger.LogWarning("Default admin could not be seeded: username/email/phone already used by another account.");
            return;
        }

        var admin = new UserAccount
        {
            Id = Guid.NewGuid(),
            Username = username,
            PasswordHash = passwordHasher.Hash(password),
            FullName = fullName,
            Email = email,
            Phone = phone,
            Dob = new DateOnly(1990, 1, 1),
            Gender = Gender.Other,
            Role = UserRole.SystemAdmin,
            IsActive = true,
            FailedLoginCount = 0,
            CreatedBy = "system"
        };

        await context.UserAccounts.AddAsync(admin);
        await context.SaveChangesAsync();

        logger.LogInformation("Seeded default admin account '{Username}'.", username);
    }
}

public static class InitialiserExtensions
{
    public static async Task InitialiseDatabaseAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var initialiser = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitialiser>();
        await initialiser.InitialiseAsync();
        await initialiser.SeedAsync();
    }
}
