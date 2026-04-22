/*
 * Seed 20 staff accounts (user_account + staff) mirroring the logic in
 *   CreateStaffCommandHandler (Features/Staffs/Commands/CreateStaff).
 *
 * Password for every account: Password@123
 * Hash below is generated with BCrypt.Net-Next (work factor 11) — the same
 * library used by BCryptPasswordHasher.
 *
 * Role distribution (UserRole enum stored as string by EF config):
 *   SystemAdmin   x 2
 *   Incharge      x 4
 *   Faculty       x 5   (also inserts a matching row into faculty)
 *   Counselor     x 4
 *   AccountsStaff x 5
 *
 * Target DB: SQL Server (matches Infrastructure migrations).
 * Idempotent: skips any row whose Username/Email/Phone already exists.
 */

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRAN;

/* Password@123 */
DECLARE @Pwd NVARCHAR(255) = N'$2a$11$VLjZACoh2waqa7TNfusFx.KyY9OSQdGL0uGCfOG2PfRLuc8kIQdUe';
DECLARE @Now DATETIME2 = SYSUTCDATETIME();
DECLARE @Today DATE = CAST(SYSUTCDATETIME() AS DATE);
DECLARE @CreatedBy NVARCHAR(100) = N'seed-script';

DECLARE @Seed TABLE (
    UserId          UNIQUEIDENTIFIER,
    StaffId         UNIQUEIDENTIFIER,
    Username        NVARCHAR(50),
    FullName        NVARCHAR(100),
    Email           NVARCHAR(100),
    Phone           NVARCHAR(20),
    Dob             DATE,
    Gender          NVARCHAR(10),
    Role            NVARCHAR(30),
    Position        NVARCHAR(60),
    Department      NVARCHAR(60),
    JoinedDate      DATE
);

INSERT INTO @Seed VALUES
-- SystemAdmin (2)
('11111111-1111-1111-1111-000000000001','22222222-2222-2222-2222-000000000001','admin.root',     N'Nguyen Van An',     'an.nguyen@zeal.edu.vn',      '0901000001','1985-03-12','Male',  'SystemAdmin',  'System Administrator',    'IT',          '2020-01-15'),
('11111111-1111-1111-1111-000000000002','22222222-2222-2222-2222-000000000002','admin.ops',      N'Tran Thi Bich',     'bich.tran@zeal.edu.vn',      '0901000002','1988-07-22','Female','SystemAdmin',  'Senior System Admin',     'IT',          '2021-04-01'),

-- Incharge (4)
('11111111-1111-1111-1111-000000000003','22222222-2222-2222-2222-000000000003','incharge.hr',    N'Le Minh Cuong',     'cuong.le@zeal.edu.vn',       '0901000003','1982-11-05','Male',  'Incharge',     'HR Manager',              'Human Resources','2019-06-10'),
('11111111-1111-1111-1111-000000000004','22222222-2222-2222-2222-000000000004','incharge.ops',   N'Pham Thi Dung',     'dung.pham@zeal.edu.vn',      '0901000004','1984-02-18','Female','Incharge',     'Operations Manager',      'Operations',  '2019-09-03'),
('11111111-1111-1111-1111-000000000005','22222222-2222-2222-2222-000000000005','incharge.acad',  N'Hoang Duc Thang',   'thang.hoang@zeal.edu.vn',    '0901000005','1980-05-27','Male',  'Incharge',     'Academic Manager',        'Academics',   '2018-08-20'),
('11111111-1111-1111-1111-000000000006','22222222-2222-2222-2222-000000000006','incharge.adm',   N'Vu Thi Hoa',        'hoa.vu@zeal.edu.vn',         '0901000006','1986-09-14','Female','Incharge',     'Admission Manager',       'Admissions',  '2020-02-01'),

