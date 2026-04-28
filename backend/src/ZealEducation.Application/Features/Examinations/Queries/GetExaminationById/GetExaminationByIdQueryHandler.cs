using MediatR;
using Microsoft.EntityFrameworkCore;
using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Features.Examinations.Queries.GetBatchExaminations;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.Examinations.Queries.GetExaminationById;

public class GetExaminationByIdQueryHandler(
    IRepository<Examination> examinationRepository) : IRequestHandler<GetExaminationByIdQuery, ExaminationDto>
{
    public async Task<ExaminationDto> Handle(GetExaminationByIdQuery request, CancellationToken cancellationToken)
    {
        var dto = await examinationRepository.Query()
            .Where(e => e.Id == request.ExaminationId)
            .Select(e => new ExaminationDto
            {
                ExaminationId = e.Id,
                BatchId = e.BatchId,
                ExamName = e.ExamName,
                ExamDate = e.ExamDate,
                Location = e.Location,
                MaxScore = e.MaxScore,
                PassScore = e.PassScore,
                ScheduledById = e.ScheduledById,
                ScheduledByName = e.ScheduledBy.UserAccount.FullName,
                HasResults = e.ExamResults.Any(),
                CreatedAt = e.CreatedAt,
            })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(nameof(Examination), request.ExaminationId);

        return dto;
    }
}
