using MediatR;
using Microsoft.Extensions.Logging;
using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Application.Features.Candidates.Notifications;
using ZealEducation.Application.Features.Payments.Common;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.Candidates.Commands.AddEnrollment;

public class AddEnrollmentCommandHandler(
    IRepository<Candidate> candidateRepository,
    IRepository<UserAccount> userRepository,
    IRepository<Staff> staffRepository,
    IRepository<Course> courseRepository,
    IRepository<Enrollment> enrollmentRepository,
    IRepository<FeeStructure> feeStructureRepository,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork,
    ICourseAddedNotificationService notificationService,
    ILogger<AddEnrollmentCommandHandler> logger) : IRequestHandler<AddEnrollmentCommand, AddEnrollmentResponse>
{
    public async Task<AddEnrollmentResponse> Handle(AddEnrollmentCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is null)
            throw new UnauthorizedException("Current user could not be resolved.");

        var staffMatches = await staffRepository.FindAsync(
            s => s.UserAccountId == currentUser.UserId.Value,
            cancellationToken);
        if (staffMatches.Count == 0)
            throw new ConflictException("Current user is not registered as staff.");
        var staff = staffMatches[0];

        var candidate = await candidateRepository.GetByIdAsync(request.CandidateId, cancellationToken)
            ?? throw new NotFoundException(nameof(Candidate), request.CandidateId);

        var candidateUser = await userRepository.GetByIdAsync(candidate.UserAccountId, cancellationToken)
            ?? throw new NotFoundException(nameof(UserAccount), candidate.UserAccountId);

        if (!candidateUser.IsActive)
            throw new ConflictException("Cannot add enrollment for an inactive candidate account.");

        if (candidate.Status != CandidateStatus.Active)
            throw new ConflictException("Cannot add enrollment for a non-active candidate.");

        var course = await courseRepository.GetByIdAsync(request.CourseId, cancellationToken)
            ?? throw new NotFoundException(nameof(Course), request.CourseId);

        if (!course.IsActive)
            throw new ConflictException("Course is not active.");

        var existing = await enrollmentRepository.FindAsync(
            e => e.CandidateId == candidate.Id && e.CourseId == course.Id,
            cancellationToken);
        if (existing.Count > 0)
            throw new ConflictException("Candidate is already enrolled in this course.");

        var enrollmentDate = DateOnly.FromDateTime(DateTime.UtcNow);

        var feeStructure = new FeeStructure
        {
            Id = Guid.NewGuid(),
            CandidateId = candidate.Id,
            FeeType = FeeType.Tuition,
            TotalFee = course.BaseFee,
            AmountPaid = 0,
            OutstandingBalance = course.BaseFee,
            PaymentStatus = PaymentStatus.Unpaid,
            PaymentType = PaymentType.NotSet
        };

        var enrollment = new Enrollment
        {
            Id = Guid.NewGuid(),
            CandidateId = candidate.Id,
            CourseId = course.Id,
            FeeId = feeStructure.Id,
            InchargeId = staff.Id,
            EnrollmentDate = enrollmentDate,
            Status = EnrollmentStatus.PendingAssignment,
            Notes = "Course added by incharge"
        };

        await feeStructureRepository.AddAsync(feeStructure, cancellationToken);
        await enrollmentRepository.AddAsync(enrollment, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await TryQueueCourseAddedEmailAsync(candidate, candidateUser, course, enrollmentDate, cancellationToken);

        return new AddEnrollmentResponse
        {
            EnrollmentId = enrollment.Id,
            FeeId = feeStructure.Id,
            CourseName = course.CourseName,
            TotalFee = feeStructure.TotalFee
        };
    }

    private async Task TryQueueCourseAddedEmailAsync(
        Candidate candidate,
        UserAccount candidateUser,
        Course course,
        DateOnly enrollmentDate,
        CancellationToken cancellationToken)
    {
        try
        {
            var installmentOptions = new List<CourseAddedInstallmentOption>();
            foreach (var frequency in Enum.GetValues<InstallmentFrequency>())
            {
                if (!InstallmentPlanCalculator.IsFrequencyAllowed(frequency, course.DurationWeeks))
                    continue;

                var items = InstallmentPlanCalculator.Build(course.BaseFee, course.DurationWeeks, enrollmentDate, frequency);
                installmentOptions.Add(new CourseAddedInstallmentOption
                {
                    Frequency = frequency,
                    Items = items
                });
            }

            var model = new CourseAddedEmailModel
            {
                RecipientEmail = candidateUser.Email,
                RecipientName = candidateUser.FullName,
                CandidateCode = candidate.CandidateCode,
                CourseName = course.CourseName,
                DurationWeeks = course.DurationWeeks,
                BaseFee = course.BaseFee,
                EnrollmentDate = enrollmentDate,
                LumpSumAmount = course.BaseFee,
                InstallmentOptions = installmentOptions
            };

            await notificationService.QueueAsync(model, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to queue course-added email for candidate {CandidateId}", candidate.Id);
        }
    }
}
