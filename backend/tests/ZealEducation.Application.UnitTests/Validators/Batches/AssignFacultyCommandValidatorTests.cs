using FluentValidation.TestHelper;
using ZealEducation.Application.Features.Batches.Commands.AssignFaculty;

namespace ZealEducation.Application.UnitTests.Validators.Batches;

public class AssignFacultyCommandValidatorTests
{
    private readonly AssignFacultyCommandValidator _validator = new();

    [Fact]
    public void Should_pass_with_faculty_id_present()
    {
        var cmd = new AssignFacultyCommand(Guid.NewGuid(), Guid.NewGuid());
        _validator.TestValidate(cmd).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_pass_with_faculty_id_null()
    {
        var cmd = new AssignFacultyCommand(Guid.NewGuid(), null);
        _validator.TestValidate(cmd).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_fail_when_batch_id_is_empty()
    {
        var cmd = new AssignFacultyCommand(Guid.Empty, Guid.NewGuid());
        _validator.TestValidate(cmd).ShouldHaveValidationErrorFor(c => c.BatchId);
    }
}
