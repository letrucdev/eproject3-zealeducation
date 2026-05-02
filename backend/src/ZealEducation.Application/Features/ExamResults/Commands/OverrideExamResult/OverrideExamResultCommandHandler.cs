using MediatR;
using Microsoft.EntityFrameworkCore;
using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Common.Helpers;
using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.ExamResults.Commands.OverrideExamResult;

public class OverrideExamResultCommandHandler(
    IRepository<ExamResult> examResultRepository,
    IRepository<Examination> examinationRepository,
    IRepository<Staff> staffRepository,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork) : IRequestHandler<OverrideExamResultCommand, Unit>
{
    public async Task<Unit> Handle(OverrideExamResultCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is null)
            throw new UnauthorizedException("Current user could not be resolved.");

        var result = await examResultRepository.GetByIdAsync(request.ResultId, cancellationToken)
            ?? throw new NotFoundException(nameof(ExamResult), request.ResultId);

        var examination = await examinationRepository.GetByIdAsync(result.ExamId, cancellationToken)
            ?? throw new NotFoundException(nameof(Examination), result.ExamId);

        if (request.Score > examination.MaxScore)
            throw new ConflictException($"Score must not exceed the exam's max score ({examination.MaxScore}).");

        var staff = await staffRepository.Query()
            .FirstOrDefaultAsync(s => s.UserAccountId == currentUser.UserId.Value, cancellationToken)
            ?? throw new ConflictException("Current user is not registered as staff.");


        result.Score = request.Score;
        result.Grade = ExamGradeCalculator.Calculate(request.Score, examination.MaxScore, examination.PassScore);
        result.IsPassed = request.Score >= examination.PassScore;
        result.IsFinalized = true;
        result.IsOverridden = true;
        result.OverrideById = staff.Id;
        result.OverrideReason = request.OverrideReason.Trim();

        examResultRepository.Update(result);
        await unitOfWork.SaveChangesAsync(cancellationToken);


        return Unit.Value;
    }
}
