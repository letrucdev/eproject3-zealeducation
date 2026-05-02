using System.Security.Cryptography;
using MediatR;
using Microsoft.Extensions.Logging;
using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Application.Features.CourseEnquiries.Notifications;
using ZealEducation.Application.Features.Payments.Common;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.CourseEnquiries.Commands.ConvertEnquiry;

public class ConvertEnquiryCommandHandler(
    IRepository<CourseEnquiry> enquiryRepository,
    IRepository<UserAccount> userRepository,
    IRepository<Candidate> candidateRepository,
    IRepository<Course> courseRepository,
    IRepository<Enrollment> enrollmentRepository,
    IRepository<FeeStructure> feeStructureRepository,
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    IEnquiryConvertedNotificationService notificationService,
    ILogger<ConvertEnquiryCommandHandler> logger) : IRequestHandler<ConvertEnquiryCommand, ConvertEnquiryResponse>
{
    private const string UsernameAlphabet = "abcdefghijklmnopqrstuvwxyz0123456789";
    private const string PasswordAlphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%";

    public async Task<ConvertEnquiryResponse> Handle(ConvertEnquiryCommand request, CancellationToken cancellationToken)
    {
        var enquiry = await enquiryRepository.GetByIdAsync(request.EnquiryId, cancellationToken)
            ?? throw new NotFoundException(nameof(CourseEnquiry), request.EnquiryId);

        if (enquiry.Status == EnquiryStatus.Converted)
            throw new ConflictException("This enquiry has already been converted.");

        if (enquiry.Status == EnquiryStatus.Closed)
            throw new ConflictException("This enquiry is closed.");

        var course = await courseRepository.GetByIdAsync(enquiry.CourseInterestedId, cancellationToken)
            ?? throw new NotFoundException(nameof(Course), enquiry.CourseInterestedId);

        var email = request.Email.Trim();
        var phone = enquiry.Phone.Trim();

        var emailDuplicates = await userRepository.FindAsync(u => u.Email == email, cancellationToken);
        if (emailDuplicates.Count > 0)
            throw new ConflictException("Email is already associated with another account.");

        var phoneDuplicates = await userRepository.FindAsync(u => u.Phone == phone, cancellationToken);
        if (phoneDuplicates.Count > 0)
            throw new ConflictException("Phone number is already associated with another account.");

        var username = await GenerateUniqueUsernameAsync(phone, cancellationToken);
        var tempPassword = GenerateRandomString(12, PasswordAlphabet);
        var candidateCode = await GenerateCandidateCodeAsync(cancellationToken);

        var userAccount = new UserAccount
        {
            Id = Guid.NewGuid(),
            Username = username,
            PasswordHash = passwordHasher.Hash(tempPassword),
            FullName = enquiry.FullName,
            Email = email,
            Phone = phone,
            Dob = request.Dob,
            Gender = request.Gender,
            Role = UserRole.Candidate,
            IsActive = false,
            MustChangePassword = true,
            FailedLoginCount = 0
        };

        var candidate = new Candidate
        {
            Id = Guid.NewGuid(),
            UserAccountId = userAccount.Id,
            CandidateCode = candidateCode,
            Address = string.IsNullOrWhiteSpace(request.Address) ? null : request.Address.Trim(),
            EmergencyContact = string.IsNullOrWhiteSpace(request.EmergencyContact) ? null : request.EmergencyContact.Trim(),
            Status = CandidateStatus.Active,
            RegisteredAt = DateTime.UtcNow,
            RegisteredByStaffId = enquiry.AssignedCounselorId
        };

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
            FeeId = feeStructure.Id,
            CourseId = course.Id,
            EnrollmentDate = DateOnly.FromDateTime(DateTime.UtcNow),
            Status = EnrollmentStatus.PendingAssignment,
            Notes = "New student registration"
        };

        enquiry.Email = email;
        enquiry.Status = EnquiryStatus.Converted;
        enquiry.ConvertedCandidateId = candidate.Id;
        enquiry.ConvertedAt = DateTime.UtcNow;

        await userRepository.AddAsync(userAccount, cancellationToken);
        await candidateRepository.AddAsync(candidate, cancellationToken);
        await enrollmentRepository.AddAsync(enrollment, cancellationToken);
        await feeStructureRepository.AddAsync(feeStructure, cancellationToken);
        enquiryRepository.Update(enquiry);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await TryQueueConvertedEmailAsync(enquiry, course, userAccount, candidate, tempPassword, enrollment.EnrollmentDate, cancellationToken);

        return new ConvertEnquiryResponse
        {
            CandidateId = candidate.Id,
            UserAccountId = userAccount.Id,
            CandidateCode = candidate.CandidateCode,
            Username = username,
            TemporaryPassword = tempPassword,
            Email = email
        };
    }

    private async Task TryQueueConvertedEmailAsync(
        CourseEnquiry enquiry,
        Course course,
        UserAccount userAccount,
        Candidate candidate,
        string tempPassword,
        DateOnly enrollmentDate,
        CancellationToken cancellationToken)
    {
        try
        {
            var installmentOptions = new List<EnquiryConvertedInstallmentOption>();
            foreach (var frequency in Enum.GetValues<InstallmentFrequency>())
            {
                if (!InstallmentPlanCalculator.IsFrequencyAllowed(frequency, course.DurationWeeks))
                    continue;

                var items = InstallmentPlanCalculator.Build(course.BaseFee, course.DurationWeeks, enrollmentDate, frequency);
                installmentOptions.Add(new EnquiryConvertedInstallmentOption
                {
                    Frequency = frequency,
                    Items = items
                });
            }

            var model = new EnquiryConvertedEmailModel
            {
                RecipientEmail = userAccount.Email,
                RecipientName = enquiry.FullName,
                Username = userAccount.Username,
                TemporaryPassword = tempPassword,
                CandidateCode = candidate.CandidateCode,
                ConvertedAt = enquiry.ConvertedAt ?? DateTime.UtcNow,
                CourseName = course.CourseName,
                DurationWeeks = course.DurationWeeks,
                BaseFee = course.BaseFee,
                LumpSumAmount = course.BaseFee,
                InstallmentOptions = installmentOptions
            };

            await notificationService.QueueAsync(model, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to queue enquiry-converted email for candidate {CandidateId}", candidate.Id);
        }
    }

    private async Task<string> GenerateUniqueUsernameAsync(string phone, CancellationToken cancellationToken)
    {
        var digits = new string(phone.Where(char.IsDigit).ToArray());
        var suffix = digits.Length >= 4 ? digits[^4..] : digits.PadLeft(4, '0');
        var baseName = $"cand_{suffix}";

        for (var attempt = 0; attempt < 5; attempt++)
        {
            var candidateName = attempt == 0 ? baseName : $"{baseName}_{GenerateRandomString(3, UsernameAlphabet)}";
            var existing = await userRepository.FindAsync(u => u.Username == candidateName, cancellationToken);
            if (existing.Count == 0) return candidateName;
        }

        throw new ConflictException("Could not generate a unique username; please try again.");
    }

    private async Task<string> GenerateCandidateCodeAsync(CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow;
        var prefix = $"C{today:yyyyMMdd}";

        for (var attempt = 0; attempt < 5; attempt++)
        {
            var seq = GenerateRandomString(4, "0123456789");
            var code = $"{prefix}{seq}";
            var existing = await candidateRepository.FindAsync(c => c.CandidateCode == code, cancellationToken);
            if (existing.Count == 0) return code;
        }

        throw new ConflictException("Could not generate a unique candidate code; please try again.");
    }

    private static string GenerateRandomString(int length, string alphabet)
    {
        var buffer = new char[length];
        for (var i = 0; i < length; i++)
        {
            var idx = RandomNumberGenerator.GetInt32(alphabet.Length);
            buffer[i] = alphabet[idx];
        }
        return new string(buffer);
    }
}
