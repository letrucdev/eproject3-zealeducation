/*
 * chart/02-candidate-registration-trend.sql
 *
 * Generates ~200 chart-only candidates with explicit per-day, per-status
 * counts spanning the trailing 90 days. Active counts per day swing across
 * the same five intensity buckets as 01-batch-creation-trend.sql so the
 * candidate registration line chart looks alive (not flat) and the
 * Graduated / Dropped sub-series have visible spikes:
 *
 *   VHIGH (peak)    : Active 5
 *   HIGH            : Active 3
 *   MEDIUM          : Active 2
 *   LOW             : Active 1
 *   VLOW (quiet)    : Active 0
 *
 * Plus 4 Graduated days and 4 Dropped days sprinkled across the 90-day window
 * so those status filters & legend lines never collapse to 0.
 *
 * Re-run safe: DELETE wipes prior chart-trend candidates (CHART-REG-*) and
 * their user_account rows so a tweaked schedule fully replaces an old run.
 * Username prefix used: 'chartreg'.
 */

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRAN;

DECLARE @Now    DATETIME2(7)  = SYSUTCDATETIME();
DECLARE @Seeder NVARCHAR(100) = N'seed-chart-data';
DECLARE @DefaultPasswordHash NVARCHAR(255) =
    N'$2a$11$0a1gHPHCr3zOzvF2ldRA4uuNcyMkDxPqFoJqVZboojsEKMQ7OLrwy';

DECLARE @CounselorStaff1 UNIQUEIDENTIFIER = 'aaaaaaaa-0000-0000-0000-000000000031';
DECLARE @CounselorStaff2 UNIQUEIDENTIFIER = 'aaaaaaaa-0000-0000-0000-000000000032';
DECLARE @CounselorStaff3 UNIQUEIDENTIFIER = 'aaaaaaaa-0000-0000-0000-000000000033';

-- Wipe and re-create cleanly (FK from candidate -> user_account, so candidate first).
DELETE FROM candidate     WHERE CreatedBy = @Seeder AND CandidateCode LIKE N'CHART-REG-%';
DELETE FROM user_account  WHERE CreatedBy = @Seeder AND Username      LIKE N'chartreg%';

DECLARE @RegSchedule TABLE (
    DayOffset       INT PRIMARY KEY,
    ActiveCount     INT NOT NULL,
    GraduatedCount  INT NOT NULL,
    DroppedCount    INT NOT NULL
);

-- Each row: per-day counts for Active / Graduated / Dropped registrations.
-- Active follows the same VHIGH..VLOW pattern as the batch schedule so both
-- charts ride the same admission rhythm. Graduated/Dropped are sparse spikes.
INSERT INTO @RegSchedule (DayOffset, ActiveCount, GraduatedCount, DroppedCount) VALUES
-- early warm-up (-89..-80)
(-89,1,0,0),(-88,2,0,0),(-87,1,0,0),(-86,2,0,0),(-85,3,0,0),(-84,1,0,0),(-83,0,0,0),(-82,2,0,0),(-81,3,0,0),(-80,2,0,0),
-- rising (-79..-70)
(-79,1,0,0),(-78,2,0,0),(-77,3,0,0),(-76,1,0,0),(-75,2,1,0),(-74,3,0,0),(-73,2,0,0),(-72,5,0,0),(-71,1,0,0),(-70,2,0,0),
-- climbing (-69..-60)
(-69,3,0,0),(-68,1,0,0),(-67,2,0,0),(-66,5,0,0),(-65,2,0,1),(-64,3,0,0),(-63,1,0,0),(-62,5,0,0),(-61,2,0,0),(-60,3,0,0),
-- peak (-59..-50)
(-59,5,0,0),(-58,3,0,0),(-57,1,0,0),(-56,5,0,0),(-55,2,1,0),(-54,5,0,0),(-53,3,0,0),(-52,2,0,0),(-51,5,0,0),(-50,3,0,0),
-- still high (-49..-40)
(-49,3,0,0),(-48,1,0,0),(-47,5,0,0),(-46,2,0,0),(-45,3,0,1),(-44,5,0,0),(-43,1,0,0),(-42,3,0,0),(-41,2,0,0),(-40,1,0,0),
-- declining (-39..-30)
(-39,2,0,0),(-38,3,1,0),(-37,1,0,0),(-36,2,0,0),(-35,3,0,0),(-34,0,0,0),(-33,2,0,0),(-32,1,0,0),(-31,3,0,0),(-30,2,0,0),
-- mid-low (-29..-20)
(-29,1,0,0),(-28,2,0,0),(-27,1,0,0),(-26,3,0,0),(-25,1,0,1),(-24,2,0,0),(-23,0,0,0),(-22,1,0,0),(-21,2,0,0),(-20,3,0,0),
-- calm (-19..-10)
(-19,1,0,0),(-18,2,1,0),(-17,0,0,0),(-16,1,0,0),(-15,2,0,0),(-14,1,0,0),(-13,1,0,0),(-12,2,0,0),(-11,0,0,0),(-10,1,0,0),
-- recent (-9..0)
(-9,2,0,0),(-8,1,0,1),(-7,3,0,0),(-6,1,0,0),(-5,2,0,0),(-4,0,0,0),(-3,1,0,0),(-2,2,0,0),(-1,1,0,0),(0,3,0,0);

