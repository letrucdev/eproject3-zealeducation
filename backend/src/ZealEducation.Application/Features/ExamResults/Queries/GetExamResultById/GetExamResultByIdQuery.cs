using MediatR;
using ZealEducation.Application.Features.ExamResults.Queries.GetExaminationResults;

namespace ZealEducation.Application.Features.ExamResults.Queries.GetExamResultById;

public record GetExamResultByIdQuery(Guid ResultId) : IRequest<ExamResultDto>;
