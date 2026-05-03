using Coravel.Mailer.Mail;
using ZealEducation.Application.Features.Batches.Notifications;

namespace ZealEducation.Infrastructure.Services.Email.Mailables;

public class BatchFacultyAssignedMailable(BatchFacultyAssignedEmailModel model)
    : Mailable<BatchFacultyAssignedEmailModel>
{
    private readonly BatchFacultyAssignedEmailModel _model = model;

    public override void Build()
    {
        To(new MailRecipient(_model.RecipientEmail, _model.RecipientName))
            .Subject($"You have been assigned to batch {_model.BatchCode}")
            .View("~/Views/Mail/BatchFacultyAssigned.cshtml", _model);
    }
}
