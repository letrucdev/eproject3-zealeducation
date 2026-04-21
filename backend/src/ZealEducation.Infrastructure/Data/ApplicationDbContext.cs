using System.Reflection;
using Microsoft.EntityFrameworkCore;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Infrastructure.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options), IUnitOfWork
{
    public DbSet<UserAccount> UserAccounts => Set<UserAccount>();
    public DbSet<Staff> Staff => Set<Staff>();
    public DbSet<Faculty> Faculties => Set<Faculty>();
    public DbSet<Candidate> Candidates => Set<Candidate>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}
