using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;
using ZealEducation.Infrastructure.Data;
using ZealEducation.Infrastructure.Services;

namespace ZealEducation.API.FunctionalTests.Helpers;

/// <summary>
/// Helpers for seeding the test database with the minimum data needed to
/// drive functional tests through real handlers.
/// </summary>
internal static class TestDataSeeder
{
    private static readonly BCryptPasswordHasher Hasher = new();

    public static async Task<UserAccount> SeedUserAsync(
        ApplicationDbContext ctx,
        string username,
        string password,
        UserRole role = UserRole.SystemAdmin,
        bool isActive = true,
        bool mustChangePassword = false)
    {
        var user = new UserAccount
        {
            Id = Guid.NewGuid(),
            Username = username,
            PasswordHash = Hasher.Hash(password),
            FullName = $"Test {username}",
            Email = $"{username}@example.com",
            Phone = "0900000000",
            Dob = new DateOnly(1990, 1, 1),
            Gender = Gender.Other,
            Role = role,
            IsActive = isActive,
            MustChangePassword = mustChangePassword
        };

        ctx.UserAccounts.Add(user);
        await ctx.SaveChangesAsync();
        return user;
    }

    public static async Task<Course> SeedCourseAsync(ApplicationDbContext ctx, string name = "Functional Course")
    {
        var course = new Course
        {
            Id = Guid.NewGuid(),
            CourseName = name,
            DurationWeeks = 12,
            IsActive = true,
            BaseFee = 1000m
        };
        ctx.Courses.Add(course);
        await ctx.SaveChangesAsync();
        return course;
    }
}
