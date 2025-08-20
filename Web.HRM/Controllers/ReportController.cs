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

            if (!empNo.IsNullOrWhiteSpace())
            {
                summary = (from sa in db.Saless
                           join si in db.SalesItems on sa.SalesId equals si.SalesId
                           join cus in db.Customers on sa.CusId equals cus.CusId
                           join emp in db.Employees on si.EmpNo equals emp.EmpNo
                           join pay in db.Types on sa.PaymentMethod equals pay.TypeId
                           where si.EmpNo.Equals(empNo) && sa.PaymentDate > start_date && sa.PaymentDate < end_date //&& dateRange.Contains(sa.PaymentDate.Date)
                           && si.Active.Equals(false) && si.Status.Equals(false)
                           select new SalesReportViewModels
                           {
                               SalesItemId = si.SalesItemId,
                               SalesId = si.SalesId,
                               EmpNo = si.EmpNo,
                               PICName = emp.FullName,
                               ProductId = si.ProductId,
                               TypeId = si.TypeId,
                               Quantity = si.Quantity,
                               UnitPrice = si.UnitPrice,
                               TotalAmt = sa.TotalAmt ?? 0,
                               LineTotal = si.LineTotal,
                               //sa.SubTotal ?? 0,
                               LineDiscAmt = sa.DiscAmt + sa.DiscPercentageAmt ?? 0,
                               Remarks = si.Remarks,
                               PaymentDate = sa.PaymentDate,
                               PaymentMethod = pay.TypeName,
                               CusId = sa.CusId,
                               CardNo = cus.CardNo,
                               ImagePath = cus.ImagePath,
                           }).ToList();
            }
            else
            {
                summary = (from sa in db.Saless
                           join si in db.SalesItems on sa.SalesId equals si.SalesId
                           join cus in db.Customers on sa.CusId equals cus.CusId
                           join emp in db.Employees on si.EmpNo equals emp.EmpNo
                           join pay in db.Types on sa.PaymentMethod equals pay.TypeId
                           where si.Active.Equals(false) && sa.PaymentDate > start_date && sa.PaymentDate < end_date
                           && si.Status.Equals(false)
                           select new SalesReportViewModels
                           {
                               SalesItemId = si.SalesItemId,
                               SalesId = si.SalesId,
                               EmpNo = si.EmpNo,
                               PICName = emp.FullName,
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

            // Apply filter only if empNo is not null or whitespace
            if (!string.IsNullOrWhiteSpace(empNo))
            {
                query = query.Where(x => x.PICId == empNo);
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
        public ActionResult _SearchAllInvoice(int? cusid, string period, string startDate, string endDate)
        {
            ViewBag.cusid = cusid;
            ViewBag.period = period;
            ViewBag.startDate = string.IsNullOrEmpty(startDate) ? DateTime.Now.Date.ToString() : startDate;
            ViewBag.endDate = string.IsNullOrEmpty(endDate) ? DateTime.Now.Date.ToString() : endDate;
            ViewData["Customer"] = db.Customers.Where(e => e.Active.Equals(false) && e.Status.Equals(false)).ToList();
            ViewData["PaymentType"] = db.Types.Where(x => x.Module == "PaymentType").ToList();

            // Retrieve the viewmodel for the view here, depending on your data structure.
            return PartialView();
        }
        public ActionResult GetInvoiceData(string cusid, string startDate, string endDate, [DataSourceRequest] DataSourceRequest request)
        {
            DateTime today = DateTime.Now.Date;
            var start_date = DateTime.Parse(startDate);
            var end_date = DateTime.Parse(endDate).AddDays(1).AddMilliseconds(-1);

            return Json(db.Saless.Where(x => x.Status.Equals(false)
            && x.Active.Equals(false)
             && x.AddDate > start_date
              && x.AddDate < end_date).ToDataSourceResult(request, o => new SalesViewModels()
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
                                                               EmpName = employee.FullName ?? "",
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
                                                 EmpName = employee.FullName ?? "",
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
                                 where sa.PaymentDate > today && sa.PaymentDate < tmr
                                 select sa).ToList();

                    var totalAmount = sales.Sum(x => x.TotalAmt);
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

            var sales = db.Saless.ToList();
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
                        CashTotal = salesForDate.Where(item => item.PaymentMethod == byCash).Sum(item => item?.TotalAmt ?? 0),
                        CardTotal = salesForDate.Where(item => item.PaymentMethod == byCard).Sum(item => item?.TotalAmt ?? 0),
                        BankTotal = salesForDate.Where(item => item.PaymentMethod == byBank).Sum(item => item?.TotalAmt ?? 0),
                        EWalletTotal = salesForDate.Where(item => item.PaymentMethod == byEWallet).Sum(item => item?.TotalAmt ?? 0),
                        OthersTotal = salesForDate.Where(item => item.PaymentMethod != byCash && item.PaymentMethod != byCard
                        && item.PaymentMethod != byEWallet && item.PaymentMethod != byBank).Sum(item => item?.TotalAmt ?? 0),
                        Total = salesForDate.Sum(item => item?.TotalAmt ?? 0),
                    }).ToList();
            }

            return Json(summary.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
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