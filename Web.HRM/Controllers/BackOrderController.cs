using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Meo.Web.DBContext;
using Meo.Web.ViewModels;
using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace Web.HRM.Controllers
{
    public class BackOrderController : Controller
    {
        private readonly DBContext db = new DBContext();

        #region -------------- Back Order Default Listing --------------
        public ActionResult Index()
        {
            try
            {
                if (string.IsNullOrEmpty(Session["EmpNo"] as string))
                {
                    return RedirectToAction("Login", "Account");
                }

                return View();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }
        public ActionResult _SearchBackOrder(string searchContent)
        {
            ViewData["Product"] = db.Products.Where(x => x.Status == false && x.Active == false);
            ViewBag.searchContent = searchContent;
            return PartialView();
        }
        public virtual ActionResult Read_BackOrderHistory([DataSourceRequest] DataSourceRequest request, string searchContent)
        {
            try
            {
                List<BackOrderVMs> query = new List<BackOrderVMs>();
                if (!searchContent.IsNullOrWhiteSpace())
                {
                    searchContent = searchContent.Trim();

                    query =
                        (from si in db.SalesItems
                         join s in db.Saless on si.SalesId equals s.SalesId
                         join c in db.Customers on s.CusId equals c.CusId
                         where !si.Status && si.IsBackordered && !si.Active &&
                             (c.IcNo.Contains(searchContent) || c.CardNo.Contains(searchContent) ||
                             c.ContactNo.Contains(searchContent) || c.FullName.Replace(" ", "").ToLower().Contains(searchContent)
                             && c.Active.Equals(false) && c.Status.Equals(false))
                         select new BackOrderVMs
                         {
                             SalesItemId = si.SalesItemId,
                             SalesId = si.SalesId,
                             EmpNo = si.EmpNo,
                             ProductId = si.ProductId,
                             Quantity = si.Quantity,
                             UnitPrice = si.UnitPrice,
                             LineTotal = si.LineTotal,
                             LineDiscAmt = si.LineDiscAmt,
                             Exchange = si.Exchange,
                             Remarks = si.Remarks,
                             IsBackordered = si.IsBackordered,
                             Active = si.Active,
                             Status = si.Status,
                             AddBy = si.AddBy,
                             ModBy = si.ModBy,
                             AddDate = si.AddDate,
                             ModDate = si.ModDate,
                             CusName = c.FullName + " (" + c.CardNo + ")", // assuming you want this field
                         }).ToList();
                }
                else
                {
                    query =
                        (from si in db.SalesItems
                         join s in db.Saless on si.SalesId equals s.SalesId
                         join c in db.Customers on s.CusId equals c.CusId
                         where !si.Status && si.IsBackordered && !si.Active
                         select new BackOrderVMs
                         {
                             SalesItemId = si.SalesItemId,
                             SalesId = si.SalesId,
                             EmpNo = si.EmpNo,
                             ProductId = si.ProductId,
                             Quantity = si.Quantity,
                             UnitPrice = si.UnitPrice,
                             LineTotal = si.LineTotal,
                             LineDiscAmt = si.LineDiscAmt,
                             Exchange = si.Exchange,
                             Remarks = si.Remarks,
                             IsBackordered = si.IsBackordered,
                             Active = si.Active,
                             Status = si.Status,
                             AddBy = si.AddBy,
                             ModBy = si.ModBy,
                             AddDate = si.AddDate,
                             ModDate = si.ModDate,
                             CusName = c.FullName + " (" + c.CardNo + ")", // assuming you want this field
                         }).ToList();
                }

                return Json(query.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        [HttpGet]
        public ActionResult BackOrderSales(int? id, string salesid, string flag)
        {
            try
            {
                if (!string.IsNullOrEmpty(Session["EmpNo"] as string))
                {
                    SalesInvoiceViewModels newsales = new SalesInvoiceViewModels();

                    ViewBag.Employees = db.Employees.Where(e => e.Active.Equals(false) && e.Status.Equals(false) && !e.FullName.Contains("Admin") && !e.FullName.Contains("Service")).ToList();
                    ViewData["Category"] = db.Types.Where(x => x.Module == "PaymentType" && x.Active == false && x.Status == false);

                    //ViewBag.Cusmtomer = db.Customers.Where(e => e.Active.Equals(false) && e.Status.Equals(false) && !e.FullName.Contains("Admin")).ToList();
                    ViewData["Customer"] = GetCustomerList();
                    ViewData["Employee"] = ViewBag.Employees;
                    ViewData["Products"] = GetProductList();

                    if (!salesid.IsNullOrWhiteSpace())
                    {
                        var salesVM = db.Saless.FirstOrDefault(x => x.SalesId == salesid);

                        var CusName = db.Customers.FirstOrDefault(x => x.CusId == salesVM.CusId).FullName;
                        ViewData["CustomerName"] = CusName;
                        List<SalesItemUpdate> salesitem = GetSalesItemList(salesid);

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
                            AddBy = salesVM.AddBy,
                            ModBy = salesVM.ModBy,
                            AddDate = salesVM.AddDate,
                            ModDate = salesVM.ModDate,
                            SalesDetails = salesitem,

                            subtotal = salesitem.Sum(item => item.LineTotal),
                            totalDisc = salesitem.Sum(item => item.LineDiscAmt),
                            totalQty = salesitem.Sum(item => item.Quantity),
                        };

                        return View(invoiceVM);
                    }
                    else // New Back Order
                    {
                        List<SalesItemUpdate> salesitemlist = new List<SalesItemUpdate>();
                        string currentYear = DateTime.Now.Year.ToString();
                        var counter = db.Counters.Where(x => x.count_name == "invoiceno").First();
                        salesid = salesid == null ? counter.format + currentYear.Substring(2, 2) + String.Format("{0:D5}", counter.count_no + 1) : salesid;
                        ViewBag.SalesId = salesid;

                        if (!id.ToString().IsNullOrWhiteSpace() && id > 0)
                        {
                            var Cus = db.Customers.FirstOrDefault(x => x.CusId == id);
                            ViewData["CustomerName"] = Cus.FullName + " (TEL: " + Cus.ContactNo + ")";
                        }

                        var salesitem = db.SalesItems.Where(x => x.SalesId == salesid).ToList();
                        if (salesitem.Any())
                        {
                            salesitemlist = GetSalesItemList(salesid);
                        }

                        if (flag == "new")
                        {
                            foreach (var item in salesitemlist)
                            {
                                var toDlt = db.SalesItems.FirstOrDefault(x => x.SalesItemId == item.SalesItemId);

                                db.SalesItems.Remove(toDlt);
                                db.SaveChanges();
                            }

                            salesitemlist = new List<SalesItemUpdate>();
                        }

                        var subtotal = salesitemlist.Sum(item => item.LineTotal);
                        var totalDisc = salesitemlist.Sum(item => item.LineDiscAmt);
                        var totalQty = salesitemlist.Sum(item => item.Quantity);

                        newsales = new SalesInvoiceViewModels()
                        {
                            SalesId = ViewBag.SalesId,
                            CusId = id ?? 0,
                            OrderDate = DateTime.Now,
                            SalesDetails = salesitemlist,
                            subtotal = subtotal,
                            totalDisc = totalDisc,
                            totalQty = totalQty,
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

        public List<SalesItemUpdate> GetSalesItemList(string salesid)
        {
            List<SalesItemUpdate> salesitemlist = (from si in db.SalesItems
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
                                                       LinePayAmt = si.PayAmt,
                                                       LineTotal = si.LineTotal,
                                                       LineDiscAmt = si.LineDiscAmt,
                                                       Remarks = si.Remarks,
                                                   }).ToList();

            return salesitemlist;
        }

        public IList<ProductGIRO> GetProductList()
        {
            var proList = (from p in db.Products
                           join t in db.Types on p.TypeId equals t.TypeId
                           join s in db.Stock on p.ProductId equals s.ProductId into proGroup
                           from stk in proGroup.DefaultIfEmpty()
                           where p.Active == false && p.Status == false && (stk == null || stk.Status == false)
                            && t.TypeName == "Product"
                           select new ProductGIRO
                           {
                               ProductId = p.ProductId,
                               ProductCode = p.ProductCode,
                               ProductName = p.ProductCode + " (" + p.ProductName + ")",
                               TypeName = t.TypeName,
                               Cost = p.Cost,
                               Price = p.Price,
                               QtyAvailable = stk.QtyAvailable ?? 0,
                               //CreditBuy = p.CreditBuy,
                               GIROBuy = false,
                               FirstPayAmt = 0,
                           }).ToList();

            return proList;
        }

        public IList<Customer> GetCustomerList()
        {
            return (from e in db.Customers
                    where e.Active == false && e.Status == false
                    select new Customer
                    {
                        CusId = e.CusId,
                        CardNo = e.CardNo,
                        FullName = e.CardNo + " : " + e.FullName + " (TEL: " + e.ContactNo + ")",
                    }).ToList();
        }

        #region Add/Remove in Back Order form
        [HttpPost]
        public ActionResult AddSalesItem(string salesid, int pid, int qty, decimal price, decimal disc, decimal total, string empno, string cusid)
        {
            try
            {
                if (string.IsNullOrEmpty(Session["EmpNo"] as string))
                {
                    return RedirectToAction("Login", "Account");
                }

                if (ModelState.IsValid)
                {
                    var typeid = db.Products.Find(pid)?.TypeId ?? db.Packages.FirstOrDefault(x => x.ProductId == pid)?.ProductType;
                    var pro = db.SalesItems.FirstOrDefault(x => x.SalesId == salesid && x.ProductId == pid);

                    if (pro == null)
                    {
                        var item = new SalesItemViewModels
                        {
                            SalesId = salesid,
                            ProductId = pid,
                            Quantity = qty,
                            UnitPrice = price,
                            LineDiscAmt = disc,
                            LineTotal = total,
                            EmpNo = empno,
                            TypeId = (int)typeid,
                            IsBackordered = true,

                            AddBy = Session["EmpNo"].ToString() + "|" + Session["EmpName"].ToString(),
                            ModBy = Session["EmpNo"].ToString() + "|" + Session["EmpName"].ToString(),
                            AddDate = DateTime.Now,
                            ModDate = DateTime.Now
                        };

                        db.SalesItems.Add(item);
                        db.SaveChanges();
                    }
                    else
                    {
                        // if duplicate product, add quantity
                        pro.Quantity += qty;
                        pro.LineTotal = pro.Quantity * price;
                        pro.LineDiscAmt += disc;
                        pro.EmpNo = empno;
                        pro.ModBy = Session["EmpNo"].ToString() + "|" + Session["EmpName"].ToString();
                        pro.ModDate = DateTime.Now;
                        db.SalesItems.Attach(pro);
                        db.Entry(pro).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                }

                var currentUrl = Request.Url.AbsoluteUri;
                int lastSlashIndex = currentUrl.LastIndexOf('/');
                var baseUrl = currentUrl.Substring(0, lastSlashIndex);
                lastSlashIndex = baseUrl.LastIndexOf('/');
                baseUrl = baseUrl.Substring(0, lastSlashIndex);

                if (int.TryParse(cusid, out int parsedCusId))
                {
                    ViewBag.CusId = parsedCusId;
                    return JavaScript($"window.location.href = '{baseUrl}/BackOrder/BackOrderSales/{parsedCusId}';");
                }

                return JavaScript($"window.location.href = '{baseUrl}/BackOrder/BackOrderSales';");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }
        [HttpPost]
        public ActionResult DeleteSales(int detailsid)
        {
            FocDetails item = (from i in db.FocDetails
                               where i.Id == detailsid
                               select i).FirstOrDefault();

            db.FocDetails.Remove(item);
            db.SaveChanges();

            return new EmptyResult();
        }
        #endregion

        //public ActionResult ProductHistory(int? id)
        //{
        //    try
        //    {
        //        if (!string.IsNullOrEmpty(Session["EmpNo"] as string))
        //        {
        //            ViewData["Product"] = db.Products.Where(x => x.Status == false && x.Active == false);
        //            ViewBag.CusId = id;

        //            return PartialView(id);
        //        }
        //        return RedirectToAction("Login", "Account");
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.ToString());
        //    }
        //}

        //public virtual ActionResult Read_ProductHistory([DataSourceRequest] DataSourceRequest request, int cusId)
        //{
        //    try
        //    {
        //        var productType = db.Types.FirstOrDefault(x => x.TypeName == "Product");
        //        if (productType == null)
        //        {
        //            return Json(new { success = false, message = "Product type not found" }, JsonRequestBehavior.AllowGet);
        //        }

        //        int prodId = productType.TypeId;

        //        var query = db.SalesItems
        //            .Where(o => !o.Status && o.TypeId == prodId && db.Saless.Any(s => s.CusId == cusId && s.SalesId == o.SalesId))
        //             .AsEnumerable()
        //             .Select(o => new SalesItemViewModels
        //             {
        //                 SalesItemId = o.SalesItemId,
        //                 SalesId = o.SalesId,
        //                 EmpNo = o.EmpNo,
        //                 ProductId = o.ProductId,
        //                 Quantity = o.Quantity,
        //                 UnitPrice = o.UnitPrice,
        //                 LineTotal = o.LineTotal,
        //                 LineDiscAmt = o.LineDiscAmt,
        //                 Exchange = o.Exchange,
        //                 Remarks = o.Remarks,
        //                 Active = o.Active,
        //                 Status = o.Status,
        //                 AddBy = o.AddBy,
        //                 ModBy = o.ModBy,
        //                 AddDate = o.AddDate,
        //                 ModDate = o.ModDate
        //             }).ToList();

        //        var query2 = db.PackageSoldDetails
        //           .Where(o => !o.Status && o.ItemType == prodId && db.PackageSolds.Any(s => s.CusId == cusId && s.Id == o.PackageSoldId && !o.Status && !o.Active))
        //           .AsEnumerable()
        //           .Select(o => new SalesItemViewModels
        //           {
        //               SalesItemId = o.Id,
        //               SalesId = "",
        //               EmpNo = "",
        //               ProductId = o.ItemId,
        //               Quantity = o.Qty,
        //               UnitPrice = o.Cost,
        //               LineTotal = o.TotalCost,
        //               //LineDiscAmt = o.LineDiscAmt,
        //               //Exchange = o.Exchange,
        //               Remarks = o.Remarks,
        //               Active = o.Active,
        //               Status = o.Status,
        //               AddBy = o.AddBy,
        //               ModBy = o.ModBy,
        //               AddDate = o.AddDate,
        //               ModDate = o.ModDate
        //           }).ToList();

        //        var combinedList = query.Concat(query2).ToList();
        //        return Json(combinedList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
        //    }
        //}

        #region payment
        public ActionResult PaymentPopUp(string salesid, int cusid, decimal total)
        {
            if (!string.IsNullOrEmpty(Session["EmpNo"] as string))
            {
                List<TypeViewModels> PaymentType = db.Types.Where(x => x.Module == "PaymentType" && x.TypeName != "Credit Balance" && x.Active == false).ToList();
                ViewData["PaymentType"] = PaymentType;
                ViewBag.PaymentType = PaymentType;

                var discOptions = new List<SelectListItem>
                {
                    new SelectListItem { Text = "5%", Value = "5" },
                    new SelectListItem { Text = "10%", Value = "10" },
                    new SelectListItem { Text = "15%", Value = "15" },
                    new SelectListItem { Text = "20%", Value = "20" }
                };

                ViewBag.DiscOptions = discOptions;

                PaymentViewModels payment = new PaymentViewModels();
                payment.CusId = cusid;
                payment.SalesId = salesid;
                payment.PaymentDate = DateTime.Now;
                //payment.PaidAmt = 0;
                payment.BalAmt = 0;

                if (!salesid.IsNullOrWhiteSpace())
                {
                    //ViewBag. = db.Types.Where(x => x.Module == "PaymentType").ToList();
                    var sales = db.Saless.FirstOrDefault(x => x.SalesId == salesid && x.Status == false && x.Active == false);
                    if (sales != null)
                    {
                        payment.SubTotal = sales.TotalAmt;
                        payment.TotalAmt = sales.TotalAmt;
                        payment.PaidAmt = sales.TotalAmt;
                        payment.DiscAmt = sales.DiscAmt;
                    }
                    else
                    {
                        payment.SubTotal = total;
                        payment.TotalAmt = total;
                        payment.PaidAmt = total;
                    }
                }
                else
                {
                    // error
                }

                return PartialView(payment);
            }

            return RedirectToAction("Login", "Account");
        }

        public ActionResult Update_Payment(PaymentViewModels payment)
        {
            if (string.IsNullOrEmpty(Session["EmpNo"] as string))
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid && payment != null)
            {
                var discount = payment.DiscPercentage > 0 ? payment.DiscPercentage / 100 : payment.DiscAmt;
                string currentYear = DateTime.Now.Year.ToString();
                var counter = db.Counters.Where(x => x.count_name == "invoiceno").First();
                string newSalesId = counter.format + currentYear.Substring(2, 2) + String.Format("{0:D5}", counter.count_no + 1);
                var dueBalance = payment.PaidAmt - payment.TotalAmt;

                var sales = db.Saless.FirstOrDefault(x => x.SalesId == payment.SalesId && x.Status == false && x.Active == false);
                if (sales != null)
                {
                    sales.SalesId = newSalesId;
                    sales.PaymentMethod = payment.PaymentMethod;
                    sales.DiscAmt = payment.DiscAmt;
                    sales.DiscPercentage = payment.DiscPercentage;
                    sales.DiscPercentageAmt = payment.DiscPercentageAmt;
                    sales.TotalAmt = payment.TotalAmt;
                    sales.SubTotal = payment.SubTotal;
                    sales.PaidAmt = payment.PaidAmt;
                    sales.BalAmt = payment.BalAmt; // dueBalance;
                    sales.PaymentDate = DateTime.Now;
                    sales.Remarks = payment.Remarks;
                    sales.ModBy = Session["EmpNo"].ToString() + "|" + Session["EmpName"].ToString();
                    sales.ModDate = DateTime.Now;
                    sales.GIRO = false;

                    db.Saless.Attach(sales);
                    db.Entry(sales).State = EntityState.Modified;
                    //db.SaveChanges();
                }
                else
                {
                    // header
                    var sale = new SalesViewModels
                    {
                        CusId = payment.CusId,
                        SalesId = newSalesId,
                        PaymentMethod = payment.PaymentMethod,
                        TotalAmt = payment.TotalAmt,
                        SubTotal = payment.SubTotal,
                        DiscPercentage = payment.DiscPercentage,
                        DiscPercentageAmt = payment.DiscPercentageAmt,
                        DiscAmt = payment.DiscAmt,
                        PaidAmt = payment.PaidAmt,
                        BalAmt = payment.BalAmt, //dueBalance,
                        OrderDate = DateTime.Now,
                        PaymentDate = DateTime.Now,
                        Remarks = payment.Remarks,
                        GIRO = payment.IsGIRO, // payment.TotalAmt == payment.PaidAmt ? false : true,

                        Active = false,
                        Status = false,
                        AddBy = Session["EmpNo"].ToString() + "|" + Session["EmpName"].ToString(),
                        ModBy = Session["EmpNo"].ToString() + "|" + Session["EmpName"].ToString(),
                        AddDate = DateTime.Now,
                        ModDate = DateTime.Now
                    };

                    db.Saless.Add(sale);
                    //db.SaveChanges();

                    // Add service record to service table if any
                    var type = db.Types.Where(e => e.Active.Equals(false) && e.Status.Equals(false) && e.Module == "Product");
                    var productid = type.FirstOrDefault(e => e.TypeName == "Product")?.TypeId;

                    var allsalesitem = db.SalesItems.Where(x => x.SalesId == payment.SalesId && x.Active == false && x.Status == false).ToList();
                    var productitem = allsalesitem.Where(x => x.TypeId == productid && x.Active == false && x.Status == false).ToList();
                    var product = db.Products.Where(x => x.Active == false && x.Status == false);

                    // update salesId for diff Payment Code (INV/E/CS)
                    if (allsalesitem.Any())
                    {
                        foreach (var item in allsalesitem)
                        {
                            item.SalesId = newSalesId;
                            item.IsBackordered = true;
                            db.SalesItems.Attach(item);
                            db.Entry(item).State = EntityState.Modified;
                            //db.SaveChanges();
                        }
                    }
                }

                payment.SalesId = newSalesId;
            }

            db.SaveChanges();
            return Json(new { payment.SalesId });
        }

        // Collect button
        public ActionResult Collect(int? salesitemid)
        {
            try
            {
                if (!string.IsNullOrEmpty(Session["EmpNo"] as string))
                {
                    var salesitem = db.SalesItems.FirstOrDefault(x => x.SalesItemId == salesitemid);
                    salesitem.IsBackordered = false;
                    salesitem.Remarks = "Collected! On " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");

                    db.SalesItems.Attach(salesitem);
                    db.Entry(salesitem).State = EntityState.Modified;

                    //deduct stock
                    var stock = db.Stock.FirstOrDefault(x => x.ProductId == salesitem.ProductId);

                    TempData["ErrorMessage"] = null;
                    if (stock.QtyAvailable <= 0)
                    {
                        TempData["ErrorMessage"] = "Insufficient stock to collect.";
                        return RedirectToAction("Index", new { salesitemid = salesitemid });
                    }

                    stock.QtyAvailable -= salesitem.Quantity;

                    db.Stock.Attach(stock);
                    db.Entry(stock).State = EntityState.Modified;
                    db.SaveChanges();

                    var currentUrl = Request.Url.AbsoluteUri;
                    int lastSlashIndex = currentUrl.LastIndexOf('/');
                    var baseUrl = currentUrl.Substring(0, lastSlashIndex);
                    lastSlashIndex = baseUrl.LastIndexOf('/');
                    baseUrl = baseUrl.Substring(0, lastSlashIndex);

                    var salesCusId = db.Saless.FirstOrDefault(x => x.SalesId == salesitem.SalesId)?.CusId;
                    return RedirectToAction("CusPurchaseHistory", "Sales", new { id = salesCusId });
                }

                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }
        #endregion

        #region -------------- Customer History Back Order --------------
        public ActionResult BackOrder(int? id)
        {
            try
            {
                if (!string.IsNullOrEmpty(Session["EmpNo"] as string))
                {
                    ViewData["Product"] = GetProductList();
                    //ViewData["Product"] = db.Products.Where(x => x.Status == false && x.Active == false);
                    ViewBag.CusId = id;

                    return PartialView(id);
                }
                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }
        public virtual ActionResult Read_BackOrder([DataSourceRequest] DataSourceRequest request, int cusId)
        {
            try
            {
                var productType = db.Types.FirstOrDefault(x => x.TypeName == "Product");
                if (productType == null)
                {
                    return Json(new { success = false, message = "Product type not found" }, JsonRequestBehavior.AllowGet);
                }

                int prodId = productType.TypeId;

                var query = db.SalesItems
                    .Where(o => !o.Status && o.TypeId == prodId && db.Saless.Any(s => s.CusId == cusId && s.SalesId == o.SalesId && o.IsBackordered && !o.Status && !o.Active))
                    .AsEnumerable()
                    .Select(o => new SalesItemViewModels
                    {
                        SalesItemId = o.SalesItemId,
                        SalesId = o.SalesId,
                        EmpNo = o.EmpNo,
                        ProductId = o.ProductId,
                        Quantity = o.Quantity,
                        UnitPrice = o.UnitPrice,
                        LineTotal = o.LineTotal,
                        LineDiscAmt = o.LineDiscAmt,
                        Exchange = o.Exchange,
                        Remarks = o.Remarks,
                        IsBackordered = o.IsBackordered,
                        Active = o.Active,
                        Status = o.Status,
                        AddBy = o.AddBy,
                        ModBy = o.ModBy,
                        AddDate = o.AddDate,
                        ModDate = o.ModDate
                    }).ToList();

                return Json(query.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion


        #region -------------- Pre Order Default Listing --------------
        public ActionResult _SearchPreOrder(string searchContent)
        {
            if (string.IsNullOrEmpty(Session["EmpNo"] as string))
                return RedirectToAction("Login", "Account");

            ViewData["Product"] = GetProductList();
            ViewData["Customer"] = GetCustomerList();
            ViewBag.searchContent = searchContent;
            return PartialView();
        }

        public virtual ActionResult Read_PreOrderHistory([DataSourceRequest] DataSourceRequest request, string searchContent)
        {
            try
            {
                List<PreOrderVMs> query = new List<PreOrderVMs>();
                if (!searchContent.IsNullOrWhiteSpace())
                {
                    searchContent = searchContent.Trim();

                    query =
                        (from p in db.PreOrders
                         join c in db.Customers on p.CusId equals c.CusId
                         where p.Status == false && p.CollectDate == null &&
                             (c.IcNo.Contains(searchContent) || c.CardNo.Contains(searchContent) ||
                             c.ContactNo.Contains(searchContent) || c.FullName.Replace(" ", "").ToLower().Contains(searchContent)
                             && c.Active.Equals(false) && c.Status.Equals(false))
                         select new PreOrderVMs
                         {
                             Id = p.Id,
                             PurchaseDate = p.PurchaseDate,
                             CollectDate = p.CollectDate,
                             CusId = c.CusId,
                             CusName = c.FullName + " (" + c.CardNo + ")",
                             ProductId = p.ProductId,
                             Quantity = p.Quantity,
                             Remarks = p.Remarks,
                             Status = p.Status,
                             AddBy = p.AddBy,
                             ModBy = p.ModBy,
                             AddDate = p.AddDate,
                             ModDate = p.ModDate,
                         }).ToList();
                }
                else
                {
                    query =
                        (from p in db.PreOrders
                         join c in db.Customers on p.CusId equals c.CusId
                         where p.Status.Equals(false) && p.CollectDate == null  
                         select new PreOrderVMs
                         {
                             Id = p.Id,
                             PurchaseDate = p.PurchaseDate,
                             CollectDate = p.CollectDate,
                             CusId = c.CusId,
                             CusName = c.FullName + " (" + c.CardNo + ")",
                             ProductId = p.ProductId,
                             Quantity = p.Quantity,
                             Remarks = p.Remarks,
                             Status = p.Status,
                             AddBy = p.AddBy,
                             ModBy = p.ModBy,
                             AddDate = p.AddDate,
                             ModDate = p.ModDate,
                         }).ToList();
                }

                return Json(query.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult Create_PreOrder(PreOrder obj)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var preOrder = new PreOrder
                    {
                        PurchaseDate = obj.PurchaseDate,
                        CollectDate = obj.CollectDate,
                        CusId = obj.CusId,
                        CusName = obj.CusName,
                        ProductId = obj.ProductId,
                        ProductName = obj.ProductName,
                        Quantity = 1,
                        Remarks = obj.Remarks,
                        Status = false,
                        AddBy = Session["EmpNo"].ToString() + " | " + Session["EmpName"].ToString(),
                        ModBy = Session["EmpNo"].ToString() + " | " + Session["EmpName"].ToString(),
                        AddDate = DateTime.Now,
                        ModDate = DateTime.Now
                    };

                    // Add the entity.
                    db.PreOrders.Add(preOrder);

                    // Insert the entity in the database.
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }

            return Json(new { success = true, message = "Created successfully." });
        }

        [HttpPost]
        public ActionResult Update_PreOrder(PreOrder obj)
        {
            PreOrder item = db.PreOrders.Find(obj.Id);

            item.PurchaseDate = obj.PurchaseDate;
            item.CollectDate = obj.CollectDate;
            item.CusId = obj.CusId;
            item.CusName = obj.CusName;
            item.ProductId = obj.ProductId;
            item.ProductName = obj.ProductName;
            item.Quantity = obj.Quantity;
            item.Remarks = obj.Remarks;

            db.PreOrders.Attach(item);
            db.Entry(item).State = EntityState.Modified;
            db.SaveChanges();

            return Json(new { success = true, message = "Updated successfully." });
        }


        [HttpPost]
        public ActionResult Destroy_PreOrder(int id)
        {
            PreOrder item = db.PreOrders.Find(id);
            item.Status = true;

            db.PreOrders.Attach(item);
            db.Entry(item).State = EntityState.Modified;
            db.SaveChanges();

            return Json(new { success = true, message = "Delete successfully." });
        }

        [HttpPost]
        public ActionResult CollectPreOrder(int id)
        {
            try
            {
                if (string.IsNullOrEmpty(Session["EmpNo"] as string))
                    return RedirectToAction("Login", "Account");

                var item = db.PreOrders.Find(id);
                if (item == null)
                    return Json(new { success = false, message = "PreOrder not found." });

                item.CollectDate = DateTime.Now;
                db.Entry(item).State = EntityState.Modified;

                //deduct stock
                var stock = db.Stock.FirstOrDefault(x => x.ProductId == item.ProductId);
                if (stock == null)
                    return Json(new { success = false, message = "Stock not found." });

                if (stock.QtyAvailable < item.Quantity)
                {
                    return Json(new { success = false, message = "Insufficient stock to collect." });
                }

                stock.QtyAvailable -= item.Quantity;
                db.Entry(stock).State = EntityState.Modified;
                db.SaveChanges();

                return Json(new { success = true, message = "Product collected.", redirectUrl = Url.Action("Index") });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }
        #endregion
    }
}