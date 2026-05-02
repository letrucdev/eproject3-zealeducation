using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Application.Common.Models;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Common.Services;

public class CertificateArchiver(
    IRepository<CertificateApplication> applicationRepository,
    IRepository<Enrollment> enrollmentRepository,
    IRepository<Candidate> candidateRepository,
    IRepository<UserAccount> userRepository,
    IRepository<Course> courseRepository,
    ICertificatePdfGenerator pdfGenerator,
    IFileStorageService fileStorage,
    IUnitOfWork unitOfWork) : ICertificateArchiver
{
    public async Task<byte[]> GenerateAndUploadAsync(Guid applicationId, CancellationToken cancellationToken, bool forceRegenerate = false)
    {
        // Use GetByIdAsync (FindAsync) so we re-use the change-tracker's instance
        // when the caller (e.g. ApproveCertificateApplicationCommandHandler) already
        // loaded the entity. Query() uses AsNoTracking and would create a duplicate
        // instance that conflicts with the tracked one when Update() runs.
        var application = await applicationRepository.GetByIdAsync(applicationId, cancellationToken)
            ?? throw new NotFoundException(nameof(CertificateApplication), applicationId);

        if (!forceRegenerate && !string.IsNullOrEmpty(application.CertificateFilePath))
        {
            return await fileStorage.DownloadAsync(application.CertificateFilePath, cancellationToken);
        }

        if (string.IsNullOrEmpty(application.CertificateNumber) || application.ApprovedAt is null)
        {
            throw new InvalidOperationException(
                $"Certificate application {applicationId} is not in an approved state and cannot be rendered.");
        }

        var enrollment = await enrollmentRepository.GetByIdAsync(application.EnrollmentId, cancellationToken)
            ?? throw new NotFoundException(nameof(Enrollment), application.EnrollmentId);

        var candidate = await candidateRepository.GetByIdAsync(enrollment.CandidateId, cancellationToken)
            ?? throw new NotFoundException(nameof(Candidate), enrollment.CandidateId);

        var candidateUser = await userRepository.GetByIdAsync(candidate.UserAccountId, cancellationToken)
            ?? throw new NotFoundException(nameof(UserAccount), candidate.UserAccountId);

        var course = await courseRepository.GetByIdAsync(enrollment.CourseId, cancellationToken)
            ?? throw new NotFoundException(nameof(Course), enrollment.CourseId);

        var model = new CertificatePdfModel
        {
            CertificateNumber = application.CertificateNumber!,
            CandidateName = candidateUser.FullName,
            CourseName = course.CourseName,
            IssuedDate = application.ApprovedAt!.Value,
        };

        var bytes = pdfGenerator.Generate(model);

        var objectKey = $"certificates/{application.ApprovedAt:yyyy/MM}/{application.CertificateNumber}.pdf";
        await fileStorage.UploadAsync(bytes, objectKey, "application/pdf", cancellationToken);

        application.CertificateFilePath = objectKey;
        applicationRepository.Update(application);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return bytes;
    }
}
