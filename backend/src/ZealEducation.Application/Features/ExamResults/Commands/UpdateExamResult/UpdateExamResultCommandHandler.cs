using MediatR;
using ZealEducation.Application.Common.Exceptions;
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

        var examination = await examinationRepository.GetByIdAsync(result.ExamId, cancellationToken)
            ?? throw new NotFoundException(nameof(Examination), result.ExamId);

        if (request.Score > examination.MaxScore)
            throw new ConflictException($"Score must not exceed the exam's max score ({examination.MaxScore}).");

        result.Score = request.Score;
        result.Grade = string.IsNullOrWhiteSpace(request.Grade) ? null : request.Grade.Trim();
        result.IsPassed = request.Score >= examination.PassScore;

        examResultRepository.Update(result);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
