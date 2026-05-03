using MediatR;
using ZealEducation.Application.Common.Models;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.Payments.Queries.GetFinancialTransactions;

public class GetFinancialTransactionsQueryHandler(
    IRepository<PaymentTransaction> transactionRepository,
    IRepository<FeeStructure> feeRepository,
    IRepository<Candidate> candidateRepository,
    IRepository<UserAccount> userRepository,
    IRepository<Staff> staffRepository,
    IRepository<Enrollment> enrollmentRepository,
    IRepository<Course> courseRepository) : IRequestHandler<GetFinancialTransactionsQuery, PaginatedList<FinancialTransactionListItemDto>>
{
    public async Task<PaginatedList<FinancialTransactionListItemDto>> Handle(GetFinancialTransactionsQuery request, CancellationToken cancellationToken)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 10 : Math.Min(request.PageSize, 100);

        var fromDate = DateOnly.FromDateTime(request.From.Date);
        var toDate = DateOnly.FromDateTime(request.To.Date);
        var fromDateTime = fromDate.ToDateTime(TimeOnly.MinValue);
        var toDateTimeExclusive = toDate.AddDays(1).ToDateTime(TimeOnly.MinValue);

        var query =
            from tx in transactionRepository.Query()
            where tx.PaymentDate >= fromDateTime && tx.PaymentDate < toDateTimeExclusive
            join fee in feeRepository.Query() on tx.FeeId equals fee.Id
            join candidate in candidateRepository.Query() on fee.CandidateId equals candidate.Id
            join user in userRepository.Query() on candidate.UserAccountId equals user.Id
            join staff in staffRepository.Query() on tx.ProcessedByStaffId equals staff.Id
            join staffUser in userRepository.Query() on staff.UserAccountId equals staffUser.Id
            join enrollment in enrollmentRepository.Query() on fee.Id equals enrollment.FeeId into enrollmentsByFee
            from enrollment in enrollmentsByFee.DefaultIfEmpty()
            join course in courseRepository.Query() on enrollment.CourseId equals course.Id into courses
            from course in courses.DefaultIfEmpty()
            select new
            {
                tx,
                fee,
                candidate,
                user,
                course,
                StaffName = staffUser.FullName,
            };

        if (request.FeeType.HasValue)
        {
            var feeType = request.FeeType.Value;
            query = query.Where(x => x.fee.FeeType == feeType);
        }

        if (request.Method.HasValue)
        {
            var method = request.Method.Value;
            query = query.Where(x => x.tx.PaymentMethod == method);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(x =>
                x.candidate.CandidateCode.ToLower().Contains(search) ||
                x.user.FullName.ToLower().Contains(search) ||
                x.tx.ReceiptNumber.ToLower().Contains(search) ||
                (x.course != null && x.course.CourseName.ToLower().Contains(search)));
        }

        var sortKey = (request.SortBy ?? string.Empty).Trim().ToLower();
        var direction = (request.SortDirection ?? "desc").Trim().ToLower();

        var ordered = (sortKey, direction) switch
        {
            ("amount", "asc") => query.OrderBy(x => x.tx.Amount),
            ("amount", _) => query.OrderByDescending(x => x.tx.Amount),
            ("paymentdate", "asc") => query.OrderBy(x => x.tx.PaymentDate),
            ("paymentdate", "desc") => query.OrderByDescending(x => x.tx.PaymentDate),
            ("receiptnumber", "asc") => query.OrderBy(x => x.tx.ReceiptNumber),
            ("receiptnumber", "desc") => query.OrderByDescending(x => x.tx.ReceiptNumber),
            ("candidatefullname", "asc") => query.OrderBy(x => x.user.FullName),
            ("candidatefullname", "desc") => query.OrderByDescending(x => x.user.FullName),
            _ => query.OrderByDescending(x => x.tx.PaymentDate),
        };

        var projected = ordered.Select(x => new FinancialTransactionListItemDto
        {
            TransactionId = x.tx.Id,
            FeeId = x.fee.Id,
            ReceiptNumber = x.tx.ReceiptNumber,
            PaymentDate = x.tx.PaymentDate,
            CandidateCode = x.candidate.CandidateCode,
            CandidateFullName = x.user.FullName,
            CourseTitle = x.course != null ? x.course.CourseName : null,
            FeeType = x.fee.FeeType,
            Amount = x.tx.Amount,
            PaymentMethod = x.tx.PaymentMethod,
            ProcessedByStaffName = x.StaffName,
        });

        return await PaginatedList<FinancialTransactionListItemDto>.CreateAsync(projected, page, pageSize);
    }
}
