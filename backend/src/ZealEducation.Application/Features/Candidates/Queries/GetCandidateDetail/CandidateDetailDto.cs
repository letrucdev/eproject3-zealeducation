using ZealEducation.Domain.Enums;

namespace ZealEducation.Application.Features.Candidates.Queries.GetCandidateDetail;

public class CandidateDetailDto
{
    public Guid CandidateId { get; set; }
    public string CandidateCode { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string Phone { get; set; } = default!;
    public DateOnly Dob { get; set; }
    public Gender Gender { get; set; }
    public bool IsActive { get; set; }
    public string? Address { get; set; }
    public string? EmergencyContact { get; set; }
    public string? Notes { get; set; }
    public CandidateStatus Status { get; set; }
    public DateTime RegisteredAt { get; set; }

    public List<CandidateEnrollmentDto> Enrollments { get; set; } = [];
    public List<CandidateFeeStructureSummaryDto> FeeStructures { get; set; } = [];
}

public class CandidateEnrollmentDto
{
    public Guid EnrollmentId { get; set; }
    public Guid CourseId { get; set; }
    public string CourseName { get; set; } = default!;
    public int DurationWeeks { get; set; }
    public Guid? BatchId { get; set; }
    public string? BatchCode { get; set; }
    public DateOnly? BatchStartDate { get; set; }
    public DateOnly? BatchEndDate { get; set; }
    public DateOnly EnrollmentDate { get; set; }
    public EnrollmentStatus Status { get; set; }
    public Guid? FeeId { get; set; }
    public string? Notes { get; set; }
}

public class CandidateFeeStructureSummaryDto
{
    public Guid FeeId { get; set; }
    public FeeType FeeType { get; set; }
    public decimal TotalFee { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal OutstandingBalance { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public PaymentType PaymentType { get; set; }
    public string? CourseTitle { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}
