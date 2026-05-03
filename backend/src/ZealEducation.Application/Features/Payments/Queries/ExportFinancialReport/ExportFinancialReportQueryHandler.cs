using MediatR;
using Microsoft.EntityFrameworkCore;
using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Application.Common.Models;
using ZealEducation.Application.Features.Payments.Queries.GetFinancialReport;
using ZealEducation.Application.Features.Payments.Queries.GetFinancialTransactions;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.Payments.Queries.ExportFinancialReport;

public class ExportFinancialReportQueryHandler(
    ISender sender,
    IRepository<PaymentTransaction> transactionRepository,
    IRepository<FeeStructure> feeRepository,
    IRepository<Candidate> candidateRepository,
    IRepository<UserAccount> userRepository,
    IRepository<Staff> staffRepository,
    IRepository<Enrollment> enrollmentRepository,
    IRepository<Course> courseRepository,
    ICurrentUser currentUser,
    IFinancialReportExcelGenerator excelGenerator) : IRequestHandler<ExportFinancialReportQuery, ExportFinancialReportResultDto>
{
    public async Task<ExportFinancialReportResultDto> Handle(ExportFinancialReportQuery request, CancellationToken cancellationToken)
    {
        // 1. Lay summary cho tung section theo range rieng (dedup neu trung range).
        // Card metrics (monthly profit, yearly income, outstanding, transaction count) khong phu thuoc range,
        // nen lay tu bat ky summary nao — dung trendSummary cho tien.
        var summaryCache = new Dictionary<(DateTime, DateTime), FinancialReportDto>();

        async Task<FinancialReportDto> GetSummary(DateRangeFilter range)
        {
            var key = (range.From, range.To);
            if (summaryCache.TryGetValue(key, out var existing)) return existing;
            var dto = await sender.Send(new GetFinancialReportQuery(range.From, range.To), cancellationToken);
            summaryCache[key] = dto;
            return dto;
        }

        var trendSummary = await GetSummary(request.RevenueTrend);
        var statusSummary = await GetSummary(request.PaymentStatus);
        var feeTypeSummary = await GetSummary(request.RevenueByFeeType);
        var topCoursesSummary = await GetSummary(request.TopCourses);

        // 2. Lay TAT CA transactions trong range cua bang transactions (khong phan trang)
        var transactions = await GetAllTransactionsAsync(request, cancellationToken);

        // 3. Lay ten staff hien tai (nguoi xuat bao cao)
        var generatedByName = await GetCurrentStaffNameAsync(cancellationToken);

        // 4. Build Excel
        var model = new FinancialReportExcelModel
        {
            GeneratedAt = DateTime.UtcNow,
            GeneratedByStaffName = generatedByName,
            StatsSummary = trendSummary,
            RevenueTrendRange = new ExcelDateRange(request.RevenueTrend.From, request.RevenueTrend.To),
            RevenueTrend = trendSummary.RevenueTrend,
            PaymentStatusRange = new ExcelDateRange(request.PaymentStatus.From, request.PaymentStatus.To),
            PaymentStatusDistribution = statusSummary.PaymentStatusDistribution,
            RevenueByFeeTypeRange = new ExcelDateRange(request.RevenueByFeeType.From, request.RevenueByFeeType.To),
            RevenueByFeeType = feeTypeSummary.RevenueByFeeType,
            TopCoursesRange = new ExcelDateRange(request.TopCourses.From, request.TopCourses.To),
            TopCoursesByRevenue = topCoursesSummary.TopCoursesByRevenue,
            TransactionsRange = new ExcelDateRange(request.Transactions.From, request.Transactions.To),
            Transactions = transactions,
        };

        var bytes = excelGenerator.Generate(model);

        var fileName = $"financial-report-{request.Transactions.From:yyyyMMdd}-{request.Transactions.To:yyyyMMdd}.xlsx";
        return new ExportFinancialReportResultDto
        {
            Content = bytes,
            FileName = fileName,
            ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        };
    }

    private async Task<List<FinancialTransactionListItemDto>> GetAllTransactionsAsync(
        ExportFinancialReportQuery request, CancellationToken cancellationToken)
    {
        var fromDate = DateOnly.FromDateTime(request.Transactions.From.Date);
        var toDate = DateOnly.FromDateTime(request.Transactions.To.Date);
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

        return await query
            .OrderByDescending(x => x.tx.PaymentDate)
            .Select(x => new FinancialTransactionListItemDto
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
            })
            .ToListAsync(cancellationToken);
    }

    private async Task<string> GetCurrentStaffNameAsync(CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId
            ?? throw new UnauthorizedException("Authenticated user is required.");

        var user = await userRepository.Query()
            .Where(u => u.Id == userId)
            .Select(u => u.FullName)
            .FirstOrDefaultAsync(cancellationToken);

        return user ?? "Unknown";
    }
}
