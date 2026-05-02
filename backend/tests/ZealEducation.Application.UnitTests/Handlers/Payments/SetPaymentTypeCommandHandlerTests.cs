using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Features.Payments.Commands.SetPaymentType;
using ZealEducation.Application.UnitTests.Handlers.Batches;
using ZealEducation.Application.UnitTests.Helpers;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.UnitTests.Handlers.Payments;

public class SetPaymentTypeCommandHandlerTests
{
    private readonly Mock<IRepository<FeeStructure>> _feeRepo = new();
    private readonly Mock<IRepository<Enrollment>> _enrollmentRepo = new();
    private readonly Mock<IRepository<Course>> _courseRepo = new();
    private readonly Mock<IRepository<InstallmentPlan>> _installmentRepo = new();
    private readonly Mock<IUnitOfWork> _uow = MockRepositoryExtensions.CreateUnitOfWork();

    private SetPaymentTypeCommandHandler CreateHandler() => new(
        _feeRepo.Object,
        _enrollmentRepo.Object,
        _courseRepo.Object,
        _installmentRepo.Object,
        _uow.Object);

    private void SetupInstallmentQuery(IEnumerable<InstallmentPlan> data) =>
        _installmentRepo.Setup(r => r.Query()).Returns(new TestAsyncEnumerable<InstallmentPlan>(data));

    private void SetupEnrollmentQuery(IEnumerable<Enrollment> data) =>
        _enrollmentRepo.Setup(r => r.Query()).Returns(new TestAsyncEnumerable<Enrollment>(data));

    private static FeeStructure NewFee(Guid feeId, decimal amountPaid = 0m, decimal totalFee = 1200m) => new()
    {
        Id = feeId,
        CandidateId = Guid.NewGuid(),
        TotalFee = totalFee,
        AmountPaid = amountPaid,
        OutstandingBalance = totalFee - amountPaid,
        PaymentType = PaymentType.NotSet,
        PaymentStatus = PaymentStatus.Unpaid
    };

    private static Enrollment NewEnrollment(Guid feeId, Guid courseId) => new()
    {
        Id = Guid.NewGuid(),
        CandidateId = Guid.NewGuid(),
        CourseId = courseId,
        FeeId = feeId,
        EnrollmentDate = new DateOnly(2030, 1, 1),
        Status = EnrollmentStatus.Enrolled
    };

    private static Course NewCourse(Guid courseId, int durationWeeks = 12) => new()
    {
        Id = courseId,
        CourseName = "Course",
        DurationWeeks = durationWeeks,
        BaseFee = 1200m,
        IsActive = true
    };

