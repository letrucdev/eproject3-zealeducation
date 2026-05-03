using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Application.Features.Payments.Commands.ConfirmPayment;
using ZealEducation.Domain.Enums;
using ZealEducation.Infrastructure.IntegrationTests.Fixtures;
using ZealEducation.Infrastructure.IntegrationTests.Helpers;
using ZealEducation.Infrastructure.Repositories;

namespace ZealEducation.Infrastructure.IntegrationTests.Handlers.Payments;

[Collection(DatabaseCollection.Name)]
public class ConfirmPaymentCommandHandlerTests : IAsyncLifetime
{
    private readonly MsSqlContainerFixture _fixture;
    private readonly Mock<ICurrentUser> _currentUser = new();
    private readonly Mock<IFileStorageService> _fileStorage = new();

    public ConfirmPaymentCommandHandlerTests(MsSqlContainerFixture fixture) => _fixture = fixture;

    public Task InitializeAsync() => _fixture.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private ConfirmPaymentCommandHandler CreateHandler()
    {
        var ctx = _fixture.CreateDbContext();
        return new ConfirmPaymentCommandHandler(
            new GenericRepository<Domain.Entities.FeeStructure>(ctx),
            new GenericRepository<Domain.Entities.InstallmentPlan>(ctx),
            new GenericRepository<Domain.Entities.PaymentTransaction>(ctx),
            new GenericRepository<Domain.Entities.Candidate>(ctx),
            new GenericRepository<Domain.Entities.UserAccount>(ctx),
            new GenericRepository<Domain.Entities.Staff>(ctx),
            _currentUser.Object,
            _fileStorage.Object,
            ctx);
    }

    private async Task<(Guid feeId, Guid candidateUserId, Guid staffUserId)> SeedFullPaymentSetupAsync(
        decimal totalFee = 1000m,
        decimal amountPaid = 0m,
        PaymentType paymentType = PaymentType.FullPayment,
        bool candidateActive = true)
    {
        await using var ctx = _fixture.CreateDbContext();

        var candidateUser = EntityBuilders.NewUser(username: "cand", role: UserRole.Candidate, isActive: candidateActive);
        var staffUser = EntityBuilders.NewUser(username: "staff1", role: UserRole.AccountsStaff);
        ctx.UserAccounts.AddRange(candidateUser, staffUser);

        var candidate = EntityBuilders.NewCandidate(candidateUser.Id, code: "C-100");
        var staff = EntityBuilders.NewStaff(staffUser.Id);
        ctx.Candidates.Add(candidate);
        ctx.Staff.Add(staff);

        var fee = EntityBuilders.NewFeeStructure(
            candidate.Id,
            totalFee: totalFee,
            amountPaid: amountPaid,
            status: amountPaid > 0 ? PaymentStatus.Partial : PaymentStatus.Unpaid,
            paymentType: paymentType);
        ctx.FeeStructures.Add(fee);

        await ctx.SaveChangesAsync();
        return (fee.Id, candidateUser.Id, staffUser.Id);
    }

