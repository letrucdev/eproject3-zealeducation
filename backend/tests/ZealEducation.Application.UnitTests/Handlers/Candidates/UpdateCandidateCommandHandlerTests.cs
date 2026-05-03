using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Features.Candidates.Commands.UpdateCandidate;
using ZealEducation.Application.UnitTests.Helpers;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.UnitTests.Handlers.Candidates;

public class UpdateCandidateCommandHandlerTests
{
    private readonly Mock<IRepository<Candidate>> _candidateRepo = new();
    private readonly Mock<IRepository<UserAccount>> _userRepo = new();
    private readonly Mock<IUnitOfWork> _uow = MockRepositoryExtensions.CreateUnitOfWork();

    private UpdateCandidateCommandHandler CreateHandler() => new(
        _candidateRepo.Object,
        _userRepo.Object,
        _uow.Object);

    private static UpdateCandidateCommand Cmd(
        Guid? candidateId = null,
        string fullName = "John Updated",
        string email = "john.updated@example.com",
        string phone = "0911222333",
        string? address = "1 Main St",
        string? emergencyContact = "Jane Doe",
        string? notes = null,
        CandidateStatus status = CandidateStatus.Active) => new(
            candidateId ?? Guid.NewGuid(),
            fullName,
            email,
            phone,
            address,
            emergencyContact,
            notes,
            status);

    [Fact]
    public async Task Throws_NotFoundException_when_candidate_does_not_exist()
    {
        var candidateId = Guid.NewGuid();
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
        _candidateRepo.SetupGetById(candidateId, candidate);
        _userRepo.SetupGetById(userAccountId, null);

        var act = async () => await CreateHandler().Handle(Cmd(candidateId: candidateId), default);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Throws_ConflictException_when_email_is_already_in_use_by_another_user()
    {
        var candidateId = Guid.NewGuid();
        var userAccountId = Guid.NewGuid();
        var candidate = new Candidate { Id = candidateId, UserAccountId = userAccountId };
        var user = new UserAccount
        {
            Id = userAccountId,
            FullName = "John Doe",
            Email = "old@example.com",
            Phone = "0911222333"
        };
        _candidateRepo.SetupGetById(candidateId, candidate);
        _userRepo.SetupGetById(userAccountId, user);
        _userRepo.SetupFind(new[] { new UserAccount { Id = Guid.NewGuid(), Email = "new@example.com" } });

        var act = async () => await CreateHandler().Handle(
            Cmd(candidateId: candidateId, email: "new@example.com", phone: user.Phone),
            default);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("Email is already in use.");
    }

    [Fact]
    public async Task Throws_ConflictException_when_phone_is_already_in_use_by_another_user()
    {
        var candidateId = Guid.NewGuid();
        var userAccountId = Guid.NewGuid();
        var candidate = new Candidate { Id = candidateId, UserAccountId = userAccountId };
        var user = new UserAccount
        {
            Id = userAccountId,
            FullName = "John Doe",
            Email = "john@example.com",
            Phone = "0900000000"
        };
        _candidateRepo.SetupGetById(candidateId, candidate);
        _userRepo.SetupGetById(userAccountId, user);
        _userRepo.SetupFind(new[] { new UserAccount { Id = Guid.NewGuid(), Phone = "0911222333" } });

        var act = async () => await CreateHandler().Handle(
            Cmd(candidateId: candidateId, email: user.Email, phone: "0911222333"),
            default);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("Phone is already in use.");
    }

    [Fact]
    public async Task Updates_user_and_candidate_with_normalized_values_on_success()
    {
        var candidateId = Guid.NewGuid();
        var userAccountId = Guid.NewGuid();
        var candidate = new Candidate
        {
            Id = candidateId,
            UserAccountId = userAccountId,
            Address = "old address",
            EmergencyContact = "old contact",
            Notes = "old notes",
            Status = CandidateStatus.OnBreak
        };
        var user = new UserAccount
        {
            Id = userAccountId,
            FullName = "Old Name",
            Email = "old@example.com",
            Phone = "0900000000"
        };
        _candidateRepo.SetupGetById(candidateId, candidate);
        _userRepo.SetupGetById(userAccountId, user);
        _userRepo.SetupFind(Array.Empty<UserAccount>());

        await CreateHandler().Handle(
            new UpdateCandidateCommand(
                candidateId,
                "  Jane Smith  ",
                "  jane@example.com  ",
                "  0911222333  ",
                "  789 New St  ",
                "  Mom 0911000000  ",
                "  some notes  ",
                CandidateStatus.Active),
            default);

        user.FullName.Should().Be("Jane Smith");
        user.Email.Should().Be("jane@example.com");
        user.Phone.Should().Be("0911222333");

        candidate.Address.Should().Be("789 New St");
        candidate.EmergencyContact.Should().Be("Mom 0911000000");
        candidate.Notes.Should().Be("some notes");
        candidate.Status.Should().Be(CandidateStatus.Active);

        _userRepo.Verify(r => r.Update(user), Times.Once);
        _candidateRepo.Verify(r => r.Update(candidate), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Sets_optional_string_fields_to_null_when_blank()
    {
        var candidateId = Guid.NewGuid();
        var userAccountId = Guid.NewGuid();
        var candidate = new Candidate
        {
            Id = candidateId,
            UserAccountId = userAccountId,
            Address = "old address",
            EmergencyContact = "old contact",
            Notes = "old notes"
        };
        var user = new UserAccount
        {
            Id = userAccountId,
            FullName = "Old Name",
            Email = "old@example.com",
            Phone = "0900000000"
        };
        _candidateRepo.SetupGetById(candidateId, candidate);
        _userRepo.SetupGetById(userAccountId, user);
        _userRepo.SetupFind(Array.Empty<UserAccount>());

        await CreateHandler().Handle(
            Cmd(
                candidateId: candidateId,
                email: user.Email,
                phone: user.Phone,
                address: "   ",
                emergencyContact: "",
                notes: null),
            default);

        candidate.Address.Should().BeNull();
        candidate.EmergencyContact.Should().BeNull();
        candidate.Notes.Should().BeNull();
    }

    [Fact]
    public async Task Does_not_check_email_uniqueness_when_email_unchanged_case_insensitive()
    {
        var candidateId = Guid.NewGuid();
        var userAccountId = Guid.NewGuid();
        var candidate = new Candidate { Id = candidateId, UserAccountId = userAccountId };
        var user = new UserAccount
        {
            Id = userAccountId,
            FullName = "John",
            Email = "John@Example.com",
            Phone = "0900000000"
        };
        _candidateRepo.SetupGetById(candidateId, candidate);
        _userRepo.SetupGetById(userAccountId, user);
        _userRepo.SetupFind(Array.Empty<UserAccount>());

        await CreateHandler().Handle(
            Cmd(candidateId: candidateId, email: "john@example.com", phone: user.Phone),
            default);

        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
