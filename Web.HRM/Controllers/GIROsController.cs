using DocumentFormat.OpenXml.Office2010.Excel;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Meo.Web.DBContext;
using Meo.Web.ViewModels;
using Microsoft.Ajax.Utilities;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Windows;

namespace Web.HRM.Controllers
{
    public class GIROsController : AuthController
    {
        private readonly DBContext db = new DBContext();

        // GET: GIROs By GIROs
        //#region CRUD GIROs 
        //[HttpPost]
        //public ActionResult UpdateGIROs(UpdateGIROs salesitem)
        //{
        //    DBContext db = new DBContext();
        //    var item = db.GIROsItems.Find(salesitem.GIROsItemId);

        //    if (item == null)
        //    {
        //        // not found
        //    }

        //    // Body
        //    item.Quantity = salesitem.Quantity;
        //    item.UnitPrice = salesitem.UnitPrice;
        //    item.LineDiscAmt = salesitem.LineDiscAmt;
        //    item.LineTotal = salesitem.LineTotal;
        //    item.ModBy = Session["EmpNo"].ToString() + "|" + Session["EmpName"].ToString();
        //    item.ModDate = DateTime.Now;

        //    db.GIROsItems.Attach(item);
        //    db.Entry(item).State = EntityState.Modified;
        //    db.SaveChanges();

        //    var sales = db.GIROss.Find(item.GIROsId);
        //    var allitem = db.GIROsItems.Where(x => x.GIROsId == sales.GIROsId).ToList();
        //    decimal subtotal = 0;

        //    foreach (var record in allitem)
        //    {
        //        subtotal += record.LineTotal;
        //    }

        //    // Header
        //    sales.TotalAmt = subtotal;
        //    sales.ModBy = Session["EmpNo"].ToString() + "|" + Session["EmpName"].ToString();
        //    sales.ModDate = DateTime.Now;
        //    db.GIROss.Attach(sales);
        //    db.Entry(sales).State = EntityState.Modified;
        //    db.SaveChanges();

        //    return new EmptyResult();
        //}

        //[HttpPost]
        //public ActionResult DeleteGIROs(int salesitemid)
        //{
        //    GIROsItemViewModels item = (from i in db.GIROsItems
        //                                where i.GIROsItemId == salesitemid
        //                                select i).FirstOrDefault();

        //    db.GIROsItems.Remove(item);
        //    db.SaveChanges();

        //    return new EmptyResult();
        //}
        //#endregion

        public ActionResult GIROsTopUp(int cusId, decimal dueAmt)
        {
            var sales2 = db.Saless.Where(x => x.CusId == cusId && x.GIRO &&
                x.Status == false && x.Active == false && x.BalAmt == dueAmt && x.Remarks == "GIRO TopUp").ToList();

            var sales = db.Saless.Where(x => x.CusId == cusId && x.GIRO &&
                x.Status == false && x.Active == false && x.BalAmt == dueAmt && x.Remarks == "GIRO TopUp")
                .OrderByDescending(x => x.PaymentDate)
                .FirstOrDefault();

            // Check if the sales record is found
            if (sales != null)
            {
                // Redirect to GIROSales with the SalesId
                return RedirectToAction("GIROSales", new { salesId = sales.DueInvoice ?? sales.SalesId, isTopUp = true });
            }

            return RedirectToAction("Login", "Account");
        }