    [Fact]
    public async Task Should_throw_when_current_user_anonymous()
    {
        _currentUser.SetupGet(u => u.UserId).Returns((Guid?)null);

        var act = async () => await CreateHandler().Handle(
            new ConfirmPaymentCommand(Guid.NewGuid(), null, PaymentMethod.Cash), default);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task Should_throw_NotFound_when_fee_does_not_exist()
    {
        _currentUser.SetupGet(u => u.UserId).Returns(Guid.NewGuid());

        var act = async () => await CreateHandler().Handle(
            new ConfirmPaymentCommand(Guid.NewGuid(), null, PaymentMethod.Cash), default);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Should_throw_when_payment_type_not_set()
    {
        var (feeId, _, staffUserId) = await SeedFullPaymentSetupAsync(paymentType: PaymentType.NotSet);
        _currentUser.SetupGet(u => u.UserId).Returns(staffUserId);

        var act = async () => await CreateHandler().Handle(
            new ConfirmPaymentCommand(feeId, null, PaymentMethod.Cash), default);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("Payment type has not been set for this fee.");
    }

    [Fact]
    public async Task Should_confirm_full_payment_and_persist_transaction()
    {
        var (feeId, _, staffUserId) = await SeedFullPaymentSetupAsync();
        _currentUser.SetupGet(u => u.UserId).Returns(staffUserId);

        var response = await CreateHandler().Handle(
            new ConfirmPaymentCommand(feeId, null, PaymentMethod.Cash), default);

        response.TotalAmount.Should().Be(1000m);
        response.PenaltyAmount.Should().Be(0m);
        response.NewPaymentStatus.Should().Be(PaymentStatus.Paid);
        response.ReceiptNumber.Should().StartWith("RCP-");

        await using var ctx = _fixture.CreateDbContext();
        var transactions = ctx.PaymentTransactions.Where(t => t.FeeId == feeId).ToList();
        transactions.Should().ContainSingle();
        transactions[0].Amount.Should().Be(1000m);
        transactions[0].PaymentMethod.Should().Be(PaymentMethod.Cash);

        var fee = await ctx.FeeStructures.FindAsync(feeId);
        fee!.AmountPaid.Should().Be(1000m);
        fee.PaymentStatus.Should().Be(PaymentStatus.Paid);
    }

    [Fact]
    public async Task Should_throw_when_full_payment_but_already_paid()
    {
        var (feeId, _, staffUserId) = await SeedFullPaymentSetupAsync(amountPaid: 1000m);
        _currentUser.SetupGet(u => u.UserId).Returns(staffUserId);
        // Mark as fully paid
        await using (var ctx = _fixture.CreateDbContext())
        {
            var fee = await ctx.FeeStructures.FindAsync(feeId);
            fee!.OutstandingBalance = 0m;
            fee.PaymentStatus = PaymentStatus.Paid;
            await ctx.SaveChangesAsync();
        }

        var act = async () => await CreateHandler().Handle(
            new ConfirmPaymentCommand(feeId, null, PaymentMethod.Cash), default);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("This fee has already been fully paid.");
    }

    [Fact]
    public async Task Should_reactivate_inactive_candidate_when_tuition_paid()
    {
        var (feeId, candidateUserId, staffUserId) = await SeedFullPaymentSetupAsync(candidateActive: false);
        _currentUser.SetupGet(u => u.UserId).Returns(staffUserId);

        await CreateHandler().Handle(
            new ConfirmPaymentCommand(feeId, null, PaymentMethod.Cash), default);

        await using var ctx = _fixture.CreateDbContext();
        var candidateUser = await ctx.UserAccounts.FindAsync(candidateUserId);
        candidateUser!.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Should_throw_when_installment_required_but_not_provided()
    {
        var (feeId, _, staffUserId) = await SeedFullPaymentSetupAsync(paymentType: PaymentType.Installment);
        _currentUser.SetupGet(u => u.UserId).Returns(staffUserId);

        var act = async () => await CreateHandler().Handle(
            new ConfirmPaymentCommand(feeId, null, PaymentMethod.Cash), default);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("InstallmentPlanId is required for installment payment.");
    }

    [Fact]
    public async Task Should_generate_unique_receipt_numbers_across_concurrent_payments()
    {
        var (feeId, _, staffUserId) = await SeedFullPaymentSetupAsync(totalFee: 2000m);
        _currentUser.SetupGet(u => u.UserId).Returns(staffUserId);

        var first = await CreateHandler().Handle(
            new ConfirmPaymentCommand(feeId, null, PaymentMethod.Cash), default);

        // Make another fee for second transaction
        var (feeId2, _, _) = await SeedFullPaymentSetupAsync(totalFee: 500m);
        var second = await CreateHandler().Handle(
            new ConfirmPaymentCommand(feeId2, null, PaymentMethod.Cash), default);

        first.ReceiptNumber.Should().NotBe(second.ReceiptNumber);
    }
}
