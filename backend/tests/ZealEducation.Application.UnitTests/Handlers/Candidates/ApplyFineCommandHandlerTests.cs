using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Application.Features.Candidates.Commands.ApplyFine;
using ZealEducation.Application.UnitTests.Helpers;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.UnitTests.Handlers.Candidates;

public class ApplyFineCommandHandlerTests
{
    private readonly Mock<IRepository<Candidate>> _candidateRepo = new();
    private readonly Mock<IRepository<UserAccount>> _userRepo = new();
    private readonly Mock<IRepository<Staff>> _staffRepo = new();
    private readonly Mock<IRepository<FeeStructure>> _feeRepo = new();
    private readonly Mock<IRepository<Fine>> _fineRepo = new();
    private readonly Mock<ICurrentUser> _currentUser = new();
    private readonly Mock<IUnitOfWork> _uow = MockRepositoryExtensions.CreateUnitOfWork();

    private ApplyFineCommandHandler CreateHandler() => new(
        _candidateRepo.Object,
        _userRepo.Object,
        _staffRepo.Object,
        _feeRepo.Object,
        _fineRepo.Object,
        _currentUser.Object,
        _uow.Object);

    private static ApplyFineCommand Cmd(
        Guid? candidateId = null,
        string violationReason = "Late submission",
        decimal penaltyAmount = 100m) => new(
            candidateId ?? Guid.NewGuid(),
            violationReason,
            penaltyAmount);

