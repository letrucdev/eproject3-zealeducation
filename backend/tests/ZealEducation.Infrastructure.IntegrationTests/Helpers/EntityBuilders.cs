using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;

namespace ZealEducation.Infrastructure.IntegrationTests.Helpers;

/// Fluent factories for seeding test data. Each helper returns a fully-formed
/// entity ready for `ctx.Add(...)`. Override fields via constructor args when
/// the test needs specific values; otherwise sensible defaults are used.
internal static class EntityBuilders
{
    public static UserAccount NewUser(
        string username = "test.user",
        string passwordHash = "$bcrypt$hash",
        string fullName = "Test User",
        UserRole role = UserRole.Counselor,
        bool isActive = true,
        bool mustChangePassword = false,
        int failedLoginCount = 0) => new()
    {
        Id = Guid.NewGuid(),
        Username = username,
        PasswordHash = passwordHash,
        FullName = fullName,
        Email = $"{username}@example.com",
        Phone = "0123456789",
        Dob = new DateOnly(1995, 1, 1),
        Gender = Gender.Other,
        Role = role,
        IsActive = isActive,
        MustChangePassword = mustChangePassword,
        FailedLoginCount = failedLoginCount
    };

    public static Candidate NewCandidate(Guid userAccountId, string code = "C-001") => new()
    {
        Id = Guid.NewGuid(),
        UserAccountId = userAccountId,
        CandidateCode = code,
        Status = CandidateStatus.Active,
        RegisteredAt = DateTime.UtcNow
    };

    public static Course NewCourse(string name = "Course", decimal baseFee = 1000m, bool active = true) => new()
    {
        Id = Guid.NewGuid(),
        CourseName = name,
        DurationWeeks = 8,
        BaseFee = baseFee,
        IsActive = active
    };

    public static FeeStructure NewFeeStructure(
        Guid candidateId,
        decimal totalFee = 1000m,
        decimal amountPaid = 0m,
        PaymentStatus status = PaymentStatus.Unpaid,
        PaymentType paymentType = PaymentType.NotSet) => new()
    {
        Id = Guid.NewGuid(),
        CandidateId = candidateId,
        TotalFee = totalFee,
        AmountPaid = amountPaid,
        OutstandingBalance = totalFee - amountPaid,
        FeeType = FeeType.Tuition,
        PaymentStatus = status,
        PaymentType = paymentType
    };

    public static PaymentTransaction NewPaymentTransaction(
        Guid feeId,
        Guid processedByStaffId,
        decimal amount = 100m,
        string receiptNumber = "RC-001",
        PaymentMethod method = PaymentMethod.Cash,
        Guid? installmentPlanId = null) => new()
    {
        Id = Guid.NewGuid(),
        FeeId = feeId,
        ProcessedByStaffId = processedByStaffId,
        ReceiptNumber = receiptNumber,
        PaymentDate = DateTime.UtcNow,
        Amount = amount,
        PaymentMethod = method,
        InstallmentPlanId = installmentPlanId,
        OutstandingBalanceAfter = 0m
    };

    public static Staff NewStaff(Guid userAccountId, bool isActive = true) => new()
    {
        Id = Guid.NewGuid(),
        UserAccountId = userAccountId,
        Position = "Counselor",
        Department = "Admissions",
        JoinedDate = new DateOnly(2020, 1, 1),
        IsActive = isActive
    };

    public static Faculty NewFaculty(Guid staffId, string code = "F-001") => new()
    {
        Id = Guid.NewGuid(),
        StaffId = staffId,
        FacultyCode = code,
        Qualification = "PhD",
        Specialization = "AI",
        ExperienceYears = 5
    };
}
