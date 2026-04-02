using Microsoft.EntityFrameworkCore;
using ZealEducation.Domain.Entities;

namespace ZealEducation.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Course> Courses { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
