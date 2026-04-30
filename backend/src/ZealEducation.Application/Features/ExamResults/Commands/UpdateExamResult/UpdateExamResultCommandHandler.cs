using MediatR;
using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Common.Helpers;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.ExamResults.Commands.UpdateExamResult;

public class UpdateExamResultCommandHandler(
    IRepository<ExamResult> examResultRepository,
    IRepository<Examination> examinationRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateExamResultCommand, Unit>
{
    public async Task<Unit> Handle(UpdateExamResultCommand request, CancellationToken cancellationToken)
    {
        var result = await examResultRepository.GetByIdAsync(request.ResultId, cancellationToken)
            ?? throw new NotFoundException(nameof(ExamResult), request.ResultId);

        if (result.IsOverridden)
            throw new ConflictException("This result has been overridden and can no longer be updated through this endpoint.");

        if (result.IsFinalized)
            throw new ConflictException("This result has been finalized and can no longer be updated. Ask Incharge for an override.");

        var examination = await examinationRepository.GetByIdAsync(result.ExamId, cancellationToken)
            ?? throw new NotFoundException(nameof(Examination), result.ExamId);

        if (request.Score > examination.MaxScore)
            throw new ConflictException($"Score must not exceed the exam's max score ({examination.MaxScore}).");

        result.Score = request.Score;
        result.Grade = ExamGradeCalculator.Calculate(request.Score, examination.MaxScore, examination.PassScore);
        result.IsPassed = request.Score >= examination.PassScore;
        result.IsFinalized = request.IsFinalized;

        examResultRepository.Update(result);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
