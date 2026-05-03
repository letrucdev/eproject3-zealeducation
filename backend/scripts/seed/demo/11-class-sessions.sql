/*
 * 11-class-sessions.sql — class_session rows for active batches.
 *
 *   ACCP-2026-DEMO  8 sessions (1 cancelled, 1 future scheduled)
 *   CPISM / WEBPRO / ADIM / HDSE / ACCP-02   3-4 sessions each
 *
 * Past sessions are Completed; later than @Today are Scheduled. Unique by Id.
 * Requires batches.
 */

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRAN;

DECLARE @Now    DATETIME2(7)  = SYSUTCDATETIME();
DECLARE @Today  DATE          = CAST(SYSUTCDATETIME() AS DATE);
DECLARE @Seeder NVARCHAR(100) = N'seed-demo-data';

DECLARE @Sessions TABLE (
    Id          UNIQUEIDENTIFIER,
    BatchId     UNIQUEIDENTIFIER,
    DateOffset  INT,
    StartTime   TIME,
    EndTime     TIME,
    Topic       NVARCHAR(200),
    Location    NVARCHAR(100),
    Status      NVARCHAR(20)
);

INSERT INTO @Sessions VALUES
-- ACCP-2026-DEMO
('d1d1d1d1-0000-0000-0000-000000000001','eeeeeeee-0000-0000-0000-000000000003', -49, '18:00', '21:00', N'C/C++ Fundamentals',      N'Lab A', N'Completed'),
('d1d1d1d1-0000-0000-0000-000000000002','eeeeeeee-0000-0000-0000-000000000003', -42, '18:00', '21:00', N'Pointers & Memory',       N'Lab A', N'Completed'),
('d1d1d1d1-0000-0000-0000-000000000003','eeeeeeee-0000-0000-0000-000000000003', -35, '18:00', '21:00', N'OOP with Java - Intro',   N'Lab A', N'Completed'),
('d1d1d1d1-0000-0000-0000-000000000004','eeeeeeee-0000-0000-0000-000000000003', -28, '18:00', '21:00', N'Java Collections',        N'Lab A', N'Completed'),
('d1d1d1d1-0000-0000-0000-000000000005','eeeeeeee-0000-0000-0000-000000000003', -21, '18:00', '21:00', N'SQL & Database Design',   N'Lab A', N'Cancelled'),
('d1d1d1d1-0000-0000-0000-000000000006','eeeeeeee-0000-0000-0000-000000000003', -14, '18:00', '21:00', N'Web Front-end - HTML/CSS',N'Lab A', N'Completed'),
('d1d1d1d1-0000-0000-0000-000000000007','eeeeeeee-0000-0000-0000-000000000003',  -7, '18:00', '21:00', N'JavaScript & DOM',        N'Lab A', N'Completed'),
('d1d1d1d1-0000-0000-0000-000000000008','eeeeeeee-0000-0000-0000-000000000003',   3, '18:00', '21:00', N'.NET Core Introduction',  N'Lab A', N'Scheduled'),
-- CPISM-2026-01
('d1d1d1d1-0000-0000-0000-000000000010','eeeeeeee-0000-0000-0000-000000000001', -45, '18:00', '20:00', N'IT Foundations',          N'Room 201', N'Completed'),
('d1d1d1d1-0000-0000-0000-000000000011','eeeeeeee-0000-0000-0000-000000000001', -30, '18:00', '20:00', N'Static HTML & CSS',       N'Room 201', N'Completed'),
('d1d1d1d1-0000-0000-0000-000000000012','eeeeeeee-0000-0000-0000-000000000001', -15, '18:00', '20:00', N'Intro to Programming',    N'Room 201', N'Completed'),
('d1d1d1d1-0000-0000-0000-000000000013','eeeeeeee-0000-0000-0000-000000000001',   5, '18:00', '20:00', N'Database Basics',         N'Room 201', N'Scheduled'),
-- WEBPRO-2026-01
('d1d1d1d1-0000-0000-0000-000000000014','eeeeeeee-0000-0000-0000-000000000002', -40, '18:00', '20:00', N'HTML5 Semantics',         N'Room 305', N'Completed'),
('d1d1d1d1-0000-0000-0000-000000000015','eeeeeeee-0000-0000-0000-000000000002', -25, '18:00', '20:00', N'CSS Flex/Grid',           N'Room 305', N'Completed'),
('d1d1d1d1-0000-0000-0000-000000000016','eeeeeeee-0000-0000-0000-000000000002', -10, '18:00', '20:00', N'React Components',        N'Room 305', N'Completed'),
('d1d1d1d1-0000-0000-0000-000000000017','eeeeeeee-0000-0000-0000-000000000002',   2, '18:00', '20:00', N'React Hooks',             N'Room 305', N'Scheduled'),
-- ADIM-2026-01
('d1d1d1d1-0000-0000-0000-000000000018','eeeeeeee-0000-0000-0000-000000000004', -30, '18:00', '21:00', N'Photoshop Basics',        N'Room 401', N'Completed'),
('d1d1d1d1-0000-0000-0000-000000000019','eeeeeeee-0000-0000-0000-000000000004', -20, '18:00', '21:00', N'Illustrator Vectors',     N'Room 401', N'Completed'),
('d1d1d1d1-0000-0000-0000-00000000001a','eeeeeeee-0000-0000-0000-000000000004',   4, '18:00', '21:00', N'2D Animation Workflow',   N'Room 401', N'Scheduled'),
-- HDSE-2026-01
('d1d1d1d1-0000-0000-0000-00000000001c','eeeeeeee-0000-0000-0000-000000000006', -25, '18:00', '21:00', N'Algorithms & Big-O',      N'Room 102', N'Completed'),
('d1d1d1d1-0000-0000-0000-00000000001d','eeeeeeee-0000-0000-0000-000000000006', -15, '18:00', '21:00', N'Software Architecture',   N'Room 102', N'Completed'),
('d1d1d1d1-0000-0000-0000-00000000001e','eeeeeeee-0000-0000-0000-000000000006',   6, '18:00', '21:00', N'Microservices',           N'Room 102', N'Scheduled'),
-- ACCP-2026-02
('d1d1d1d1-0000-0000-0000-000000000020','eeeeeeee-0000-0000-0000-000000000007', -22, '18:00', '21:00', N'C/C++ Fundamentals',      N'Room 103', N'Completed'),
('d1d1d1d1-0000-0000-0000-000000000021','eeeeeeee-0000-0000-0000-000000000007', -12, '18:00', '21:00', N'OOP Basics',              N'Room 103', N'Completed'),
('d1d1d1d1-0000-0000-0000-000000000022','eeeeeeee-0000-0000-0000-000000000007',   7, '18:00', '21:00', N'Database Connectivity',   N'Room 103', N'Scheduled');

INSERT INTO class_session (Id, BatchId, SessionDate, StartTime, EndTime, Topic, Location,
                           Status, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy)
SELECT s.Id, s.BatchId, DATEADD(day, s.DateOffset, @Today),
       s.StartTime, s.EndTime, s.Topic, s.Location, s.Status,
       @Now, @Seeder, NULL, NULL
FROM @Sessions s
WHERE NOT EXISTS (SELECT 1 FROM class_session x WHERE x.Id = s.Id);

COMMIT TRAN;
PRINT '11-class-sessions.sql: 25 class_session rows ensured.';
