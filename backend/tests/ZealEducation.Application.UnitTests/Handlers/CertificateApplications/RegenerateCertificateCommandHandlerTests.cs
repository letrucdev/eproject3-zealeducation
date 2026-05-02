using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Application.Features.CertificateApplications.Commands.RegenerateCertificate;
using ZealEducation.Application.UnitTests.Helpers;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.UnitTests.Handlers.CertificateApplications;

public class RegenerateCertificateCommandHandlerTests
{
    private readonly Mock<IRepository<CertificateApplication>> _applicationRepo = new();
    private readonly Mock<ICertificateArchiver> _archiver = new();

    private RegenerateCertificateCommandHandler CreateHandler() =>
        new(_applicationRepo.Object, _archiver.Object);

    [Fact]
    public async Task Throws_NotFoundException_when_application_does_not_exist()
    {
        var applicationId = Guid.NewGuid();
        _applicationRepo.SetupGetById(applicationId, null);

        var act = async () => await CreateHandler().Handle(
            new RegenerateCertificateCommand(applicationId), default);

        await act.Should().ThrowAsync<NotFoundException>();
        _archiver.Verify(a => a.GenerateAndUploadAsync(
            It.IsAny<Guid>(), It.IsAny<CancellationToken>(), It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public async Task Throws_ConflictException_when_application_is_pending()
    {
        var applicationId = Guid.NewGuid();
        _applicationRepo.SetupGetById(applicationId, new CertificateApplication
        {
            Id = applicationId,
            EnrollmentId = Guid.NewGuid(),
            Status = CertificateApplicationStatus.Pending,
            CertificateNumber = null
        });

        var act = async () => await CreateHandler().Handle(
            new RegenerateCertificateCommand(applicationId), default);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("Certificate has not been approved yet.");
        _archiver.Verify(a => a.GenerateAndUploadAsync(
            It.IsAny<Guid>(), It.IsAny<CancellationToken>(), It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public async Task Throws_ConflictException_when_certificate_number_is_missing_even_if_approved()
    {
        var applicationId = Guid.NewGuid();
        _applicationRepo.SetupGetById(applicationId, new CertificateApplication
        {
            Id = applicationId,
            EnrollmentId = Guid.NewGuid(),
            Status = CertificateApplicationStatus.Approved,
            CertificateNumber = string.Empty
        });

        var act = async () => await CreateHandler().Handle(
            new RegenerateCertificateCommand(applicationId), default);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("Certificate has not been approved yet.");
    }

    [Fact]
    public async Task Regenerates_certificate_and_returns_existing_number_when_approved()
    {
        var applicationId = Guid.NewGuid();
        var application = new CertificateApplication
        {
            Id = applicationId,
            EnrollmentId = Guid.NewGuid(),
            Status = CertificateApplicationStatus.Approved,
            CertificateNumber = "CERT-20300101-123456"
        };
        _applicationRepo.SetupGetById(applicationId, application);
        _archiver.Setup(a => a.GenerateAndUploadAsync(
                It.IsAny<Guid>(), It.IsAny<CancellationToken>(), It.IsAny<bool>()))
            .ReturnsAsync(new byte[] { 9, 9, 9 });

        var response = await CreateHandler().Handle(
            new RegenerateCertificateCommand(applicationId), default);

        response.ApplicationId.Should().Be(applicationId);
        response.CertificateNumber.Should().Be("CERT-20300101-123456");
        _archiver.Verify(a => a.GenerateAndUploadAsync(
            applicationId, It.IsAny<CancellationToken>(), true), Times.Once);
    }
}
