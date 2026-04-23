using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Meo.Web.DBContext;
using Meo.Web.ViewModels;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Web.HRM.Common;

namespace Web.HRM.Controllers
{
    /// <summary>
    /// POS transaction entry point.
    /// Inherits AuthController — every action requires Session["EmpNo"].
    /// </summary>
    public class PosController : AuthController
    {
        private readonly DBContext db = new DBContext();

        // ── GET: Pos/NewSale ─────────────────────────────────────────────────
        public ActionResult NewSale(int? cusId)
        {
            if (cusId == null)
                return RedirectToAction("Index", "Home");

            var customer = db.Customers.FirstOrDefault(x => x.CusId == cusId && x.Active == false && x.Status == false);
            if (customer == null)
                return HttpNotFound();

            ViewBag.CusId      = cusId;
            ViewBag.CusName    = customer.FullName;
            ViewBag.CreditBal  = customer.CreditBal;
            ViewBag.TPDueAmt   = customer.TPDueAmt;
            ViewBag.SVDueAmt   = customer.SVDueAmt;

            ViewBag.Products     = db.Products.Where(x => x.Status == false && x.Active == false).ToList();
            ViewBag.Packages     = db.Packages.Where(x => x.Status == false).ToList();
            ViewBag.PaymentTypes = db.Types.Where(x => x.Module == "PaymentType").ToList();
            ViewBag.Employees    = db.Employees.Where(x => x.Status == false && x.Active == false).ToList();

            return View();
        }

