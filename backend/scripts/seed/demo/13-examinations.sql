/*
 * 13-examinations.sql — examinations across the seeded batches.
 *
 *   ACCP-2026-DEMO  8 exams (-88..-3) — feeds the per-batch scores trend chart.
 *   Other batches   1-2 each — keeps grade-distribution charts non-empty.
 *
 * Idempotent on Id. Requires batches + staff (Faculty).
 */

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRAN;

DECLARE @Now    DATETIME2(7)  = SYSUTCDATETIME();
DECLARE @Today  DATE          = CAST(SYSUTCDATETIME() AS DATE);
DECLARE @Seeder NVARCHAR(100) = N'seed-demo-data';

DECLARE @DemoBatch        UNIQUEIDENTIFIER = 'eeeeeeee-0000-0000-0000-000000000003';
DECLARE @DemoFacultyStaff UNIQUEIDENTIFIER = 'aaaaaaaa-0000-0000-0000-000000000021';

DECLARE @Examinations TABLE (
    Id            UNIQUEIDENTIFIER,
    BatchId       UNIQUEIDENTIFIER,
    ExamName      NVARCHAR(100),
    ExamDate      DATE,
    Location      NVARCHAR(100),
    MaxScore      INT,
    PassScore     INT,
    ScheduledById UNIQUEIDENTIFIER
);

INSERT INTO @Examinations VALUES
-- ACCP-DEMO mountain-shape (avg ~55 -> 87 peak -> 61)
('f2f2f2f2-0000-0000-0000-000000000001', @DemoBatch, N'Module 1 - Fundamentals',  DATEADD(day,-88,@Today), N'Lab A', 100, 50, @DemoFacultyStaff),
('f2f2f2f2-0000-0000-0000-000000000002', @DemoBatch, N'Module 2 - C/C++',         DATEADD(day,-75,@Today), N'Lab A', 100, 50, @DemoFacultyStaff),
('f2f2f2f2-0000-0000-0000-000000000003', @DemoBatch, N'Module 3 - OOP & Java',    DATEADD(day,-60,@Today), N'Lab A', 100, 50, @DemoFacultyStaff),
('f2f2f2f2-0000-0000-0000-000000000004', @DemoBatch, N'Module 4 - Database',      DATEADD(day,-45,@Today), N'Lab A', 100, 50, @DemoFacultyStaff),
('f2f2f2f2-0000-0000-0000-000000000005', @DemoBatch, N'Module 5 - Web Front-end', DATEADD(day,-30,@Today), N'Lab A', 100, 50, @DemoFacultyStaff),
('f2f2f2f2-0000-0000-0000-000000000006', @DemoBatch, N'Module 6 - Web Back-end',  DATEADD(day,-20,@Today), N'Lab A', 100, 50, @DemoFacultyStaff),
('f2f2f2f2-0000-0000-0000-000000000007', @DemoBatch, N'Module 7 - .NET Framework',DATEADD(day,-10,@Today), N'Lab A', 100, 50, @DemoFacultyStaff),
('f2f2f2f2-0000-0000-0000-000000000008', @DemoBatch, N'Module 8 - Mobile App',    DATEADD(day, -3,@Today), N'Lab A', 100, 50, @DemoFacultyStaff),
-- Other batches — light coverage
('f2f2f2f2-0000-0000-0000-000000000010','eeeeeeee-0000-0000-0000-000000000001', N'CPISM Mid-term', DATEADD(day,-50,@Today), N'Lab B', 100, 50, 'aaaaaaaa-0000-0000-0000-000000000023'),
('f2f2f2f2-0000-0000-0000-000000000011','eeeeeeee-0000-0000-0000-000000000001', N'CPISM Module 2', DATEADD(day,-15,@Today), N'Lab B', 100, 50, 'aaaaaaaa-0000-0000-0000-000000000023'),
('f2f2f2f2-0000-0000-0000-000000000012','eeeeeeee-0000-0000-0000-000000000002', N'WEBPRO HTML/CSS',DATEADD(day,-40,@Today), N'Lab C', 100, 50, 'aaaaaaaa-0000-0000-0000-000000000023'),
('f2f2f2f2-0000-0000-0000-000000000013','eeeeeeee-0000-0000-0000-000000000002', N'WEBPRO React',   DATEADD(day,-12,@Today), N'Lab C', 100, 50, 'aaaaaaaa-0000-0000-0000-000000000023'),
('f2f2f2f2-0000-0000-0000-000000000014','eeeeeeee-0000-0000-0000-000000000004', N'ADIM Photoshop', DATEADD(day,-22,@Today), N'Lab D', 100, 50, 'aaaaaaaa-0000-0000-0000-000000000023'),
('f2f2f2f2-0000-0000-0000-000000000015','eeeeeeee-0000-0000-0000-000000000006', N'HDSE Algorithms',DATEADD(day,-25,@Today), N'Lab A', 100, 50, @DemoFacultyStaff),
('f2f2f2f2-0000-0000-0000-000000000016','eeeeeeee-0000-0000-0000-000000000007', N'ACCP-02 Mod 1',  DATEADD(day,-15,@Today), N'Lab A', 100, 50, 'aaaaaaaa-0000-0000-0000-000000000022');

INSERT INTO examination (Id, BatchId, ExamName, ExamDate, Location, MaxScore, PassScore,
                         ScheduledById, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy)
SELECT e.Id, e.BatchId, e.ExamName, e.ExamDate, e.Location, e.MaxScore, e.PassScore,
       e.ScheduledById, @Now, @Seeder, NULL, NULL
FROM @Examinations e
WHERE NOT EXISTS (SELECT 1 FROM examination x WHERE x.Id = e.Id);

COMMIT TRAN;
PRINT '13-examinations.sql: 14 examination rows ensured.';
