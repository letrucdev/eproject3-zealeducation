using FluentValidation;

namespace ZealEducation.Application.Features.CourseEnquiries.Commands.CreateEnquiry;

public class CreateEnquiryCommandValidator : AbstractValidator<CreateEnquiryCommand>
{
    public CreateEnquiryCommandValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required")
            .MaximumLength(100);

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone is required")
            .Length(10).WithMessage("Phone number must be 10 digits")
            .Matches(@"^\d+$").WithMessage("Only digits are allowed");

        RuleFor(x => x.Email)
            .EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email))
            .MaximumLength(100);

        RuleFor(x => x.CourseInterestedId)
            .NotEmpty().WithMessage("Course is required");

        RuleFor(x => x.Source)
            .IsInEnum().WithMessage("Source is invalid");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Status is invalid");

        RuleFor(x => x.NextFollowUpDate)
            .Must(d => d is null || d >= DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1)))
            .WithMessage("Next follow-up date is too far in the past");
    }
}
