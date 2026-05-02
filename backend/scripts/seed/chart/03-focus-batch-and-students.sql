/*
 * chart/03-focus-batch-and-students.sql
 *
 * Sets up the dedicated focus batch CHART-FOCUS-2026 + 12 students + their
 * enrollments. This batch backs the per-batch exam-scores trend chart and
 * the grade-distribution pie (filled by chart/04-focus-exams-and-results.sql).
 *
 * The 12 students each carry a deterministic ScoreOffset stored in their
 * Notes field — referenced by 04-focus-exams-and-results.sql so re-runs of
 * either file stay consistent.
 *
 * Registration timestamps are deliberately ~120 days back, OUTSIDE the
 * 90-day registration trend window, so these students do not skew
 * chart/02 candidate counts.
 *
 * Idempotent: NOT EXISTS guards on BatchCode / Username / CandidateCode /
 * EnrollmentId. Safe to re-run after 01 / 02.
 */

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRAN;

DECLARE @Now    DATETIME2(7)  = SYSUTCDATETIME();
DECLARE @Today  DATE          = CAST(SYSUTCDATETIME() AS DATE);
DECLARE @Seeder NVARCHAR(100) = N'seed-chart-data';
DECLARE @DefaultPasswordHash NVARCHAR(255) =
    N'$2a$11$0a1gHPHCr3zOzvF2ldRA4uuNcyMkDxPqFoJqVZboojsEKMQ7OLrwy';

DECLARE @CourseAccp     UNIQUEIDENTIFIER = 'dddddddd-0000-0000-0000-000000000001';
DECLARE @Faculty1       UNIQUEIDENTIFIER = 'bbbbbbbb-0000-0000-0000-000000000001';
DECLARE @InchargeStaff1 UNIQUEIDENTIFIER = 'aaaaaaaa-0000-0000-0000-000000000011';
DECLARE @InchargeStaff2 UNIQUEIDENTIFIER = 'aaaaaaaa-0000-0000-0000-000000000012';
DECLARE @CounselorStaff1 UNIQUEIDENTIFIER = 'aaaaaaaa-0000-0000-0000-000000000031';
DECLARE @CounselorStaff2 UNIQUEIDENTIFIER = 'aaaaaaaa-0000-0000-0000-000000000032';
DECLARE @CounselorStaff3 UNIQUEIDENTIFIER = 'aaaaaaaa-0000-0000-0000-000000000033';

DECLARE @ChartBatch UNIQUEIDENTIFIER = 'e1c1e1c1-9999-0000-0000-000000000001';

-- Focus batch
INSERT INTO batch (Id, CourseId, BatchCode, FacultyId, StartDate, EndDate, Location,
                   MaxCapacity, Status, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy)
SELECT @ChartBatch, @CourseAccp, N'CHART-FOCUS-2026', @Faculty1,
       DATEADD(day, -100, @Today), DATEADD(day, 600, @Today),
       N'Lab F', 35, N'Active',
       DATEADD(day, -100, @Now), @Seeder, NULL, NULL
WHERE NOT EXISTS (SELECT 1 FROM batch x WHERE x.BatchCode = N'CHART-FOCUS-2026');

-- 12 chart-focus students.
DECLARE @ChartCandidates TABLE (
    Slot          INT PRIMARY KEY,
    UserId        UNIQUEIDENTIFIER,
    Username      NVARCHAR(50),
    FullName      NVARCHAR(100),
    Email         NVARCHAR(100),
    Phone         NVARCHAR(20),
    Dob           DATE,
    Gender        NVARCHAR(10),
    CandidateId   UNIQUEIDENTIFIER,
    CandidateCode NVARCHAR(20),
    EnrollmentId  UNIQUEIDENTIFIER,
    -- ScoreOffset (stored in Notes) drives the per-student score variance in
    -- 04-focus-exams-and-results.sql. Range -25..+19 spans F-grade to A-grade.
    ScoreOffset   DECIMAL(6,2)
);

INSERT INTO @ChartCandidates VALUES
(1,  '66666666-0000-0000-0000-000000000001', N'chartcand01', N'Tran Anh Dung',     N'chartcand01@zealedu.vn', N'0931000001','2003-01-10',N'Male',
     'c2c2c2c2-0000-0000-0000-000000000001', N'CHART-CAND-01', 'f3f3f3f3-0000-0000-0000-000000000001', -25),
(2,  '66666666-0000-0000-0000-000000000002', N'chartcand02', N'Nguyen Bich Tram',  N'chartcand02@zealedu.vn', N'0931000002','2003-02-14',N'Female',
     'c2c2c2c2-0000-0000-0000-000000000002', N'CHART-CAND-02', 'f3f3f3f3-0000-0000-0000-000000000002', -18),
(3,  '66666666-0000-0000-0000-000000000003', N'chartcand03', N'Le Quang Huy',      N'chartcand03@zealedu.vn', N'0931000003','2002-08-22',N'Male',
     'c2c2c2c2-0000-0000-0000-000000000003', N'CHART-CAND-03', 'f3f3f3f3-0000-0000-0000-000000000003', -10),
(4,  '66666666-0000-0000-0000-000000000004', N'chartcand04', N'Pham Hai Yen',      N'chartcand04@zealedu.vn', N'0931000004','2003-05-04',N'Female',
     'c2c2c2c2-0000-0000-0000-000000000004', N'CHART-CAND-04', 'f3f3f3f3-0000-0000-0000-000000000004',  -6),
