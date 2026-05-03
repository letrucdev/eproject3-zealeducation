using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Common.Helpers;
using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Application.UnitTests.Helpers;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.UnitTests.Common.Helpers;

public class CurrentFacultyExtensionsTests
{
    private readonly Mock<IRepository<Faculty>> _facultyRepo = new();
    private readonly Mock<IRepository<Batch>> _batchRepo = new();
    private readonly Mock<ICurrentUser> _currentUser = new();

    private static Faculty BuildFaculty(Guid facultyId, Guid userAccountId) => new()
    {
        Id = facultyId,
        StaffId = Guid.NewGuid(),
        FacultyCode = "F-001",
        Qualification = "PhD",
        Specialization = "AI",
        Staff = new Staff
        {
            Id = Guid.NewGuid(),
            UserAccountId = userAccountId,
            Position = "Lecturer",
            Department = "CS",
            JoinedDate = new DateOnly(2020, 1, 1)
        }
    };

    [Fact]
    public async Task ResolveFacultyIdAsync_throws_when_current_user_is_anonymous()
    {
        _currentUser.SetupGet(u => u.UserId).Returns((Guid?)null);

        var act = async () => await _facultyRepo.Object.ResolveFacultyIdAsync(_currentUser.Object);

        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Current user could not be resolved.");
    }

    [Fact]
    public async Task ResolveFacultyIdAsync_throws_when_user_is_not_faculty()
    {
        _currentUser.SetupGet(u => u.UserId).Returns(Guid.NewGuid());
        _facultyRepo.Setup(r => r.Query()).Returns(new TestAsyncEnumerable<Faculty>([]));

        var act = async () => await _facultyRepo.Object.ResolveFacultyIdAsync(_currentUser.Object);

        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Current user is not registered as faculty.");
    }

    [Fact]
    public async Task ResolveFacultyIdAsync_returns_faculty_id_when_user_is_faculty()
    {
        var userId = Guid.NewGuid();
        var facultyId = Guid.NewGuid();
        _currentUser.SetupGet(u => u.UserId).Returns(userId);
        _facultyRepo.Setup(r => r.Query())
            .Returns(new TestAsyncEnumerable<Faculty>([BuildFaculty(facultyId, userId)]));

        var result = await _facultyRepo.Object.ResolveFacultyIdAsync(_currentUser.Object);

        result.Should().Be(facultyId);
    }

    [Fact]
    public async Task EnsureBatchOwnedByFacultyAsync_throws_NotFound_when_batch_not_owned()
    {
        var batchId = Guid.NewGuid();
        var facultyId = Guid.NewGuid();
        _batchRepo.Setup(r => r.Query()).Returns(new TestAsyncEnumerable<Batch>([
            new Batch
            {
                Id = batchId,
                CourseId = Guid.NewGuid(),
                BatchCode = "B-001",
                FacultyId = Guid.NewGuid(), // different faculty
                StartDate = new DateOnly(2030, 1, 1),
                EndDate = new DateOnly(2030, 6, 1)
            }
        ]));

        var act = async () => await _batchRepo.Object.EnsureBatchOwnedByFacultyAsync(batchId, facultyId);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task EnsureBatchOwnedByFacultyAsync_passes_when_batch_owned_by_faculty()
    {
        var batchId = Guid.NewGuid();
        var facultyId = Guid.NewGuid();
        _batchRepo.Setup(r => r.Query()).Returns(new TestAsyncEnumerable<Batch>([
            new Batch
            {
                Id = batchId,
                CourseId = Guid.NewGuid(),
                BatchCode = "B-001",
                FacultyId = facultyId,
                StartDate = new DateOnly(2030, 1, 1),
                EndDate = new DateOnly(2030, 6, 1)
            }
        ]));

        var act = async () => await _batchRepo.Object.EnsureBatchOwnedByFacultyAsync(batchId, facultyId);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task EnsureBatchOwnedByFacultyAsync_throws_when_batch_does_not_exist()
    {
        var batchId = Guid.NewGuid();
        var facultyId = Guid.NewGuid();
        _batchRepo.Setup(r => r.Query()).Returns(new TestAsyncEnumerable<Batch>([]));

        var act = async () => await _batchRepo.Object.EnsureBatchOwnedByFacultyAsync(batchId, facultyId);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
