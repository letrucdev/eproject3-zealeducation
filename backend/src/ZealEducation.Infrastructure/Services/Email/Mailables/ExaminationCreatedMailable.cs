using Coravel.Mailer.Mail;
using ZealEducation.Application.Features.Examinations.Notifications;

namespace ZealEducation.Infrastructure.Services.Email.Mailables;

public class ExaminationCreatedMailable(ExaminationCreatedEmailModel model)
    : Mailable<ExaminationCreatedEmailModel>
{
    private readonly ExaminationCreatedEmailModel _model = model;

    public override void Build()
    {
        To(new MailRecipient(_model.RecipientEmail, _model.RecipientName))
            .Subject($"New exam scheduled: {_model.ExamName} - {_model.BatchCode}")
            .View("~/Views/Mail/ExaminationCreated.cshtml", _model);
    }
}
