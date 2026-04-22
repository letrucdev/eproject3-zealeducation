using FluentValidation;

namespace ZealEducation.Application.Features.CourseEnquiries.Commands.ConvertEnquiry;

public class ConvertEnquiryCommandValidator : AbstractValidator<ConvertEnquiryCommand>
{
    public ConvertEnquiryCommandValidator()
    {
        RuleFor(x => x.EnquiryId).NotEmpty();

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required to send login information")
            .EmailAddress()
            .MaximumLength(100);

        RuleFor(x => x.Dob)
            .NotEmpty().WithMessage("Date of birth is required")
            .Must(dob => dob < DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Date of birth must be in the past");

        RuleFor(x => x.Gender).IsInEnum();

        RuleFor(x => x.Address).MaximumLength(500);
        RuleFor(x => x.EmergencyContact).MaximumLength(100);
    }
}
