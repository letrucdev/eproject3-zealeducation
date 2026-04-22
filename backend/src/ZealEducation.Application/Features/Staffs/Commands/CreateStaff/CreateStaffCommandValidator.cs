using FluentValidation;
using ZealEducation.Domain.Enums;

namespace ZealEducation.Application.Features.Staffs.Commands.CreateStaff;

public class CreateStaffCommandValidator : AbstractValidator<CreateStaffCommand>
{
    private static readonly UserRole[] AllowedRoles =
    [
        UserRole.Incharge,
        UserRole.Counselor,
        UserRole.AccountsStaff
    ];

    public CreateStaffCommandValidator()
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
            .Length(10).WithMessage("Phone number must be 10 digits")
            .Matches(@"^\d+$").WithMessage("Only digits are allowed");

        RuleFor(x => x.Dob)
            .NotEmpty().WithMessage("Date of birth is required")
            .Must(dob => dob < DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Date of birth must be in the past");

        RuleFor(x => x.Gender)
            .IsInEnum().WithMessage("Gender is invalid");

        RuleFor(x => x.Role)
            .IsInEnum().WithMessage("Role is invalid")
            .Must(r => AllowedRoles.Contains(r))
            .WithMessage($"Role must be one of: {string.Join(", ", AllowedRoles)}. Use the dedicated endpoints for Faculty or Candidate, and SystemAdmin cannot be assigned.");

        RuleFor(x => x.Position)
            .NotEmpty().WithMessage("Position is required")
            .MaximumLength(60);

        RuleFor(x => x.Department)
            .NotEmpty().WithMessage("Department is required")
            .MaximumLength(60);

        RuleFor(x => x.JoinedDate)
            .Must(d => d is null || d <= DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Joined date cannot be in the future");
    }
}
