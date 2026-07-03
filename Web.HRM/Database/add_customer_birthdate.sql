-- ============================================================
-- Add BirthDate to dbo.Customer
-- Run once against the BeYou database.
-- Idempotent: column added only if it doesn't already exist.
-- ============================================================
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.Customer') AND name = 'BirthDate')
BEGIN
    ALTER TABLE dbo.Customer ADD BirthDate DATE NULL;
END
GO
