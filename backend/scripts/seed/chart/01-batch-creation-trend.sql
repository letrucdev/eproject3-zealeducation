/*
 * chart/01-batch-creation-trend.sql
 *
 * Generates ~400 chart-only batches with explicit per-day, per-status counts
 * spanning the trailing 90 days. Daily totals deliberately swing across five
 * intensity buckets so the dashboard line chart reads like real production
 * traffic instead of a flat band:
 *
 *   VHIGH (peak / admission rush) : Active 6  Completed 3  Cancelled 2  (Total 11)
 *   HIGH                           : Active 4  Completed 2  Cancelled 1  (Total  7)
 *   MEDIUM                         : Active 3  Completed 1  Cancelled 1  (Total  5)
 *   LOW                            : Active 1  Completed 1  Cancelled 0  (Total  2)
 *   VLOW (quiet)                   : Active 1  Completed 0  Cancelled 0  (Total  1)
 *
 * Pattern: warm-up -> rising -> peak (~-55..-40) -> declining -> calm tail,
 * with weekly low/high oscillation overlaid so adjacent days clearly differ.
 *
 * Re-run safe: every batch row is inserted with NOT EXISTS on BatchCode.
 * The DELETE at the top wipes only generic chart batches (CHART-B-*) so a
 * tweaked schedule fully replaces an old run; CHART-FOCUS-2026 is owned by
 * 03-focus-batch-and-students.sql and is left alone.
 */

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRAN;

DECLARE @Now    DATETIME2(7)  = SYSUTCDATETIME();
DECLARE @Today  DATE          = CAST(SYSUTCDATETIME() AS DATE);
DECLARE @Seeder NVARCHAR(100) = N'seed-chart-data';

DECLARE @CourseAccp   UNIQUEIDENTIFIER = 'dddddddd-0000-0000-0000-000000000001';
DECLARE @CourseHdse   UNIQUEIDENTIFIER = 'dddddddd-0000-0000-0000-000000000002';
DECLARE @CourseAdim   UNIQUEIDENTIFIER = 'dddddddd-0000-0000-0000-000000000003';
DECLARE @CourseCpism  UNIQUEIDENTIFIER = 'dddddddd-0000-0000-0000-000000000004';
DECLARE @CourseWebpro UNIQUEIDENTIFIER = 'dddddddd-0000-0000-0000-000000000005';
DECLARE @CourseEngit  UNIQUEIDENTIFIER = 'dddddddd-0000-0000-0000-000000000006';

DECLARE @Faculty1 UNIQUEIDENTIFIER = 'bbbbbbbb-0000-0000-0000-000000000001';
DECLARE @Faculty2 UNIQUEIDENTIFIER = 'bbbbbbbb-0000-0000-0000-000000000002';
DECLARE @Faculty3 UNIQUEIDENTIFIER = 'bbbbbbbb-0000-0000-0000-000000000003';

DELETE FROM batch WHERE CreatedBy = @Seeder AND BatchCode LIKE N'CHART-B-%';

DECLARE @BatchSchedule TABLE (
    DayOffset       INT PRIMARY KEY,
    ActiveCount     INT NOT NULL,
    CompletedCount  INT NOT NULL,
    CancelledCount  INT NOT NULL
);

-- Buckets shorthand used in the values below:
--   V=VHIGH(6,3,2)  H=HIGH(4,2,1)  M=MED(3,1,1)  L=LOW(1,1,0)  Z=VLOW(1,0,0)
INSERT INTO @BatchSchedule (DayOffset, ActiveCount, CompletedCount, CancelledCount) VALUES
-- early warm-up (-89..-80) — mostly L/M, single H bump
(-89,1,0,0),(-88,3,1,1),(-87,1,1,0),(-86,3,1,1),(-85,4,2,1),(-84,1,1,0),(-83,1,0,0),(-82,3,1,1),(-81,4,2,1),(-80,3,1,1),
-- rising (-79..-70) — H spikes more often
(-79,1,1,0),(-78,3,1,1),(-77,4,2,1),(-76,1,1,0),(-75,3,1,1),(-74,4,2,1),(-73,3,1,1),(-72,6,3,2),(-71,1,1,0),(-70,3,1,1),
-- climbing (-69..-60) — V spikes appear
(-69,4,2,1),(-68,1,1,0),(-67,3,1,1),(-66,6,3,2),(-65,3,1,1),(-64,4,2,1),(-63,1,1,0),(-62,6,3,2),(-61,3,1,1),(-60,4,2,1),
-- peak window (-59..-50) — V dominates
(-59,6,3,2),(-58,4,2,1),(-57,1,1,0),(-56,6,3,2),(-55,3,1,1),(-54,6,3,2),(-53,4,2,1),(-52,3,1,1),(-51,6,3,2),(-50,4,2,1),
-- still high but slipping (-49..-40)
(-49,4,2,1),(-48,1,1,0),(-47,6,3,2),(-46,3,1,1),(-45,4,2,1),(-44,6,3,2),(-43,1,1,0),(-42,4,2,1),(-41,3,1,1),(-40,1,1,0),
-- declining (-39..-30)
(-39,3,1,1),(-38,4,2,1),(-37,1,1,0),(-36,3,1,1),(-35,4,2,1),(-34,1,0,0),(-33,3,1,1),(-32,1,1,0),(-31,4,2,1),(-30,3,1,1),
-- mid-low (-29..-20)
(-29,1,1,0),(-28,3,1,1),(-27,1,1,0),(-26,4,2,1),(-25,1,1,0),(-24,3,1,1),(-23,1,0,0),(-22,1,1,0),(-21,3,1,1),(-20,4,2,1),
-- calm (-19..-10)
(-19,1,1,0),(-18,3,1,1),(-17,1,0,0),(-16,1,1,0),(-15,3,1,1),(-14,1,1,0),(-13,1,1,0),(-12,3,1,1),(-11,1,0,0),(-10,1,1,0),
-- recent tail (-9..0) — small H spike on -7 and 0
(-9,3,1,1),(-8,1,1,0),(-7,4,2,1),(-6,1,1,0),(-5,3,1,1),(-4,1,0,0),(-3,1,1,0),(-2,3,1,1),(-1,1,1,0),(0,4,2,1);

