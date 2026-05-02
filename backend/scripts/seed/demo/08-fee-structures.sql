/*
 * 08-fee-structures.sql — fee_structure rows for the seeded enrollments.
 *
 * Mix of FullPayment / Installment, Paid / Partial / Overdue / Unpaid so the
 * finance dashboards have meaningful spread. Plus one stand-alone fine for
 * candidate06 to cover the "Fine" fee type.
 *
 * OutstandingBalance is a computed column on the table, so it is NOT inserted.
 *
 * After insert, the script back-links each enrollment to its tuition fee via
 * enrollment.FeeId so the navigation between enrollment and fee_structure
 * works in the UI.
 *
 * Idempotent on Id. Requires enrollments.
 */

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRAN;

DECLARE @Now    DATETIME2(7)  = SYSUTCDATETIME();
DECLARE @Seeder NVARCHAR(100) = N'seed-demo-data';

DECLARE @Fees TABLE (
    Id            UNIQUEIDENTIFIER,
    CandidateId   UNIQUEIDENTIFIER,
    EnrollmentId  UNIQUEIDENTIFIER NULL,
    TotalFee      DECIMAL(12,2),
    AmountPaid    DECIMAL(12,2),
    FeeType       NVARCHAR(20),
    PaymentStatus NVARCHAR(20),
    PaymentType   NVARCHAR(20),
    Notes         NVARCHAR(MAX)
);

INSERT INTO @Fees VALUES
-- ACCP-2026-DEMO (BaseFee 32M)
('a1a1a1a1-0000-0000-0000-000000000001','cccccccc-0000-0000-0000-000000000001','f1f1f1f1-0000-0000-0000-000000000001', 32000000, 32000000, N'Tuition', N'Paid',    N'FullPayment', N'Da thanh toan toan bo'),
('a1a1a1a1-0000-0000-0000-000000000002','cccccccc-0000-0000-0000-000000000002','f1f1f1f1-0000-0000-0000-000000000002', 32000000, 16000000, N'Tuition', N'Partial', N'Installment', N'Tra gop 4 ky, da dong 2'),
('a1a1a1a1-0000-0000-0000-000000000003','cccccccc-0000-0000-0000-000000000003','f1f1f1f1-0000-0000-0000-000000000003', 32000000, 32000000, N'Tuition', N'Paid',    N'FullPayment', N'Hoc vien tot nghiep'),
('a1a1a1a1-0000-0000-0000-000000000004','cccccccc-0000-0000-0000-000000000004','f1f1f1f1-0000-0000-0000-000000000004', 32000000, 32000000, N'Tuition', N'Paid',    N'Installment', N'Da hoan tat 4 ky tra gop'),
('a1a1a1a1-0000-0000-0000-000000000005','cccccccc-0000-0000-0000-000000000005','f1f1f1f1-0000-0000-0000-000000000005', 32000000,  8000000, N'Tuition', N'Partial', N'Installment', NULL),
('a1a1a1a1-0000-0000-0000-000000000006','cccccccc-0000-0000-0000-000000000006','f1f1f1f1-0000-0000-0000-000000000006', 32000000,  8000000, N'Tuition', N'Overdue', N'Installment', N'Qua han ky 2'),
('a1a1a1a1-0000-0000-0000-000000000007','cccccccc-0000-0000-0000-000000000007','f1f1f1f1-0000-0000-0000-000000000007', 32000000,        0, N'Tuition', N'Unpaid',  N'NotSet',      N'Cho thanh toan'),
('a1a1a1a1-0000-0000-0000-000000000008','cccccccc-0000-0000-0000-000000000008','f1f1f1f1-0000-0000-0000-000000000008', 32000000, 32000000, N'Tuition', N'Paid',    N'FullPayment', NULL),
-- CPISM-2026-01 (BaseFee 18M)
('a1a1a1a1-0000-0000-0000-000000000009','cccccccc-0000-0000-0000-000000000009','f1f1f1f1-0000-0000-0000-000000000010', 18000000, 18000000, N'Tuition', N'Paid',    N'FullPayment', N'Hoc vien tot nghiep'),
('a1a1a1a1-0000-0000-0000-00000000000a','cccccccc-0000-0000-0000-000000000010','f1f1f1f1-0000-0000-0000-000000000011', 18000000, 12000000, N'Tuition', N'Partial', N'Installment', N'Tra gop 3 ky, da dong 2'),
-- WEBPRO-2026-01 (BaseFee 12M)
('a1a1a1a1-0000-0000-0000-00000000000b','cccccccc-0000-0000-0000-000000000012','f1f1f1f1-0000-0000-0000-000000000013', 12000000, 12000000, N'Tuition', N'Paid',    N'FullPayment', NULL),
-- HDSE-2026-01 (BaseFee 45M)
('a1a1a1a1-0000-0000-0000-00000000000c','cccccccc-0000-0000-0000-000000000017','f1f1f1f1-0000-0000-0000-000000000018', 45000000, 18000000, N'Tuition', N'Partial', N'Installment', N'Tra gop 5 ky, da dong 2'),
-- ACCP-2026-02
('a1a1a1a1-0000-0000-0000-00000000000d','cccccccc-0000-0000-0000-000000000019','f1f1f1f1-0000-0000-0000-00000000001c', 32000000,        0, N'Tuition', N'Unpaid',  N'NotSet',      N'Vua dang ky, chua dong'),
-- Stand-alone fine (no enrollment link) for candidate06
('a1a1a1a1-0000-0000-0000-00000000000e','cccccccc-0000-0000-0000-000000000006', NULL,                                       500000,        0, N'Fine',    N'Unpaid',  N'NotSet',      N'Phat tre han ky 2');

INSERT INTO fee_structure (Id, CandidateId, TotalFee, AmountPaid, FeeType,
                           PaymentStatus, PaymentType, Notes,
                           CreatedAt, CreatedBy, UpdatedAt, UpdatedBy)
SELECT f.Id, f.CandidateId, f.TotalFee, f.AmountPaid, f.FeeType,
       f.PaymentStatus, f.PaymentType, f.Notes,
       @Now, @Seeder, NULL, NULL
FROM @Fees f
WHERE NOT EXISTS (SELECT 1 FROM fee_structure x WHERE x.Id = f.Id);

-- Back-link enrollment.FeeId where the fee row carries an EnrollmentId.
UPDATE e
SET e.FeeId = f.Id, e.UpdatedAt = @Now, e.UpdatedBy = @Seeder
FROM enrollment e
INNER JOIN @Fees f ON f.EnrollmentId = e.Id
WHERE f.EnrollmentId IS NOT NULL AND e.FeeId IS NULL;

COMMIT TRAN;
PRINT '08-fee-structures.sql: 13 tuition + 1 fine fee_structure rows ensured.';
