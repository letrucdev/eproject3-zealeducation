using ZealEducation.Domain.Enums;

namespace ZealEducation.Application.Features.Payments.Queries.GetFeeStructures;

public class FeeStructureListItemDto
{
    public Guid FeeId { get; set; }
    public Guid CandidateId { get; set; }
    public string CandidateCode { get; set; } = default!;
    public string CandidateFullName { get; set; } = default!;
    public string? CourseTitle { get; set; }
    public FeeType FeeType { get; set; }
    public decimal TotalFee { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal OutstandingBalance { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public PaymentType PaymentType { get; set; }
    public DateTime CreatedAt { get; set; }
}
