using ZealEducation.Application.Common.Helpers;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;

namespace ZealEducation.Application.UnitTests.Common.Helpers;

public class CertificateEligibilityCheckerTests
{
    private static FeeStructure PaidFee() => new()
    {
        CandidateId = Guid.NewGuid(),
        TotalFee = 1000m,
        AmountPaid = 1000m,
        OutstandingBalance = 0m,
        PaymentStatus = PaymentStatus.Paid
    };

    private static FeeStructure UnpaidFee() => new()
    {
        CandidateId = Guid.NewGuid(),
        TotalFee = 1000m,
        AmountPaid = 500m,
        OutstandingBalance = 500m,
        PaymentStatus = PaymentStatus.Partial
    };

    private static ExamResult PassedExam() => new()
    {
        ExamId = Guid.NewGuid(),
        EnrollmentId = Guid.NewGuid(),
        GradedById = Guid.NewGuid(),
        Score = 80m,
        Grade = "B",
        IsPassed = true,
        IsFinalized = true,
        GradedAt = DateTime.UtcNow
    };

    private static ExamResult FailedExam() => new()
    {
        ExamId = Guid.NewGuid(),
        EnrollmentId = Guid.NewGuid(),
        GradedById = Guid.NewGuid(),
        Score = 30m,
        Grade = "F",
        IsPassed = false,
        IsFinalized = true,
        GradedAt = DateTime.UtcNow
    };

    private static ExamResult NotFinalizedExam() => new()
    {
        ExamId = Guid.NewGuid(),
        EnrollmentId = Guid.NewGuid(),
        GradedById = Guid.NewGuid(),
        Score = 80m,
        IsPassed = true,
        IsFinalized = false,
        GradedAt = DateTime.UtcNow
    };

    [Fact]
    public void Should_be_eligible_when_all_conditions_met()
    {
        var result = CertificateEligibilityChecker.Evaluate(
            PaidFee(),
            presentCount: 9,
            totalSessions: 10,
            examResults: [PassedExam()]);

        result.IsEligible.Should().BeTrue();
        result.FeesPaid.Should().BeTrue();
        result.AttendanceOk.Should().BeTrue();
        result.ExamsPassed.Should().BeTrue();
        result.AttendancePercent.Should().Be(90m);
        result.Reason.Should().BeNull();
    }

    [Fact]
    public void Should_not_be_eligible_when_fee_is_null()
    {
        var result = CertificateEligibilityChecker.Evaluate(
            fee: null,
            presentCount: 10,
            totalSessions: 10,
            examResults: [PassedExam()]);

        result.IsEligible.Should().BeFalse();
        result.FeesPaid.Should().BeFalse();
        result.Reason.Should().Contain("course fees are not fully paid");
    }

    [Fact]
    public void Should_not_be_eligible_when_fee_partially_paid()
    {
        var result = CertificateEligibilityChecker.Evaluate(
            UnpaidFee(),
            presentCount: 10,
            totalSessions: 10,
            examResults: [PassedExam()]);

        result.IsEligible.Should().BeFalse();
        result.FeesPaid.Should().BeFalse();
    }

    [Fact]
    public void Should_not_be_eligible_when_paid_status_but_outstanding_balance_remains()
    {
        var fee = PaidFee();
        fee.OutstandingBalance = 1m;

        var result = CertificateEligibilityChecker.Evaluate(
            fee,
            presentCount: 10,
            totalSessions: 10,
            examResults: [PassedExam()]);

        result.FeesPaid.Should().BeFalse();
        result.IsEligible.Should().BeFalse();
    }

    [Theory]
    [InlineData(8, 10, 80, true)]   // exactly threshold
    [InlineData(7, 10, 70, false)]  // below threshold
    [InlineData(10, 10, 100, true)] // perfect
    public void Should_evaluate_attendance_against_minimum_percent(int present, int total, decimal expectedPct, bool expectedOk)
    {
        var result = CertificateEligibilityChecker.Evaluate(
            PaidFee(),
            present,
            total,
            [PassedExam()]);

        result.AttendancePercent.Should().Be(expectedPct);
        result.AttendanceOk.Should().Be(expectedOk);
    }

    [Fact]
    public void Should_treat_zero_total_sessions_as_zero_attendance()
    {
        var result = CertificateEligibilityChecker.Evaluate(
            PaidFee(),
            presentCount: 0,
            totalSessions: 0,
            examResults: [PassedExam()]);

        result.AttendancePercent.Should().Be(0m);
        result.AttendanceOk.Should().BeFalse();
        result.IsEligible.Should().BeFalse();
    }

    [Fact]
    public void Should_not_be_eligible_when_no_exams_recorded()
    {
        var result = CertificateEligibilityChecker.Evaluate(
            PaidFee(),
            presentCount: 10,
            totalSessions: 10,
            examResults: []);

        result.ExamsPassed.Should().BeFalse();
        result.IsEligible.Should().BeFalse();
        result.Reason.Should().Contain("not all exams have been finalized and passed");
    }

    [Fact]
    public void Should_not_be_eligible_when_any_exam_failed()
    {
        var result = CertificateEligibilityChecker.Evaluate(
            PaidFee(),
            presentCount: 10,
            totalSessions: 10,
            examResults: [PassedExam(), FailedExam()]);

        result.ExamsPassed.Should().BeFalse();
        result.IsEligible.Should().BeFalse();
    }

    [Fact]
    public void Should_not_be_eligible_when_any_exam_not_finalized()
    {
        var result = CertificateEligibilityChecker.Evaluate(
            PaidFee(),
            presentCount: 10,
            totalSessions: 10,
            examResults: [PassedExam(), NotFinalizedExam()]);

        result.ExamsPassed.Should().BeFalse();
        result.IsEligible.Should().BeFalse();
    }

    [Fact]
    public void Should_compose_reason_with_all_failing_factors()
    {
        var result = CertificateEligibilityChecker.Evaluate(
            UnpaidFee(),
            presentCount: 5,
            totalSessions: 10,
            examResults: [FailedExam()]);

        result.IsEligible.Should().BeFalse();
        result.Reason.Should().StartWith("Not eligible:");
        result.Reason.Should().Contain("course fees are not fully paid");
        result.Reason.Should().Contain("attendance is below 80%");
        result.Reason.Should().Contain("not all exams have been finalized and passed");
    }
}
