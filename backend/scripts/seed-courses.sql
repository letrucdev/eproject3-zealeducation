/*
 * Seed course catalog.
 *
 * Mirrors the logic in CreateCourseCommandHandler (Features/Courses/Commands/CreateCourse):
 *   - CourseName is unique
 *   - BaseFee >= 0
 *   - DurationWeeks > 0
 *
 * Target DB: SQL Server (matches Infrastructure migrations / EF config).
 * Idempotent: skips any row whose CourseName already exists.
 */

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRAN;

DECLARE @Now DATETIME2 = SYSUTCDATETIME();
DECLARE @CreatedBy NVARCHAR(100) = N'seed-script';

DECLARE @CourseSeed TABLE (
    CourseId        UNIQUEIDENTIFIER,
    CourseName      NVARCHAR(100),
    Description     NVARCHAR(MAX),
    DurationWeeks   INT,
    BaseFee         DECIMAL(12,2),
    IsActive        BIT
);

INSERT INTO @CourseSeed VALUES
('44444444-4444-4444-4444-000000000001', N'Full-Stack Web Development',   N'Học HTML/CSS/JS, React, Node.js, database và DevOps cơ bản. Kết thúc với capstone project triển khai cloud.',       24,  15000000.00, 1),
('44444444-4444-4444-4444-000000000002', N'Data Science Fundamentals',    N'Nhập môn Python, pandas, numpy, trực quan hoá dữ liệu và thống kê ứng dụng cho phân tích dữ liệu thực tế.',        20,  13500000.00, 1),
('44444444-4444-4444-4444-000000000003', N'Machine Learning Bootcamp',    N'Supervised/Unsupervised learning, scikit-learn, feature engineering, model evaluation, intro to deep learning.',   16,  18000000.00, 1),
('44444444-4444-4444-4444-000000000004', N'Cloud Engineering on Azure',   N'IaaS, PaaS, networking, identity, cost management và triển khai workload .NET lên Azure.',                        12,  12000000.00, 1),
('44444444-4444-4444-4444-000000000005', N'DevOps & CI/CD',               N'Linux, Docker, Kubernetes, GitHub Actions, quan sát hệ thống và triển khai liên tục.',                             14,  14000000.00, 1),
('44444444-4444-4444-4444-000000000006', N'Cybersecurity Essentials',     N'Nền tảng bảo mật, mạng, phân tích lỗ hổng, hardening hệ điều hành và ứng dụng web.',                               18,  16500000.00, 1),
('44444444-4444-4444-4444-000000000007', N'Mobile App Development',       N'Phát triển ứng dụng di động đa nền tảng với Flutter và React Native, từ UI đến đưa lên store.',                   20,  13000000.00, 1),
('44444444-4444-4444-4444-000000000008', N'UI/UX Design Foundations',     N'Thiết kế giao diện, trải nghiệm, nghiên cứu người dùng, Figma, design system cơ bản.',                             12,   9500000.00, 1),
('44444444-4444-4444-4444-000000000009', N'Business English for IT',      N'Tiếng Anh giao tiếp và viết email/technical documentation chuyên ngành công nghệ.',                                 8,   4500000.00, 1),
('44444444-4444-4444-4444-00000000000A', N'Project Management (PMP Prep)', N'Khung kiến thức PMBOK, kỹ năng điều phối dự án, ôn thi chứng chỉ PMP.',                                           10,   8000000.00, 1),
('44444444-4444-4444-4444-00000000000B', N'SQL Server Administration',    N'Quản trị SQL Server: backup/restore, tuning, security, high availability.',                                        10,   7500000.00, 1),
('44444444-4444-4444-4444-00000000000C', N'Accounting for Non-Finance',   N'Nguyên lý kế toán cơ bản dành cho người không chuyên, phù hợp khoá nền tảng chuyển nghề.',                         8,   5000000.00, 0);
-- Một khoá bị vô hiệu hoá để demo "is_active = false vẫn giữ lịch sử".

----------------------------------------------------------------------
-- Insert into course (skip rows that collide on CourseName unique key)
----------------------------------------------------------------------
INSERT INTO course (
    Id, CourseName, Description, DurationWeeks, BaseFee, IsActive,
    CreatedAt, CreatedBy, UpdatedAt, UpdatedBy
)
SELECT
    c.CourseId, c.CourseName, c.Description, c.DurationWeeks, c.BaseFee, c.IsActive,
    @Now, @CreatedBy, NULL, NULL
FROM @CourseSeed c
WHERE NOT EXISTS (
    SELECT 1 FROM course existing
    WHERE existing.CourseName = c.CourseName
);

COMMIT TRAN;

PRINT 'Seed complete. Courses inserted (or skipped if already present).';
