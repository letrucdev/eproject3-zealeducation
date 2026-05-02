using System.Linq.Expressions;
using Moq;
using ZealEducation.Domain.Common;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.UnitTests.Helpers;

internal static class MockRepositoryExtensions
{
    public static Mock<IRepository<T>> SetupExists<T>(this Mock<IRepository<T>> mock, Guid id, bool exists) where T : BaseEntity
    {
        mock.Setup(r => r.ExistsAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(exists);
        return mock;
    }

    public static Mock<IRepository<T>> SetupGetById<T>(this Mock<IRepository<T>> mock, Guid id, T? entity) where T : BaseEntity
    {
        mock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        return mock;
    }

    public static Mock<IRepository<T>> SetupFind<T>(this Mock<IRepository<T>> mock, IEnumerable<T> result) where T : BaseEntity
    {
        mock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<T, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result.ToList());
        return mock;
    }

    public static Mock<IRepository<T>> SetupAdd<T>(this Mock<IRepository<T>> mock) where T : BaseEntity
    {
        mock.Setup(r => r.AddAsync(It.IsAny<T>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((T e, CancellationToken _) => e);
        return mock;
    }

    public static Mock<IUnitOfWork> CreateUnitOfWork()
    {
        var mock = new Mock<IUnitOfWork>();
        mock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        return mock;
    }
}
