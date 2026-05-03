using MediatR;
using Microsoft.EntityFrameworkCore;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.Payments.Queries.GetFinancialReport;

public class GetFinancialReportQueryHandler(
    IRepository<PaymentTransaction> transactionRepository,
    IRepository<FeeStructure> feeRepository,
    IRepository<Enrollment> enrollmentRepository,
    IRepository<Course> courseRepository) : IRequestHandler<GetFinancialReportQuery, FinancialReportDto>
{
    public async Task<FinancialReportDto> Handle(GetFinancialReportQuery request, CancellationToken cancellationToken)
    {
        var fromDate = DateOnly.FromDateTime(request.From.Date);
        var toDate = DateOnly.FromDateTime(request.To.Date);
        var fromDateTime = fromDate.ToDateTime(TimeOnly.MinValue);
        var toDateTimeExclusive = toDate.AddDays(1).ToDateTime(TimeOnly.MinValue);

        // 1. PaymentTransactions trong khoang thoi gian + JOIN sang FeeStructure de biet FeeType + JOIN sang Enrollment/Course
        var txQuery =
            from tx in transactionRepository.Query()
            where tx.PaymentDate >= fromDateTime && tx.PaymentDate < toDateTimeExclusive
            join fee in feeRepository.Query() on tx.FeeId equals fee.Id
            join enrollment in enrollmentRepository.Query() on fee.Id equals enrollment.FeeId into enrollmentsByFee
            from enrollment in enrollmentsByFee.DefaultIfEmpty()
            join course in courseRepository.Query() on enrollment.CourseId equals course.Id into courses
            from course in courses.DefaultIfEmpty()
            select new
            {
                tx.Amount,
                tx.PaymentDate,
                FeeType = fee.FeeType,
                CourseId = course != null ? (Guid?)course.Id : null,
                CourseTitle = course != null ? course.CourseName : null,
            };

        var txRows = await txQuery.ToListAsync(cancellationToken);

        // 2. Tat ca FeeStructure (snapshot hien tai) — phuc vu Outstanding (lifetime)
        var feeSnapshot = await feeRepository.Query()
            .Select(f => new
            {
                f.OutstandingBalance,
            })
            .ToListAsync(cancellationToken);

        // 3. FeeStructure trong khoang thoi gian — phuc vu Payment Status Distribution
        var feesInRange = await feeRepository.Query()
            .Where(f => f.CreatedAt >= fromDateTime && f.CreatedAt < toDateTimeExclusive)
            .Select(f => new
            {
                f.PaymentStatus,
                f.OutstandingBalance,
            })
            .ToListAsync(cancellationToken);

        // 4. Monthly profit / Yearly income / Yearly transaction count — luon dua tren thang/nam hien tai (UTC), khong phu thuoc range filter
        var nowUtc = DateTime.UtcNow;
        var monthStart = new DateTime(nowUtc.Year, nowUtc.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var monthEndExclusive = monthStart.AddMonths(1);
        var yearStart = new DateTime(nowUtc.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var yearEndExclusive = yearStart.AddYears(1);

        var monthlyProfit = await transactionRepository.Query()
            .Where(t => t.PaymentDate >= monthStart && t.PaymentDate < monthEndExclusive)
            .SumAsync(t => (decimal?)t.Amount, cancellationToken) ?? 0m;

        var yearlyTxQuery = transactionRepository.Query()
            .Where(t => t.PaymentDate >= yearStart && t.PaymentDate < yearEndExclusive);

        var yearlyIncome = await yearlyTxQuery.SumAsync(t => (decimal?)t.Amount, cancellationToken) ?? 0m;
        var transactionCount = await yearlyTxQuery.CountAsync(cancellationToken);

        // KPI
        var outstanding = feeSnapshot.Sum(f => f.OutstandingBalance);

        // Revenue trend — group theo ngay, fill 0 cho ngay trong
        var trend = new List<RevenueTrendPointDto>();
        var groupedByDay = txRows
            .GroupBy(r => DateOnly.FromDateTime(r.PaymentDate))
            .ToDictionary(g => g.Key, g => g.ToList());

        var totalDays = (toDate.DayNumber - fromDate.DayNumber) + 1;
        for (var i = 0; i < totalDays; i++)
        {
            var day = fromDate.AddDays(i);
            if (groupedByDay.TryGetValue(day, out var bucket))
            {
                trend.Add(new RevenueTrendPointDto
                {
                    Date = day,
                    Amount = bucket.Sum(b => b.Amount),
                    Count = bucket.Count,
                });
            }
            else
            {
                trend.Add(new RevenueTrendPointDto { Date = day });
            }
        }

        // Payment status distribution (FeeStructure tao trong khoang thoi gian)
        var statusBuckets = Enum.GetValues<PaymentStatus>()
            .Select(s => new PaymentStatusBucketDto
            {
                Status = s,
                Count = feesInRange.Count(f => f.PaymentStatus == s),
                OutstandingAmount = feesInRange.Where(f => f.PaymentStatus == s).Sum(f => f.OutstandingBalance),
            })
            .ToList();

        // Revenue by FeeType (theo transactions trong range)
        var feeTypeBuckets = Enum.GetValues<FeeType>()
            .Select(t => new FeeTypeRevenueDto
            {
                FeeType = t,
                Amount = txRows.Where(r => r.FeeType == t).Sum(r => r.Amount),
                Count = txRows.Count(r => r.FeeType == t),
            })
            .ToList();

        // Top 5 courses theo revenue trong range
        var topCourses = txRows
            .Where(r => r.CourseId.HasValue)
            .GroupBy(r => new { r.CourseId, r.CourseTitle })
            .Select(g => new CourseRevenueDto
            {
                CourseId = g.Key.CourseId!.Value,
                CourseTitle = g.Key.CourseTitle ?? string.Empty,
                Amount = g.Sum(r => r.Amount),
                TransactionCount = g.Count(),
            })
            .OrderByDescending(c => c.Amount)
            .Take(5)
            .ToList();

        return new FinancialReportDto
        {
            From = fromDateTime,
            To = toDate.ToDateTime(TimeOnly.MaxValue),
            MonthlyProfit = monthlyProfit,
            YearlyIncome = yearlyIncome,
            Outstanding = outstanding,
            TransactionCount = transactionCount,
            RevenueTrend = trend,
            PaymentStatusDistribution = statusBuckets,
            RevenueByFeeType = feeTypeBuckets,
            TopCoursesByRevenue = topCourses,
        };
    }
}
