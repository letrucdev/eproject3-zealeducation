/*
 * 09-installment-plans.sql — installment_plan rows for fees with PaymentType
 * Installment. Mix of Paid / Pending / Overdue so the installments table UI
 * and reminder logic both have realistic input.
 *
 * Idempotent on Id. Requires fee_structure.
 */

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRAN;

DECLARE @Now    DATETIME2(7)  = SYSUTCDATETIME();
DECLARE @Today  DATE          = CAST(SYSUTCDATETIME() AS DATE);
DECLARE @Seeder NVARCHAR(100) = N'seed-demo-data';

DECLARE @Installments TABLE (
    Id              UNIQUEIDENTIFIER,
    FeeId           UNIQUEIDENTIFIER,
    InstallmentNo   INT,
    AmountDue       DECIMAL(12,2),
    DueOffsetDays   INT,
    AmountPaid      DECIMAL(12,2),
    PaidOffsetDays  INT NULL,
    Status          NVARCHAR(15),
    PenaltyAmount   DECIMAL(12,2)
);

INSERT INTO @Installments VALUES
-- candidate02 ACCP-DEMO 4x8M, paid 2/4
('b1b1b1b1-0000-0000-0000-000000000001','a1a1a1a1-0000-0000-0000-000000000002', 1, 8000000, -60, 8000000,  -58, N'Paid',    0),
('b1b1b1b1-0000-0000-0000-000000000002','a1a1a1a1-0000-0000-0000-000000000002', 2, 8000000, -30, 8000000,  -28, N'Paid',    0),
('b1b1b1b1-0000-0000-0000-000000000003','a1a1a1a1-0000-0000-0000-000000000002', 3, 8000000,   0,       0, NULL, N'Pending', 0),
('b1b1b1b1-0000-0000-0000-000000000004','a1a1a1a1-0000-0000-0000-000000000002', 4, 8000000,  30,       0, NULL, N'Pending', 0),
-- candidate04 ACCP-DEMO 4x8M, all paid
('b1b1b1b1-0000-0000-0000-000000000005','a1a1a1a1-0000-0000-0000-000000000004', 1, 8000000, -60, 8000000,  -59, N'Paid',    0),
('b1b1b1b1-0000-0000-0000-000000000006','a1a1a1a1-0000-0000-0000-000000000004', 2, 8000000, -30, 8000000,  -29, N'Paid',    0),
('b1b1b1b1-0000-0000-0000-000000000007','a1a1a1a1-0000-0000-0000-000000000004', 3, 8000000,  -3, 8000000,   -2, N'Paid',    0),
('b1b1b1b1-0000-0000-0000-000000000008','a1a1a1a1-0000-0000-0000-000000000004', 4, 8000000,  30, 8000000,   -1, N'Paid',    0),
-- candidate05 ACCP-DEMO 4x8M, paid 1/4
('b1b1b1b1-0000-0000-0000-000000000009','a1a1a1a1-0000-0000-0000-000000000005', 1, 8000000, -45, 8000000,  -42, N'Paid',    0),
('b1b1b1b1-0000-0000-0000-00000000000a','a1a1a1a1-0000-0000-0000-000000000005', 2, 8000000, -15,       0, NULL, N'Pending', 0),
('b1b1b1b1-0000-0000-0000-00000000000b','a1a1a1a1-0000-0000-0000-000000000005', 3, 8000000,  15,       0, NULL, N'Pending', 0),
('b1b1b1b1-0000-0000-0000-00000000000c','a1a1a1a1-0000-0000-0000-000000000005', 4, 8000000,  45,       0, NULL, N'Pending', 0),
-- candidate06 ACCP-DEMO 4x8M, ky 2 OVERDUE (penalty 500K)
('b1b1b1b1-0000-0000-0000-00000000000d','a1a1a1a1-0000-0000-0000-000000000006', 1, 8000000, -60, 8000000,  -58, N'Paid',    0),
('b1b1b1b1-0000-0000-0000-00000000000e','a1a1a1a1-0000-0000-0000-000000000006', 2, 8000000, -10,       0, NULL, N'Overdue', 500000),
('b1b1b1b1-0000-0000-0000-00000000000f','a1a1a1a1-0000-0000-0000-000000000006', 3, 8000000,  20,       0, NULL, N'Pending', 0),
('b1b1b1b1-0000-0000-0000-000000000010','a1a1a1a1-0000-0000-0000-000000000006', 4, 8000000,  50,       0, NULL, N'Pending', 0),
-- candidate10 CPISM-01 3x6M, paid 2/3
('b1b1b1b1-0000-0000-0000-000000000011','a1a1a1a1-0000-0000-0000-00000000000a', 1, 6000000, -50, 6000000,  -48, N'Paid',    0),
('b1b1b1b1-0000-0000-0000-000000000012','a1a1a1a1-0000-0000-0000-00000000000a', 2, 6000000, -20, 6000000,  -18, N'Paid',    0),
('b1b1b1b1-0000-0000-0000-000000000013','a1a1a1a1-0000-0000-0000-00000000000a', 3, 6000000,  10,       0, NULL, N'Pending', 0),
-- candidate17 HDSE-01 5x9M, paid 2/5
('b1b1b1b1-0000-0000-0000-000000000014','a1a1a1a1-0000-0000-0000-00000000000c', 1, 9000000, -30, 9000000,  -28, N'Paid',    0),
('b1b1b1b1-0000-0000-0000-000000000015','a1a1a1a1-0000-0000-0000-00000000000c', 2, 9000000,  -5, 9000000,   -3, N'Paid',    0),
('b1b1b1b1-0000-0000-0000-000000000016','a1a1a1a1-0000-0000-0000-00000000000c', 3, 9000000,  25,       0, NULL, N'Pending', 0),
('b1b1b1b1-0000-0000-0000-000000000017','a1a1a1a1-0000-0000-0000-00000000000c', 4, 9000000,  55,       0, NULL, N'Pending', 0),
('b1b1b1b1-0000-0000-0000-000000000018','a1a1a1a1-0000-0000-0000-00000000000c', 5, 9000000,  85,       0, NULL, N'Pending', 0);

INSERT INTO installment_plan (Id, FeeId, InstallmentNo, AmountDue, DueDate, AmountPaid,
                              PaidDate, Status, PenaltyAmount,
                              CreatedAt, CreatedBy, UpdatedAt, UpdatedBy)
SELECT i.Id, i.FeeId, i.InstallmentNo, i.AmountDue,
       DATEADD(day, i.DueOffsetDays, @Today),
       i.AmountPaid,
       CASE WHEN i.PaidOffsetDays IS NULL THEN NULL
            ELSE DATEADD(day, i.PaidOffsetDays, @Today) END,
       i.Status, i.PenaltyAmount,
       @Now, @Seeder, NULL, NULL
FROM @Installments i
WHERE NOT EXISTS (SELECT 1 FROM installment_plan x WHERE x.Id = i.Id);

COMMIT TRAN;
PRINT '09-installment-plans.sql: 24 installment_plan rows ensured.';
