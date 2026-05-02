/*
 * 06-batches.sql — 8 batches covering the active mix of statuses for the
 * batch-list & batch-detail UI. Includes one DEMO batch (ACCP-2026-DEMO)
 * which subsequent files load with students / sessions / exams.
 *
 * For the daily batch creation chart, run chart/01-batch-creation-trend.sql.
 *
 * Idempotent on BatchCode. Requires courses + faculty.
 */

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRAN;

DECLARE @Now    DATETIME2(7)  = SYSUTCDATETIME();
DECLARE @Seeder NVARCHAR(100) = N'seed-demo-data';

DECLARE @Batches TABLE (
    Id          UNIQUEIDENTIFIER,
    CourseId    UNIQUEIDENTIFIER,
    BatchCode   NVARCHAR(30),
    FacultyId   UNIQUEIDENTIFIER NULL,
    StartDate   DATE,
    EndDate     DATE,
    Location    NVARCHAR(100),
    MaxCapacity INT,
    Status      NVARCHAR(20),
    DaysAgo     INT
);

INSERT INTO @Batches VALUES
('eeeeeeee-0000-0000-0000-000000000001','dddddddd-0000-0000-0000-000000000004', N'CPISM-2026-01',
    'bbbbbbbb-0000-0000-0000-000000000003', '2026-02-15', '2027-01-15', N'Room 201',  30, N'Active',          85),
('eeeeeeee-0000-0000-0000-000000000002','dddddddd-0000-0000-0000-000000000005', N'WEBPRO-2026-01',
    'bbbbbbbb-0000-0000-0000-000000000003', '2026-02-25', '2026-08-25', N'Room 305',  25, N'Active',          73),
-- DEMO batch sits ~95 days back so all 8 of its exams (-88..-3) fit inside it.
('eeeeeeee-0000-0000-0000-000000000003','dddddddd-0000-0000-0000-000000000001', N'ACCP-2026-DEMO',
    'bbbbbbbb-0000-0000-0000-000000000001', '2026-01-25', '2028-01-25', N'Room 101',  35, N'Active',          95),
('eeeeeeee-0000-0000-0000-000000000004','dddddddd-0000-0000-0000-000000000003', N'ADIM-2026-01',
    'bbbbbbbb-0000-0000-0000-000000000003', '2026-03-12', '2027-09-12', N'Room 401',  25, N'Active',          56),
('eeeeeeee-0000-0000-0000-000000000005','dddddddd-0000-0000-0000-000000000006', N'ENGIT-2026-01',
    NULL,                                   '2026-04-01', '2026-07-30', N'Room 203',  30, N'Cancelled',       52),
('eeeeeeee-0000-0000-0000-000000000006','dddddddd-0000-0000-0000-000000000002', N'HDSE-2026-01',
    'bbbbbbbb-0000-0000-0000-000000000001', '2026-04-10', '2028-09-10', N'Room 102',  30, N'Active',          43),
('eeeeeeee-0000-0000-0000-000000000007','dddddddd-0000-0000-0000-000000000001', N'ACCP-2026-02',
    'bbbbbbbb-0000-0000-0000-000000000002', '2026-04-15', '2028-03-15', N'Room 103',  35, N'Active',          39),
('eeeeeeee-0000-0000-0000-000000000008','dddddddd-0000-0000-0000-000000000005', N'WEBPRO-2026-02',
    NULL,                                   '2026-05-15', '2026-11-15', N'Room 306',  25, N'NeedsInstructor', 5);

INSERT INTO batch (Id, CourseId, BatchCode, FacultyId, StartDate, EndDate, Location,
                   MaxCapacity, Status, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy)
SELECT b.Id, b.CourseId, b.BatchCode, b.FacultyId, b.StartDate, b.EndDate, b.Location,
       b.MaxCapacity, b.Status, DATEADD(day, -b.DaysAgo, @Now), @Seeder, NULL, NULL
FROM @Batches b
WHERE NOT EXISTS (SELECT 1 FROM batch x WHERE x.BatchCode = b.BatchCode);

COMMIT TRAN;
PRINT '06-batches.sql: 8 batches ensured.';
