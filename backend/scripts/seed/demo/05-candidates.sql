/*
 * 05-candidates.sql — 20 candidate rows, 1:1 with the candidate user_account
 * rows from 02-user-accounts.sql.
 *
 * Status spread: 16 Active + 2 Graduated (slot 3, 9) + 2 Dropped (slot 14, 18)
 * so the candidate-list filter UI has at least one of each.
 *
 * RegisteredAt mirrors user_account.CreatedAt so the registration ladder is
 * preserved across the join.
 *
 * Idempotent on UserAccountId. Requires 02-user-accounts.sql + 03-staff.sql.
 */

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRAN;

DECLARE @Now    DATETIME2(7)  = SYSUTCDATETIME();
DECLARE @Seeder NVARCHAR(100) = N'seed-demo-data';

DECLARE @Candidates TABLE (
    Id              UNIQUEIDENTIFIER,
    UserAccountId   UNIQUEIDENTIFIER,
    CandidateCode   NVARCHAR(20),
    Address         NVARCHAR(MAX),
    EmergencyContact NVARCHAR(100),
    Status          NVARCHAR(20),
    RegisteredAt    DATETIME2(7),
    RegisteredByStaffId UNIQUEIDENTIFIER
);

INSERT INTO @Candidates
SELECT
    CAST('cccccccc-0000-0000-0000-' + RIGHT('000000000000' + CAST(seq AS NVARCHAR(12)), 12) AS UNIQUEIDENTIFIER) AS Id,
    u.Id AS UserAccountId,
    N'CAND-' + RIGHT('0000' + CAST(seq AS NVARCHAR(4)), 4) AS CandidateCode,
    N'So ' + CAST(seq + 100 AS NVARCHAR(10)) + N' Hai Ba Trung, Q.1, TP.HCM' AS Address,
    N'09' + RIGHT('00000000' + CAST(seq * 13 AS NVARCHAR(8)), 8) AS EmergencyContact,
    CASE
        WHEN seq IN (3, 9)  THEN N'Graduated'
        WHEN seq IN (14, 18) THEN N'Dropped'
        ELSE                     N'Active'
    END AS Status,
    u.CreatedAt AS RegisteredAt,
    CASE seq % 3
        WHEN 0 THEN CAST('aaaaaaaa-0000-0000-0000-000000000031' AS UNIQUEIDENTIFIER)
        WHEN 1 THEN CAST('aaaaaaaa-0000-0000-0000-000000000032' AS UNIQUEIDENTIFIER)
        ELSE        CAST('aaaaaaaa-0000-0000-0000-000000000033' AS UNIQUEIDENTIFIER)
    END AS RegisteredByStaffId
FROM (
    SELECT u.Id, u.Username, u.CreatedAt,
           CAST(SUBSTRING(u.Username, 10, 3) AS INT) AS seq
    FROM user_account u
    WHERE u.Role = N'Candidate' AND u.Username LIKE N'candidate[0-9][0-9]'
) u;

INSERT INTO candidate (Id, UserAccountId, CandidateCode, Address, EmergencyContact, Notes,
                       Status, RegisteredAt, RegisteredByStaffId,
                       CreatedAt, CreatedBy, UpdatedAt, UpdatedBy)
SELECT c.Id, c.UserAccountId, c.CandidateCode, c.Address, c.EmergencyContact, NULL,
       c.Status, c.RegisteredAt, c.RegisteredByStaffId,
       c.RegisteredAt, @Seeder, NULL, NULL
FROM @Candidates c
WHERE NOT EXISTS (SELECT 1 FROM candidate x WHERE x.UserAccountId = c.UserAccountId);

COMMIT TRAN;
PRINT '05-candidates.sql: 20 candidate rows ensured.';
