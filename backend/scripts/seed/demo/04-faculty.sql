/*
 * 04-faculty.sql — 3 faculty profiles, one per Faculty-role staff.
 * Idempotent on FacultyCode. Requires 03-staff.sql.
 */

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRAN;

DECLARE @Now    DATETIME2(7)  = SYSUTCDATETIME();
DECLARE @Seeder NVARCHAR(100) = N'seed-demo-data';

INSERT INTO faculty (Id, StaffId, FacultyCode, Qualification, Specialization, ExperienceYears,
                     CreatedAt, CreatedBy, UpdatedAt, UpdatedBy)
SELECT * FROM (VALUES
    (CAST('bbbbbbbb-0000-0000-0000-000000000001' AS UNIQUEIDENTIFIER),
     CAST('aaaaaaaa-0000-0000-0000-000000000021' AS UNIQUEIDENTIFIER),
     N'FAC-001', N'M.Sc. Computer Science',     N'Java / Backend',    12, @Now, @Seeder, CAST(NULL AS DATETIME2), CAST(NULL AS NVARCHAR(100))),
    (CAST('bbbbbbbb-0000-0000-0000-000000000002' AS UNIQUEIDENTIFIER),
     CAST('aaaaaaaa-0000-0000-0000-000000000022' AS UNIQUEIDENTIFIER),
     N'FAC-002', N'B.E. Software Engineering',  N'.NET / C#',          8, @Now, @Seeder, NULL, NULL),
    (CAST('bbbbbbbb-0000-0000-0000-000000000003' AS UNIQUEIDENTIFIER),
     CAST('aaaaaaaa-0000-0000-0000-000000000023' AS UNIQUEIDENTIFIER),
     N'FAC-003', N'B.A. Multimedia Design',     N'Web / UI / Mobile',  6, @Now, @Seeder, NULL, NULL)
) v(Id, StaffId, FacultyCode, Qualification, Specialization, ExperienceYears,
    CreatedAt, CreatedBy, UpdatedAt, UpdatedBy)
WHERE NOT EXISTS (SELECT 1 FROM faculty f WHERE f.FacultyCode = v.FacultyCode);

COMMIT TRAN;
PRINT '04-faculty.sql: 3 faculty rows ensured.';
