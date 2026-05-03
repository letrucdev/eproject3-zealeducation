/*
 * 19-system-assets.sql — labs / AV / network gear with a mix of conditions
 * (Good / Maintenance / Faulty / Decommissioned) so the asset list filters
 * have non-empty buckets.
 *
 * Idempotent on SerialNumber. Requires staff (assigned to ManagedBy).
 */

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRAN;

DECLARE @Now    DATETIME2(7)  = SYSUTCDATETIME();
DECLARE @Seeder NVARCHAR(100) = N'seed-demo-data';

DECLARE @Assets TABLE (
    Id            UNIQUEIDENTIFIER,
    AssetName     NVARCHAR(255),
    AssetType     NVARCHAR(100),
    SerialNumber  NVARCHAR(100),
    Location      NVARCHAR(255),
    Condition     NVARCHAR(50),
    PurchaseOffsetDays        INT,
    LastMaintenanceOffsetDays INT NULL,
    Notes         NVARCHAR(MAX),
    ManagedBy     UNIQUEIDENTIFIER
);

INSERT INTO @Assets VALUES
('e1e1e1e1-0000-0000-0000-000000000001', N'Laptop Dell Latitude 5430 - 01',  N'Laptop',     N'SN-DL5430-001',  N'Lab A',       N'Good',          -300,  -30, N'Cap cho giang vien Java',   'aaaaaaaa-0000-0000-0000-000000000001'),
('e1e1e1e1-0000-0000-0000-000000000002', N'Lab PC HP EliteDesk - 01',        N'Desktop',    N'SN-HPED-001',    N'Lab A',       N'Good',          -540,  -90, NULL,                          'aaaaaaaa-0000-0000-0000-000000000002'),
('e1e1e1e1-0000-0000-0000-000000000003', N'Lab PC HP EliteDesk - 02',        N'Desktop',    N'SN-HPED-002',    N'Lab A',       N'Maintenance',   -540,   -7, N'Dang thay o cung SSD',       'aaaaaaaa-0000-0000-0000-000000000002'),
('e1e1e1e1-0000-0000-0000-000000000004', N'Projector Epson EB-X51',          N'Projector',  N'SN-EPSX51-001',  N'Room 101',    N'Good',          -180,  -60, NULL,                          'aaaaaaaa-0000-0000-0000-000000000003'),
('e1e1e1e1-0000-0000-0000-000000000005', N'Cisco Switch Catalyst 2960',      N'Network',    N'SN-CSC2960-01',  N'Server Room', N'Good',          -730, -120, N'Switch chinh cho mang LAN',  'aaaaaaaa-0000-0000-0000-000000000001'),
('e1e1e1e1-0000-0000-0000-000000000006', N'UPS APC Smart-UPS 1500',          N'Power',      N'SN-APCSU1500-1', N'Server Room', N'Faulty',        -900, -180, N'Ac quy chai, can thay',      'aaaaaaaa-0000-0000-0000-000000000001'),
('e1e1e1e1-0000-0000-0000-000000000007', N'Printer Canon LBP6230dn',         N'Printer',    N'SN-LBP6230-01',  N'Office',      N'Decommissioned',-1500,-365, N'Het han, da thay model moi', 'aaaaaaaa-0000-0000-0000-000000000003');

INSERT INTO system_asset (Id, AssetName, AssetType, SerialNumber, Location, ConditionStatus,
                          PurchaseDate, LastMaintenance, Notes, ManagedBy,
                          CreatedAt, CreatedBy, UpdatedAt, UpdatedBy)
SELECT a.Id, a.AssetName, a.AssetType, a.SerialNumber, a.Location, a.Condition,
       DATEADD(day, a.PurchaseOffsetDays, @Now),
       CASE WHEN a.LastMaintenanceOffsetDays IS NULL THEN NULL
            ELSE DATEADD(day, a.LastMaintenanceOffsetDays, @Now) END,
       a.Notes, a.ManagedBy,
       @Now, @Seeder, NULL, NULL
FROM @Assets a
WHERE NOT EXISTS (SELECT 1 FROM system_asset x WHERE x.SerialNumber = a.SerialNumber);

COMMIT TRAN;
PRINT '19-system-assets.sql: 7 system_asset rows ensured.';