-- Faculty (5)
('11111111-1111-1111-1111-000000000007','22222222-2222-2222-2222-000000000007','fac.math01',     N'Do Van Khanh',      'khanh.do@zeal.edu.vn',       '0901000007','1983-12-01','Male',  'Faculty',      'Senior Lecturer',         'Mathematics', '2019-02-15'),
('11111111-1111-1111-1111-000000000008','22222222-2222-2222-2222-000000000008','fac.phys01',     N'Nguyen Thi Lan',    'lan.nguyen@zeal.edu.vn',     '0901000008','1987-06-08','Female','Faculty',      'Lecturer',                'Physics',     '2020-08-10'),
('11111111-1111-1111-1111-000000000009','22222222-2222-2222-2222-000000000009','fac.cs01',       N'Bui Hoang Nam',     'nam.bui@zeal.edu.vn',        '0901000009','1985-10-23','Male',  'Faculty',      'Senior Lecturer',         'Computer Science','2018-03-12'),
('11111111-1111-1111-1111-00000000000A','22222222-2222-2222-2222-00000000000A','fac.eng01',      N'Ngo Thi My',        'my.ngo@zeal.edu.vn',         '0901000010','1989-01-30','Female','Faculty',      'Lecturer',                'English',     '2021-07-05'),
('11111111-1111-1111-1111-00000000000B','22222222-2222-2222-2222-00000000000B','fac.chem01',     N'Ly Quoc Phong',     'phong.ly@zeal.edu.vn',       '0901000011','1984-04-17','Male',  'Faculty',      'Associate Professor',     'Chemistry',   '2017-11-22'),

-- Counselor (4)
('11111111-1111-1111-1111-00000000000C','22222222-2222-2222-2222-00000000000C','cou.admissions', N'Dang Thi Quyen',    'quyen.dang@zeal.edu.vn',     '0901000012','1990-03-11','Female','Counselor',    'Admissions Counselor',    'Admissions',  '2022-01-09'),
('11111111-1111-1111-1111-00000000000D','22222222-2222-2222-2222-00000000000D','cou.career',     N'Truong Van Son',    'son.truong@zeal.edu.vn',     '0901000013','1988-08-25','Male',  'Counselor',    'Career Counselor',        'Student Affairs','2021-03-14'),
('11111111-1111-1111-1111-00000000000E','22222222-2222-2222-2222-00000000000E','cou.student',    N'Phan Thi Thu',      'thu.phan@zeal.edu.vn',       '0901000014','1991-12-07','Female','Counselor',    'Student Counselor',       'Student Affairs','2022-06-20'),
('11111111-1111-1111-1111-00000000000F','22222222-2222-2222-2222-00000000000F','cou.acad',       N'Trinh Minh Uyen',   'uyen.trinh@zeal.edu.vn',     '0901000015','1989-05-19','Female','Counselor',    'Academic Counselor',      'Academics',   '2020-10-05'),

-- AccountsStaff (5)
('11111111-1111-1111-1111-000000000010','22222222-2222-2222-2222-000000000010','acc.bill01',     N'Mai Van Vinh',      'vinh.mai@zeal.edu.vn',       '0901000016','1986-07-03','Male',  'AccountsStaff','Billing Officer',         'Finance',     '2019-12-01'),
('11111111-1111-1111-1111-000000000011','22222222-2222-2222-2222-000000000011','acc.pay01',      N'Chu Thi Xuan',      'xuan.chu@zeal.edu.vn',       '0901000017','1988-09-29','Female','AccountsStaff','Payroll Officer',         'Finance',     '2020-05-18'),
('11111111-1111-1111-1111-000000000012','22222222-2222-2222-2222-000000000012','acc.aud01',      N'Dinh Quoc Yen',     'yen.dinh@zeal.edu.vn',       '0901000018','1985-11-12','Male',  'AccountsStaff','Internal Auditor',        'Finance',     '2018-10-25'),
('11111111-1111-1111-1111-000000000013','22222222-2222-2222-2222-000000000013','acc.rec01',      N'Ha Thi Bao',        'bao.ha@zeal.edu.vn',         '0901000019','1992-02-28','Female','AccountsStaff','Accounts Receivable',     'Finance',     '2022-08-15'),
('11111111-1111-1111-1111-000000000014','22222222-2222-2222-2222-000000000014','acc.tax01',      N'Duong Van Chi',     'chi.duong@zeal.edu.vn',      '0901000020','1987-04-06','Male',  'AccountsStaff','Tax Officer',             'Finance',     '2021-11-02');

