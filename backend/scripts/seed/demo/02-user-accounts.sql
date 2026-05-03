/*
 * 02-user-accounts.sql — login accounts for every actor.
 *
 *   - 3 SystemAdmin / 3 Incharge / 3 Faculty / 3 Counselor / 3 AccountsStaff
 *   - 20 candidate user accounts (CreatedAt ladder spread across last 90 days,
 *     just enough so the basic UI lists & detail screens have data; the chart
 *     candidate-registration trend is fed by the chart/ scripts, not these).
 *
 * Default password for all accounts: "Password123!"
 *   (BCrypt-Net cost 11 hash; matches Infrastructure verification.)
 *
 * Idempotent on Username. The UPDATE at the top resets the password on every
 * run in case an earlier seed wrote a stale hash.
 */

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRAN;

DECLARE @Now    DATETIME2(7)  = SYSUTCDATETIME();
DECLARE @Seeder NVARCHAR(100) = N'seed-demo-data';
DECLARE @DefaultPasswordHash NVARCHAR(255) =
    N'$2a$11$0a1gHPHCr3zOzvF2ldRA4uuNcyMkDxPqFoJqVZboojsEKMQ7OLrwy';

-- Force-reset any previously-seeded passwords back to the working default.
UPDATE user_account
SET PasswordHash = @DefaultPasswordHash, FailedLoginCount = 0
WHERE CreatedBy = @Seeder;

DECLARE @Users TABLE (
    Id            UNIQUEIDENTIFIER,
    Username      NVARCHAR(50),
    FullName      NVARCHAR(100),
    Email         NVARCHAR(100),
    Phone         NVARCHAR(20),
    Dob           DATE,
    Gender        NVARCHAR(10),
    Role          NVARCHAR(30),
    RegisteredAt  DATETIME2(7) NULL
);

-- Staff (15)
INSERT INTO @Users VALUES
('00000000-0000-0000-0000-000000000001', N'admin01',  N'Nguyen Van An',  N'admin01@zealedu.vn',  N'0901000001', '1985-03-12', N'Male',   N'SystemAdmin',   NULL),
('00000000-0000-0000-0000-000000000002', N'admin02',  N'Tran Thi Bich',  N'admin02@zealedu.vn',  N'0901000002', '1988-07-05', N'Female', N'SystemAdmin',   NULL),
('00000000-0000-0000-0000-000000000003', N'admin03',  N'Le Hoang Anh',   N'admin03@zealedu.vn',  N'0901000003', '1990-11-22', N'Male',   N'SystemAdmin',   NULL),
('11111111-0000-0000-0000-000000000001', N'incharge01', N'Pham Quoc Viet', N'incharge01@zealedu.vn', N'0902000001', '1986-05-18', N'Male',   N'Incharge', NULL),
('11111111-0000-0000-0000-000000000002', N'incharge02', N'Vu Thi Mai',     N'incharge02@zealedu.vn', N'0902000002', '1989-09-30', N'Female', N'Incharge', NULL),
('11111111-0000-0000-0000-000000000003', N'incharge03', N'Dang Hong Son',  N'incharge03@zealedu.vn', N'0902000003', '1984-02-14', N'Male',   N'Incharge', NULL),
('22222222-0000-0000-0000-000000000001', N'faculty01', N'Nguyen Huu Hung', N'faculty01@zealedu.vn', N'0903000001', '1982-04-10', N'Male',   N'Faculty',  NULL),
('22222222-0000-0000-0000-000000000002', N'faculty02', N'Tran Minh Duc',   N'faculty02@zealedu.vn', N'0903000002', '1987-08-25', N'Male',   N'Faculty',  NULL),
('22222222-0000-0000-0000-000000000003', N'faculty03', N'Le Thi Thanh',    N'faculty03@zealedu.vn', N'0903000003', '1990-12-03', N'Female', N'Faculty',  NULL),
('33333333-0000-0000-0000-000000000001', N'counselor01', N'Bui Thanh Ha',  N'counselor01@zealedu.vn', N'0904000001', '1991-06-15', N'Female', N'Counselor', NULL),
('33333333-0000-0000-0000-000000000002', N'counselor02', N'Do Van Phuc',   N'counselor02@zealedu.vn', N'0904000002', '1989-10-08', N'Male',   N'Counselor', NULL),
('33333333-0000-0000-0000-000000000003', N'counselor03', N'Phan Thi Lan',  N'counselor03@zealedu.vn', N'0904000003', '1992-01-21', N'Female', N'Counselor', NULL),
('44444444-0000-0000-0000-000000000001', N'accounts01', N'Hoang Minh Tam', N'accounts01@zealedu.vn', N'0905000001', '1986-09-17', N'Male',   N'AccountsStaff', NULL),
('44444444-0000-0000-0000-000000000002', N'accounts02', N'Ngo Thi Yen',    N'accounts02@zealedu.vn', N'0905000002', '1990-03-29', N'Female', N'AccountsStaff', NULL),
('44444444-0000-0000-0000-000000000003', N'accounts03', N'Trinh Van Khoa', N'accounts03@zealedu.vn', N'0905000003', '1988-11-04', N'Male',   N'AccountsStaff', NULL);

