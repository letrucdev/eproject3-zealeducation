/*
 * 17-feedbacks.sql — feedback rows from candidates about Faculty / Course /
 * General. Mix of processed / unprocessed so the incharge processing screen
 * has both queues.
 *
 * Idempotent on Id. Requires candidate + batch + faculty.
 */

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRAN;

DECLARE @Now    DATETIME2(7)  = SYSUTCDATETIME();
DECLARE @Seeder NVARCHAR(100) = N'seed-demo-data';

DECLARE @Feedbacks TABLE (
    Id              UNIQUEIDENTIFIER,
    CandidateId     UNIQUEIDENTIFIER,
    BatchId         UNIQUEIDENTIFIER,
    Type            NVARCHAR(20),
    TargetFacultyId UNIQUEIDENTIFIER NULL,
    Rating          INT,
    Comment         NVARCHAR(1000),
    IsProcessed     BIT,
    ProcessedById   UNIQUEIDENTIFIER NULL,
    ProcessedDays   INT NULL
);

INSERT INTO @Feedbacks VALUES
-- Faculty feedback (must have TargetFacultyId)
('a3a3a3a3-0000-0000-0000-000000000001','cccccccc-0000-0000-0000-000000000001','eeeeeeee-0000-0000-0000-000000000003', N'Faculty', 'bbbbbbbb-0000-0000-0000-000000000001',
   5, N'Thay giang day rat tan tinh, vi du de hieu.', 1, '11111111-0000-0000-0000-000000000001', -2),
('a3a3a3a3-0000-0000-0000-000000000002','cccccccc-0000-0000-0000-000000000002','eeeeeeee-0000-0000-0000-000000000003', N'Faculty', 'bbbbbbbb-0000-0000-0000-000000000001',
   4, N'Bai giang on, mong them lab thuc hanh.',     0, NULL, NULL),
-- Course feedback (no faculty target)
('a3a3a3a3-0000-0000-0000-000000000003','cccccccc-0000-0000-0000-000000000003','eeeeeeee-0000-0000-0000-000000000003', N'Course',  NULL,
   5, N'Noi dung khoa hoc bam sat thuc te.',          1, '11111111-0000-0000-0000-000000000001', -1),
('a3a3a3a3-0000-0000-0000-000000000004','cccccccc-0000-0000-0000-000000000009','eeeeeeee-0000-0000-0000-000000000001', N'Course',  NULL,
   3, N'Tien do hoi cham, can chia nho module.',      0, NULL, NULL),
-- General feedback
('a3a3a3a3-0000-0000-0000-000000000005','cccccccc-0000-0000-0000-000000000012','eeeeeeee-0000-0000-0000-000000000002', N'General', NULL,
   4, N'Co so vat chat sach se, mang wifi nhanh.',    1, '11111111-0000-0000-0000-000000000002', -3),
('a3a3a3a3-0000-0000-0000-000000000006','cccccccc-0000-0000-0000-000000000017','eeeeeeee-0000-0000-0000-000000000006', N'General', NULL,
   2, N'Cho gui xe hoi chat, nen mo rong.',           0, NULL, NULL);

INSERT INTO feedback (Id, CandidateId, BatchId, Type, TargetFacultyId, Rating, Comment,
                      IsProcessed, ProcessedById, ProcessedAt,
                      CreatedAt, CreatedBy, UpdatedAt, UpdatedBy)
SELECT f.Id, f.CandidateId, f.BatchId, f.Type, f.TargetFacultyId, f.Rating, f.Comment,
       f.IsProcessed, f.ProcessedById,
       CASE WHEN f.ProcessedDays IS NULL THEN NULL ELSE DATEADD(day, f.ProcessedDays, @Now) END,
       @Now, @Seeder, NULL, NULL
FROM @Feedbacks f
WHERE NOT EXISTS (SELECT 1 FROM feedback x WHERE x.Id = f.Id);

COMMIT TRAN;
PRINT '17-feedbacks.sql: 6 feedback rows ensured (3 processed, 3 pending).';
