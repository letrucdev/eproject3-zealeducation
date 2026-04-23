using FluentValidation;

namespace ZealEducation.Application.Features.CourseEnquiries.Commands.AddEnquiryNote;

public class AddEnquiryNoteCommandValidator : AbstractValidator<AddEnquiryNoteCommand>
{
    public AddEnquiryNoteCommandValidator()
    {
        RuleFor(x => x.EnquiryId).NotEmpty();

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Note content is required")
            .MaximumLength(2000);
    }
}
