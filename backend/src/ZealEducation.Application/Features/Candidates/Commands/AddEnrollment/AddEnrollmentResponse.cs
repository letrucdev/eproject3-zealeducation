namespace ZealEducation.Application.Features.Candidates.Commands.AddEnrollment;

public class AddEnrollmentResponse
{
    public Guid EnrollmentId { get; init; }
    public Guid FeeId { get; init; }
    public string CourseName { get; init; } = default!;
    public decimal TotalFee { get; init; }
}
