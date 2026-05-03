/*
 * 01-courses.sql — 6 IT courses (Aptech VN inspired).
 *
 * Each row guarded by NOT EXISTS on CourseName so re-running is safe.
 * Run order: this file FIRST; everything else references CourseId.
 */

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRAN;

DECLARE @Now    DATETIME2(7)  = SYSUTCDATETIME();
DECLARE @Seeder NVARCHAR(100) = N'seed-demo-data';

DECLARE @Courses TABLE (
    Id              UNIQUEIDENTIFIER,
    CourseName      NVARCHAR(100),
    Description     NVARCHAR(MAX),
    DurationWeeks   INT,
    BaseFee         DECIMAL(12,2),
    IsActive        BIT
);

INSERT INTO @Courses VALUES
('dddddddd-0000-0000-0000-000000000001', N'ACCP - Aptech Certified Computer Professional',
 N'Lap trinh vien quoc te 2 nam: C/C++, Java, .NET, SQL Server, Web va du an thuc te.',  96, 32000000.00, 1),
('dddddddd-0000-0000-0000-000000000002', N'HDSE - Higher Diploma in Software Engineering',
 N'Cao dang quoc te phan mem 2.5 nam: kien truc he thong, cloud, DevOps, capstone.',     110, 45000000.00, 1),
('dddddddd-0000-0000-0000-000000000003', N'ADIM - Aptech Diploma in Multimedia',
 N'Thiet ke do hoa, dung phim, 2D/3D animation, game art va UI/UX co ban.',               80, 26000000.00, 1),
('dddddddd-0000-0000-0000-000000000004', N'CPISM - Certificate in Programming and IS Management',
 N'Khoa nen tang 1 nam: lap trinh co ban, CSDL, web tinh, soft skill cho fresher IT.',    48, 18000000.00, 1),
('dddddddd-0000-0000-0000-000000000005', N'WEBPRO - Web Pro Developer',
 N'Khoa ngan han 6 thang: HTML/CSS/JS, ReactJS, NodeJS, deploy cloud.',                   24, 12000000.00, 1),
('dddddddd-0000-0000-0000-000000000006', N'ENGIT - Business English for IT',
 N'Tieng Anh giao tiep va viet tai lieu ky thuat cho dan IT, 4 thang.',                   16,  5500000.00, 1);

INSERT INTO course (Id, CourseName, Description, DurationWeeks, BaseFee, IsActive,
                    CreatedAt, CreatedBy, UpdatedAt, UpdatedBy)
SELECT c.Id, c.CourseName, c.Description, c.DurationWeeks, c.BaseFee, c.IsActive,
       @Now, @Seeder, NULL, NULL
FROM @Courses c
WHERE NOT EXISTS (SELECT 1 FROM course x WHERE x.CourseName = c.CourseName);

COMMIT TRAN;
PRINT '01-courses.sql: 6 courses ensured.';
