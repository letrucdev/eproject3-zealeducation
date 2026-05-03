using MediatR;

namespace ZealEducation.Application.Features.Payments.Queries.GetFinancialReport;

public record GetFinancialReportQuery(
    DateTime From,
    DateTime To) : IRequest<FinancialReportDto>;
