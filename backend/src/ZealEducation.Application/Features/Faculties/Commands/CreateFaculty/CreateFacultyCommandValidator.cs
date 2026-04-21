using FluentValidation;

namespace ZealEducation.Application.Features.Faculties.Commands.CreateFaculty;

public class CreateFacultyCommandValidator : AbstractValidator<CreateFacultyCommand>
{
    public CreateFacultyCommandValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required")
            .MinimumLength(3)
            .MaximumLength(50)
            .Matches("^[a-zA-Z0-9_.]+$").WithMessage("Username can only contain letters, digits, dot and underscore");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters")
            .MaximumLength(100);

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required")
            .MaximumLength(100);

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress()
            .MaximumLength(100);

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone is required")
            .MaximumLength(20)
            .Matches(@"^\+?[0-9\-\s]{6,20}$").WithMessage("Phone number is invalid");

        RuleFor(x => x.Dob)
            .NotEmpty().WithMessage("Date of birth is required")
            .Must(dob => dob < DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Date of birth must be in the past");

        RuleFor(x => x.Gender)
            .IsInEnum().WithMessage("Gender is invalid");

        RuleFor(x => x.Position)
            .NotEmpty().WithMessage("Position is required")
            .MaximumLength(60);

        RuleFor(x => x.Department)
            .NotEmpty().WithMessage("Department is required")
            .MaximumLength(60);

        RuleFor(x => x.JoinedDate)
            .Must(d => d is null || d <= DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Joined date cannot be in the future");

        RuleFor(x => x.FacultyCode)
            .NotEmpty().WithMessage("Faculty code is required")
            .MaximumLength(20);

        RuleFor(x => x.Qualification)
            .NotEmpty().WithMessage("Qualification is required")
            .MaximumLength(100);

        RuleFor(x => x.Specialization)
            .NotEmpty().WithMessage("Specialization is required")
            .MaximumLength(100);

        RuleFor(x => x.ExperienceYears)
            .GreaterThanOrEqualTo(0).WithMessage("Experience years cannot be negative")
            .LessThanOrEqualTo(80).WithMessage("Experience years is unrealistic");
    }
}
