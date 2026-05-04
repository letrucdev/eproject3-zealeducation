using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Application.Common.Models;
using ZealEducation.Application.Common.Services;
using ZealEducation.Application.UnitTests.Helpers;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.UnitTests.Common.Services;

public class CertificateArchiverTests
{
    private readonly Mock<IRepository<CertificateApplication>> _appRepo = new();
    private readonly Mock<IRepository<Enrollment>> _enrollmentRepo = new();
    private readonly Mock<IRepository<Candidate>> _candidateRepo = new();
    private readonly Mock<IRepository<UserAccount>> _userRepo = new();
    private readonly Mock<IRepository<Course>> _courseRepo = new();
    private readonly Mock<ICertificatePdfGenerator> _pdfGen = new();
    private readonly Mock<IFileStorageService> _fileStorage = new();
    private readonly Mock<IUnitOfWork> _uow = MockRepositoryExtensions.CreateUnitOfWork();

    private CertificateArchiver CreateService() => new(
        _appRepo.Object,
        _enrollmentRepo.Object,
        _candidateRepo.Object,
        _userRepo.Object,
        _courseRepo.Object,
        _pdfGen.Object,
        _fileStorage.Object,
        _uow.Object);

    [Fact]
    public async Task Should_throw_NotFoundException_when_application_does_not_exist()
    {
        var appId = Guid.NewGuid();
        _appRepo.SetupGetById(appId, null);

        var act = async () => await CreateService().GenerateAndUploadAsync(appId, default);

        await act.Should().ThrowAsync<NotFoundException>();
        _pdfGen.Verify(p => p.Generate(It.IsAny<CertificatePdfModel>()), Times.Never);
    }

    [Fact]
    public async Task Should_return_cached_pdf_when_certificate_already_archived()
    {
        var appId = Guid.NewGuid();
        var cachedBytes = new byte[] { 1, 2, 3 };
        var existing = new CertificateApplication
        {
            Id = appId,
            EnrollmentId = Guid.NewGuid(),
            Status = CertificateApplicationStatus.Approved,
            CertificateNumber = "CERT-001",
            CertificateFilePath = "certificates/2030/05/CERT-001.pdf",
            ApprovedAt = DateTime.UtcNow
        };
        _appRepo.SetupGetById(appId, existing);
        _fileStorage.Setup(s => s.DownloadAsync(existing.CertificateFilePath, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedBytes);

        var bytes = await CreateService().GenerateAndUploadAsync(appId, default);

        bytes.Should().BeEquivalentTo(cachedBytes);
        _pdfGen.Verify(p => p.Generate(It.IsAny<CertificatePdfModel>()), Times.Never);
        _fileStorage.Verify(s => s.UploadAsync(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Should_throw_when_application_not_in_approved_state()
    {
        var appId = Guid.NewGuid();
        _appRepo.SetupGetById(appId, new CertificateApplication
        {
            Id = appId,
            EnrollmentId = Guid.NewGuid(),
            Status = CertificateApplicationStatus.Pending,
            CertificateNumber = null,
            ApprovedAt = null
        });

        var act = async () => await CreateService().GenerateAndUploadAsync(appId, default);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not in an approved state*");
    }

    [Fact]
    public async Task Should_force_regenerate_when_flag_set_even_if_cached()
    {
        var appId = Guid.NewGuid();
        var application = new CertificateApplication
        {
            Id = appId,
            EnrollmentId = Guid.NewGuid(),
            Status = CertificateApplicationStatus.Approved,
            CertificateNumber = "CERT-001",
            CertificateFilePath = "certificates/2030/05/CERT-001.pdf",
            ApprovedAt = new DateTime(2030, 5, 1)
        };
        _appRepo.SetupGetById(appId, application);

        var enrollment = new Enrollment
        {
            Id = application.EnrollmentId,
            CandidateId = Guid.NewGuid(),
            CourseId = Guid.NewGuid(),
            BatchId = Guid.NewGuid()
        };
        _enrollmentRepo.SetupGetById(enrollment.Id, enrollment);

        var candidate = new Candidate
        {
            Id = enrollment.CandidateId,
            UserAccountId = Guid.NewGuid(),
            CandidateCode = "C-001"
        };
        _candidateRepo.SetupGetById(candidate.Id, candidate);

        var user = new UserAccount
        {
            Id = candidate.UserAccountId,
            Username = "alice.doe",
            PasswordHash = "$hash$",
            FullName = "Alice Doe",
            Email = "alice@example.com",
            Phone = "0123456789",
            Dob = new DateOnly(1990, 1, 1),
            Gender = Gender.Female,
            Role = UserRole.Candidate
        };
        _userRepo.SetupGetById(user.Id, user);

        var course = new Course { Id = enrollment.CourseId, CourseName = "AI 101", BaseFee = 1000m };
        _courseRepo.SetupGetById(course.Id, course);

        var generated = new byte[] { 9, 9, 9 };
        _pdfGen.Setup(p => p.Generate(It.IsAny<CertificatePdfModel>())).Returns(generated);
        _fileStorage.Setup(s => s.UploadAsync(generated, It.IsAny<string>(), "application/pdf", It.IsAny<CancellationToken>()))
            .ReturnsAsync("uploaded-key");

        var bytes = await CreateService().GenerateAndUploadAsync(appId, default, forceRegenerate: true);

        bytes.Should().BeEquivalentTo(generated);
        _pdfGen.Verify(p => p.Generate(It.IsAny<CertificatePdfModel>()), Times.Once);
        _fileStorage.Verify(s => s.UploadAsync(generated, It.IsAny<string>(), "application/pdf", It.IsAny<CancellationToken>()), Times.Once);
        _fileStorage.Verify(s => s.DownloadAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