        #region GIROs
        public ActionResult GIROSales(string salesId, bool isTopUp = false)
        {
            try
            {
                if (!string.IsNullOrEmpty(Session["EmpNo"] as string))
                {
                    //ViewData["Product"] = db.Products.Where(x => x.Status == false && x.Active == false).ToList();
                    //ViewData["Employee"] = db.Employees.Where(e => e.Active.Equals(false) && e.Status.Equals(false) && !e.FullName.Contains("Admin")).ToList();

                    if (!string.IsNullOrWhiteSpace(salesId))
                    {
                        var sales = db.Saless.Where(x => x.SalesId == salesId && x.Status.Equals(false) && x.Active.Equals(false))
                            .OrderByDescending(x => x.PaymentDate)
                            .FirstOrDefault();
                        var salesitem = db.SalesItems.Where(x => x.SalesId == salesId && x.Status.Equals(false) && x.Active.Equals(false)).ToList();
                        var cus = db.Customers.FirstOrDefault(x => x.CusId == sales.CusId && x.Status.Equals(false) && x.Active.Equals(false));

                        GIROViewModels data = new GIROViewModels()
                        {
                            SalesId = salesId,
                            CusId = cus.CusId,
                            CusName = cus.FullName,
                            OrderDate = sales.OrderDate,
                            PaymentDate = DateTime.Now,
                            BalAmt = sales.BalAmt,
                            IsTopUp = isTopUp,
                        };

                        return View(data);
                    }

                    //return View();
                }

                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public ActionResult _SearchAllInvoice(string salesId)
        {
            ViewBag.salesId = salesId;
            ViewData["Customer"] = db.Customers.Where(e => e.Active.Equals(false) && e.Status.Equals(false)).ToList();
            ViewData["PaymentType"] = db.Types.Where(x => x.Module == "PaymentType").ToList();

            return PartialView();
        }

        public ActionResult GetInvoiceData([DataSourceRequest] DataSourceRequest request, string salesId)
        {
            return Json(db.Saless.Where(x => x.Status.Equals(false) && x.Active.Equals(false) && (x.SalesId == salesId || x.DueInvoice == salesId)).ToDataSourceResult(request, o => new SalesViewModels()
            {
                SalesId = o.SalesId,
                CusId = o.CusId,
                TotalAmt = o.TotalAmt,
                DiscAmt = o.DiscAmt + o.DiscPercentageAmt,
                PaidAmt = o.PaidAmt,
                BalAmt = o.BalAmt,
                PaymentMethod = o.PaymentMethod,
                PaymentDate = o.PaymentDate,
                OrderDate = o.OrderDate,
                Remarks = o.Remarks,
                Active = o.Active,
                Status = o.Status,
                AddBy = o.AddBy,
                ModBy = o.ModBy,
                AddDate = o.AddDate,
                ModDate = o.ModDate,
            }), JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Payment
        public ActionResult GIROPopUp(string salesid, bool isTopUp)
        {
            if (!string.IsNullOrEmpty(Session["EmpNo"] as string))
            {
                List<TypeViewModels> PaymentType = db.Types.Where(x => x.Module == "PaymentType" && x.TypeName != "Credit Balance" && x.Active == false).ToList();
                ViewData["PaymentType"] = PaymentType;
                ViewBag.PaymentType = PaymentType;

                string currentYear = DateTime.Now.Year.ToString();
                var counter = db.Counters.Where(x => x.count_name == "invoiceno").First();
                string newSalesId = counter.format + currentYear.Substring(2, 2) + String.Format("{0:D5}", counter.count_no + 1);
                    
                // Service balance
                var service = db.Services.FirstOrDefault(x => x.SalesId == salesid);
                
                PaymentViewModels payment = new PaymentViewModels();
                if (!salesid.IsNullOrWhiteSpace())
                {
                    var sales = db.Saless.Where(x => (x.SalesId == salesid || x.DueInvoice == salesid) && x.Status == false && x.Active == false).OrderByDescending(x => x.PaymentDate).ToList();
                    var lastPayment = sales.FirstOrDefault();
                    var firstPayment = sales.LastOrDefault();
                    //var sales = db.Saless.Where(x => (x.SalesId == salesid || x.DueInvoice == salesid) && x.Status == false && x.Active == false).ToList();

                    if (sales.Count > 0)
                    {
                        var totalPaid = sales.Sum(x => x.PaidAmt);
                        var balanceAmt = firstPayment.TotalAmt - totalPaid;

                        payment.OldSalesId = firstPayment.SalesId;
                        payment.SalesId = newSalesId;
                        payment.PaymentDate = DateTime.Now;
                        payment.TotalAmt = firstPayment.TotalAmt;
                        payment.BalAmt = lastPayment.BalAmt;
                        payment.PaidAmt = -lastPayment.BalAmt;
                        payment.CusId = firstPayment.CusId;
                        payment.IsTopUp = isTopUp;

                        decimal minPercentToPay = !isTopUp ? SafeDivide(lastPayment.BalAmt ?? 0, service.CourseBal ?? 0) : 0;
                        ViewBag.minAmount = Math.Round(-minPercentToPay, 2, MidpointRounding.AwayFromZero);
                    }
                }

                return PartialView(payment);
            }

            return RedirectToAction("Login", "Account");
        }

        public static decimal SafeDivide(decimal numerator, int denominator)
        {
            return denominator != 0 ? numerator / denominator : 0;
        }

        public ActionResult UpdatePaymentTopUp(PaymentViewModels payment)
        {
            if (!string.IsNullOrEmpty(Session["EmpNo"] as string))
            {
                if (ModelState.IsValid && payment != null)
                {
                    payment.PaymentDate = DateTime.Now;
                }

                var cus = db.Customers.FirstOrDefault(x => x.CusId == payment.CusId);
                if (payment.IsTopUp)
                    cus.TPDueAmt += payment.PaidAmt;
                //else
                //    cus.SVDueAmt += payment.PaidAmt;

                var sales = new SalesViewModels
                {
                    CusId = payment.CusId,
                    SalesId = payment.SalesId,
                    DueInvoice = payment.OldSalesId,
                    PaymentMethod = payment.PaymentMethod,
                    TotalAmt = payment.TotalAmt,
                    PaidAmt = payment.PaidAmt,
                    BalAmt = payment.PaidAmt + payment.TotalAmt,
                    OrderDate = DateTime.Now,
                    PaymentDate = payment.PaymentDate,// DateTime.Now,
                    Remarks = payment.Remarks,
                    GIRO = true,

                    Active = false,
                    Status = false,
                    AddBy = Session["EmpNo"].ToString() + "|" + Session["EmpName"].ToString(),
                    ModBy = Session["EmpNo"].ToString() + "|" + Session["EmpName"].ToString(),
                    AddDate = DateTime.Now,
                    ModDate = DateTime.Now
                };

                db.Saless.Add(sales);

                // Update service DueFlag to false 
                if (sales.BalAmt == 0 && !payment.IsTopUp)
                {
                    var service = db.Services.FirstOrDefault(x => x.SalesId == payment.OldSalesId);
                    service.DueFlag = false;
                }
                else if (payment.IsTopUp && sales.BalAmt >= 0)
                {
                    // Add package free credit - buy one package at a time
                    var topupid = db.Types.FirstOrDefault(e => e.Active.Equals(false) && e.Status.Equals(false) && e.TypeName == "TopUp")?.TypeId;
                    var origSales = db.SalesItems.FirstOrDefault(x => x.SalesId == payment.OldSalesId);
                    var package = db.Packages.FirstOrDefault(x => x.ProductId == origSales.ProductId);
                    var packagedetails = db.PackageDetails.FirstOrDefault(x => x.PackageId == package.Id && x.ItemType == topupid);

                    var salesList = db.Saless.Where(x => x.SalesId == payment.OldSalesId || x.DueInvoice == payment.OldSalesId);
                    var ToCredit = packagedetails.TotalCost - salesList.Sum(x => x.PaidAmt);
                    cus.CreditBal += ToCredit ?? 0;

                    // unlock free gift if any
                    var service = db.Services.FirstOrDefault(x => x.SalesId == payment.OldSalesId);
                    service.DueFlag = false;
                    service.Remarks = "";
                }
                else
                {
                    // Credit payment amount ONLY
                    cus.CreditBal += payment.PaidAmt ?? 0;
                }

                db.SaveChanges();
                return Json(new { payment.SalesId });
            }

            return RedirectToAction("Login", "Account");
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult UpdatePayment(PaymentViewModels payment)
        {
            if (!string.IsNullOrEmpty(Session["EmpNo"] as string))
            {
                if (ModelState.IsValid && payment != null)
                {
                    payment.PaymentDate = DateTime.Now;
                }

                //var origSales = db.Saless.FirstOrDefault(x => x.SalesId == payment.OldSalesId);

                // handle to deduct service amt
                var cus = db.Customers.FirstOrDefault(x => x.CusId == payment.CusId);
                if (payment.IsTopUp)
                    cus.TPDueAmt += payment.PaidAmt;
                else
                    cus.SVDueAmt += payment.PaidAmt;

                var sales = new SalesViewModels
                {
                    CusId = payment.CusId,
                    SalesId = payment.SalesId,
                    DueInvoice = payment.OldSalesId,
                    PaymentMethod = payment.PaymentMethod,
                    TotalAmt = payment.TotalAmt,
                    PaidAmt = payment.PaidAmt,
                    BalAmt = payment.PaidAmt + payment.TotalAmt,
                    OrderDate = DateTime.Now,
                    PaymentDate = payment.PaymentDate,// DateTime.Now,
                    Remarks = payment.Remarks,
                    GIRO = true,

                    Active = false,
                    Status = false,
                    AddBy = Session["EmpNo"].ToString() + "|" + Session["EmpName"].ToString(),
                    ModBy = Session["EmpNo"].ToString() + "|" + Session["EmpName"].ToString(),
                    AddDate = DateTime.Now,
                    ModDate = DateTime.Now
                };

                db.Saless.Add(sales);

                // Update service DueFlag to false 
                if (sales.BalAmt == 0)
                {
                    var service = db.Services.FirstOrDefault(x => x.SalesId == payment.OldSalesId);
                    service.DueFlag = false;
                }
                //else if (sales.BalAmt > 0)
                //{
                //    return Json(new { error = true, message = "" });
                //}

                db.SaveChanges();

                return Json(new { payment.SalesId });
            }

            return RedirectToAction("Login", "Account");
        }
        #endregion

        #region Update - update remark
        [AcceptVerbs(HttpVerbs.Post)]
        public virtual ActionResult Update_ServiceHistory([DataSourceRequest] DataSourceRequest request, ServiceHistory history)
        {
            if (ModelState.IsValid)
            {
                using (var db = new DBContext())
                {
                    var historyRecord = db.ServiceHistories.Find(history.Id);

                    if (historyRecord != null)
                    {
                        historyRecord.Remarks = history.Remarks;
                        db.ServiceHistories.Attach(historyRecord);
                        db.Entry(historyRecord).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                }
            }
            return Json(new[] { history }.ToDataSourceResult(request, ModelState), JsonRequestBehavior.AllowGet);
        }
        #endregion

        // GIRO Receipt
        public ActionResult GIROReceipt(string salesid)
        {
            try
            {
                if (!string.IsNullOrEmpty(Session["EmpNo"] as string))
                {
                    SalesInvoiceViewModels invoiceVM = new SalesInvoiceViewModels();

                    if (!salesid.IsNullOrWhiteSpace())
                    {
                        var salesVM = db.Saless.FirstOrDefault(x => x.SalesId == salesid);
                        var Cus = db.Customers.FirstOrDefault(x => x.CusId == salesVM.CusId);
                        var paymentType = db.Types.Find(salesVM.PaymentMethod).TypeName;
                        ViewData["CustomerName"] = Cus.FullName;

                        // Total Calculation summary
                        var searchId = salesid;
                        if (!string.IsNullOrEmpty(salesVM.DueInvoice))
                        {
                            searchId = salesVM.DueInvoice;

                            // Previous payment has been made
                            int count = 1;
                            var giroSales = db.Saless.Where(x => (x.DueInvoice == salesVM.DueInvoice || x.SalesId == salesVM.DueInvoice)
                                && x.PaymentDate < salesVM.PaymentDate && x.SalesId != salesVM.SalesId)
                                .OrderBy(x => x.PaymentDate).ToList();

                            giroSales.ForEach(item =>
                            {
                                var payment = new ReceiptAmountItem
                                {
                                    SaledId = item.SalesId,
                                    PaymentDate = item.PaymentDate,
                                    PaidCount = count++,
                                    TotalPaid = item.PaidAmt,
                                };

                                invoiceVM.GIROPaymentCounts.Add(payment);
                            });

                            var main = new ReceiptAmountItem
                            {
                                SaledId = salesVM.SalesId,
                                PaymentDate = salesVM.PaymentDate,
                                PaidCount = count++,
                                TotalPaid = salesVM.PaidAmt,
                            };

                            invoiceVM.GIROPaymentCounts.Add(main);

                            //sales.ReceiptItems =
                            //(from si in db.SalesItems
                            // join pack in db.Packages on si.ProductId equals pack.ProductId
                            // where si.SalesId == searchId
                            // select new ReceiptItem
                            // {
                            //     ProductCode = pack.Code,
                            //     ProductName = pack.Description,
                            //     UnitPrice = si.UnitPrice,
                            //     Quantity = si.Quantity,
                            //     LineTotal = si.LineTotal
                            // }).ToList();
                        }

                        // Get Sales Item details 
                        List<SalesItemUpdate> salesitem =
                                (from si in db.SalesItems
                                 join emp in db.Employees on si.EmpNo equals emp.EmpNo into empGroup
                                 from employee in empGroup.DefaultIfEmpty()
                                 join pro in db.Products on si.ProductId equals pro.ProductId
                                 where si.SalesId.Equals(searchId)
                                 select new SalesItemUpdate
                                 {
                                     SalesItemId = si.SalesItemId,
                                     SalesId = salesid,
                                     EmpName = employee.FullName ?? "",
                                     ProductName = pro.ProductCode + " - " + pro.ProductName,
                                     Quantity = si.Quantity,
                                     UnitPrice = si.UnitPrice,
                                     LineTotal = si.LineTotal,
                                     LinePayAmt = si.PayAmt,
                                     LineDiscAmt = si.LineDiscAmt,
                                     LineOutstandingAmt = si.PayAmt - si.LineTotal,
                                     Remarks = si.Remarks,
                                 }).ToList();

                        var salesitem2 =
                            (from si in db.SalesItems
                             join emp in db.Employees on si.EmpNo equals emp.EmpNo into empGroup
                             from employee in empGroup.DefaultIfEmpty()
                             join pack in db.Packages on si.ProductId equals pack.ProductId
                             where si.SalesId.Equals(searchId)
                             select new SalesItemUpdate
                             {
                                 SalesItemId = si.SalesItemId,
                                 SalesId = salesid,
                                 EmpName = employee.FullName ?? "",
                                 ProductName = pack.Code + " - " + pack.Remarks,
                                 Quantity = si.Quantity,
                                 UnitPrice = si.UnitPrice,
                                 LineTotal = si.LineTotal,
                                 LinePayAmt = si.PayAmt,
                                 LineDiscAmt = si.LineDiscAmt,
                                 LineOutstandingAmt = si.PayAmt - si.LineTotal,
                                 Remarks = si.Remarks,
                             }).ToList();

                        salesitem = salesitem.Concat(salesitem2).ToList();

                        invoiceVM = new SalesInvoiceViewModels()
                        {
                            SalesId = salesVM.SalesId,
                            CusId = salesVM.CusId,
                            CusName = Cus.FullName,
                            CardNo = Cus.CardNo,
                            CreditBal = Cus.CreditBal,
                            PaymentMethod = salesVM.PaymentMethod,
                            PaymentMethodName = paymentType,
                            OrderDate = salesVM.OrderDate,
                            PaymentDate = salesVM.PaymentDate,
                            TotalAmt = salesVM.TotalAmt,
                            PaidAmt = salesVM.PaidAmt,
                            DiscAmt = salesVM.DiscAmt + salesVM.DiscPercentageAmt,
                            BalAmt = salesVM.BalAmt,
                            Remarks = salesVM.Remarks,
                            Active = salesVM.Active,
                            Status = salesVM.Status,
                            Exchange = salesVM.Exchange,
                            AddBy = salesVM.AddBy,
                            ModBy = salesVM.ModBy,
                            AddDate = salesVM.AddDate,
                            ModDate = salesVM.ModDate,
                            SalesDetails = salesitem,

                            GIROPaymentCounts = invoiceVM.GIROPaymentCounts,
                            dueInvoice = salesVM.DueInvoice,
                            subtotal = salesitem.Sum(item => item.LineTotal),
                            totalPayAmt = salesitem.Sum(item => item.LinePayAmt),
                            totalQty = salesitem.Sum(item => item.Quantity),
                            totalOutStanding = salesitem.Sum(item => item.LineOutstandingAmt),
                        };
                    }

                    return View(invoiceVM);
                }

                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        //public ActionResult CheckDuplicatePayment(string salesid)
        //{
        //    try
        //    {

        //        return RedirectToAction("Login", "Account");
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.ToString());
        //    }
        //}

        [HttpGet]
        public JsonResult PaymentCount(string salesId)
        {
            var paymentCount = db.Saless
                .Where(p => p.Status == false && p.Active == false && (p.SalesId == salesId || p.DueInvoice == salesId))
                .Count();

            return Json(new { count = paymentCount }, JsonRequestBehavior.AllowGet);
        }
    }
}