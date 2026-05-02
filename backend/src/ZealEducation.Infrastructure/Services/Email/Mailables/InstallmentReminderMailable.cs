using System.Globalization;
using Coravel.Mailer.Mail;
using ZealEducation.Application.Features.Candidates.Notifications;
using ZealEducation.Domain.Enums;

namespace ZealEducation.Infrastructure.Services.Email.Mailables;

public class InstallmentReminderMailable(InstallmentReminderEmailModel model) : Mailable<InstallmentReminderEmailModel>
{
    private readonly InstallmentReminderEmailModel _model = model;

    public override void Build()
    {
        var subject = _model.ReminderType == ReminderType.BeforeDue
            ? string.Format(CultureInfo.InvariantCulture,
                "Tuition payment reminder - due in {0} days", -_model.DaysOffset)
            : string.Format(CultureInfo.InvariantCulture,
                "Tuition payment overdue - {0} days past due", _model.DaysOffset);

        To(new MailRecipient(_model.RecipientEmail, _model.RecipientName))
            .Subject(subject)
            .View("~/Views/Mail/InstallmentReminder.cshtml", _model);
    }
}
