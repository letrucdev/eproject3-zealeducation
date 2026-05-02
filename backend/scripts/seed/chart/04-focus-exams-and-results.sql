/*
 * chart/04-focus-exams-and-results.sql
 *
 * 30 examinations across the trailing 90 days for the focus batch
 * CHART-FOCUS-2026, plus 30 x 12 = 360 exam results.
 *
 *   - Per-exam AvgTarget oscillates so the daily-average line on the
 *     batch-exam-scores chart shows multiple peaks and valleys, not just
 *     one mountain.
 *   - Per-student offsets (ScoreOffset, -25..+19) come from chart/03 and span
 *     the A..F grade range so the grade distribution pie covers every grade.
 *   - Score = clamp(AvgTarget + StudentOffset + tiny noise, 0, 100). Noise is
 *     deterministic from (|DayOffset| * Slot) so re-runs produce the same data.
 *
 * Idempotent: NOT EXISTS on examination.Id and (exam_result.ExamId, EnrollmentId).
 * Requires chart/03 to have run.
 */

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRAN;

DECLARE @Now    DATETIME2(7)  = SYSUTCDATETIME();
DECLARE @Today  DATE          = CAST(SYSUTCDATETIME() AS DATE);
DECLARE @Seeder NVARCHAR(100) = N'seed-chart-data';

DECLARE @ChartBatch     UNIQUEIDENTIFIER = 'e1c1e1c1-9999-0000-0000-000000000001';
DECLARE @FacultyStaff1  UNIQUEIDENTIFIER = 'aaaaaaaa-0000-0000-0000-000000000021';

DECLARE @ChartExams TABLE (
    ExamId    UNIQUEIDENTIFIER PRIMARY KEY,
    DayOffset INT NOT NULL,
    ExamName  NVARCHAR(100) NOT NULL,
    AvgTarget DECIMAL(6,2) NOT NULL
);

INSERT INTO @ChartExams VALUES
('f4f4f4f4-0000-0000-0000-000000000001', -89, N'Module 01 - Diagnostic Quiz',  62),
('f4f4f4f4-0000-0000-0000-000000000002', -86, N'Module 01 - Lab Test',         55),
('f4f4f4f4-0000-0000-0000-000000000003', -82, N'Module 02 - Quiz',             70),
('f4f4f4f4-0000-0000-0000-000000000004', -78, N'Module 02 - Test',             58),
('f4f4f4f4-0000-0000-0000-000000000005', -74, N'Module 02 - Lab Test',         72),
('f4f4f4f4-0000-0000-0000-000000000006', -70, N'Module 03 - Quiz',             80),
('f4f4f4f4-0000-0000-0000-000000000007', -66, N'Module 03 - Lab',              65),
('f4f4f4f4-0000-0000-0000-000000000008', -62, N'Module 04 - Quiz',             78),
('f4f4f4f4-0000-0000-0000-000000000009', -58, N'Module 04 - Mid-term',         85),
('f4f4f4f4-0000-0000-0000-00000000000a', -54, N'Module 04 - Lab Test',         68),
('f4f4f4f4-0000-0000-0000-00000000000b', -50, N'Module 05 - Quiz',             75),
('f4f4f4f4-0000-0000-0000-00000000000c', -47, N'Module 05 - Test',             88),
('f4f4f4f4-0000-0000-0000-00000000000d', -44, N'Module 05 - Lab Test',         62),
('f4f4f4f4-0000-0000-0000-00000000000e', -40, N'Module 06 - Quiz',             77),
('f4f4f4f4-0000-0000-0000-00000000000f', -37, N'Module 06 - Lab',              70),
('f4f4f4f4-0000-0000-0000-000000000010', -33, N'Module 06 - Test',             83),
('f4f4f4f4-0000-0000-0000-000000000011', -29, N'Module 07 - Quiz',             60),
('f4f4f4f4-0000-0000-0000-000000000012', -26, N'Module 07 - Test',             72),
('f4f4f4f4-0000-0000-0000-000000000013', -22, N'Module 08 - Quiz',             85),
('f4f4f4f4-0000-0000-0000-000000000014', -19, N'Module 08 - Lab Test',         65),
('f4f4f4f4-0000-0000-0000-000000000015', -15, N'Module 09 - Quiz',             78),
('f4f4f4f4-0000-0000-0000-000000000016', -12, N'Module 09 - Test',             55),
('f4f4f4f4-0000-0000-0000-000000000017', -10, N'Module 09 - Lab Test',         70),
('f4f4f4f4-0000-0000-0000-000000000018',  -8, N'Module 10 - Quiz',             80),
('f4f4f4f4-0000-0000-0000-000000000019',  -6, N'Module 10 - Test',             62),
('f4f4f4f4-0000-0000-0000-00000000001a',  -5, N'Module 10 - Lab Test',         75),
('f4f4f4f4-0000-0000-0000-00000000001b',  -4, N'Final - Mock Theory',          68),
('f4f4f4f4-0000-0000-0000-00000000001c',  -3, N'Final - Practical',            82),
('f4f4f4f4-0000-0000-0000-00000000001d',  -2, N'Final - Theory',               58),
('f4f4f4f4-0000-0000-0000-00000000001e',  -1, N'Capstone - Review',            76);