        // ── POST: Pos/CreateSale ─────────────────────────────────────────────
        /// <summary>
        /// Executes the full POS write flow in a single DB transaction:
        ///   1. s_Sales
        ///   2. s_SalesItem (per line)
        ///   3. s_Service   (if treatment/package)
        ///   4. m_Stock     (decrement qty)
        ///   5. Customer    (credit / due update)
        ///   6. pos.Payment
        ///   7. pos.GiroBilling (GIRO only)
        ///   8. pos.AuditLog
        ///   9. Counter     (increment invoice number)
        /// </summary>
        [HttpPost]
        public ActionResult CreateSale(PosTransactionViewModel model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Validation failed." });

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var empNo = CurrentEmpNo;

                    // ── 9 (read first): get next invoice number ──────────────
                    var counter = db.Counters.FirstOrDefault(x => x.module == "Sales");
                    if (counter == null)
                        throw new Exception("Counter record for 'Sales' not found.");

                    string invoiceNo = counter.format + counter.count_no.ToString("D6");

                    // ── 1. INSERT s_Sales ────────────────────────────────────
                    var sale = new SalesViewModels
                    {
                        SalesId       = invoiceNo,
                        CusId         = model.CusId,
                        PaymentMethod = model.PaymentMethodTypeId,
                        OrderDate     = DateTime.Now,
                        PaymentDate   = DateTime.Now,
                        SubTotal      = model.SubTotal,
                        DiscAmt       = model.DiscAmt,
                        DiscPercentage = model.DiscPercentage,
                        TotalAmt      = model.TotalAmt,
                        PaidAmt       = model.TotalAmt,
                        BalAmt        = 0,
                        Remarks       = model.Remarks,
                        Active        = false,   // false = active (project convention)
                        Status        = false
                    };
                    db.Saless.Add(sale);
                    db.SaveChanges();

                    // ── 2. INSERT s_SalesItem ────────────────────────────────
                    foreach (var line in model.Items)
                    {
                        var item = new SalesItemViewModels
                        {
                            SalesId      = invoiceNo,
                            ProductId    = line.ProductId,
                            Quantity     = line.Qty,
                            UnitPrice    = line.UnitPrice,
                            LineDiscAmt  = line.DiscAmt,
                            LineTotal    = line.TotalAmt,
                            EmpNo        = line.EmpNo,
                            Remarks      = line.Remarks,
                            Active       = false,
                            Status       = false
                        };
                        db.SalesItems.Add(item);

                        // ── 4. UPDATE m_Stock (decrement) ────────────────────
                        var stock = db.Stock.FirstOrDefault(x => x.ProductId == line.ProductId);
                        if (stock != null)
                        {
                            stock.QtyAvailable = (stock.QtyAvailable ?? 0) - line.Qty;
                            db.Entry(stock).State = EntityState.Modified;
                        }
                    }
                    db.SaveChanges();

                    // ── 3. INSERT s_Service (treatment / package items) ──────
                    foreach (var svc in model.Services)
                    {
                        var product = db.Products.Find(svc.ProductId);
                        var service = new Service
                        {
                            SalesId      = invoiceNo,
                            CusId        = model.CusId,
                            ServiceName  = product?.ProductName,
                            Course       = svc.SessionQty,
                            CourseBal    = svc.SessionQty,
                            PurchaseDate = DateTime.Now,
                            Status       = false
                        };
                        db.Services.Add(service);
                    }
                    db.SaveChanges();

                    // ── 5. UPDATE Customer (credit / due) ────────────────────
                    var customer = db.Customers.Find(model.CusId);
                    if (model.ApplyCredit && model.CreditUsed > 0)
                    {
                        customer.CreditBal -= model.CreditUsed;
                    }
                    if (model.PaymentMethodName == "GIRO")
                    {
                        customer.TPDueAmt = (customer.TPDueAmt ?? 0) + model.TotalAmt;
                    }
                    db.Entry(customer).State = EntityState.Modified;
                    db.SaveChanges();

                    // ── 6. INSERT pos.Payment ────────────────────────────────
                    var payment = new PosPaymentViewModel
                    {
                        SalesId       = invoiceNo,
                        PaymentMethod = model.PaymentMethodTypeId,
                        Amount        = model.TotalAmt,
                        ReferenceNo   = model.PaymentReferenceNo,
                        CreatedBy     = empNo,
                        CreatedDate   = DateTime.Now
                    };
                    db.PosPayments.Add(payment);
                    db.SaveChanges();

                    // ── 7. INSERT pos.GiroBilling (GIRO only) ────────────────
                    if (model.PaymentMethodName == "GIRO")
                    {
                        var billingMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                        var giro = new PosGiroBillingViewModel
                        {
                            CusId        = model.CusId,
                            SalesId      = invoiceNo,
                            BillingMonth = billingMonth,
                            Amount       = model.TotalAmt,
                            IsPaid       = false,
                            Remarks      = model.Remarks,
                            CreatedBy    = empNo,
                            CreatedDate  = DateTime.Now
                        };
                        db.PosGiroBillings.Add(giro);
                        db.SaveChanges();
                    }

                    // ── 8. INSERT pos.AuditLog ───────────────────────────────
                    AuditLogger.Log(
                        db,
                        tableName: "dbo.s_Sales",
                        recordId:  invoiceNo,
                        action:    "INSERT",
                        empNo:     empNo,
                        newValues: $"{{\"CusId\":{model.CusId},\"TotalAmt\":{model.TotalAmt},\"PaymentMethod\":\"{model.PaymentMethodName}\"}}");
                    db.SaveChanges();

                    // ── 9. UPDATE Counter ────────────────────────────────────
                    counter.count_no += 1;
                    db.Entry(counter).State = EntityState.Modified;
                    db.SaveChanges();

                    transaction.Commit();

                    return Json(new { success = true, invoiceNo });
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return Json(new { success = false, message = ex.Message });
                }
            }
        }

        // ── GET: Pos/Receipt ─────────────────────────────────────────────────
        public ActionResult Receipt(string id)
        {
            if (string.IsNullOrEmpty(id))
                return HttpNotFound();

            var sale  = db.Saless.FirstOrDefault(x => x.SalesId == id);
            if (sale == null)
                return HttpNotFound();

            var items    = db.SalesItems.Where(x => x.SalesId == id).ToList();
            var payment  = db.PosPayments.FirstOrDefault(x => x.SalesId == id);
            var customer = db.Customers.Find(sale.CusId);
            var company  = db.CompanyProfile.FirstOrDefault();

            var vm = new ReceiptModel
            {
                SalesId       = sale.SalesId,
                StoreName     = company?.Name,
                Tel           = company?.Tel,
                Date          = sale.OrderDate.ToString("dd/MM/yyyy"),
                PaymentMethod = payment?.PaymentMethod.ToString(),
                TotalAmount   = sale.TotalAmt ?? 0,
                TotalPaid     = sale.PaidAmt ?? 0,
                TotalBalance  = sale.BalAmt ?? 0,
                SubTotal      = sale.SubTotal ?? 0,
                TotalDisc     = sale.DiscAmt ?? 0
            };

            return View(vm);
        }

        // ── GET: Pos/GiroLedger ──────────────────────────────────────────────
        public ActionResult GiroLedger(int? cusId)
        {
            if (cusId == null)
                return RedirectToAction("Index", "Home");

            ViewBag.CusId = cusId;
            return View();
        }

        [HttpPost]
        public ActionResult GiroLedger_Read([DataSourceRequest] DataSourceRequest request, int cusId)
        {
            var data = db.PosGiroBillings
                         .Where(x => x.CusId == cusId)
                         .OrderByDescending(x => x.BillingMonth)
                         .ToList();

            return Json(data.ToDataSourceResult(request));
        }

        // ── POST: Pos/MarkGiroPaid ───────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MarkGiroPaid(int giroId)
        {
            var giro = db.PosGiroBillings.Find(giroId);
            if (giro == null)
                return Json(new { success = false, message = "Record not found." });

            var empNo = CurrentEmpNo;

            giro.IsPaid      = true;
            giro.PaidDate    = DateTime.Now;
            giro.UpdatedBy   = empNo;
            giro.UpdatedDate = DateTime.Now;
            db.Entry(giro).State = EntityState.Modified;

            AuditLogger.Log(db, "pos.GiroBilling", giroId.ToString(), "UPDATE", empNo,
                oldValues: "{\"IsPaid\":false}",
                newValues: $"{{\"IsPaid\":true,\"PaidDate\":\"{giro.PaidDate:yyyy-MM-dd}\"}}");

            db.SaveChanges();

            return Json(new { success = true });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
