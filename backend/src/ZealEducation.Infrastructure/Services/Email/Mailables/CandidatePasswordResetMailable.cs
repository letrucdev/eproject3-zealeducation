using Coravel.Mailer.Mail;
using ZealEducation.Application.Features.Candidates.Notifications;

namespace ZealEducation.Infrastructure.Services.Email.Mailables;

public class CandidatePasswordResetMailable(CandidatePasswordResetEmailModel model) : Mailable<CandidatePasswordResetEmailModel>
{
    private readonly CandidatePasswordResetEmailModel _model = model;

    public override void Build()
    {
        To(new MailRecipient(_model.RecipientEmail, _model.RecipientName))
            .Subject("Your Zeal Education password has been reset")
            .View("~/Views/Mail/CandidatePasswordReset.cshtml", _model);
    }
}
