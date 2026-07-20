-- ============================================================
-- Add dbo.s_SalesItemCollection
-- Logs each back-order collection (one row per collect action)
-- so the Product Settlement report can show collected items
-- on their collection date.
-- Run once against the BeYou database.
-- Idempotent: table created only if it doesn't already exist.
-- ============================================================
IF NOT EXISTS (
    SELECT 1 FROM sys.tables
    WHERE object_id = OBJECT_ID(N'dbo.s_SalesItemCollection'))
BEGIN
    CREATE TABLE dbo.s_SalesItemCollection (
        Id          INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_s_SalesItemCollection PRIMARY KEY,
        SalesItemId INT NOT NULL,
        Qty         INT NOT NULL CONSTRAINT DF_s_SalesItemCollection_Qty DEFAULT (1),
        CollectDate DATETIME NOT NULL,
        AddBy       NVARCHAR(100) NULL,
        AddDate     DATETIME NULL,
        ModBy       NVARCHAR(100) NULL,
        ModDate     DATETIME NULL
    );

    CREATE INDEX IX_s_SalesItemCollection_SalesItemId
        ON dbo.s_SalesItemCollection (SalesItemId);
    CREATE INDEX IX_s_SalesItemCollection_CollectDate
        ON dbo.s_SalesItemCollection (CollectDate);
END
GO
