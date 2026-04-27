using FluentValidation;
using ZealEducation.Domain.Enums;

namespace ZealEducation.Application.Features.CourseEnquiries.Commands.UpdateEnquiry;

public class UpdateEnquiryCommandValidator : AbstractValidator<UpdateEnquiryCommand>
{
    public UpdateEnquiryCommandValidator()
    {
        RuleFor(x => x.EnquiryId).NotEmpty();

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

        RuleFor(x => x.Source).IsInEnum();

        RuleFor(x => x.Status)
            .IsInEnum()
            .NotEqual(EnquiryStatus.Converted)
            .WithMessage("Use the convert endpoint to mark an enquiry as Converted.");
    }
}
