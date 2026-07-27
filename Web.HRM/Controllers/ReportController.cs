using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Meo.Web.DBContext;
using Meo.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Data.Entity;
using System.Data;
using Microsoft.Ajax.Utilities;
using NPOI.SS.Formula.Functions;

namespace Web.HRM.Controllers
{
    public class ReportController : Controller
    {
        DBContext db = new DBContext();

        // GET: Employee Sales Report
        public ActionResult Search(string reportType)
        {
            try
            {
                if (!string.IsNullOrEmpty(Session["EmpNo"] as string))
                {
                    ViewData["Employee"] = db.Employees.Where(e => e.Active.Equals(false) && e.Status.Equals(false) && !e.FullName.Contains("Admin") && !e.FullName.Contains("Service")).ToList();
                    DateTime today = DateTime.Now.Date.AddMilliseconds(1);
                    DateTime tmr = DateTime.Now.Date.AddDays(1).AddMilliseconds(-1);
                    var sales = (from sa in db.Saless
                                 where sa.PaymentDate > today && sa.PaymentDate < tmr
                                 select sa).ToList();

                    var totalAmount = sales.Sum(x => x.TotalAmt);
                    ViewBag.TotalAmount = totalAmount;
                    ViewBag.ReportType = reportType;

                    ReportSearchContent search = new ReportSearchContent();
                    search.reportType = reportType;
                    search.StartDate = DateTime.Now.Date;
                    search.EndDate = DateTime.Now.Date;

                    return View(search);
                }
                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }
        public ActionResult _SearchReport(string empNo, string startDate, string endDate)
        {
            ViewBag.empNo = empNo;
            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;
            ViewData["Customer"] = db.Customers.Where(e => e.Active.Equals(false) && e.Status.Equals(false)).ToList();
            ViewData["Product"] = db.Products.Where(x => x.Status == false && x.Active == false).ToList();
            ViewData["Category"] = db.Types.Where(x => x.Module == "Product" && x.Active == false && x.Status == false);

            return PartialView();
        }
        public ActionResult GetReportData(string empNo, string startDate, string endDate, [DataSourceRequest] DataSourceRequest request)
        {
            List<SalesReportViewModels> summary = new List<SalesReportViewModels>();

            DateTime today = DateTime.Now.Date;
            var start_date = DateTime.Parse(startDate);
            var end_date = DateTime.Parse(endDate).AddDays(1).AddMilliseconds(-1);

            // Fetch all items in the date range; EmpNo may be comma-separated for multi-PIC items
            // so we use a left-join on Employee and fall back to the stored EmpName.
            var query = (from sa in db.Saless
                         join si in db.SalesItems on sa.SalesId equals si.SalesId
                         join cus in db.Customers on sa.CusId equals cus.CusId
                         join emp in db.Employees on si.EmpNo equals emp.EmpNo into empGroup
                         from employee in empGroup.DefaultIfEmpty()
                         join pay in db.Types on sa.PaymentMethod equals pay.TypeId
                         where si.Active.Equals(false) && si.Status.Equals(false)
                               && sa.PaymentDate >= start_date && sa.PaymentDate < end_date
                         select new SalesReportViewModels
                         {
                             SalesItemId = si.SalesItemId,
                             SalesId = si.SalesId,
                             EmpNo = si.EmpNo,
                             PICName = (si.EmpName != null && si.EmpName != "") ? si.EmpName : (employee != null ? employee.DisplayName : ""),
                             ProductId = si.ProductId,
                             TypeId = si.TypeId,
                             Quantity = si.Quantity,
                             UnitPrice = si.UnitPrice,
                             TotalAmt = sa.TotalAmt ?? 0,
                             LineTotal = si.LineTotal,
                             LineDiscAmt = sa.DiscAmt + sa.DiscPercentageAmt ?? 0,
                             Remarks = si.Remarks,
                             PaymentDate = sa.PaymentDate,
                             PaymentMethod = pay.TypeName,
                             CusId = sa.CusId,
                             CardNo = cus.CardNo,
                             ImagePath = cus.ImagePath,
                         }).ToList();

            // Apply employee filter after fetch to support comma-separated multi-PIC EmpNo values
            if (!empNo.IsNullOrWhiteSpace())
            {
                summary = query.Where(x => x.EmpNo == empNo
                    || x.EmpNo.StartsWith(empNo + ",")
                    || x.EmpNo.EndsWith("," + empNo)
                    || x.EmpNo.Contains("," + empNo + ",")).ToList();
            }
            else
            {
                summary = query;
            }

            return Json(summary.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        // GET: Employee Service Report
        public ActionResult SearchService()
        {
            try
            {
                if (!string.IsNullOrEmpty(Session["EmpNo"] as string))
                {
                    ViewData["Employee"] = db.Employees.Where(e => e.Active.Equals(false) && e.Status.Equals(false) && !e.FullName.Contains("Admin") && !e.FullName.Contains("Service")).ToList();

                    return View();
                }
                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }
        public ActionResult _SearchServiceReport(string empNo, string startDate, string endDate)
        {
            ViewBag.empNo = empNo;
            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;
            ViewData["Customer"] = db.Customers.Where(e => e.Active.Equals(false) && e.Status.Equals(false)).ToList();

            return PartialView();
        }
        public ActionResult GetServiceReportData(string empNo, string startDate, string endDate, [DataSourceRequest] DataSourceRequest request)
        {
            List<ServiceReportViewModels> summary = new List<ServiceReportViewModels>();

            DateTime today = DateTime.Now.Date;
            var start_date = DateTime.Parse(startDate);
            var end_date = DateTime.Parse(endDate).AddDays(1).AddMilliseconds(-1);

            var dateRange = Enumerable.Range(0, (end_date - start_date).Days + 1)
            .Select(offset => start_date.AddDays(offset));

            var query
                = from sh in db.ServiceHistories
                  join cus in db.Customers on sh.CusId equals cus.CusId
                  join si in db.SalesItems on sh.SalesItemId equals si.SalesItemId
                  where sh.ServiceDate > start_date
                        && sh.ServiceDate < end_date
                        && sh.Status == false
                  select new ServiceReportViewModels
                  {
                      Id = sh.Id,
                      SalesItemId = sh.SalesItemId,
                      SalesId = sh.SalesId,
                      CusId = sh.CusId,
                      CardNo = cus.CardNo,
                      PICId = sh.PICId,
                      PICName = sh.PICName,
                      Remarks = sh.Remarks,
                      ServiceName = sh.ServiceName,
                      ServiceDate = sh.ServiceDate,
                      ImagePath = cus.ImagePath,
                      //TotalAmt = si.LineTotal, 
                      //LineTotal = si.LineTotal,
                      //TotalDiscAmt = si.LineDiscAmt + sa.DiscPercentageAmt ?? 0,
                  };

            // Apply filter only if empNo is not null or whitespace.
            // PICId may be comma-separated (e.g. "E001,E002"), so match any position.
            if (!string.IsNullOrWhiteSpace(empNo))
            {
                query = query.Where(x => x.PICId == empNo
                    || x.PICId.StartsWith(empNo + ",")
                    || x.PICId.EndsWith("," + empNo)
                    || x.PICId.Contains("," + empNo + ","));
            }

            return Json(query.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        #region All invoice 
        public ActionResult AllInvoice()
        {
            try
            {
                if (!string.IsNullOrEmpty(Session["EmpNo"] as string))
                {
                    ViewData["Employee"] = db.Employees.Where(e => e.Active.Equals(false) && e.Status.Equals(false) && !e.FullName.Contains("Admin")).ToList();
                    ViewData["Customer"] = db.Customers.Where(e => e.Active.Equals(false) && e.Status.Equals(false)).ToList();

                    return View();
                }
                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }
        public ActionResult _SearchAllInvoice(int? cusid, string period, string invoiceNo, string startDate, string endDate)
        {
            ViewBag.cusid = cusid;
            ViewBag.period = period;
            ViewBag.invoiceNo = invoiceNo;
            ViewBag.startDate = startDate;
            ViewBag.endDate = endDate;
            ViewData["Customer"] = db.Customers.Where(e => e.Active.Equals(false) && e.Status.Equals(false)).ToList();
            ViewData["PaymentType"] = db.Types.Where(x => x.Module == "PaymentType").ToList();

            // Retrieve the viewmodel for the view here, depending on your data structure.
            return PartialView();
        }
        public ActionResult GetInvoiceData(string cusid, string invoiceNo, string startDate, string endDate, [DataSourceRequest] DataSourceRequest request)
        {
            bool hasDateFilter = !string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate);
            DateTime start_date = hasDateFilter ? DateTime.Parse(startDate) : DateTime.MinValue;
            DateTime end_date = hasDateFilter ? DateTime.Parse(endDate).AddDays(1).AddMilliseconds(-1) : DateTime.MaxValue;

            return Json(db.Saless.Where(x => x.Status.Equals(false)
            && x.Active.Equals(false)
             && (!hasDateFilter || (x.AddDate > start_date && x.AddDate < end_date))
              && (string.IsNullOrEmpty(invoiceNo) || x.SalesId.Contains(invoiceNo))).ToDataSourceResult(request, o => new SalesViewModels()
              {
                  SalesId = o.SalesId,
                  CusId = o.CusId,
                  TotalAmt = o.TotalAmt,
                  DiscAmt = o.DiscAmt,
                  PaidAmt = o.PaidAmt,
                  BalAmt = o.BalAmt,
                  PaymentMethod = o.PaymentMethod,
                  PaymentDate = o.PaymentDate,
                  OrderDate = o.OrderDate,
                  Remarks = o.Remarks,
                  Exchange = o.Exchange,
                  GIRO = o.GIRO,
                  Active = o.Active,
                  Status = o.Status,
                  AddBy = o.AddBy,
                  ModBy = o.ModBy,
                  AddDate = o.AddDate,
                  ModDate = o.ModDate,
              }), JsonRequestBehavior.AllowGet);
        }
        #endregion

        public ActionResult PrintInvoice(int? id, string salesid)
        {
            try
            {
                if (!string.IsNullOrEmpty(Session["EmpNo"] as string))
                {
                    SalesInvoiceViewModels newsales = new SalesInvoiceViewModels();

                    if (!salesid.IsNullOrWhiteSpace())
                    {
                        var salesVM = db.Saless.FirstOrDefault(x => x.SalesId == salesid);

                        var CusName = db.Customers.FirstOrDefault(x => x.CusId == salesVM.CusId).FullName;
                        ViewData["CustomerName"] = CusName;
                        ViewBag.Employees = db.Employees.Where(e => e.Active.Equals(false) && e.Status.Equals(false) && !e.FullName.Contains("Admin")).ToList();
                        ViewData["Employee"] = ViewBag.Employees;
                        ViewBag.Products = db.Products.Where(x => x.Status == false && x.Active == false).ToList();
                        ViewData["Products"] = ViewBag.Products;

                        List<SalesItemUpdate> salesitem = (from si in db.SalesItems
                                                           join emp in db.Employees on si.EmpNo equals emp.EmpNo into empGroup
                                                           from employee in empGroup.DefaultIfEmpty()
                                                           join pro in db.Products on si.ProductId equals pro.ProductId
                                                           where si.SalesId.Equals(salesid)
                                                           select new SalesItemUpdate
                                                           {
                                                               SalesItemId = si.SalesItemId,
                                                               SalesId = si.SalesId,
                                                               EmpName = employee.DisplayName ?? "",
                                                               ProductName = pro.ProductName,
                                                               Quantity = si.Quantity,
                                                               UnitPrice = si.UnitPrice,
                                                               LineTotal = si.LineTotal,
                                                               LineDiscAmt = si.LineDiscAmt,
                                                               Remarks = si.Remarks,
                                                           }).ToList();

                        SalesInvoiceViewModels invoiceVM = new SalesInvoiceViewModels()
                        {
                            SalesId = salesVM.SalesId,
                            CusId = salesVM.CusId,
                            CusName = CusName,
                            PaymentMethod = salesVM.PaymentMethod,
                            OrderDate = salesVM.OrderDate,
                            PaymentDate = salesVM.PaymentDate,
                            TotalAmt = salesVM.TotalAmt,
                            PaidAmt = salesVM.PaidAmt,
                            DiscAmt = salesVM.DiscAmt,
                            BalAmt = salesVM.BalAmt,
                            Remarks = salesVM.Remarks,
                            Active = salesVM.Active,
                            Status = salesVM.Status,
                            AddBy = salesVM.AddBy,
                            ModBy = salesVM.ModBy,
                            AddDate = salesVM.AddDate,
                            ModDate = salesVM.ModDate,
                            SalesDetails = salesitem,

                            subtotal = Math.Round(salesitem.Sum(item => item.LineTotal), 2),
                            totalDisc = Math.Round(salesitem.Sum(item => item.LineDiscAmt), 2),
                            totalQty = salesitem.Sum(item => item.Quantity),
                        };

                        return View(invoiceVM);
                    }
                    else
                    {
                        List<SalesItemUpdate> salesitemlist = new List<SalesItemUpdate>();
                        string currentYear = DateTime.Now.Year.ToString();
                        var counter = db.Counters.Where(x => x.count_name == "invoiceno").First();
                        salesid = salesid == null ? counter.format + currentYear.Substring(2, 2) + String.Format("{0:D5}", counter.count_no + 1) : salesid;
                        ViewBag.SalesId = salesid;
                        if (!id.ToString().IsNullOrWhiteSpace())
                        {
                            var CusName = db.Customers.FirstOrDefault(x => x.CusId == id).FullName;
                            ViewData["CustomerName"] = CusName;
                        }
                        else
                        {
                            ViewData["Customer"] = db.Customers.Where(x => x.Active == false && x.Status == false);
                        }
                        ViewData["Category"] = db.Types.Where(x => x.Module == "PaymentType" && x.Active == false && x.Status == false);

                        ViewBag.Employees = db.Employees.Where(e => e.Active.Equals(false) && e.Status.Equals(false) && !e.FullName.Contains("Admin")).ToList();
                        ViewData["Employee"] = ViewBag.Employees;
                        ViewBag.Products = db.Products.Where(x => x.Status == false && x.Active == false).ToList();
                        ViewData["Products"] = ViewBag.Products;

                        var salesitem = db.SalesItems.Where(x => x.SalesId == salesid).ToList();
                        if (salesitem.Any())
                        {
                            salesitemlist = (from si in db.SalesItems
                                             join emp in db.Employees on si.EmpNo equals emp.EmpNo into empGroup
                                             from employee in empGroup.DefaultIfEmpty()
                                             join pro in db.Products on si.ProductId equals pro.ProductId
                                             where si.SalesId.Equals(salesid)
                                             select new SalesItemUpdate
                                             {
                                                 SalesItemId = si.SalesItemId,
                                                 SalesId = si.SalesId,
                                                 EmpName = employee.DisplayName ?? "",
                                                 ProductName = pro.ProductName,
                                                 Quantity = si.Quantity,
                                                 UnitPrice = si.UnitPrice,
                                                 LineTotal = si.LineTotal,
                                                 LineDiscAmt = si.LineDiscAmt,
                                                 Remarks = si.Remarks,
                                             }).ToList();
                        }

                        var subtotal = salesitem.Sum(item => item.LineTotal);
                        var totalDisc = salesitem.Sum(item => item.LineDiscAmt);
                        var totalQty = salesitem.Sum(item => item.Quantity);

                        newsales = new SalesInvoiceViewModels()
                        {
                            SalesId = ViewBag.SalesId,
                            CusId = id ?? 0,
                            OrderDate = DateTime.Now,
                            SalesDetails = salesitemlist,
                            subtotal = subtotal,
                            totalDisc = totalDisc,
                            totalQty = totalQty
                        };

                    }

                    return View(newsales);
                }

                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        #region Daily Transaction Summary
        public ActionResult DailyTransaction(string date)
        {
            try
            {
                if (!string.IsNullOrEmpty(Session["EmpNo"] as string))
                {
                    DateTime salesDate = DateTime.ParseExact(date, "dd-MM-yyyy", System.Globalization.CultureInfo.InvariantCulture);
                    ViewBag.SalesDateRaw = date;
                    ViewBag.SalesDate = salesDate.ToString("dd/MM/yyyy");
                    return View();
                }
                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public ActionResult GetDailyTransactionData(string date, [DataSourceRequest] DataSourceRequest request)
        {
            DateTime salesDate = DateTime.ParseExact(date, "dd-MM-yyyy", System.Globalization.CultureInfo.InvariantCulture);
            var start_date = salesDate;
            var end_date = salesDate.AddDays(1).AddMilliseconds(-1);

            var summary = BuildDailyTransactionRows(start_date, end_date)
                .OrderBy(x => x.SalesId)
                .ToList();

            return Json(summary.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetDailyTransactionRangeData(string startDate, string endDate, [DataSourceRequest] DataSourceRequest request)
        {
            var start_date = DateTime.Parse(startDate);
            var end_date = DateTime.Parse(endDate).AddDays(1).AddMilliseconds(-1);

            var summary = BuildDailyTransactionRows(start_date, end_date)
                .OrderBy(x => x.PaymentDate).ThenBy(x => x.SalesId)
                .ToList();

            return Json(summary.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        private List<DailyTransactionReport> BuildDailyTransactionRows(DateTime start_date, DateTime end_date)
        {
            // Fetch all matching invoices first, tagged with GIRO flag
            var invoices = (from sa in db.Saless
                            join cus in db.Customers on sa.CusId equals cus.CusId
                            join pay in db.Types on sa.PaymentMethod equals pay.TypeId
                            where sa.PaymentDate >= start_date && sa.PaymentDate <= end_date
                            && sa.Active == false && sa.Status == false
                            select new
                            {
                                SalesId        = sa.SalesId,
                                CardNo         = cus.CardNo,
                                CustomerName   = cus.FullName,
                                PaymentTypeName = pay.TypeName,
                                InvoiceTotalAmt = sa.TotalAmt ?? 0,
                                InvoicePaidAmt  = sa.PaidAmt ?? 0,
                                IsGiro         = sa.GIRO,
                                DueInvoice     = sa.DueInvoice,
                                Remarks        = sa.Remarks,
                                PaymentDate    = sa.PaymentDate
                            }).ToList();

            // GIRO installment payments have no SalesItems of their own, so resolve the
            // beautician from the first SalesItem of the root invoice (DueInvoice, or the
            // row itself if it has no DueInvoice) that they were created against.
            var rootInvoiceIds = invoices
                .Where(x => x.IsGiro == true)
                .Select(x => string.IsNullOrEmpty(x.DueInvoice) ? x.SalesId : x.DueInvoice)
                .Distinct()
                .ToList();

            var firstBeauticianByRootInvoice = (from si in db.SalesItems
                                                 join emp in db.Employees on si.EmpNo equals emp.EmpNo into empGroup
                                                 from employee in empGroup.DefaultIfEmpty()
                                                 where rootInvoiceIds.Contains(si.SalesId)
                                                 && si.Active == false && si.Status == false
                                                 select new
                                                 {
                                                     si.SalesId,
                                                     si.SalesItemId,
                                                     BeauticianName = (si.EmpName != null && si.EmpName != "") ? si.EmpName : (employee != null ? employee.DisplayName : "")
                                                 }).ToList()
                .GroupBy(x => x.SalesId)
                .ToDictionary(g => g.Key, g => g.OrderBy(x => x.SalesItemId).First().BeauticianName);

            // Pre-compute: package ProductIds whose contents include at least one Service item.
            // Used for both GIRO and regular row classification.
            var serviceTypeId = db.Types
                .Where(x => x.TypeName == "Service")
                .Select(x => x.TypeId)
                .FirstOrDefault();
            var pkgWithServiceProductIds = (
                from pkg in db.Packages
                join det in db.PackageDetails on pkg.Id equals det.PackageId
                where det.ItemType == serviceTypeId
                select pkg.ProductId
            ).Distinct().ToList();

            // Pre-compute: package ProductIds whose contents include at least one TopUp item,
            // plus the TopUp TypeId itself for direct (non-package) TopUp lines.
            var topupTypeId = db.Types
                .Where(x => x.TypeName == "TopUp")
                .Select(x => x.TypeId)
                .FirstOrDefault();
            var pkgWithTopUpProductIds = (
                from pkg in db.Packages
                join det in db.PackageDetails on pkg.Id equals det.PackageId
                where det.ItemType == topupTypeId
                select pkg.ProductId
            ).Distinct().ToList();

            // For each root invoice, determine Facial vs Product by inspecting its SalesItems.
            // Root invoices may be outside the current date range (old original package sale),
            // so we query by SalesId directly rather than by date.
            var rootInvoiceTypeMap = (from si in db.SalesItems
                                      join pro in db.Products on si.ProductId equals pro.ProductId into proGroup
                                      from product in proGroup.DefaultIfEmpty()
                                      join typ in db.Types on (product != null ? product.TypeId : -1) equals typ.TypeId into typGroup
                                      from productType in typGroup.DefaultIfEmpty()
                                      where rootInvoiceIds.Contains(si.SalesId)
                                      && si.Active == false && si.Status == false
                                      select new
                                      {
                                          si.SalesId,
                                          IsTopUp = (si.TypeId == topupTypeId)
                                                    || pkgWithTopUpProductIds.Contains(si.ProductId),
                                          IsService = (productType != null && productType.TypeName == "Service")
                                                      || pkgWithServiceProductIds.Contains(si.ProductId)
                                      }).ToList()
                .GroupBy(x => x.SalesId)
                .ToDictionary(g => g.Key, g => g.Any(x => x.IsTopUp) ? "TopUp" : (g.Any(x => x.IsService) ? "Facial" : "Product"));

            // GIRO invoices (original package or installment): one row per invoice using PaidAmt
            var giroRows = invoices
                .Where(x => x.IsGiro == true)
                .Select(x =>
                {
                    string payType = x.PaymentTypeName.ToLower();
                    decimal amt = x.InvoicePaidAmt;
                    string rootInvoiceId = string.IsNullOrEmpty(x.DueInvoice) ? x.SalesId : x.DueInvoice;
                    return new DailyTransactionReport
                    {
                        SalesId       = x.SalesId,
                        CardNo        = x.CardNo,
                        CustomerName  = x.CustomerName,
                        FacialProduct = rootInvoiceTypeMap.ContainsKey(rootInvoiceId) ? rootInvoiceTypeMap[rootInvoiceId] : "GIRO",
                        BankAmt = payType.Contains("bank") ? amt : 0,
                        TNGAmt  = (payType.Contains("tng") || payType.Contains("touch") || payType.Contains("ewallet") || payType.Contains("e-wallet")) ? amt : 0,
                        CashAmt = payType.Contains("cash") ? amt : 0,
                        CardAmt = payType.Contains("card") ? amt : 0,
                        Beautician = firstBeauticianByRootInvoice.ContainsKey(rootInvoiceId) ? firstBeauticianByRootInvoice[rootInvoiceId] : "",
                        GroupAmt   = 0,
                        Remarks    = x.Remarks,
                        IsGiro     = true,
                        PaymentDate = x.PaymentDate
                    };
                }).ToList();

            // Regular invoices: fetch with item detail for type/beautician breakdown
            var rawData = (from sa in db.Saless
                           join cus in db.Customers on sa.CusId equals cus.CusId
                           join pay in db.Types on sa.PaymentMethod equals pay.TypeId
                           join si in db.SalesItems on sa.SalesId equals si.SalesId
                           join emp in db.Employees on si.EmpNo equals emp.EmpNo into empGroup
                           from employee in empGroup.DefaultIfEmpty()
                           join pro in db.Products on si.ProductId equals pro.ProductId into proGroup
                           from product in proGroup.DefaultIfEmpty()
                           join typ in db.Types on (product != null ? product.TypeId : -1) equals typ.TypeId into typGroup
                           from productType in typGroup.DefaultIfEmpty()
                           where sa.PaymentDate >= start_date && sa.PaymentDate <= end_date
                           && sa.Active == false && sa.Status == false
                           && si.Active == false && si.Status == false
                           && (sa.GIRO == false || sa.GIRO == null)
                           select new
                           {
                               SalesId         = sa.SalesId,
                               CardNo          = cus.CardNo,
                               CustomerName    = cus.FullName,
                               PaymentTypeName = pay.TypeName,
                               LineTotal       = si.LineTotal,
                               InvoiceTotalAmt = sa.TotalAmt ?? 0,
                               ProductTypeName = ((si.TypeId == topupTypeId) || pkgWithTopUpProductIds.Contains(si.ProductId))
                                                 ? "TopUp"
                                                 : (productType != null && productType.TypeName == "Service")
                                                   ? "Service"
                                                   : (pkgWithServiceProductIds.Contains(si.ProductId) ? "Service" : "Product"),
                               BeauticianName  = (si.EmpName != null && si.EmpName != "") ? si.EmpName : (employee != null ? employee.DisplayName : ""),
                               Remarks         = sa.Remarks,
                               PaymentDate     = sa.PaymentDate
                           }).ToList();

            // Pre-compute the sum of LineTotals per invoice so we can distribute
            // the invoice-level discount (lump sum or percentage) proportionally.
            var invoiceLineTotals = rawData
                .GroupBy(x => x.SalesId)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.LineTotal));

            // One row per unique combination of invoice + type + beautician.
            var regularRows = rawData
                .GroupBy(x => new { x.SalesId, x.CardNo, x.CustomerName, x.PaymentTypeName, x.Remarks, x.ProductTypeName, x.BeauticianName, x.PaymentDate })
                .Select(g =>
                {
                    var first = g.First();
                    string payType = first.PaymentTypeName.ToLower();
                    decimal groupLineTotal = g.Sum(x => x.LineTotal);

                    // Apply invoice-level discount proportionally across groups
                    decimal invoiceLineTotal = invoiceLineTotals.ContainsKey(first.SalesId) ? invoiceLineTotals[first.SalesId] : 0;
                    decimal ratio = invoiceLineTotal > 0 ? first.InvoiceTotalAmt / invoiceLineTotal : 1;
                    decimal amt = Math.Round(groupLineTotal * ratio, 2);

                    string label = first.ProductTypeName == "TopUp" ? "TopUp" : (first.ProductTypeName == "Service" ? "Facial" : "Product");

                    return new DailyTransactionReport
                    {
                        SalesId       = first.SalesId,
                        CardNo        = first.CardNo,
                        CustomerName  = first.CustomerName,
                        FacialProduct = label,
                        BankAmt = payType.Contains("bank") ? amt : 0,
                        TNGAmt  = (payType.Contains("tng") || payType.Contains("touch") || payType.Contains("ewallet") || payType.Contains("e-wallet")) ? amt : 0,
                        CashAmt = payType.Contains("cash") ? amt : 0,
                        CardAmt = payType.Contains("card") ? amt : 0,
                        Beautician = first.BeauticianName,
                        GroupAmt   = 0,
                        Remarks    = first.Remarks,
                        IsGiro     = false,
                        PaymentDate = first.PaymentDate
                    };
                }).ToList();

            return giroRows.Concat(regularRows).ToList();
        }
        #endregion

        #region Daily Sales Report
        public ActionResult SearchSales(string reportType)
        {
            try
            {
                if (!string.IsNullOrEmpty(Session["EmpNo"] as string))
                {
                    ViewData["Employee"] = db.Employees.Where(e => e.Active.Equals(false) && e.Status.Equals(false) && !e.FullName.Contains("Admin") && !e.FullName.Contains("Service")).ToList();

                    // Today's Sales Summary
                    DateTime today = DateTime.Now.Date.AddMilliseconds(1);
                    DateTime tmr = DateTime.Now.Date.AddDays(1).AddMilliseconds(-1);
                    var sales = (from sa in db.Saless
                                 where (sa.PaymentDate > today && sa.PaymentDate < tmr) && sa.Active.Equals(false) && sa.Status.Equals(false)
                                 select sa).ToList();

                    var totalAmount = sales.Sum(x => x.PaidAmt);
                    ViewBag.TotalAmount = totalAmount;
                    // End 

                    ReportSearchContent search = new ReportSearchContent();
                    search.reportType = reportType;
                    search.StartDate = DateTime.Now.Date;
                    search.EndDate = DateTime.Now.Date;

                    return View(search);
                }
                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }
        public ActionResult _SearchSalesReport(string startDate, string endDate)
        {
            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;

            return PartialView();
        }
        public ActionResult GetSalesReportData(string startDate, string endDate, [DataSourceRequest] DataSourceRequest request)
        {
            List<DailySalesReport> summary = new List<DailySalesReport>();

            DateTime today = DateTime.Now.Date;
            var start_date = DateTime.Parse(startDate);
            var end_date = DateTime.Parse(endDate).AddDays(1).AddMilliseconds(-1);

            var sales = db.Saless.Where(x => x.Active.Equals(false) && x.Status.Equals(false)).ToList();
            var paymentType = db.Types.Where(o => o.Status.Equals(false) && o.Active.Equals(false) && o.Module == "PaymentType");
            var byCash = paymentType.FirstOrDefault(x => x.TypeName == "Cash")?.TypeId;
            var byCard = paymentType.FirstOrDefault(x => x.TypeName == "Card")?.TypeId;
            var byEWallet = paymentType.FirstOrDefault(x => x.TypeName == "E-Wallet")?.TypeId;
            var byBank = paymentType.FirstOrDefault(x => x.TypeName == "Bank")?.TypeId;

            if (sales != null && sales.Any())
            {
                var dateRange = Enumerable.Range(0, (end_date - start_date).Days + 1)
                .Select(offset => start_date.AddDays(offset));

                summary = dateRange
                    .GroupJoin(sales,
                    date => date,
                    sale => sale.PaymentDate.Date,
                    (date, salesForDate) => new DailySalesReport
                    {
                        SalesDate = date,
                        CashTotal = salesForDate.Where(item => item.PaymentMethod == byCash).Sum(item => item?.PaidAmt ?? 0),
                        CardTotal = salesForDate.Where(item => item.PaymentMethod == byCard).Sum(item => item?.PaidAmt ?? 0),
                        BankTotal = salesForDate.Where(item => item.PaymentMethod == byBank).Sum(item => item?.PaidAmt ?? 0),
                        EWalletTotal = salesForDate.Where(item => item.PaymentMethod == byEWallet).Sum(item => item?.PaidAmt ?? 0),
                        OthersTotal = salesForDate.Where(item => item.PaymentMethod != byCash && item.PaymentMethod != byCard
                        && item.PaymentMethod != byEWallet && item.PaymentMethod != byBank).Sum(item => item?.PaidAmt ?? 0),
                        Total = salesForDate.Sum(item => item?.PaidAmt ?? 0),
                    }).ToList();
            }

            return Json(summary.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Product Settlement Report
        public ActionResult SearchProductSettlement()
        {
            try
            {
                if (!string.IsNullOrEmpty(Session["EmpNo"] as string))
                {
                    ReportSearchContent search = new ReportSearchContent();
                    search.StartDate = DateTime.Now.Date;
                    search.EndDate = DateTime.Now.Date;
                    return View(search);
                }
                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public ActionResult _SearchProductSettlementReport(string startDate, string endDate)
        {
            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;
            return PartialView();
        }

        public ActionResult GetProductSettlementData(string startDate, string endDate, [DataSourceRequest] DataSourceRequest request)
        {
            var start_date = DateTime.Parse(startDate);
            var end_date = DateTime.Parse(endDate).AddDays(1).AddMilliseconds(-1);

            var productTypeId = db.Types.FirstOrDefault(e => e.Active.Equals(false) && e.Status.Equals(false)
                                    && e.Module == "Product" && e.TypeName == "Product")?.TypeId;

            // normal (non-backordered) items settle on invoice payment date
            var query = from sa in db.Saless
                        join si in db.SalesItems on sa.SalesId equals si.SalesId
                        join cus in db.Customers on sa.CusId equals cus.CusId
                        join p in db.Products on si.ProductId equals p.ProductId
                        where sa.PaymentDate > start_date && sa.PaymentDate < end_date
                              && si.Active.Equals(false) && si.Status.Equals(false)
                              && sa.Active.Equals(false) && sa.Status.Equals(false)
                              && si.TypeId == productTypeId
                              && !si.IsBackordered
                        select new ProductSettlementReport
                        {
                            SalesId = sa.SalesId,
                            CustomerName = cus.FullName,
                            ProductName = p.ProductName,
                            Quantity = si.Quantity,
                            UnitPrice = si.UnitPrice,
                            LineDiscAmt = si.LineDiscAmt,
                            LineTotal = si.LineTotal,
                            PaymentDate = sa.PaymentDate,
                        };

            // backordered items settle per collection, on the collect date,
            // with quantity/discount/total prorated per collected unit
            var collected = from col in db.SalesItemCollections
                            join si in db.SalesItems on col.SalesItemId equals si.SalesItemId
                            join sa in db.Saless on si.SalesId equals sa.SalesId
                            join cus in db.Customers on sa.CusId equals cus.CusId
                            join p in db.Products on si.ProductId equals p.ProductId
                            where col.CollectDate > start_date && col.CollectDate < end_date
                                  && si.Active.Equals(false) && si.Status.Equals(false)
                                  && sa.Active.Equals(false) && sa.Status.Equals(false)
                                  && si.TypeId == productTypeId
                                  && si.IsBackordered
                            select new ProductSettlementReport
                            {
                                SalesId = sa.SalesId,
                                CustomerName = cus.FullName,
                                ProductName = p.ProductName,
                                Quantity = col.Qty,
                                UnitPrice = si.UnitPrice,
                                LineDiscAmt = si.LineDiscAmt * col.Qty / si.Quantity,
                                LineTotal = si.LineTotal * col.Qty / si.Quantity,
                                PaymentDate = col.CollectDate,
                            };

            return Json(query.Concat(collected).ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }
        #endregion


        //backup
        //#region All invoice 
        //public ActionResult AllInvoice()
        //{
        //    try
        //    {
        //        if (!string.IsNullOrEmpty(Session["EmpNo"] as string))
        //        {
        //            ViewData["Employee"] = db.Employees.Where(e => e.Active.Equals(false) && e.Status.Equals(false) && !e.FullName.Contains("Admin")).ToList();
        //            ViewData["Customer"] = db.Customers.Where(e => e.Active.Equals(false) && e.Status.Equals(false)).ToList();

        //            return View();
        //        }
        //        return RedirectToAction("Login", "Account");
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.ToString());
        //    }
        //}
        //public ActionResult _SearchAllInvoice(int? cusid, string period)
        //{
        //    ViewBag.cusid = cusid;
        //    ViewBag.period = period;
        //    ViewData["Customer"] = db.Customers.Where(e => e.Active.Equals(false) && e.Status.Equals(false)).ToList();
        //    ViewData["PaymentType"] = db.Types.Where(x => x.Module == "PaymentType").ToList();

        //    // Retrieve the viewmodel for the view here, depending on your data structure.
        //    return PartialView();
        //}
        //public ActionResult GetInvoiceData(string cusid, [DataSourceRequest] DataSourceRequest request)
        //{
        //    return Json(db.Saless.Where(x => x.Status.Equals(false) && x.Active.Equals(false)).ToDataSourceResult(request, o => new SalesViewModels()
        //    {
        //        SalesId = o.SalesId,
        //        CusId = o.CusId,
        //        TotalAmt = o.TotalAmt,
        //        DiscAmt = o.DiscAmt,
        //        PaidAmt = o.PaidAmt,
        //        BalAmt = o.BalAmt,
        //        PaymentMethod = o.PaymentMethod,
        //        PaymentDate = o.PaymentDate,
        //        OrderDate = o.OrderDate,
        //        Remarks = o.Remarks,
        //        Exchange = o.Exchange,
        //        GIRO = o.GIRO,
        //        Active = o.Active,
        //        Status = o.Status,
        //        AddBy = o.AddBy,
        //        ModBy = o.ModBy,
        //        AddDate = o.AddDate,
        //        ModDate = o.ModDate,
        //    }), JsonRequestBehavior.AllowGet);
        //}
        //#endregion

        public ActionResult CancelInvoice(string salesId)
        {
            try
            {
                if (!string.IsNullOrEmpty(Session["EmpNo"] as string))
                {
                    SalesInvoiceViewModels newsales = new SalesInvoiceViewModels();
                    return PartialView();
                  //  return View(newsales);
                }

                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public List<EmployeeViewModels> EmployeeList()
        {
            return db.Employees.Where(e => e.Active.Equals(false) && e.Status.Equals(false) && !e.FullName.Contains("Admin") && !e.FullName.Contains("Service")).ToList();
        }
    }
}