----------------------------------------------------------------------
-- 1) Insert into user_account (skip rows that collide on unique keys)
----------------------------------------------------------------------
INSERT INTO user_account (
    Id, Username, PasswordHash, FullName, Email, Phone,
    Dob, Gender, Role, IsActive, FailedLoginCount, LastLogin,
    CreatedAt, CreatedBy, UpdatedAt, UpdatedBy
)
SELECT
    s.UserId, s.Username, @Pwd, s.FullName, s.Email, s.Phone,
    s.Dob, s.Gender, s.Role, 1, 0, NULL,
    @Now, @CreatedBy, NULL, NULL
FROM @Seed s
WHERE NOT EXISTS (
    SELECT 1 FROM user_account u
    WHERE u.Username = s.Username
       OR u.Email    = s.Email
       OR u.Phone    = s.Phone
);

----------------------------------------------------------------------
-- 2) Insert into staff (only for UserAccounts that got created and
--    don't already have a staff row)
----------------------------------------------------------------------
INSERT INTO staff (
    Id, UserAccountId, Position, Department, JoinedDate, IsActive,
    CreatedAt, CreatedBy, UpdatedAt, UpdatedBy
)
SELECT
    s.StaffId, s.UserId, s.Position, s.Department, s.JoinedDate, 1,
    @Now, @CreatedBy, NULL, NULL
FROM @Seed s
INNER JOIN user_account u ON u.Id = s.UserId
WHERE NOT EXISTS (
    SELECT 1 FROM staff st WHERE st.UserAccountId = s.UserId
);

----------------------------------------------------------------------
-- 3) Insert into faculty (one row per Faculty-role staff above).
--    Mirrors CreateFacultyCommandHandler: faculty links to an existing
--    Staff via StaffId, plus FacultyCode/Qualification/Specialization/
--    ExperienceYears. StaffIds and FacultyCodes below line up with the
--    five Faculty rows in @Seed.
----------------------------------------------------------------------
DECLARE @FacultySeed TABLE (
    FacultyId       UNIQUEIDENTIFIER,
    StaffId         UNIQUEIDENTIFIER,
    FacultyCode     NVARCHAR(20),
    Qualification   NVARCHAR(100),
    Specialization  NVARCHAR(100),
    ExperienceYears INT
);

INSERT INTO @FacultySeed VALUES
('33333333-3333-3333-3333-000000000007','22222222-2222-2222-2222-000000000007','FAC-MATH-01', N'PhD in Mathematics',           N'Algebra & Number Theory',        12),
('33333333-3333-3333-3333-000000000008','22222222-2222-2222-2222-000000000008','FAC-PHYS-01', N'MSc in Physics',               N'Quantum Mechanics',               6),
('33333333-3333-3333-3333-000000000009','22222222-2222-2222-2222-000000000009','FAC-CS-01',   N'PhD in Computer Science',      N'Artificial Intelligence',        10),
('33333333-3333-3333-3333-00000000000A','22222222-2222-2222-2222-00000000000A','FAC-ENG-01',  N'MA in English Literature',     N'Applied Linguistics',             5),
('33333333-3333-3333-3333-00000000000B','22222222-2222-2222-2222-00000000000B','FAC-CHEM-01', N'PhD in Chemistry',             N'Organic Chemistry',              15);

INSERT INTO faculty (
    Id, StaffId, FacultyCode, Qualification, Specialization, ExperienceYears,
    CreatedAt, CreatedBy, UpdatedAt, UpdatedBy
)
SELECT
    f.FacultyId, f.StaffId, f.FacultyCode, f.Qualification, f.Specialization, f.ExperienceYears,
    @Now, @CreatedBy, NULL, NULL
FROM @FacultySeed f
INNER JOIN staff st ON st.Id = f.StaffId
WHERE NOT EXISTS (
    SELECT 1 FROM faculty fa
    WHERE fa.StaffId = f.StaffId
       OR fa.FacultyCode = f.FacultyCode
);

COMMIT TRAN;

PRINT 'Seed complete. Password for every seeded account: Password@123';
