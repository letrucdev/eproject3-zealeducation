/*
 * 03-staff.sql — staff (15) wrapping each non-candidate user_account.
 *
 * IDs follow the pattern aaaaaaaa-...-NN where the high nibble of NN encodes
 * the role: 0x01 admin, 0x11 incharge, 0x21 faculty, 0x31 counselor, 0x41 accounts.
 * Other seed files reference these IDs directly.
 *
 * Idempotent on UserAccountId. Requires 02-user-accounts.sql to have run.
 */

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRAN;

DECLARE @Now    DATETIME2(7)  = SYSUTCDATETIME();
DECLARE @Seeder NVARCHAR(100) = N'seed-demo-data';

DECLARE @Staff TABLE (
    Id            UNIQUEIDENTIFIER,
    UserAccountId UNIQUEIDENTIFIER,
    Position      NVARCHAR(60),
    Department    NVARCHAR(60),
    JoinedDate    DATE
);

INSERT INTO @Staff VALUES
-- SystemAdmin
('aaaaaaaa-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001', N'System Administrator',     N'IT Operations', '2018-04-02'),
('aaaaaaaa-0000-0000-0000-000000000002', '00000000-0000-0000-0000-000000000002', N'Senior System Admin',      N'IT Operations', '2019-08-12'),
('aaaaaaaa-0000-0000-0000-000000000003', '00000000-0000-0000-0000-000000000003', N'IT Support Lead',          N'IT Operations', '2020-01-15'),
-- Incharge
('aaaaaaaa-0000-0000-0000-000000000011', '11111111-0000-0000-0000-000000000001', N'Center Incharge',          N'Academics',     '2017-06-10'),
('aaaaaaaa-0000-0000-0000-000000000012', '11111111-0000-0000-0000-000000000002', N'Academic Coordinator',     N'Academics',     '2019-03-22'),
('aaaaaaaa-0000-0000-0000-000000000013', '11111111-0000-0000-0000-000000000003', N'Academic Coordinator',     N'Academics',     '2021-09-01'),
-- Faculty
('aaaaaaaa-0000-0000-0000-000000000021', '22222222-0000-0000-0000-000000000001', N'Senior Faculty - Java',    N'Faculty',       '2016-02-18'),
('aaaaaaaa-0000-0000-0000-000000000022', '22222222-0000-0000-0000-000000000002', N'Faculty - .NET',           N'Faculty',       '2018-11-05'),
('aaaaaaaa-0000-0000-0000-000000000023', '22222222-0000-0000-0000-000000000003', N'Faculty - Web/UI',         N'Faculty',       '2020-07-20'),
-- Counselor
('aaaaaaaa-0000-0000-0000-000000000031', '33333333-0000-0000-0000-000000000001', N'Senior Counselor',         N'Admissions',    '2018-05-14'),
('aaaaaaaa-0000-0000-0000-000000000032', '33333333-0000-0000-0000-000000000002', N'Course Counselor',         N'Admissions',    '2020-10-30'),
('aaaaaaaa-0000-0000-0000-000000000033', '33333333-0000-0000-0000-000000000003', N'Course Counselor',         N'Admissions',    '2022-02-08'),
-- AccountsStaff
('aaaaaaaa-0000-0000-0000-000000000041', '44444444-0000-0000-0000-000000000001', N'Accounts Manager',         N'Finance',       '2017-09-25'),
('aaaaaaaa-0000-0000-0000-000000000042', '44444444-0000-0000-0000-000000000002', N'Accountant',               N'Finance',       '2019-12-03'),
('aaaaaaaa-0000-0000-0000-000000000043', '44444444-0000-0000-0000-000000000003', N'Accountant',               N'Finance',       '2021-04-19');

INSERT INTO staff (Id, UserAccountId, Position, Department, JoinedDate, IsActive,
                   CreatedAt, CreatedBy, UpdatedAt, UpdatedBy)
SELECT s.Id, s.UserAccountId, s.Position, s.Department, s.JoinedDate, 1,
       @Now, @Seeder, NULL, NULL
FROM @Staff s
WHERE NOT EXISTS (SELECT 1 FROM staff x WHERE x.UserAccountId = s.UserAccountId);

COMMIT TRAN;
PRINT '03-staff.sql: 15 staff rows ensured.';
