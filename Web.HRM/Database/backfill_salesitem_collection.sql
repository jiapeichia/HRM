-- ============================================================
-- Backfill dbo.s_SalesItemCollection for back-order items
-- collected BEFORE the collection log existed.
--
-- For each backordered sales item with collected units
-- (Quantity - QtyBalance > 0), inserts one log row with the
-- collected quantity, using the item's ModDate as a proxy for
-- the collection date (the real per-unit collect dates were
-- never recorded).
--
-- Run once against the BeYou database, AFTER
-- add_salesitem_collection.sql.
-- Idempotent: skips items that already have any log row.
-- ============================================================
IF EXISTS (
    SELECT 1 FROM sys.tables
    WHERE object_id = OBJECT_ID(N'dbo.s_SalesItemCollection'))
BEGIN
    INSERT INTO dbo.s_SalesItemCollection
        (SalesItemId, Qty, CollectDate, AddBy, AddDate, ModBy, ModDate)
    SELECT
        si.SalesItemId,
        si.Quantity - si.QtyBalance,
        si.ModDate,
        'backfill',
        GETDATE(),
        'backfill',
        GETDATE()
    FROM dbo.s_SalesItem si
    WHERE si.IsBackordered = 1
      AND si.Active = 0
      AND si.Status = 0
      AND si.QtyBalance IS NOT NULL
      AND si.Quantity - si.QtyBalance > 0
      AND NOT EXISTS (
          SELECT 1 FROM dbo.s_SalesItemCollection c
          WHERE c.SalesItemId = si.SalesItemId);

    PRINT CAST(@@ROWCOUNT AS VARCHAR(10)) + ' collection row(s) backfilled.';
END
ELSE
    PRINT 'dbo.s_SalesItemCollection does not exist - run add_salesitem_collection.sql first.';
GO