DECLARE @Slots TABLE (Slot INT PRIMARY KEY);
INSERT INTO @Slots VALUES (1),(2),(3),(4),(5),(6),(7),(8);

;WITH StatusRows AS (
    SELECT DayOffset, N'Active'    AS Status, ActiveCount    AS Cnt FROM @BatchSchedule
    UNION ALL
    SELECT DayOffset, N'Completed', CompletedCount               FROM @BatchSchedule
    UNION ALL
    SELECT DayOffset, N'Cancelled', CancelledCount               FROM @BatchSchedule
),
ChartBatchRows AS (
    SELECT
        ROW_NUMBER() OVER (
            ORDER BY sr.DayOffset,
                     CASE sr.Status WHEN N'Active' THEN 1 WHEN N'Completed' THEN 2 ELSE 3 END,
                     sl.Slot
        ) AS Seq,
        sr.DayOffset, sr.Status, sl.Slot
    FROM StatusRows sr
    INNER JOIN @Slots sl ON sl.Slot <= sr.Cnt
),
ChartBatches AS (
    SELECT
        Seq, DayOffset, Slot, Status,
        DATEADD(day, DayOffset, @Now) AS CreatedAt,
        N'CHART-B-' + RIGHT('00000' + CAST(Seq AS NVARCHAR(5)), 5) AS BatchCode,
        CASE Seq % 6
            WHEN 0 THEN @CourseAccp
            WHEN 1 THEN @CourseHdse
            WHEN 2 THEN @CourseAdim
            WHEN 3 THEN @CourseCpism
            WHEN 4 THEN @CourseWebpro
            ELSE        @CourseEngit
        END AS CourseId,
        CASE Seq % 3
            WHEN 0 THEN @Faculty1
            WHEN 1 THEN @Faculty2
            ELSE        @Faculty3
        END AS FacultyId,
        DATEADD(day, DayOffset + 14, @Today)        AS StartDate,
        DATEADD(day, DayOffset + 14 + 168, @Today)  AS EndDate,
        N'Room ' + CAST(100 + (Seq % 8) AS NVARCHAR(3)) AS Location,
        25 + (Seq % 3) * 5 AS MaxCapacity
    FROM ChartBatchRows
)
INSERT INTO batch (Id, CourseId, BatchCode, FacultyId, StartDate, EndDate, Location,
                   MaxCapacity, Status, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy)
SELECT
    CAST('e1c1e1c1-0000-0000-0000-' + RIGHT('000000000000' + CAST(cb.Seq AS NVARCHAR(12)), 12)
         AS UNIQUEIDENTIFIER),
    cb.CourseId, cb.BatchCode, cb.FacultyId, cb.StartDate, cb.EndDate, cb.Location,
    cb.MaxCapacity, cb.Status, cb.CreatedAt, @Seeder, NULL, NULL
FROM ChartBatches cb
WHERE NOT EXISTS (SELECT 1 FROM batch x WHERE x.BatchCode = cb.BatchCode);

COMMIT TRAN;

PRINT 'chart/01-batch-creation-trend.sql complete.';
PRINT '  90-day batch CreatedAt distribution generated with VHIGH/HIGH/MED/LOW/VLOW buckets.';
PRINT '  Verify daily counts:';
PRINT '    SELECT CAST(CreatedAt AS DATE) d,';
PRINT '           SUM(CASE WHEN Status=''Active''    THEN 1 ELSE 0 END) A,';
PRINT '           SUM(CASE WHEN Status=''Completed'' THEN 1 ELSE 0 END) C,';
PRINT '           SUM(CASE WHEN Status=''Cancelled'' THEN 1 ELSE 0 END) X,';
PRINT '           COUNT(*) T';
PRINT '    FROM batch WHERE CreatedBy = ''seed-chart-data''';
PRINT '    GROUP BY CAST(CreatedAt AS DATE) ORDER BY d;';