(5,  '66666666-0000-0000-0000-000000000005', N'chartcand05', N'Hoang Trong Tan',   N'chartcand05@zealedu.vn', N'0931000005','2004-03-19',N'Male',
     'c2c2c2c2-0000-0000-0000-000000000005', N'CHART-CAND-05', 'f3f3f3f3-0000-0000-0000-000000000005',  -2),
(6,  '66666666-0000-0000-0000-000000000006', N'chartcand06', N'Vu Thi Lan',        N'chartcand06@zealedu.vn', N'0931000006','2003-11-27',N'Female',
     'c2c2c2c2-0000-0000-0000-000000000006', N'CHART-CAND-06', 'f3f3f3f3-0000-0000-0000-000000000006',   1),
(7,  '66666666-0000-0000-0000-000000000007', N'chartcand07', N'Do Minh Phuc',      N'chartcand07@zealedu.vn', N'0931000007','2002-07-09',N'Male',
     'c2c2c2c2-0000-0000-0000-000000000007', N'CHART-CAND-07', 'f3f3f3f3-0000-0000-0000-000000000007',   4),
(8,  '66666666-0000-0000-0000-000000000008', N'chartcand08', N'Bui Ngoc Mai',      N'chartcand08@zealedu.vn', N'0931000008','2003-09-15',N'Female',
     'c2c2c2c2-0000-0000-0000-000000000008', N'CHART-CAND-08', 'f3f3f3f3-0000-0000-0000-000000000008',   7),
(9,  '66666666-0000-0000-0000-000000000009', N'chartcand09', N'Dang Tien Hai',     N'chartcand09@zealedu.vn', N'0931000009','2004-01-23',N'Male',
     'c2c2c2c2-0000-0000-0000-000000000009', N'CHART-CAND-09', 'f3f3f3f3-0000-0000-0000-000000000009',  10),
(10, '66666666-0000-0000-0000-000000000010', N'chartcand10', N'Phan Bao Han',      N'chartcand10@zealedu.vn', N'0931000010','2003-04-30',N'Female',
     'c2c2c2c2-0000-0000-0000-000000000010', N'CHART-CAND-10', 'f3f3f3f3-0000-0000-0000-000000000010',  13),
(11, '66666666-0000-0000-0000-000000000011', N'chartcand11', N'Nguyen Duy Khanh',  N'chartcand11@zealedu.vn', N'0931000011','2002-12-08',N'Male',
     'c2c2c2c2-0000-0000-0000-000000000011', N'CHART-CAND-11', 'f3f3f3f3-0000-0000-0000-000000000011',  16),
(12, '66666666-0000-0000-0000-000000000012', N'chartcand12', N'Tran Thuy Linh',    N'chartcand12@zealedu.vn', N'0931000012','2003-06-17',N'Female',
     'c2c2c2c2-0000-0000-0000-000000000012', N'CHART-CAND-12', 'f3f3f3f3-0000-0000-0000-000000000012',  19);

INSERT INTO user_account (Id, Username, PasswordHash, FullName, Email, Phone, Dob, Gender,
                          Role, IsActive, MustChangePassword, FailedLoginCount, LastLogin,
                          CreatedAt, CreatedBy, UpdatedAt, UpdatedBy)
SELECT cc.UserId, cc.Username, @DefaultPasswordHash, cc.FullName, cc.Email, cc.Phone,
       cc.Dob, cc.Gender, N'Candidate', 1, 0, 0, NULL,
       DATEADD(day, -120, @Now), @Seeder, NULL, NULL
FROM @ChartCandidates cc
WHERE NOT EXISTS (SELECT 1 FROM user_account x WHERE x.Username = cc.Username);

INSERT INTO candidate (Id, UserAccountId, CandidateCode, Address, EmergencyContact, Notes,
                       Status, RegisteredAt, RegisteredByStaffId,
                       CreatedAt, CreatedBy, UpdatedAt, UpdatedBy)
SELECT cc.CandidateId, cc.UserId, cc.CandidateCode,
       N'So ' + CAST(200 + cc.Slot AS NVARCHAR(10)) + N' Hai Ba Trung, Q.1, TP.HCM',
       N'09320' + RIGHT('0000000' + CAST(cc.Slot * 17 AS NVARCHAR(7)), 7),
       -- Stash the ScoreOffset in Notes so chart/04 can re-derive it.
       N'ScoreOffset=' + CONVERT(NVARCHAR(20), cc.ScoreOffset),
       N'Active',
       DATEADD(day, -120, @Now),
       CASE cc.Slot % 3
           WHEN 0 THEN @CounselorStaff1
           WHEN 1 THEN @CounselorStaff2
           ELSE        @CounselorStaff3
       END,
       DATEADD(day, -120, @Now), @Seeder, NULL, NULL
FROM @ChartCandidates cc
WHERE NOT EXISTS (SELECT 1 FROM candidate x WHERE x.UserAccountId = cc.UserId);

INSERT INTO enrollment (Id, CandidateId, BatchId, InchargeId, FeeId, EnrollmentDate,
                        CourseId, Status, Notes, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy)
SELECT cc.EnrollmentId, cc.CandidateId, @ChartBatch,
       CASE cc.Slot % 2 WHEN 0 THEN @InchargeStaff1 ELSE @InchargeStaff2 END,
       NULL, DATEADD(day, -98, @Today), @CourseAccp, N'Enrolled', NULL,
       DATEADD(day, -98, @Now), @Seeder, NULL, NULL
FROM @ChartCandidates cc
WHERE NOT EXISTS (SELECT 1 FROM enrollment x WHERE x.Id = cc.EnrollmentId);

COMMIT TRAN;
PRINT 'chart/03-focus-batch-and-students.sql complete: 1 batch + 12 candidates + 12 enrollments.';
