using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Features.Courses.Commands.CreateCourse;
using ZealEducation.Application.UnitTests.Helpers;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.UnitTests.Handlers.Courses;

public class CreateCourseCommandHandlerTests
{
    private readonly Mock<IRepository<Course>> _courseRepo = new();
    private readonly Mock<IUnitOfWork> _uow = MockRepositoryExtensions.CreateUnitOfWork();

    private CreateCourseCommandHandler CreateHandler() =>
        new(_courseRepo.Object, _uow.Object);

    private static CreateCourseCommand Cmd(
        string courseName = "Intro to Math",
        string? description = "A short description",
        int durationWeeks = 12,
        decimal baseFee = 100m,
        bool isActive = true) => new(
            courseName,
            description,
            durationWeeks,
            baseFee,
            isActive);

    [Fact]
    public async Task Throws_ConflictException_when_course_name_is_already_in_use()
    {
        _courseRepo.SetupFind(new[] { new Course { Id = Guid.NewGuid(), CourseName = "Intro to Math" } });

        var act = async () => await CreateHandler().Handle(Cmd(), default);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("Course name is already in use.");
        _courseRepo.Verify(r => r.AddAsync(It.IsAny<Course>(), It.IsAny<CancellationToken>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Creates_course_and_returns_response_when_name_is_unique()
    {
        _courseRepo.SetupFind(Array.Empty<Course>());
        _courseRepo.SetupAdd();

        var response = await CreateHandler().Handle(
            Cmd(courseName: "Algebra 101", durationWeeks: 8, baseFee: 250m, isActive: true),
            default);

        response.CourseId.Should().NotBe(Guid.Empty);
        response.CourseName.Should().Be("Algebra 101");
        response.DurationWeeks.Should().Be(8);
        response.BaseFee.Should().Be(250m);
        response.IsActive.Should().BeTrue();
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Trims_course_name_and_description_before_saving()
    {
        _courseRepo.SetupFind(Array.Empty<Course>());

        Course? captured = null;
        _courseRepo.Setup(r => r.AddAsync(It.IsAny<Course>(), It.IsAny<CancellationToken>()))
            .Callback<Course, CancellationToken>((c, _) => captured = c)
            .ReturnsAsync((Course c, CancellationToken _) => c);

        await CreateHandler().Handle(
            Cmd(courseName: "  Geometry  ", description: "  Some text  "),
            default);

        captured.Should().NotBeNull();
        captured!.CourseName.Should().Be("Geometry");
        captured.Description.Should().Be("Some text");
    }

    [Fact]
    public async Task Sets_description_to_null_when_input_is_null()
    {
        _courseRepo.SetupFind(Array.Empty<Course>());

        Course? captured = null;
        _courseRepo.Setup(r => r.AddAsync(It.IsAny<Course>(), It.IsAny<CancellationToken>()))
            .Callback<Course, CancellationToken>((c, _) => captured = c)
            .ReturnsAsync((Course c, CancellationToken _) => c);

        await CreateHandler().Handle(Cmd(description: null), default);

        captured!.Description.Should().BeNull();
    }

    [Fact]
    public async Task Sets_description_to_null_when_input_is_whitespace()
    {
        _courseRepo.SetupFind(Array.Empty<Course>());

        Course? captured = null;
        _courseRepo.Setup(r => r.AddAsync(It.IsAny<Course>(), It.IsAny<CancellationToken>()))
            .Callback<Course, CancellationToken>((c, _) => captured = c)
            .ReturnsAsync((Course c, CancellationToken _) => c);

        await CreateHandler().Handle(Cmd(description: "   "), default);

        captured!.Description.Should().BeNull();
    }

    [Fact]
    public async Task Persists_all_command_fields_to_entity()
    {
        _courseRepo.SetupFind(Array.Empty<Course>());

        Course? captured = null;
        _courseRepo.Setup(r => r.AddAsync(It.IsAny<Course>(), It.IsAny<CancellationToken>()))
            .Callback<Course, CancellationToken>((c, _) => captured = c)
            .ReturnsAsync((Course c, CancellationToken _) => c);

        await CreateHandler().Handle(
            Cmd(courseName: "Physics", description: "Basics", durationWeeks: 16, baseFee: 999.99m, isActive: false),
            default);

        captured!.Id.Should().NotBe(Guid.Empty);
        captured.CourseName.Should().Be("Physics");
        captured.Description.Should().Be("Basics");
        captured.DurationWeeks.Should().Be(16);
        captured.BaseFee.Should().Be(999.99m);
        captured.IsActive.Should().BeFalse();
    }
}
