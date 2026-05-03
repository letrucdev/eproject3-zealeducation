using MediatR;
using ZealEducation.Application.Common.Models;
using ZealEducation.Domain.Enums;

namespace ZealEducation.Application.Features.Payments.Queries.GetFinancialTransactions;

public record GetFinancialTransactionsQuery(
    DateTime From,
    DateTime To,
    string? Search = null,
    FeeType? FeeType = null,
    PaymentMethod? Method = null,
    int Page = 1,
    int PageSize = 10,
    string? SortBy = null,
    string? SortDirection = null) : IRequest<PaginatedList<FinancialTransactionListItemDto>>;
