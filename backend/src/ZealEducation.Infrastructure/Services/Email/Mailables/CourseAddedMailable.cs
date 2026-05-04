using Coravel.Mailer.Mail;
using ZealEducation.Application.Features.Candidates.Notifications;

namespace ZealEducation.Infrastructure.Services.Email.Mailables;

public class CourseAddedMailable(CourseAddedEmailModel model) : Mailable<CourseAddedEmailModel>
{
    private readonly CourseAddedEmailModel _model = model;

    public override void Build()
    {
        To(new MailRecipient(_model.RecipientEmail, _model.RecipientName))
            .Subject($"New course enrollment - {_model.CourseName}")
            .View("~/Views/Mail/CourseAdded.cshtml", _model);
    }
}
