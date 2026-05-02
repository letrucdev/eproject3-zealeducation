/*
 * 16-fines.sql — penalty rows linked to the stand-alone Fine fee_structure
 * seeded in 08-fee-structures.sql. Issued by an accounts staff.
 *
 * Idempotent on Id. Requires fee_structure + candidate + staff.
 */

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRAN;

DECLARE @Now    DATETIME2(7)  = SYSUTCDATETIME();
DECLARE @Today  DATE          = CAST(SYSUTCDATETIME() AS DATE);
DECLARE @Seeder NVARCHAR(100) = N'seed-demo-data';

-- One unpaid fine for candidate06 (overdue installment) — links to fee row 0E.
INSERT INTO fine (Id, FeeId, CandidateId, IssuedByStaffId, ViolationReason, PenaltyAmount,
                  IssuedDate, IsPaid, PaidDate,
                  CreatedAt, CreatedBy, UpdatedAt, UpdatedBy)
SELECT * FROM (VALUES
    (CAST('a2a2a2a2-0000-0000-0000-000000000001' AS UNIQUEIDENTIFIER),
     CAST('a1a1a1a1-0000-0000-0000-00000000000e' AS UNIQUEIDENTIFIER),
     CAST('cccccccc-0000-0000-0000-000000000006' AS UNIQUEIDENTIFIER),
     CAST('aaaaaaaa-0000-0000-0000-000000000041' AS UNIQUEIDENTIFIER),
     N'Phat tre han thanh toan ky 2 cua hoc phi ACCP-DEMO',
     CAST(500000 AS DECIMAL(12,2)),
     DATEADD(day, -10, @Today),
     CAST(0 AS BIT), CAST(NULL AS DATE),
     @Now, @Seeder, CAST(NULL AS DATETIME2), CAST(NULL AS NVARCHAR(100)))
) v(Id, FeeId, CandidateId, IssuedByStaffId, ViolationReason, PenaltyAmount,
    IssuedDate, IsPaid, PaidDate, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy)
WHERE NOT EXISTS (SELECT 1 FROM fine f WHERE f.Id = v.Id);

COMMIT TRAN;
PRINT '16-fines.sql: 1 fine row ensured.';