    [Fact]
    public async Task Throws_unauthorized_when_current_user_is_not_resolved()
    {
        _currentUser.SetupGet(u => u.UserId).Returns((Guid?)null);

        var act = async () => await CreateHandler().Handle(Cmd(), default);

        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Current user could not be resolved.");
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Throws_NotFoundException_when_candidate_does_not_exist()
    {
        var candidateId = Guid.NewGuid();
        _currentUser.SetupGet(u => u.UserId).Returns(Guid.NewGuid());
        _candidateRepo.SetupGetById(candidateId, null);

        var act = async () => await CreateHandler().Handle(Cmd(candidateId: candidateId), default);

        await act.Should().ThrowAsync<NotFoundException>();
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Throws_NotFoundException_when_user_account_does_not_exist()
    {
        var candidateId = Guid.NewGuid();
        var userAccountId = Guid.NewGuid();
        var candidate = new Candidate { Id = candidateId, UserAccountId = userAccountId };
        _currentUser.SetupGet(u => u.UserId).Returns(Guid.NewGuid());
        _candidateRepo.SetupGetById(candidateId, candidate);
        _userRepo.SetupGetById(userAccountId, null);

        var act = async () => await CreateHandler().Handle(Cmd(candidateId: candidateId), default);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Throws_ConflictException_when_user_account_is_inactive()
    {
        var candidateId = Guid.NewGuid();
        var userAccountId = Guid.NewGuid();
        var candidate = new Candidate { Id = candidateId, UserAccountId = userAccountId };
        var user = new UserAccount { Id = userAccountId, IsActive = false };
        _currentUser.SetupGet(u => u.UserId).Returns(Guid.NewGuid());
        _candidateRepo.SetupGetById(candidateId, candidate);
        _userRepo.SetupGetById(userAccountId, user);

        var act = async () => await CreateHandler().Handle(Cmd(candidateId: candidateId), default);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("Cannot apply a fine to a candidate with an inactive account.");
    }

    [Fact]
    public async Task Throws_ConflictException_when_current_user_is_not_staff()
    {
        var candidateId = Guid.NewGuid();
        var userAccountId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();
        var candidate = new Candidate { Id = candidateId, UserAccountId = userAccountId };
        var user = new UserAccount { Id = userAccountId, IsActive = true };
        _currentUser.SetupGet(u => u.UserId).Returns(currentUserId);
        _candidateRepo.SetupGetById(candidateId, candidate);
        _userRepo.SetupGetById(userAccountId, user);
        _staffRepo.SetupFind(Array.Empty<Staff>());

        var act = async () => await CreateHandler().Handle(Cmd(candidateId: candidateId), default);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("Current user is not registered as staff.");
    }

    [Fact]
    public async Task Creates_fee_and_fine_with_correct_properties_on_success()
    {
        var candidateId = Guid.NewGuid();
        var userAccountId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();
        var staffId = Guid.NewGuid();
        var candidate = new Candidate { Id = candidateId, UserAccountId = userAccountId };
        var user = new UserAccount { Id = userAccountId, IsActive = true };
        var staff = new Staff { Id = staffId, UserAccountId = currentUserId };

        _currentUser.SetupGet(u => u.UserId).Returns(currentUserId);
        _candidateRepo.SetupGetById(candidateId, candidate);
        _userRepo.SetupGetById(userAccountId, user);
        _staffRepo.SetupFind(new[] { staff });

        FeeStructure? capturedFee = null;
        Fine? capturedFine = null;
        _feeRepo.Setup(r => r.AddAsync(It.IsAny<FeeStructure>(), It.IsAny<CancellationToken>()))
            .Callback<FeeStructure, CancellationToken>((f, _) => capturedFee = f)
            .ReturnsAsync((FeeStructure f, CancellationToken _) => f);
        _fineRepo.Setup(r => r.AddAsync(It.IsAny<Fine>(), It.IsAny<CancellationToken>()))
            .Callback<Fine, CancellationToken>((f, _) => capturedFine = f)
            .ReturnsAsync((Fine f, CancellationToken _) => f);

        var response = await CreateHandler().Handle(
            Cmd(candidateId: candidateId, violationReason: "  Disrupted class  ", penaltyAmount: 250m),
            default);

        capturedFee.Should().NotBeNull();
        capturedFee!.CandidateId.Should().Be(candidateId);
        capturedFee.TotalFee.Should().Be(250m);
        capturedFee.AmountPaid.Should().Be(0m);
        capturedFee.OutstandingBalance.Should().Be(250m);
        capturedFee.FeeType.Should().Be(FeeType.Fine);
        capturedFee.PaymentStatus.Should().Be(PaymentStatus.Unpaid);
        capturedFee.PaymentType.Should().Be(PaymentType.NotSet);
        capturedFee.Notes.Should().Be("Disrupted class");

        capturedFine.Should().NotBeNull();
        capturedFine!.FeeId.Should().Be(capturedFee.Id);
        capturedFine.CandidateId.Should().Be(candidateId);
        capturedFine.IssuedByStaffId.Should().Be(staffId);
        capturedFine.ViolationReason.Should().Be("Disrupted class");
        capturedFine.PenaltyAmount.Should().Be(250m);
        capturedFine.IsPaid.Should().BeFalse();
        capturedFine.IssuedDate.Should().Be(DateOnly.FromDateTime(DateTime.UtcNow));

        response.FineId.Should().Be(capturedFine.Id);
        response.FeeId.Should().Be(capturedFee.Id);

        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Rounds_penalty_amount_to_two_decimal_places()
    {
        var candidateId = Guid.NewGuid();
        var userAccountId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();
        var candidate = new Candidate { Id = candidateId, UserAccountId = userAccountId };
        var user = new UserAccount { Id = userAccountId, IsActive = true };
        var staff = new Staff { Id = Guid.NewGuid(), UserAccountId = currentUserId };

        _currentUser.SetupGet(u => u.UserId).Returns(currentUserId);
        _candidateRepo.SetupGetById(candidateId, candidate);
        _userRepo.SetupGetById(userAccountId, user);
        _staffRepo.SetupFind(new[] { staff });

        Fine? capturedFine = null;
        FeeStructure? capturedFee = null;
        _feeRepo.Setup(r => r.AddAsync(It.IsAny<FeeStructure>(), It.IsAny<CancellationToken>()))
            .Callback<FeeStructure, CancellationToken>((f, _) => capturedFee = f)
            .ReturnsAsync((FeeStructure f, CancellationToken _) => f);
        _fineRepo.Setup(r => r.AddAsync(It.IsAny<Fine>(), It.IsAny<CancellationToken>()))
            .Callback<Fine, CancellationToken>((f, _) => capturedFine = f)
            .ReturnsAsync((Fine f, CancellationToken _) => f);

        await CreateHandler().Handle(
            Cmd(candidateId: candidateId, penaltyAmount: 99.555m),
            default);

        capturedFine!.PenaltyAmount.Should().Be(99.56m);
        capturedFee!.TotalFee.Should().Be(99.56m);
        capturedFee.OutstandingBalance.Should().Be(99.56m);
    }
}
