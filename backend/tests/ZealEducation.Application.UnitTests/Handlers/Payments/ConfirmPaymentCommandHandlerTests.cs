using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Application.Features.Payments.Commands.ConfirmPayment;
using ZealEducation.Application.UnitTests.Helpers;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.UnitTests.Handlers.Payments;

public class ConfirmPaymentCommandHandlerTests
{
    private readonly Mock<IRepository<FeeStructure>> _feeRepo = new();
    private readonly Mock<IRepository<InstallmentPlan>> _installmentRepo = new();
    private readonly Mock<IRepository<PaymentTransaction>> _paymentRepo = new();
    private readonly Mock<IRepository<Candidate>> _candidateRepo = new();
    private readonly Mock<IRepository<UserAccount>> _userRepo = new();
    private readonly Mock<IRepository<Staff>> _staffRepo = new();
    private readonly Mock<ICurrentUser> _currentUser = new();
    private readonly Mock<IFileStorageService> _fileStorage = new();
    private readonly Mock<IUnitOfWork> _uow = MockRepositoryExtensions.CreateUnitOfWork();

    private ConfirmPaymentCommandHandler CreateHandler() => new(
        _feeRepo.Object,
        _installmentRepo.Object,
        _paymentRepo.Object,
        _candidateRepo.Object,
        _userRepo.Object,
        _staffRepo.Object,
        _currentUser.Object,
        _fileStorage.Object,
        _uow.Object);

    [Fact]
    public async Task Throws_unauthorized_when_current_user_is_not_resolved()
    {
        _currentUser.SetupGet(u => u.UserId).Returns((Guid?)null);

        var act = async () => await CreateHandler().Handle(
            new ConfirmPaymentCommand(Guid.NewGuid(), null, PaymentMethod.Cash), default);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task Throws_NotFoundException_when_fee_does_not_exist()
    {
        var feeId = Guid.NewGuid();
        _currentUser.SetupGet(u => u.UserId).Returns(Guid.NewGuid());
        _feeRepo.SetupGetById(feeId, null);

        var act = async () => await CreateHandler().Handle(
            new ConfirmPaymentCommand(feeId, null, PaymentMethod.Cash), default);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Throws_ConflictException_when_payment_type_is_NotSet()
    {
        var feeId = Guid.NewGuid();
        _currentUser.SetupGet(u => u.UserId).Returns(Guid.NewGuid());
        _feeRepo.SetupGetById(feeId, new FeeStructure
        {
            Id = feeId,
            PaymentType = PaymentType.NotSet,
            TotalFee = 1000,
            OutstandingBalance = 1000
        });

        var act = async () => await CreateHandler().Handle(
            new ConfirmPaymentCommand(feeId, null, PaymentMethod.Cash), default);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("Payment type has not been set for this fee.");
    }

    [Fact]
    public async Task Throws_ConflictException_when_full_payment_already_settled()
    {
        var feeId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _currentUser.SetupGet(u => u.UserId).Returns(userId);
        _feeRepo.SetupGetById(feeId, new FeeStructure
        {
            Id = feeId,
            PaymentType = PaymentType.FullPayment,
            TotalFee = 1000,
            AmountPaid = 1000,
            OutstandingBalance = 0
        });
        _staffRepo.SetupFind(new[] { new Staff { Id = Guid.NewGuid(), UserAccountId = userId } });

        var act = async () => await CreateHandler().Handle(
            new ConfirmPaymentCommand(feeId, null, PaymentMethod.Cash), default);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("This fee has already been fully paid.");
    }

    [Fact]
    public async Task Records_full_cash_payment_and_marks_fee_as_paid()
    {
        var feeId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var fee = new FeeStructure
        {
            Id = feeId,
            CandidateId = Guid.NewGuid(),
            FeeType = FeeType.Tuition,
            PaymentType = PaymentType.FullPayment,
            TotalFee = 1000m,
            AmountPaid = 0m,
            OutstandingBalance = 1000m,
            PaymentStatus = PaymentStatus.Unpaid
        };
        _currentUser.SetupGet(u => u.UserId).Returns(userId);
        _feeRepo.SetupGetById(feeId, fee);
        _staffRepo.SetupFind(new[] { new Staff { Id = Guid.NewGuid(), UserAccountId = userId } });
        _paymentRepo.SetupFind(Array.Empty<PaymentTransaction>());
        _candidateRepo.SetupGetById(fee.CandidateId, null);
        _paymentRepo.SetupAdd();

        var response = await CreateHandler().Handle(
            new ConfirmPaymentCommand(feeId, null, PaymentMethod.Cash), default);

        response.BaseAmount.Should().Be(1000m);
        response.PenaltyAmount.Should().Be(0m);
        response.TotalAmount.Should().Be(1000m);
        response.NewPaymentStatus.Should().Be(PaymentStatus.Paid);
        response.ReceiptNumber.Should().StartWith("RCP-");
        fee.AmountPaid.Should().Be(1000m);
        fee.PaymentStatus.Should().Be(PaymentStatus.Paid);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