DECLARE @Slots TABLE (Slot INT PRIMARY KEY);
INSERT INTO @Slots VALUES (1),(2),(3),(4),(5);

;WITH StatusRows AS (
    SELECT DayOffset, N'Active'    AS Status, ActiveCount    AS Cnt FROM @RegSchedule
    UNION ALL
    SELECT DayOffset, N'Graduated', GraduatedCount               FROM @RegSchedule
    UNION ALL
    SELECT DayOffset, N'Dropped',   DroppedCount                 FROM @RegSchedule
),
RegRows AS (
    SELECT
        ROW_NUMBER() OVER (
            ORDER BY sr.DayOffset,
                     CASE sr.Status WHEN N'Active' THEN 1 WHEN N'Graduated' THEN 2 ELSE 3 END,
                     sl.Slot
        ) AS Seq,
        sr.DayOffset, sr.Status, sl.Slot
    FROM StatusRows sr
    INNER JOIN @Slots sl ON sl.Slot <= sr.Cnt
),
ChartRegistrations AS (
    SELECT
        Seq, DayOffset, Status,
        DATEADD(day, DayOffset, @Now) AS RegisteredAt,
        N'chartreg' + RIGHT('000' + CAST(Seq AS NVARCHAR(3)), 3) AS Username,
        N'CHART-REG-' + RIGHT('000' + CAST(Seq AS NVARCHAR(3)), 3) AS CandidateCode,
        CAST('77777777-0000-0000-0000-' + RIGHT('000000000000' + CAST(Seq AS NVARCHAR(12)), 12)
             AS UNIQUEIDENTIFIER) AS UserId,
        CAST('c7c7c7c7-0000-0000-0000-' + RIGHT('000000000000' + CAST(Seq AS NVARCHAR(12)), 12)
             AS UNIQUEIDENTIFIER) AS CandidateId
    FROM RegRows
)
INSERT INTO user_account (Id, Username, PasswordHash, FullName, Email, Phone, Dob, Gender,
                          Role, IsActive, MustChangePassword, FailedLoginCount, LastLogin,
                          CreatedAt, CreatedBy, UpdatedAt, UpdatedBy)
SELECT
    cr.UserId, cr.Username, @DefaultPasswordHash,
    N'Chart Reg ' + CAST(cr.Seq AS NVARCHAR(5)),
    cr.Username + N'@chart.zealedu.vn',
    N'0939' + RIGHT('000000' + CAST(cr.Seq AS NVARCHAR(6)), 6),
    DATEADD(year, -22, CAST(SYSUTCDATETIME() AS DATE)),
    CASE cr.Seq % 2 WHEN 0 THEN N'Female' ELSE N'Male' END,
    N'Candidate', 1, 0, 0, NULL,
    cr.RegisteredAt, @Seeder, NULL, NULL
