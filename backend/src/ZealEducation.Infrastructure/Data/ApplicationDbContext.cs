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
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<CourseEnquiry> CourseEnquiries => Set<CourseEnquiry>();
    public DbSet<EnquiryNote> EnquiryNotes => Set<EnquiryNote>();
    public DbSet<Batch> Batches => Set<Batch>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<FeeStructure> FeeStructures => Set<FeeStructure>();
    public DbSet<InstallmentPlan> InstallmentPlans => Set<InstallmentPlan>();
    public DbSet<PaymentTransaction> PaymentTransactions => Set<PaymentTransaction>();
    public DbSet<Fine> Fines => Set<Fine>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}
