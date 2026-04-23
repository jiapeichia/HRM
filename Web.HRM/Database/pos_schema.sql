-- ============================================================
-- BeYou POS — new [pos] schema
-- Run once against the BeYou database.
-- Idempotent: all objects are created only if they don't exist.
--
-- NOTE: FK constraints are intentionally omitted because the live
-- dbo.s_Sales and dbo.Customer PKs may use a different collation
-- or constraint name than expected.  Referential integrity is
-- enforced at the application layer (PosController transaction).
-- ============================================================

-- ── 0. Schema ────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'pos')
    EXEC('CREATE SCHEMA pos');
GO

-- ── 1. pos.Payment ───────────────────────────────────────────
-- One row per sale.
-- SalesId  → soft ref to dbo.s_Sales.SalesId
-- PaymentMethod → dbo.m_Type.TypeId (Module = 'PaymentType')
IF NOT EXISTS (
    SELECT 1 FROM sys.objects
    WHERE object_id = OBJECT_ID(N'pos.Payment') AND type = 'U')
BEGIN
    CREATE TABLE pos.Payment (
        PaymentId       INT             NOT NULL IDENTITY(1,1),
        SalesId         NVARCHAR(50)    NOT NULL,
        PaymentMethod   INT             NOT NULL,
        Amount          DECIMAL(18,2)   NOT NULL,
        ReferenceNo     NVARCHAR(100)   NULL,
        CreatedBy       NVARCHAR(20)    NOT NULL,
        CreatedDate     DATETIME        NOT NULL    DEFAULT GETDATE(),

        CONSTRAINT PK_pos_Payment PRIMARY KEY (PaymentId)
    );
END
GO

-- ── 2. pos.GiroBilling ───────────────────────────────────────
-- Monthly GIRO ledger per customer.
-- CusId   → soft ref to dbo.Customer.CusId
-- SalesId → soft ref to dbo.s_Sales.SalesId
IF NOT EXISTS (
    SELECT 1 FROM sys.objects
    WHERE object_id = OBJECT_ID(N'pos.GiroBilling') AND type = 'U')
BEGIN
    CREATE TABLE pos.GiroBilling (
        GiroId          INT             NOT NULL IDENTITY(1,1),
        CusId           INT             NOT NULL,
        SalesId         NVARCHAR(50)    NOT NULL,
        BillingMonth    DATE            NOT NULL,
        Amount          DECIMAL(18,2)   NOT NULL,
        IsPaid          BIT             NOT NULL    DEFAULT 0,
        PaidDate        DATETIME        NULL,
        Remarks         NVARCHAR(255)   NULL,
        CreatedBy       NVARCHAR(20)    NOT NULL,
        CreatedDate     DATETIME        NOT NULL    DEFAULT GETDATE(),
        UpdatedBy       NVARCHAR(20)    NULL,
        UpdatedDate     DATETIME        NULL,

        CONSTRAINT PK_pos_GiroBilling PRIMARY KEY (GiroId)
    );
END
GO

-- ── 3. pos.AuditLog ──────────────────────────────────────────
-- Append-only.  Never UPDATE or DELETE rows here.
IF NOT EXISTS (
    SELECT 1 FROM sys.objects
    WHERE object_id = OBJECT_ID(N'pos.AuditLog') AND type = 'U')
BEGIN
    CREATE TABLE pos.AuditLog (
        AuditId         BIGINT          NOT NULL IDENTITY(1,1),
        TableName       NVARCHAR(100)   NOT NULL,
        RecordId        NVARCHAR(100)   NOT NULL,
        Action          NVARCHAR(20)    NOT NULL,
        OldValues       NVARCHAR(MAX)   NULL,
        NewValues       NVARCHAR(MAX)   NULL,
        EmpNo           NVARCHAR(20)    NOT NULL,
        ActionDate      DATETIME        NOT NULL    DEFAULT GETDATE(),
        IpAddress       NVARCHAR(50)    NULL,

        CONSTRAINT PK_pos_AuditLog PRIMARY KEY (AuditId)
    );
END
GO

-- ── 4. Indexes ───────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_pos_Payment_SalesId')
    CREATE INDEX IX_pos_Payment_SalesId ON pos.Payment (SalesId);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_pos_GiroBilling_CusId')
    CREATE INDEX IX_pos_GiroBilling_CusId ON pos.GiroBilling (CusId);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_pos_GiroBilling_BillingMonth')
    CREATE INDEX IX_pos_GiroBilling_BillingMonth ON pos.GiroBilling (BillingMonth);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_pos_AuditLog_TableRecord')
    CREATE INDEX IX_pos_AuditLog_TableRecord ON pos.AuditLog (TableName, RecordId);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_pos_AuditLog_EmpNo')
    CREATE INDEX IX_pos_AuditLog_EmpNo ON pos.AuditLog (EmpNo);
GO
