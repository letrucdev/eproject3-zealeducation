using MediatR;
using Microsoft.EntityFrameworkCore;
using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.Examinations.Queries.GetBatchGradeDistribution;

public class GetBatchGradeDistributionQueryHandler(
    IRepository<Batch> batchRepository,
    IRepository<Examination> examinationRepository,
    IRepository<ExamResult> examResultRepository) : IRequestHandler<GetBatchGradeDistributionQuery, BatchGradeDistributionDto>
{
    public async Task<BatchGradeDistributionDto> Handle(
        GetBatchGradeDistributionQuery request,
        CancellationToken cancellationToken)
    {
        var batchExists = await batchRepository.ExistsAsync(request.BatchId, cancellationToken);
        if (!batchExists)
            throw new NotFoundException(nameof(Batch), request.BatchId);

        var grades = await (from result in examResultRepository.Query()
                            join exam in examinationRepository.Query() on result.ExamId equals exam.Id
                            where exam.BatchId == request.BatchId
                            select result.Grade)
            .ToListAsync(cancellationToken);

        var dto = new BatchGradeDistributionDto { Total = grades.Count };

        foreach (var grade in grades)
        {
            switch (grade)
            {
                case "A": dto.A++; break;
                case "B": dto.B++; break;
                case "C": dto.C++; break;
                case "D": dto.D++; break;
                case "F": dto.F++; break;
                default: dto.Ungraded++; break;
            }
        }

        return dto;
    }
}
