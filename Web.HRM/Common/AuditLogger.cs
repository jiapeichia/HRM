using Meo.Web.DBContext;
using Meo.Web.ViewModels;
using System;
using System.Web;

namespace Web.HRM.Common
{
    /// <summary>
    /// Writes a row to pos.AuditLog.  Always call inside the same DB transaction
    /// as the business operation so the audit record rolls back with the data.
    /// </summary>
    public static class AuditLogger
    {
        public static void Log(
            DBContext db,
            string tableName,
            string recordId,
            string action,
            string empNo,
            string oldValues = null,
            string newValues = null)
        {
            var entry = new PosAuditLogViewModel
            {
                TableName  = tableName,
                RecordId   = recordId,
                Action     = action,
                OldValues  = oldValues,
                NewValues  = newValues,
                EmpNo      = empNo,
                ActionDate = DateTime.Now,
                IpAddress  = HttpContext.Current?.Request?.UserHostAddress
            };

            db.PosAuditLogs.Add(entry);
            // Caller must call db.SaveChanges() (inside the surrounding transaction).
        }
    }
}
