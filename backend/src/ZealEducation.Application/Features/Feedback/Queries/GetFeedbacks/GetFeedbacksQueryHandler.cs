using MediatR;
using ZealEducation.Application.Common.Models;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.Feedback.Queries.GetFeedbacks;

public class GetFeedbacksQueryHandler(
    IRepository<Domain.Entities.Feedback> feedbackRepository,
    IRepository<Candidate> candidateRepository,
    IRepository<UserAccount> userRepository,
    IRepository<Batch> batchRepository,
    IRepository<Course> courseRepository,
    IRepository<Faculty> facultyRepository,
    IRepository<Staff> staffRepository) : IRequestHandler<GetFeedbacksQuery, PaginatedList<FeedbackListItemDto>>
{
    public async Task<PaginatedList<FeedbackListItemDto>> Handle(GetFeedbacksQuery request, CancellationToken cancellationToken)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 10 : Math.Min(request.PageSize, 100);

        // Join feedback -> candidate -> candidateUser -> batch -> course
        var query =
            from feedback in feedbackRepository.Query()
            join candidate in candidateRepository.Query() on feedback.CandidateId equals candidate.Id
            join candidateUser in userRepository.Query() on candidate.UserAccountId equals candidateUser.Id
            join batch in batchRepository.Query() on feedback.BatchId equals batch.Id
            join course in courseRepository.Query() on batch.CourseId equals course.Id
            select new
            {
                feedback,
                candidate,
                candidateUser,
                batch,
                course
            };

        if (request.IsProcessed.HasValue)
        {
            var isProcessed = request.IsProcessed.Value;
            query = query.Where(x => x.feedback.IsProcessed == isProcessed);
        }

        if (request.Type.HasValue)
        {
            var type = request.Type.Value;
            query = query.Where(x => x.feedback.Type == type);
        }

        if (request.BatchId.HasValue)
        {
            var batchId = request.BatchId.Value;
            query = query.Where(x => x.feedback.BatchId == batchId);
        }

        if (request.Rating.HasValue)
        {
            var rating = request.Rating.Value;
            query = query.Where(x => x.feedback.Rating == rating);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(x =>
                x.candidateUser.FullName.ToLower().Contains(search) ||
                x.candidate.CandidateCode.ToLower().Contains(search) ||
                (x.feedback.Comment != null && x.feedback.Comment.ToLower().Contains(search)));
        }

        var sortKey = (request.SortBy ?? string.Empty).Trim().ToLower();
        var direction = (request.SortDirection ?? "desc").Trim().ToLower();

        var ordered = (sortKey, direction) switch
        {
            ("rating", "asc") => query.OrderBy(x => x.feedback.Rating),
            ("rating", _) => query.OrderByDescending(x => x.feedback.Rating),
            ("type", "asc") => query.OrderBy(x => x.feedback.Type),
            ("type", _) => query.OrderByDescending(x => x.feedback.Type),
            ("isprocessed", "asc") => query.OrderBy(x => x.feedback.IsProcessed),
            ("isprocessed", _) => query.OrderByDescending(x => x.feedback.IsProcessed),
            ("createdat", "asc") => query.OrderBy(x => x.feedback.CreatedAt),
            _ => query.OrderByDescending(x => x.feedback.CreatedAt),
        };

        var projected = ordered
            .ThenByDescending(x => x.feedback.CreatedAt)
            .Select(x => new FeedbackListItemDto
            {
                FeedbackId = x.feedback.Id,
                CandidateId = x.candidate.Id,
                CandidateCode = x.candidate.CandidateCode,
                CandidateName = x.candidateUser.FullName,
                BatchId = x.batch.Id,
                BatchCode = x.batch.BatchCode,
                CourseName = x.course.CourseName,
                Type = x.feedback.Type,
                TargetFacultyId = x.feedback.TargetFacultyId,
                TargetFacultyName = x.feedback.TargetFacultyId == null
                    ? null
                    : (from faculty in facultyRepository.Query()
                       join staff in staffRepository.Query() on faculty.StaffId equals staff.Id
                       join facultyUser in userRepository.Query() on staff.UserAccountId equals facultyUser.Id
                       where faculty.Id == x.feedback.TargetFacultyId
                       select facultyUser.FullName).FirstOrDefault(),
                Rating = x.feedback.Rating,
                Comment = x.feedback.Comment,
                IsProcessed = x.feedback.IsProcessed,
                ProcessedById = x.feedback.ProcessedById,
                ProcessedByName = x.feedback.ProcessedById == null
                    ? null
                    : userRepository.Query()
                        .Where(u => u.Id == x.feedback.ProcessedById)
                        .Select(u => u.FullName)
                        .FirstOrDefault(),
                ProcessedAt = x.feedback.ProcessedAt,
                CreatedAt = x.feedback.CreatedAt,
                UpdatedAt = x.feedback.UpdatedAt,
            });

        return await PaginatedList<FeedbackListItemDto>.CreateAsync(projected, page, pageSize);
    }
}