FROM ChartRegistrations cr
WHERE NOT EXISTS (SELECT 1 FROM user_account x WHERE x.Username = cr.Username);

;WITH StatusRows AS (
    SELECT DayOffset, N'Active'    AS Status, ActiveCount    AS Cnt FROM @RegSchedule
    UNION ALL
    SELECT DayOffset, N'Graduated', GraduatedCount               FROM @RegSchedule
    UNION ALL
    SELECT DayOffset, N'Dropped',   DroppedCount                 FROM @RegSchedule
),
RegRows AS (
    SELECT
        ROW_NUMBER() OVER (
            ORDER BY sr.DayOffset,
                     CASE sr.Status WHEN N'Active' THEN 1 WHEN N'Graduated' THEN 2 ELSE 3 END,
                     sl.Slot
        ) AS Seq,
        sr.DayOffset, sr.Status, sl.Slot
    FROM StatusRows sr
    INNER JOIN @Slots sl ON sl.Slot <= sr.Cnt
),
ChartRegistrations AS (
    SELECT
        Seq, DayOffset, Status,
        DATEADD(day, DayOffset, @Now) AS RegisteredAt,
        N'chartreg' + RIGHT('000' + CAST(Seq AS NVARCHAR(3)), 3) AS Username,
        N'CHART-REG-' + RIGHT('000' + CAST(Seq AS NVARCHAR(3)), 3) AS CandidateCode,
        CAST('77777777-0000-0000-0000-' + RIGHT('000000000000' + CAST(Seq AS NVARCHAR(12)), 12)
             AS UNIQUEIDENTIFIER) AS UserId,
        CAST('c7c7c7c7-0000-0000-0000-' + RIGHT('000000000000' + CAST(Seq AS NVARCHAR(12)), 12)
             AS UNIQUEIDENTIFIER) AS CandidateId
    FROM RegRows
)
INSERT INTO candidate (Id, UserAccountId, CandidateCode, Address, EmergencyContact, Notes,
                       Status, RegisteredAt, RegisteredByStaffId,
                       CreatedAt, CreatedBy, UpdatedAt, UpdatedBy)
SELECT
    cr.CandidateId, cr.UserId, cr.CandidateCode,
    N'Chart address ' + CAST(cr.Seq AS NVARCHAR(5)),
    NULL, NULL, cr.Status,
    cr.RegisteredAt,
    CASE cr.Seq % 3
        WHEN 0 THEN @CounselorStaff1
        WHEN 1 THEN @CounselorStaff2
        ELSE        @CounselorStaff3
    END,
    cr.RegisteredAt, @Seeder, NULL, NULL
FROM ChartRegistrations cr
WHERE NOT EXISTS (SELECT 1 FROM candidate x WHERE x.UserAccountId = cr.UserId);

COMMIT TRAN;

PRINT 'chart/02-candidate-registration-trend.sql complete.';
PRINT '  90-day candidate.RegisteredAt distribution generated.';
PRINT '  Verify daily counts:';
PRINT '    SELECT CAST(RegisteredAt AS DATE) d,';
PRINT '           SUM(CASE WHEN Status=''Active''    THEN 1 ELSE 0 END) AS A,';
PRINT '           SUM(CASE WHEN Status=''Graduated'' THEN 1 ELSE 0 END) AS G,';
PRINT '           SUM(CASE WHEN Status=''Dropped''   THEN 1 ELSE 0 END) AS D,';
PRINT '           COUNT(*) AS T';
PRINT '    FROM candidate WHERE CreatedBy = ''seed-chart-data''';
PRINT '    GROUP BY CAST(RegisteredAt AS DATE) ORDER BY d;';
