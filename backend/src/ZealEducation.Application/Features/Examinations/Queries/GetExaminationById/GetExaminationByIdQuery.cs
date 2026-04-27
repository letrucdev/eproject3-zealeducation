using MediatR;
using ZealEducation.Application.Features.Examinations.Queries.GetBatchExaminations;

namespace ZealEducation.Application.Features.Examinations.Queries.GetExaminationById;

public record GetExaminationByIdQuery(Guid ExaminationId) : IRequest<ExaminationDto>;