    [Fact]
    public async Task Throws_NotFoundException_when_fee_does_not_exist()
    {
        var feeId = Guid.NewGuid();
        _feeRepo.SetupGetById(feeId, null);

        var act = async () => await CreateHandler().Handle(
            new SetPaymentTypeCommand(feeId, PaymentType.FullPayment, null), default);

        await act.Should().ThrowAsync<NotFoundException>();
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Throws_ConflictException_when_amount_already_paid()
    {
        var feeId = Guid.NewGuid();
        _feeRepo.SetupGetById(feeId, NewFee(feeId, amountPaid: 100m));

        var act = async () => await CreateHandler().Handle(
            new SetPaymentTypeCommand(feeId, PaymentType.FullPayment, null), default);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("Cannot change payment type after a transaction has been recorded.");
    }

    [Fact]
    public async Task Sets_full_payment_and_clears_existing_installments()
    {
        var feeId = Guid.NewGuid();
        var fee = NewFee(feeId);
        _feeRepo.SetupGetById(feeId, fee);

        var existing = new InstallmentPlan
        {
            Id = Guid.NewGuid(),
            FeeId = feeId,
            InstallmentNo = 1,
            AmountDue = 100m,
            DueDate = new DateOnly(2030, 2, 1)
        };
        SetupInstallmentQuery(new[] { existing });

        var response = await CreateHandler().Handle(
            new SetPaymentTypeCommand(feeId, PaymentType.FullPayment, null), default);

        response.PaymentType.Should().Be(PaymentType.FullPayment);
        response.Frequency.Should().BeNull();
        response.Installments.Should().BeEmpty();
        fee.PaymentType.Should().Be(PaymentType.FullPayment);
        _installmentRepo.Verify(r => r.Delete(existing), Times.Once);
        _installmentRepo.Verify(r => r.AddAsync(It.IsAny<InstallmentPlan>(), It.IsAny<CancellationToken>()), Times.Never);
        _feeRepo.Verify(r => r.Update(fee), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Throws_ConflictException_when_installment_with_no_enrollment()
    {
        var feeId = Guid.NewGuid();
        _feeRepo.SetupGetById(feeId, NewFee(feeId));
        SetupInstallmentQuery(Array.Empty<InstallmentPlan>());
        SetupEnrollmentQuery(Array.Empty<Enrollment>());

        var act = async () => await CreateHandler().Handle(
            new SetPaymentTypeCommand(feeId, PaymentType.Installment, InstallmentFrequency.Monthly), default);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("Cannot set installment plan: enrollment is missing for this fee.");
    }

    [Fact]
    public async Task Throws_NotFoundException_when_course_for_installment_is_missing()
    {
        var feeId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        _feeRepo.SetupGetById(feeId, NewFee(feeId));
        SetupInstallmentQuery(Array.Empty<InstallmentPlan>());
        SetupEnrollmentQuery(new[] { NewEnrollment(feeId, courseId) });
        _courseRepo.SetupGetById(courseId, null);

        var act = async () => await CreateHandler().Handle(
            new SetPaymentTypeCommand(feeId, PaymentType.Installment, InstallmentFrequency.Monthly), default);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Throws_ConflictException_when_course_duration_below_minimum_for_installment()
    {
        var feeId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        _feeRepo.SetupGetById(feeId, NewFee(feeId));
        SetupInstallmentQuery(Array.Empty<InstallmentPlan>());
        SetupEnrollmentQuery(new[] { NewEnrollment(feeId, courseId) });
        _courseRepo.SetupGetById(courseId, NewCourse(courseId, durationWeeks: 4));

        var act = async () => await CreateHandler().Handle(
            new SetPaymentTypeCommand(feeId, PaymentType.Installment, InstallmentFrequency.Monthly), default);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("Course duration must be at least 2 months (8 weeks) to allow installment.");
    }

    [Fact]
    public async Task Throws_ConflictException_when_frequency_is_not_allowed_for_course_duration()
    {
        var feeId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        _feeRepo.SetupGetById(feeId, NewFee(feeId));
        SetupInstallmentQuery(Array.Empty<InstallmentPlan>());
        SetupEnrollmentQuery(new[] { NewEnrollment(feeId, courseId) });
        _courseRepo.SetupGetById(courseId, NewCourse(courseId, durationWeeks: 12));

        var act = async () => await CreateHandler().Handle(
            new SetPaymentTypeCommand(feeId, PaymentType.Installment, InstallmentFrequency.Quarterly), default);

        await act.Should().ThrowAsync<ConflictException>()
            .Where(e => e.Message.Contains("Quarterly") && e.Message.Contains("12-week"));
    }

    [Fact]
    public async Task Generates_installments_when_frequency_is_allowed()
    {
        var feeId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var fee = NewFee(feeId, totalFee: 1200m);
        _feeRepo.SetupGetById(feeId, fee);
        SetupInstallmentQuery(Array.Empty<InstallmentPlan>());
        var enrollment = NewEnrollment(feeId, courseId);
        SetupEnrollmentQuery(new[] { enrollment });
        _courseRepo.SetupGetById(courseId, NewCourse(courseId, durationWeeks: 12));

        var added = new List<InstallmentPlan>();
        _installmentRepo.Setup(r => r.AddAsync(It.IsAny<InstallmentPlan>(), It.IsAny<CancellationToken>()))
            .Callback<InstallmentPlan, CancellationToken>((p, _) => added.Add(p))
            .ReturnsAsync((InstallmentPlan p, CancellationToken _) => p);

        var response = await CreateHandler().Handle(
            new SetPaymentTypeCommand(feeId, PaymentType.Installment, InstallmentFrequency.Monthly), default);

        response.PaymentType.Should().Be(PaymentType.Installment);
        response.Frequency.Should().Be(InstallmentFrequency.Monthly);
        response.Installments.Should().NotBeEmpty();
        response.Installments.Sum(i => i.AmountDue).Should().Be(fee.TotalFee);
        response.Installments.Select(i => i.InstallmentNo).Should().BeInAscendingOrder();
        added.Should().HaveCount(response.Installments.Count);
        added.Should().OnlyContain(i => i.FeeId == feeId);
        added.Should().OnlyContain(i => i.Status == InstallmentStatus.Pending);
        added.Should().OnlyContain(i => i.AmountPaid == 0);
        added.Should().OnlyContain(i => i.PenaltyAmount == 0);

        fee.PaymentType.Should().Be(PaymentType.Installment);
        _feeRepo.Verify(r => r.Update(fee), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Removes_existing_installments_before_generating_new_ones()
    {
        var feeId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        _feeRepo.SetupGetById(feeId, NewFee(feeId));
        var existing = new InstallmentPlan
        {
            Id = Guid.NewGuid(),
            FeeId = feeId,
            InstallmentNo = 1,
            AmountDue = 50m,
            DueDate = new DateOnly(2030, 2, 1)
        };
        SetupInstallmentQuery(new[] { existing });
        SetupEnrollmentQuery(new[] { NewEnrollment(feeId, courseId) });
        _courseRepo.SetupGetById(courseId, NewCourse(courseId, durationWeeks: 12));

        _installmentRepo.Setup(r => r.AddAsync(It.IsAny<InstallmentPlan>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((InstallmentPlan p, CancellationToken _) => p);

        await CreateHandler().Handle(
            new SetPaymentTypeCommand(feeId, PaymentType.Installment, InstallmentFrequency.Monthly), default);

        _installmentRepo.Verify(r => r.Delete(existing), Times.Once);
    }
}
