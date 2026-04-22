namespace ZealEducation.Application.Features.Staffs.Queries.GetStaffStatistics;

public class StaffStatisticsDto
{
    public int Total { get; set; }
    public int Active { get; set; }
    public int Inactive { get; set; }
    public int Incharge { get; set; }
    public int Counselor { get; set; }
    public int AccountsStaff { get; set; }
}
