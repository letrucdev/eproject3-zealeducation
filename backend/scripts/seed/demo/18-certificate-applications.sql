/*
 * 18-certificate-applications.sql — graduation certificate apps.
 *   - 1 Approved with CertificateNumber + ApprovedAt (graduated candidate03)
 *   - 1 Pending awaiting incharge review (graduated candidate09)
 *
 * CertificateFilePath stays NULL (UI shows "no file generated yet" until the
 * actual PDF is rendered).
 *
 * Idempotent on Id. Requires enrollment.
 */

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRAN;

DECLARE @Now    DATETIME2(7)  = SYSUTCDATETIME();
DECLARE @Seeder NVARCHAR(100) = N'seed-demo-data';

INSERT INTO certificate_application (Id, EnrollmentId, Status, CertificateNumber, CertificateFilePath,
                                     ApprovedAt, ApprovedByStaffId,
                                     CreatedAt, CreatedBy, UpdatedAt, UpdatedBy)
SELECT * FROM (VALUES
    -- Approved (candidate03 ACCP-DEMO Completed)
    (CAST('a4a4a4a4-0000-0000-0000-000000000001' AS UNIQUEIDENTIFIER),
     CAST('f1f1f1f1-0000-0000-0000-000000000003' AS UNIQUEIDENTIFIER),
     N'Approved', N'CERT-2026-0001', CAST(NULL AS NVARCHAR(500)),
     DATEADD(day, -5, @Now),
     CAST('aaaaaaaa-0000-0000-0000-000000000011' AS UNIQUEIDENTIFIER),
     @Now, @Seeder, CAST(NULL AS DATETIME2), CAST(NULL AS NVARCHAR(100))),
    -- Pending (candidate09 CPISM Enrolled but graduated)
    (CAST('a4a4a4a4-0000-0000-0000-000000000002' AS UNIQUEIDENTIFIER),
     CAST('f1f1f1f1-0000-0000-0000-000000000010' AS UNIQUEIDENTIFIER),
     N'Pending', CAST(NULL AS NVARCHAR(50)), CAST(NULL AS NVARCHAR(500)),
     CAST(NULL AS DATETIME2), CAST(NULL AS UNIQUEIDENTIFIER),
     @Now, @Seeder, NULL, NULL)
) v(Id, EnrollmentId, Status, CertificateNumber, CertificateFilePath,
    ApprovedAt, ApprovedByStaffId, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy)
WHERE NOT EXISTS (SELECT 1 FROM certificate_application c WHERE c.Id = v.Id);

COMMIT TRAN;
PRINT '18-certificate-applications.sql: 2 certificate applications ensured (1 approved, 1 pending).';
