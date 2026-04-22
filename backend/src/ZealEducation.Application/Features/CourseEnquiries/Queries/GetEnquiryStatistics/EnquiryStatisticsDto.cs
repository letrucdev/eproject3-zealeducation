namespace ZealEducation.Application.Features.CourseEnquiries.Queries.GetEnquiryStatistics;

public class EnquiryStatisticsDto
{
    public int Total { get; set; }
    public int New { get; set; }
    public int InFollowUp { get; set; }
    public int Converted { get; set; }
    public int Overdue { get; set; }
}
