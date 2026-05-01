using Coravel.Mailer.Mail;
using ZealEducation.Application.Features.CourseEnquiries.Notifications;

namespace ZealEducation.Infrastructure.Services.Email.Mailables;

public class EnquiryConvertedMailable(EnquiryConvertedEmailModel model) : Mailable<EnquiryConvertedEmailModel>
{
    private readonly EnquiryConvertedEmailModel _model = model;

    public override void Build()
    {
        To(new MailRecipient(_model.RecipientEmail, _model.RecipientName))
            .Subject($"Enrollment confirmation - {_model.CourseName}")
            .View("~/Views/Mail/EnquiryConverted.cshtml", _model);
    }
}
