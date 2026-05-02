/*
 * 10-payment-transactions.sql — payment_transaction rows behind every
 * AmountPaid in fee_structure / installment_plan.
 *
 * NOTE: ReceiptFilePath and BankTransferProofPath are intentionally left NULL
 * in this seed — the UI shows a "no proof attached" empty state.
 *
 * Idempotent on Id (and ReceiptNumber is unique). Requires fee_structure +
 * installment_plan + staff.
 */

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRAN;

DECLARE @Now    DATETIME2(7)  = SYSUTCDATETIME();
DECLARE @Seeder NVARCHAR(100) = N'seed-demo-data';

DECLARE @AccountsManager UNIQUEIDENTIFIER = 'aaaaaaaa-0000-0000-0000-000000000041';
DECLARE @Accountant1     UNIQUEIDENTIFIER = 'aaaaaaaa-0000-0000-0000-000000000042';
DECLARE @Accountant2     UNIQUEIDENTIFIER = 'aaaaaaaa-0000-0000-0000-000000000043';

DECLARE @Payments TABLE (
    Id               UNIQUEIDENTIFIER,
    FeeId            UNIQUEIDENTIFIER,
    StaffId          UNIQUEIDENTIFIER,
    InstallmentId    UNIQUEIDENTIFIER NULL,
    Amount           DECIMAL(12,2),
    OutstandingAfter DECIMAL(12,2),
    Method           NVARCHAR(30),
    Receipt          NVARCHAR(50),
    PaidOffsetDays   INT
);

INSERT INTO @Payments VALUES
-- candidate01 — full payment (no installment)
('c1c1c1c1-0000-0000-0000-000000000001','a1a1a1a1-0000-0000-0000-000000000001', @AccountsManager, NULL,                                       32000000,        0, N'BankTransfer', N'RC-2026-0001', -55),
-- candidate02 — 2 installment payments
('c1c1c1c1-0000-0000-0000-000000000002','a1a1a1a1-0000-0000-0000-000000000002', @Accountant1,    'b1b1b1b1-0000-0000-0000-000000000001',  8000000, 24000000, N'Cash',         N'RC-2026-0002', -58),
('c1c1c1c1-0000-0000-0000-000000000003','a1a1a1a1-0000-0000-0000-000000000002', @Accountant1,    'b1b1b1b1-0000-0000-0000-000000000002',  8000000, 16000000, N'BankTransfer', N'RC-2026-0003', -28),
-- candidate03 (graduated) — full payment
('c1c1c1c1-0000-0000-0000-000000000004','a1a1a1a1-0000-0000-0000-000000000003', @AccountsManager, NULL,                                       32000000,        0, N'BankTransfer', N'RC-2026-0004', -85),
-- candidate04 — 4 installments fully paid
('c1c1c1c1-0000-0000-0000-000000000005','a1a1a1a1-0000-0000-0000-000000000004', @AccountsManager,'b1b1b1b1-0000-0000-0000-000000000005',  8000000, 24000000, N'Cash',         N'RC-2026-0005', -59),
('c1c1c1c1-0000-0000-0000-000000000006','a1a1a1a1-0000-0000-0000-000000000004', @Accountant2,    'b1b1b1b1-0000-0000-0000-000000000006',  8000000, 16000000, N'BankTransfer', N'RC-2026-0006', -29),
('c1c1c1c1-0000-0000-0000-000000000007','a1a1a1a1-0000-0000-0000-000000000004', @Accountant2,    'b1b1b1b1-0000-0000-0000-000000000007',  8000000,  8000000, N'BankTransfer', N'RC-2026-0007',  -2),
('c1c1c1c1-0000-0000-0000-000000000008','a1a1a1a1-0000-0000-0000-000000000004', @Accountant2,    'b1b1b1b1-0000-0000-0000-000000000008',  8000000,        0, N'BankTransfer', N'RC-2026-0008',  -1),
-- candidate05 — 1 installment payment
('c1c1c1c1-0000-0000-0000-000000000009','a1a1a1a1-0000-0000-0000-000000000005', @Accountant1,    'b1b1b1b1-0000-0000-0000-000000000009',  8000000, 24000000, N'BankTransfer', N'RC-2026-0009', -42),
-- candidate06 — 1 installment paid (then ky 2 overdue)
('c1c1c1c1-0000-0000-0000-00000000000a','a1a1a1a1-0000-0000-0000-000000000006', @Accountant1,    'b1b1b1b1-0000-0000-0000-00000000000d',  8000000, 24000000, N'Cash',         N'RC-2026-0010', -58),
-- candidate08 — full payment
('c1c1c1c1-0000-0000-0000-00000000000b','a1a1a1a1-0000-0000-0000-000000000008', @AccountsManager, NULL,                                       32000000,        0, N'BankTransfer', N'RC-2026-0011', -50),
-- candidate09 (graduated CPISM) — full payment
('c1c1c1c1-0000-0000-0000-00000000000c','a1a1a1a1-0000-0000-0000-000000000009', @AccountsManager, NULL,                                       18000000,        0, N'BankTransfer', N'RC-2026-0012', -78),
-- candidate10 (CPISM installments) — 2 payments
('c1c1c1c1-0000-0000-0000-00000000000d','a1a1a1a1-0000-0000-0000-00000000000a', @Accountant2,    'b1b1b1b1-0000-0000-0000-000000000011',  6000000, 12000000, N'Cash',         N'RC-2026-0013', -48),
('c1c1c1c1-0000-0000-0000-00000000000e','a1a1a1a1-0000-0000-0000-00000000000a', @Accountant2,    'b1b1b1b1-0000-0000-0000-000000000012',  6000000,  6000000, N'BankTransfer', N'RC-2026-0014', -18),
-- candidate12 (WEBPRO) — full payment
('c1c1c1c1-0000-0000-0000-00000000000f','a1a1a1a1-0000-0000-0000-00000000000b', @AccountsManager, NULL,                                       12000000,        0, N'Cash',         N'RC-2026-0015', -68),
-- candidate17 (HDSE installments) — 2 payments
('c1c1c1c1-0000-0000-0000-000000000010','a1a1a1a1-0000-0000-0000-00000000000c', @AccountsManager,'b1b1b1b1-0000-0000-0000-000000000014',  9000000, 36000000, N'BankTransfer', N'RC-2026-0016', -28),
('c1c1c1c1-0000-0000-0000-000000000011','a1a1a1a1-0000-0000-0000-00000000000c', @Accountant1,    'b1b1b1b1-0000-0000-0000-000000000015',  9000000, 27000000, N'BankTransfer', N'RC-2026-0017',  -3);

INSERT INTO payment_transaction (Id, FeeId, ProcessedByStaffId, InstallmentPlanId, Amount,
                                 OutstandingBalanceAfter, PaymentMethod, ReceiptNumber,
                                 PaymentDate, ReceiptFilePath, BankTransferProofPath,
                                 CreatedAt, CreatedBy, UpdatedAt, UpdatedBy)
SELECT p.Id, p.FeeId, p.StaffId, p.InstallmentId, p.Amount,
       p.OutstandingAfter, p.Method, p.Receipt,
       DATEADD(day, p.PaidOffsetDays, @Now),
       NULL, NULL,                              -- proof / receipt files intentionally empty
       @Now, @Seeder, NULL, NULL
FROM @Payments p
WHERE NOT EXISTS (SELECT 1 FROM payment_transaction x WHERE x.Id = p.Id);

COMMIT TRAN;
PRINT '10-payment-transactions.sql: 17 payment_transaction rows ensured (no file paths).';
