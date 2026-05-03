using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Features.Courses.Commands.UpdateCourse;
using ZealEducation.Application.UnitTests.Helpers;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.UnitTests.Handlers.Courses;

public class UpdateCourseCommandHandlerTests
{
    private readonly Mock<IRepository<Course>> _courseRepo = new();
    private readonly Mock<IUnitOfWork> _uow = MockRepositoryExtensions.CreateUnitOfWork();

    private UpdateCourseCommandHandler CreateHandler() =>
        new(_courseRepo.Object, _uow.Object);

    private static UpdateCourseCommand Cmd(
        Guid? courseId = null,
        string courseName = "Intro to Math",
        string? description = "A short description",
        int durationWeeks = 12,
        decimal baseFee = 100m,
        bool isActive = true) => new(
            courseId ?? Guid.NewGuid(),
            courseName,
            description,
            durationWeeks,
            baseFee,
            isActive);

    private static Course ExistingCourse(
        Guid id,
        string name = "Intro to Math",
        string? description = "Old description",
        int durationWeeks = 10,
        decimal baseFee = 50m,
        bool isActive = true) => new()
        {
            Id = id,
            CourseName = name,
            Description = description,
            DurationWeeks = durationWeeks,
            BaseFee = baseFee,
            IsActive = isActive
        };

    [Fact]
    public async Task Throws_NotFoundException_when_course_does_not_exist()
    {
        var courseId = Guid.NewGuid();
        _courseRepo.SetupGetById(courseId, null);

        var act = async () => await CreateHandler().Handle(Cmd(courseId), default);

        await act.Should().ThrowAsync<NotFoundException>();
        _courseRepo.Verify(r => r.Update(It.IsAny<Course>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Throws_ConflictException_when_renaming_to_existing_course_name()
    {
        var courseId = Guid.NewGuid();
        var course = ExistingCourse(courseId, name: "Old Name");
        _courseRepo.SetupGetById(courseId, course);
        _courseRepo.SetupFind(new[] { new Course { Id = Guid.NewGuid(), CourseName = "New Name" } });

        var act = async () => await CreateHandler().Handle(
            Cmd(courseId, courseName: "New Name"),
            default);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("Course name is already in use.");
        _courseRepo.Verify(r => r.Update(It.IsAny<Course>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Does_not_check_duplicate_when_course_name_is_unchanged()
    {
        var courseId = Guid.NewGuid();
        var course = ExistingCourse(courseId, name: "Same Name");
        _courseRepo.SetupGetById(courseId, course);

        await CreateHandler().Handle(Cmd(courseId, courseName: "Same Name"), default);

        _courseRepo.Verify(r => r.FindAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Course, bool>>>(),
            It.IsAny<CancellationToken>()), Times.Never);
        _courseRepo.Verify(r => r.Update(course), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Updates_course_when_name_is_changed_and_unique()
    {
        var courseId = Guid.NewGuid();
        var course = ExistingCourse(courseId, name: "Old Name");
        _courseRepo.SetupGetById(courseId, course);
        _courseRepo.SetupFind(Array.Empty<Course>());

        await CreateHandler().Handle(
            Cmd(courseId, courseName: "Brand New Name", description: "New desc",
                durationWeeks: 20, baseFee: 500m, isActive: false),
            default);

        course.CourseName.Should().Be("Brand New Name");
        course.Description.Should().Be("New desc");
        course.DurationWeeks.Should().Be(20);
        course.BaseFee.Should().Be(500m);
        course.IsActive.Should().BeFalse();
        _courseRepo.Verify(r => r.Update(course), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Trims_course_name_and_description_before_saving()
    {
        var courseId = Guid.NewGuid();
        var course = ExistingCourse(courseId, name: "Old Name");
        _courseRepo.SetupGetById(courseId, course);
        _courseRepo.SetupFind(Array.Empty<Course>());

        await CreateHandler().Handle(
            Cmd(courseId, courseName: "  Trimmed  ", description: "  Some desc  "),
            default);

        course.CourseName.Should().Be("Trimmed");
        course.Description.Should().Be("Some desc");
    }

    [Fact]
    public async Task Sets_description_to_null_when_input_is_null()
    {
        var courseId = Guid.NewGuid();
        var course = ExistingCourse(courseId, name: "Same Name", description: "Existing");
        _courseRepo.SetupGetById(courseId, course);

        await CreateHandler().Handle(
            Cmd(courseId, courseName: "Same Name", description: null),
            default);

        course.Description.Should().BeNull();
    }

    [Fact]
    public async Task Sets_description_to_null_when_input_is_whitespace()
    {
        var courseId = Guid.NewGuid();
        var course = ExistingCourse(courseId, name: "Same Name", description: "Existing");
        _courseRepo.SetupGetById(courseId, course);

        await CreateHandler().Handle(
            Cmd(courseId, courseName: "Same Name", description: "   "),
            default);

        course.Description.Should().BeNull();
    }
}