-- Candidate accounts (20) — minimal, just enough to back the candidate / enrollment
-- detail screens. Distribution is a simple ladder; the rich daily chart series
-- is produced by chart/02-candidate-registration-trend.sql.
INSERT INTO @Users VALUES
('55555555-0000-0000-0000-000000000001', N'candidate01', N'Nguyen Thi Huong', N'candidate01@zealedu.vn', N'0911000001', '2003-04-12', N'Female', N'Candidate', DATEADD(day,-85,@Now)),
('55555555-0000-0000-0000-000000000002', N'candidate02', N'Tran Van Khanh',   N'candidate02@zealedu.vn', N'0911000002', '2002-09-23', N'Male',   N'Candidate', DATEADD(day,-78,@Now)),
('55555555-0000-0000-0000-000000000003', N'candidate03', N'Le Quoc Bao',      N'candidate03@zealedu.vn', N'0911000003', '2003-01-15', N'Male',   N'Candidate', DATEADD(day,-72,@Now)),
('55555555-0000-0000-0000-000000000004', N'candidate04', N'Pham Thi Linh',    N'candidate04@zealedu.vn', N'0911000004', '2004-06-08', N'Female', N'Candidate', DATEADD(day,-65,@Now)),
('55555555-0000-0000-0000-000000000005', N'candidate05', N'Hoang Minh Quan',  N'candidate05@zealedu.vn', N'0911000005', '2003-11-02', N'Male',   N'Candidate', DATEADD(day,-58,@Now)),
('55555555-0000-0000-0000-000000000006', N'candidate06', N'Vu Thanh Tung',    N'candidate06@zealedu.vn', N'0911000006', '2002-08-19', N'Male',   N'Candidate', DATEADD(day,-52,@Now)),
('55555555-0000-0000-0000-000000000007', N'candidate07', N'Do Thi Ngoc',      N'candidate07@zealedu.vn', N'0911000007', '2004-02-11', N'Female', N'Candidate', DATEADD(day,-45,@Now)),
('55555555-0000-0000-0000-000000000008', N'candidate08', N'Bui Van Hieu',     N'candidate08@zealedu.vn', N'0911000008', '2003-07-28', N'Male',   N'Candidate', DATEADD(day,-40,@Now)),
('55555555-0000-0000-0000-000000000009', N'candidate09', N'Dang Ngoc Diep',   N'candidate09@zealedu.vn', N'0911000009', '2002-12-30', N'Female', N'Candidate', DATEADD(day,-36,@Now)),
('55555555-0000-0000-0000-000000000010', N'candidate10', N'Phan Tien Dung',   N'candidate10@zealedu.vn', N'0911000010', '2003-03-17', N'Male',   N'Candidate', DATEADD(day,-32,@Now)),
('55555555-0000-0000-0000-000000000011', N'candidate11', N'Nguyen Van Thinh', N'candidate11@zealedu.vn', N'0911000011', '2004-05-21', N'Male',   N'Candidate', DATEADD(day,-28,@Now)),
('55555555-0000-0000-0000-000000000012', N'candidate12', N'Tran Thi Huyen',   N'candidate12@zealedu.vn', N'0911000012', '2003-10-09', N'Female', N'Candidate', DATEADD(day,-25,@Now)),
('55555555-0000-0000-0000-000000000013', N'candidate13', N'Le Hoang Khoi',    N'candidate13@zealedu.vn', N'0911000013', '2002-04-04', N'Male',   N'Candidate', DATEADD(day,-22,@Now)),
('55555555-0000-0000-0000-000000000014', N'candidate14', N'Pham Ngoc Anh',    N'candidate14@zealedu.vn', N'0911000014', '2003-08-13', N'Female', N'Candidate', DATEADD(day,-19,@Now)),
('55555555-0000-0000-0000-000000000015', N'candidate15', N'Hoang Van Tai',    N'candidate15@zealedu.vn', N'0911000015', '2004-01-26', N'Male',   N'Candidate', DATEADD(day,-16,@Now)),
('55555555-0000-0000-0000-000000000016', N'candidate16', N'Vu Thi Hong',      N'candidate16@zealedu.vn', N'0911000016', '2003-06-07', N'Female', N'Candidate', DATEADD(day,-13,@Now)),
('55555555-0000-0000-0000-000000000017', N'candidate17', N'Do Quang Minh',    N'candidate17@zealedu.vn', N'0911000017', '2002-11-19', N'Male',   N'Candidate', DATEADD(day,-10,@Now)),
('55555555-0000-0000-0000-000000000018', N'candidate18', N'Bui Thi Nhi',      N'candidate18@zealedu.vn', N'0911000018', '2004-03-24', N'Female', N'Candidate', DATEADD(day, -7,@Now)),
('55555555-0000-0000-0000-000000000019', N'candidate19', N'Dang Van Long',    N'candidate19@zealedu.vn', N'0911000019', '2003-09-01', N'Male',   N'Candidate', DATEADD(day, -4,@Now)),
('55555555-0000-0000-0000-000000000020', N'candidate20', N'Phan Thi Tuyet',   N'candidate20@zealedu.vn', N'0911000020', '2002-05-16', N'Female', N'Candidate', DATEADD(day, -1,@Now));

INSERT INTO user_account (Id, Username, PasswordHash, FullName, Email, Phone, Dob, Gender,
                          Role, IsActive, MustChangePassword, FailedLoginCount, LastLogin,
                          CreatedAt, CreatedBy, UpdatedAt, UpdatedBy)
SELECT u.Id, u.Username, @DefaultPasswordHash, u.FullName, u.Email, u.Phone, u.Dob, u.Gender,
       u.Role, 1, 0, 0, NULL,
       COALESCE(u.RegisteredAt, @Now), @Seeder, NULL, NULL
FROM @Users u
WHERE NOT EXISTS (SELECT 1 FROM user_account x WHERE x.Username = u.Username);

COMMIT TRAN;
PRINT '02-user-accounts.sql: 15 staff + 20 candidate user_account rows ensured.';
