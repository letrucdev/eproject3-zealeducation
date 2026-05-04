using Coravel.Mailer.Mail;
using ZealEducation.Application.Features.Batches.Notifications;

namespace ZealEducation.Infrastructure.Services.Email.Mailables;

public class BatchCandidateEnrolledMailable(BatchCandidateEnrolledEmailModel model)
    : Mailable<BatchCandidateEnrolledEmailModel>
{
    private readonly BatchCandidateEnrolledEmailModel _model = model;

    public override void Build()
    {
        To(new MailRecipient(_model.RecipientEmail, _model.RecipientName))
            .Subject($"You have been enrolled in batch {_model.BatchCode} - {_model.CourseName}")
            .View("~/Views/Mail/BatchCandidateEnrolled.cshtml", _model);
    }
}
