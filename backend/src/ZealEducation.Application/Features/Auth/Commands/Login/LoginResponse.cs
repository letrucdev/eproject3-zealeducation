using System.Text.Json.Serialization;
using ZealEducation.Domain.Enums;

namespace ZealEducation.Application.Features.Auth.Commands.Login;

public class LoginResponse
{
    public string Token { get; set; } = default!;
    public DateTime ExpiresAt { get; set; }
    public UserAccountDto User { get; set; } = default!;
}

public class UserAccountDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string Phone { get; set; } = default!;
    public DateOnly Dob { get; set; }
    public Gender Gender { get; set; }
    public UserRole Role { get; set; }
    public bool IsActive { get; set; }
    public DateTime? LastLogin { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public StaffInfoDto? Staff { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public FacultyInfoDto? Faculty { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public CandidateInfoDto? Candidate { get; set; }
}

public class StaffInfoDto
{
    public Guid Id { get; set; }
    public string Position { get; set; } = default!;
    public string Department { get; set; } = default!;
    public DateOnly JoinedDate { get; set; }
    public bool IsActive { get; set; }
}

public class FacultyInfoDto
{
    public Guid Id { get; set; }
    public Guid StaffId { get; set; }
    public string FacultyCode { get; set; } = default!;
    public string Qualification { get; set; } = default!;
    public string Specialization { get; set; } = default!;
    public int ExperienceYears { get; set; }
}

public class CandidateInfoDto
{
    public Guid Id { get; set; }
    public string CandidateCode { get; set; } = default!;
    public string? Address { get; set; }
    public string? EmergencyContact { get; set; }
    public CandidateStatus Status { get; set; }
    public DateTime RegisteredAt { get; set; }
}
