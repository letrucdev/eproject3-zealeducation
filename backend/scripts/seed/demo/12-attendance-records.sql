/*
 * 12-attendance-records.sql — derive an attendance row per (Completed session,
 * active enrollment in that batch). Status varies deterministically so the
 * mix is roughly Present 70%, Late 20%, Absent / Excused 10%.
 *
 * Idempotent on (ClassSessionId, EnrollmentId). Requires class_session +
 * enrollment.
 */

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRAN;

DECLARE @Now    DATETIME2(7)  = SYSUTCDATETIME();
DECLARE @Seeder NVARCHAR(100) = N'seed-demo-data';

;WITH ActiveEnrollments AS (
    SELECT e.Id AS EnrollmentId, e.BatchId,
           ROW_NUMBER() OVER (PARTITION BY e.BatchId ORDER BY e.Id) AS StudentNo
    FROM enrollment e
    WHERE e.CreatedBy = @Seeder
      AND e.BatchId IS NOT NULL
      AND e.Status IN (N'Enrolled', N'Completed')
),
CompletedSessions AS (
    SELECT cs.Id AS SessionId, cs.BatchId, cs.SessionDate,
           ROW_NUMBER() OVER (PARTITION BY cs.BatchId ORDER BY cs.SessionDate) AS SessionNo
    FROM class_session cs
    WHERE cs.CreatedBy = @Seeder
      AND cs.Status = N'Completed'
)
INSERT INTO attendance_record (Id, ClassSessionId, EnrollmentId, Status, PracticalHours,
                               Remarks, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy)
SELECT NEWID(),
       s.SessionId,
       e.EnrollmentId,
       CASE (s.SessionNo + e.StudentNo) % 10
           WHEN 0 THEN N'Absent'
           WHEN 1 THEN N'Excused'
           WHEN 2 THEN N'Late'
           WHEN 3 THEN N'Late'
           ELSE        N'Present'
       END,
       CASE WHEN (s.SessionNo + e.StudentNo) % 10 IN (0, 1) THEN NULL ELSE 3.00 END,
       CASE (s.SessionNo + e.StudentNo) % 10
           WHEN 0 THEN N'Vang khong phep'
           WHEN 1 THEN N'Co phep - viec gia dinh'
           WHEN 2 THEN N'Den muon 15 phut'
           WHEN 3 THEN N'Den muon 10 phut'
           ELSE NULL
       END,
       @Now, @Seeder, NULL, NULL
FROM CompletedSessions s
INNER JOIN ActiveEnrollments e ON e.BatchId = s.BatchId
WHERE NOT EXISTS (
    SELECT 1 FROM attendance_record a
    WHERE a.ClassSessionId = s.SessionId AND a.EnrollmentId = e.EnrollmentId
);

COMMIT TRAN;
PRINT '12-attendance-records.sql: attendance rows derived from completed sessions.';
