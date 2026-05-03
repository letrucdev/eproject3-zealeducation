/*
 * 15-course-enquiries.sql — sales pipeline rows + a couple of follow-up notes.
 * Status mix: New / Contacted / InFollowUp / Interested / Closed.
 *
 * Idempotent on Phone (course_enquiry has a unique index there) and Note Id.
 */

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRAN;

DECLARE @Now    DATETIME2(7)  = SYSUTCDATETIME();
DECLARE @Today  DATE          = CAST(SYSUTCDATETIME() AS DATE);
DECLARE @Seeder NVARCHAR(100) = N'seed-demo-data';

DECLARE @Enquiries TABLE (
    Id              UNIQUEIDENTIFIER,
    FullName        NVARCHAR(100),
    Phone           NVARCHAR(20),
    Email           NVARCHAR(100),
    CourseId        UNIQUEIDENTIFIER,
    Source          NVARCHAR(30),
    Status          NVARCHAR(30),
    NextFollowUp    DATE,
    CounselorId     UNIQUEIDENTIFIER
);

INSERT INTO @Enquiries VALUES
('f7f7f7f7-0000-0000-0000-000000000001', N'Vu Hoang Nam',     N'0921000001', N'namvu@gmail.com',     'dddddddd-0000-0000-0000-000000000001', N'Website',     N'New',         DATEADD(day, 2,@Today), 'aaaaaaaa-0000-0000-0000-000000000031'),
('f7f7f7f7-0000-0000-0000-000000000002', N'Le Phuong Nhi',    N'0921000002', N'nhile@gmail.com',     'dddddddd-0000-0000-0000-000000000002', N'Phone',       N'Contacted',   DATEADD(day, 3,@Today), 'aaaaaaaa-0000-0000-0000-000000000031'),
('f7f7f7f7-0000-0000-0000-000000000003', N'Tran Anh Tuan',    N'0921000003', N'tuantran@gmail.com',  'dddddddd-0000-0000-0000-000000000003', N'Referral',    N'InFollowUp',  DATEADD(day, 5,@Today), 'aaaaaaaa-0000-0000-0000-000000000032'),
('f7f7f7f7-0000-0000-0000-000000000004', N'Pham Bao Thy',     N'0921000004', N'thypham@gmail.com',   'dddddddd-0000-0000-0000-000000000005', N'WalkIn',      N'Interested',  DATEADD(day, 1,@Today), 'aaaaaaaa-0000-0000-0000-000000000032'),
('f7f7f7f7-0000-0000-0000-000000000005', N'Hoang Quoc Cuong', N'0921000005', N'cuonghoang@gmail.com','dddddddd-0000-0000-0000-000000000004', N'SocialMedia', N'New',         DATEADD(day, 4,@Today), 'aaaaaaaa-0000-0000-0000-000000000033'),
('f7f7f7f7-0000-0000-0000-000000000006', N'Nguyen Thi Quynh', N'0921000006', N'quynhnt@gmail.com',   'dddddddd-0000-0000-0000-000000000001', N'Website',     N'Closed',      NULL,                   'aaaaaaaa-0000-0000-0000-000000000033'),
('f7f7f7f7-0000-0000-0000-000000000007', N'Bui Tien Phong',   N'0921000007', N'phongbui@gmail.com',  'dddddddd-0000-0000-0000-000000000006', N'Phone',       N'Contacted',   DATEADD(day, 7,@Today), 'aaaaaaaa-0000-0000-0000-000000000031');

INSERT INTO course_enquiry (Id, FullName, Phone, Email, CourseInterestedId, Source, Status,
                            NextFollowUpDate, AssignedCounselorId, ConvertedCandidateId,
                            ConvertedAt, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy)
SELECT e.Id, e.FullName, e.Phone, e.Email, e.CourseId, e.Source, e.Status,
       e.NextFollowUp, e.CounselorId, NULL, NULL,
       DATEADD(day, -7, @Now), @Seeder, NULL, NULL
FROM @Enquiries e
WHERE NOT EXISTS (SELECT 1 FROM course_enquiry x WHERE x.Phone = e.Phone);

-- Two follow-up notes so the enquiry detail screen isn't empty.
INSERT INTO enquiry_note (Id, EnquiryId, AuthorStaffId, Content,
                          CreatedAt, CreatedBy, UpdatedAt, UpdatedBy)
SELECT * FROM (VALUES
    (CAST('f7f7f7f7-1000-0000-0000-000000000001' AS UNIQUEIDENTIFIER),
     CAST('f7f7f7f7-0000-0000-0000-000000000002' AS UNIQUEIDENTIFIER),
     CAST('aaaaaaaa-0000-0000-0000-000000000031' AS UNIQUEIDENTIFIER),
     N'Da goi, hen tu van truc tiep tuan toi.',
     @Now, @Seeder, CAST(NULL AS DATETIME2), CAST(NULL AS NVARCHAR(100))),
    (CAST('f7f7f7f7-1000-0000-0000-000000000002' AS UNIQUEIDENTIFIER),
     CAST('f7f7f7f7-0000-0000-0000-000000000004' AS UNIQUEIDENTIFIER),
     CAST('aaaaaaaa-0000-0000-0000-000000000032' AS UNIQUEIDENTIFIER),
     N'Ung vien quan tam ban hoc cuoi tuan, gui them thong tin hoc phi.',
     @Now, @Seeder, NULL, NULL)
) v(Id, EnquiryId, AuthorStaffId, Content, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy)
WHERE NOT EXISTS (SELECT 1 FROM enquiry_note n WHERE n.Id = v.Id);

COMMIT TRAN;
PRINT '15-course-enquiries.sql: 7 enquiries + 2 notes ensured.';
