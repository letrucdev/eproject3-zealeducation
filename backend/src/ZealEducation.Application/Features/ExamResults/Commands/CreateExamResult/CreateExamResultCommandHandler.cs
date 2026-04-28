using MediatR;
using Microsoft.EntityFrameworkCore;
using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.ExamResults.Commands.CreateExamResult;

public class CreateExamResultCommandHandler(
    IRepository<Examination> examinationRepository,
    IRepository<Enrollment> enrollmentRepository,
    IRepository<ExamResult> examResultRepository,
    IRepository<Staff> staffRepository,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateExamResultCommand, Guid>
{
    public async Task<Guid> Handle(CreateExamResultCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is null)
            throw new UnauthorizedException("Current user could not be resolved.");

        var examination = await examinationRepository.GetByIdAsync(request.ExaminationId, cancellationToken)
            ?? throw new NotFoundException(nameof(Examination), request.ExaminationId);

        if (request.Score > examination.MaxScore)
            throw new ConflictException($"Score must not exceed the exam's max score ({examination.MaxScore}).");

        var enrollment = await enrollmentRepository.GetByIdAsync(request.EnrollmentId, cancellationToken)
            ?? throw new NotFoundException(nameof(Enrollment), request.EnrollmentId);

        if (enrollment.BatchId != examination.BatchId)
            throw new ConflictException("The enrollment does not belong to the same batch as the examination.");

        var duplicate = await examResultRepository.Query()
            .AnyAsync(r => r.ExamId == examination.Id && r.EnrollmentId == enrollment.Id, cancellationToken);

        if (duplicate)
            throw new ConflictException("A result already exists for this candidate on this examination.");

        var staff = await staffRepository.Query()
            .FirstOrDefaultAsync(s => s.UserAccountId == currentUser.UserId.Value, cancellationToken)
            ?? throw new ConflictException("Current user is not registered as staff.");

        var result = new ExamResult
        {
            Id = Guid.NewGuid(),
            ExamId = examination.Id,
            EnrollmentId = enrollment.Id,
            Score = request.Score,
            Grade = string.IsNullOrWhiteSpace(request.Grade) ? null : request.Grade.Trim(),
            IsPassed = request.Score >= examination.PassScore,
            GradedById = staff.Id,
            IsOverridden = false,
            OverrideById = null,
            GradedAt = DateTime.UtcNow,
        };

        await examResultRepository.AddAsync(result, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return result.Id;
    }
}
