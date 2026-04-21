using FluentValidation;
using ZealEducation.Domain.Enums;

namespace ZealEducation.Application.Features.Staffs.Commands.UpdateStaff;

public class UpdateStaffCommandValidator : AbstractValidator<UpdateStaffCommand>
{
    private static readonly UserRole[] AllowedRoles =
    [
        UserRole.Incharge,
        UserRole.Counselor,
        UserRole.AccountsStaff
    ];

    public UpdateStaffCommandValidator()
    {
        RuleFor(x => x.StaffId)
            .NotEmpty().WithMessage("Staff id is required");

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

        RuleFor(x => x.Role)
            .IsInEnum().WithMessage("Role is invalid")
            .Must(r => AllowedRoles.Contains(r))
            .WithMessage($"Role must be one of: {string.Join(", ", AllowedRoles)}.");

        RuleFor(x => x.Position)
            .NotEmpty().WithMessage("Position is required")
            .MaximumLength(60);

        RuleFor(x => x.Department)
            .NotEmpty().WithMessage("Department is required")
            .MaximumLength(60);

        RuleFor(x => x.JoinedDate)
            .Must(d => d <= DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Joined date cannot be in the future");
    }
}
