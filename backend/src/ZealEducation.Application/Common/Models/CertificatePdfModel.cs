namespace ZealEducation.Application.Common.Models;

public class CertificatePdfModel
{
    public string CertificateNumber { get; set; } = default!;
    public string CandidateName { get; set; } = default!;
    public string CourseName { get; set; } = default!;
    public DateTime IssuedDate { get; set; }
}