INSERT INTO examination (Id, BatchId, ExamName, ExamDate, Location, MaxScore, PassScore,
                         ScheduledById, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy)
SELECT e.ExamId, @ChartBatch, e.ExamName, DATEADD(day, e.DayOffset, @Today),
       N'Lab F', 100, 50, @FacultyStaff1,
       DATEADD(day, e.DayOffset - 7, @Now), @Seeder, NULL, NULL
FROM @ChartExams e
WHERE NOT EXISTS (SELECT 1 FROM examination x WHERE x.Id = e.ExamId);

-- Pull ScoreOffset back out of candidate.Notes (set by chart/03), and slot
-- ordering from candidate code suffix CHART-CAND-NN.
DECLARE @ChartCandidates TABLE (
    Slot         INT,
    EnrollmentId UNIQUEIDENTIFIER,
    ScoreOffset  DECIMAL(6,2)
);

INSERT INTO @ChartCandidates (Slot, EnrollmentId, ScoreOffset)
SELECT
    CAST(SUBSTRING(c.CandidateCode, 12, 2) AS INT) AS Slot,
    e.Id AS EnrollmentId,
    CAST(REPLACE(c.Notes, N'ScoreOffset=', N'') AS DECIMAL(6,2)) AS ScoreOffset
FROM candidate c
INNER JOIN enrollment e ON e.CandidateId = c.Id AND e.BatchId = @ChartBatch
WHERE c.CandidateCode LIKE N'CHART-CAND-%'
  AND c.Notes LIKE N'ScoreOffset=%';

;WITH RawScores AS (
    SELECT
        e.ExamId,
        cc.EnrollmentId,
        e.AvgTarget
            + cc.ScoreOffset
            + CAST(((ABS(e.DayOffset) * cc.Slot) % 7) - 3 AS DECIMAL(6,2)) AS RawScore
    FROM @ChartExams e
    CROSS JOIN @ChartCandidates cc
),
ClampedScores AS (
    SELECT
        ExamId,
        EnrollmentId,
        CASE
            WHEN RawScore > 100 THEN CAST(100 AS DECIMAL(6,2))
            WHEN RawScore < 0   THEN CAST(0   AS DECIMAL(6,2))
            ELSE                     RawScore
        END AS Score
    FROM RawScores
)
INSERT INTO exam_result (Id, ExamId, EnrollmentId, Score, Grade, IsPassed, IsFinalized,
                         GradedById, IsOverridden, OverrideById, OverrideReason, GradedAt,
                         CreatedAt, CreatedBy, UpdatedAt, UpdatedBy)
SELECT
    NEWID(), cs.ExamId, cs.EnrollmentId, cs.Score,
    CASE WHEN cs.Score >= 85 THEN N'A'
         WHEN cs.Score >= 70 THEN N'B'
         WHEN cs.Score >= 55 THEN N'C'
         WHEN cs.Score >= 40 THEN N'D'
         ELSE                    N'F' END,
    CASE WHEN cs.Score >= 50 THEN 1 ELSE 0 END,
    1, @FacultyStaff1, 0, NULL, NULL, @Now,
    @Now, @Seeder, NULL, NULL
FROM ClampedScores cs
WHERE NOT EXISTS (
    SELECT 1 FROM exam_result x
    WHERE x.ExamId = cs.ExamId AND x.EnrollmentId = cs.EnrollmentId
);

COMMIT TRAN;
PRINT 'chart/04-focus-exams-and-results.sql complete: 30 examinations + ~360 exam_result rows.';
