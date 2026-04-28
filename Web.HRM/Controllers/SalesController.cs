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
    public class SalesController : Controller
    {
        private readonly DBContext db = new DBContext();

        // GET: Sales By Sales
        public ActionResult SalesByCus(int? id)
        {
            try
            {
                if (!string.IsNullOrEmpty(Session["EmpNo"] as string))
                {
                    var customer = db.Customers.FirstOrDefault(x => x.CusId == id);
                    ViewBag.CusId = id;
                    ViewData["Credit"] = customer.CreditBal;
                    ViewData["CusContactNo"] = customer.ContactNo;
                    ViewData["CusFullName"] = customer.FullName;
                    ViewData["CusCardNo"] = customer.CardNo;
                    ViewData["CusImagePath"] = customer.ImagePath;
                    ViewData["TPDueAmt"] = customer.TPDueAmt;
                    ViewData["SVDueAmt"] = customer.SVDueAmt;

                    // Get available service
                    var type = db.Types.FirstOrDefault(x => x.Module == "Product" && x.TypeName == "Service").TypeId;
                    List<Service> avaiService = db.Services.Where(x => x.CusId == id).ToList();

                    return View(id);
                }

                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        // GET: Sales By Sales
        public ActionResult CusPurchaseHistory(int? id) //id = cusid
        {
            try
            {
                if (!string.IsNullOrEmpty(Session["EmpNo"] as string))
                {
                    ViewData["Product"] = db.Products.Where(x => x.Status == false && x.Active == false);
                    ViewData["PaymentType"] = db.Types.Where(x => x.Module == "PaymentType").ToList();

                    if (id > 0)
                    {
                        var customers = db.Customers.Where(e => e.Active.Equals(false) && e.Status.Equals(false)).ToList();
                        ViewData["Customer"] = customers;

                        var customer = customers.FirstOrDefault(x => x.CusId == id);
                        ViewBag.CusId = id;
                        ViewData["Credit"] = customer.CreditBal;
                        ViewData["CusContactNo"] = customer.ContactNo;
                        ViewData["CusFullName"] = customer.FullName;
                        ViewData["CusCardNo"] = customer.CardNo;
                        ViewData["CusImagePath"] = customer.ImagePath;
                        return View(id);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public ActionResult Read_SalesByCus([DataSourceRequest] DataSourceRequest request, int cusId)
        {
            //List<SalesViewModels> sales = new List<SalesViewModels>();
            try
            {
                return Json(db.Saless.Where(x => x.CusId == cusId && x.Status.Equals(false) && x.Active.Equals(false)).ToDataSourceResult(request, o => new SalesViewModels()
                {
                    SalesId = o.SalesId,
                    CusId = o.CusId,
                    PaymentMethod = o.PaymentMethod,
                    OrderDate = o.OrderDate,
                    PaymentDate = o.PaymentDate,
                    TotalAmt = o.TotalAmt,
                    PaidAmt = o.PaidAmt,
                    DiscAmt = o.DiscAmt,
                    BalAmt = o.BalAmt,
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
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        #region Invoice 
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

            var salesitemlist2 = (from si in db.SalesItems
                                  join emp in db.Employees on si.EmpNo equals emp.EmpNo into empGroup
                                  from employee in empGroup.DefaultIfEmpty()
                                  join pack in db.Packages on si.ProductId equals pack.ProductId
                                  where si.SalesId.Equals(salesid)
                                  select new SalesItemUpdate
                                  {
                                      SalesItemId = si.SalesItemId,
                                      SalesId = si.SalesId,
                                      EmpName = employee.FullName ?? "",
                                      ProductName = pack.Code + " (" + pack.Remarks + ")",
                                      Quantity = si.Quantity,
                                      UnitPrice = si.UnitPrice,
                                      LinePayAmt = si.PayAmt,
                                      LineTotal = si.LineTotal,
                                      LineDiscAmt = si.LineDiscAmt,
                                      Remarks = si.Remarks,
                                  }).ToList();

            return salesitemlist.Concat(salesitemlist2).ToList();
        }

        public IList<ProductGIRO> GetProductList()
        {
            var proList = (from p in db.Products
                           join t in db.Types on p.TypeId equals t.TypeId
                           join s in db.Stock on p.ProductId equals s.ProductId into proGroup
                           from stk in proGroup.DefaultIfEmpty()
                           where p.Active == false && p.Status == false && (stk == null || stk.Status == false)
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

            var packList = (from p in db.Packages
                                //join d in db.PackageDetails on p.Id equals d.PackageId
                            join t in db.Types on p.ProductType equals t.TypeId
                            where p.Active == false && p.Status == false
                            select new ProductGIRO
                            {
                                ProductId = p.ProductId,
                                ProductCode = p.Code,
                                ProductName = p.Code + " (" + p.Description + ")",
                                TypeName = t.TypeName,
                                Cost = p.TotalCost,
                                Price = p.SellingPrice,
                                //QtyAvailable = stk.QtyAvailable ?? 0,
                                //CreditBuy = p.CreditBuy,
                                GIROBuy = p.GIROBuy,
                                FirstPayAmt = p.FirstPayAmt,
                            }).ToList();

            IList<ProductGIRO> combinedList = proList.Concat(packList).ToList();
            return combinedList.OrderBy(p => p.ProductCode).ToList();
        }

        [HttpGet]
        public ActionResult Invoice(int? id, string salesid, string flag)
        {
            try
            {
                if (!string.IsNullOrEmpty(Session["EmpNo"] as string))
                {
                    SalesInvoiceViewModels newsales = new SalesInvoiceViewModels();

                    ViewBag.Employees = db.Employees.Where(e => e.Active.Equals(false) && e.Status.Equals(false) && !e.FullName.Contains("Admin") && !e.FullName.Contains("Service")).ToList();
                    ViewData["Category"] = db.Types.Where(x => x.Module == "PaymentType" && x.Active == false && x.Status == false);
                    ViewBag.Products = GetProductList();

                    ViewBag.Cusmtomer = db.Customers.Where(e => e.Active.Equals(false) && e.Status.Equals(false) && !e.FullName.Contains("Admin")).ToList();
                    var cus = (from e in db.Customers
                               where e.Active == false && e.Status == false
                               select new Customer
                               {
                                   CusId = e.CusId,
                                   CardNo = e.CardNo,
                                   FullName = e.CardNo + " : " + e.FullName + " (TEL: " + e.ContactNo + ")",//+ " / IC: " + e.IcNo,
                               }).ToList();

                    ViewData["Customer"] = cus;
                    ViewData["Employee"] = ViewBag.Employees;
                    ViewData["Products"] = ViewBag.Products;

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
                    else
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

        [HttpGet]
        public ActionResult GIROSales(int? id, string salesid, string flag)
        {
            try
            {
                if (!string.IsNullOrEmpty(Session["EmpNo"] as string))
                {
                    return View(SalesFunction(salesid, flag, id ?? 0));
                }

                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public SalesInvoiceViewModels SalesFunction(string salesid, string flag, int? id)
        {
            SalesInvoiceViewModels newsales = new SalesInvoiceViewModels();

            ViewBag.Employees = db.Employees.Where(e => e.Active.Equals(false) && e.Status.Equals(false) && !e.FullName.Contains("Admin") && !e.FullName.Contains("Service")).ToList();
            ViewData["Category"] = db.Types.Where(x => x.Module == "PaymentType" && x.Active == false && x.Status == false);
            ViewBag.Products = GetProductList().Where(x => x.GIROBuy == true).ToList();

            ViewBag.Cusmtomer = db.Customers.Where(e => e.Active.Equals(false) && e.Status.Equals(false) && !e.FullName.Contains("Admin")).ToList();
            var cus = (from e in db.Customers
                       where e.Active == false && e.Status == false
                       select new Customer
                       {
                           CusId = e.CusId,
                           CardNo = e.CardNo,
                           FullName = e.CardNo + " : " + e.FullName + " (TEL: " + e.ContactNo + ")",//+ " / IC: " + e.IcNo,
                       }).ToList();

            ViewData["Customer"] = cus;
            ViewData["Employee"] = ViewBag.Employees;
            ViewData["Products"] = ViewBag.Products;

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
                    TotalAmt = salesVM.TotalAmt ?? 0,
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
                    totalPayAmt = salesitem.Sum(item => item.LinePayAmt),
                    totalQty = salesitem.Sum(item => item.Quantity),
                };

                return invoiceVM;
            }
            else
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
                var totalPayAmt = salesitemlist.Sum(item => item.LinePayAmt);

                newsales = new SalesInvoiceViewModels()
                {
                    SalesId = ViewBag.SalesId,
                    CusId = id ?? 0,
                    OrderDate = DateTime.Now,
                    SalesDetails = salesitemlist,
                    subtotal = subtotal,
                    totalDisc = totalDisc,
                    totalQty = totalQty,
                    totalPayAmt = totalPayAmt,
                };
            }

            return newsales;
        }

        [HttpPost]
        public ActionResult AddSalesItem(string path, string salesid, int pid, int qty, decimal price, decimal? payamt, decimal disc, decimal total, string empno, string cusid, bool backorder = false, string flag = "")
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var packageId = db.Packages.FirstOrDefault(x => x.ProductId == pid)?.Id;
                    var productTypeId = db.Types.FirstOrDefault(x => x.TypeName == "Product")?.TypeId;
                    var packageDetails = db.PackageDetails.Where(x => x.PackageId == packageId && x.ItemType == productTypeId && x.Active == false).ToList();
                    var stockAvailable = db.Stock.Where(x => x.Status == false).ToList();
                    var noStockMsg = "";

                    foreach (var details in packageDetails)
                    {
                        // packageDetails itemId = ProductId -Fail-> Stock check product qty !!! check cus select how many Qty 
                        var itemAvailableQty = stockAvailable.FirstOrDefault(x => x.ProductId == details.ItemId)?.QtyAvailable ?? 0;
                        bool isNoStock = itemAvailableQty > (details.Qty * qty);

                        if (isNoStock)
                        {
                            noStockMsg = "Insufficient stock.";
                            //break; can add product name
                            return Json(new { error = true, message = noStockMsg });
                        }
                    }

                    if (noStockMsg != "")
                    {
                        return Json(new { error = true, message = noStockMsg });
                    }
                }

                if (!string.IsNullOrEmpty(Session["EmpNo"] as string))
                {
                    var typeid = db.Products.Find(pid)?.TypeId ?? db.Packages.FirstOrDefault(x => x.ProductId == pid)?.ProductType;
                    var pro = db.SalesItems.FirstOrDefault(x => x.SalesId == salesid && x.ProductId == pid);

                    if (pro != null)
                    {
                        pro.PayAmt += payamt;
                        pro.Quantity += qty;
                        pro.LineTotal = pro.Quantity * price;
                        pro.LineDiscAmt += disc;
                        db.SalesItems.Attach(pro);
                        db.Entry(pro).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    else
                    {
                        var item = new SalesItemViewModels
                        {
                            SalesId = salesid,
                            ProductId = pid,
                            Quantity = qty,
                            UnitPrice = price,
                            PayAmt = payamt,
                            LineDiscAmt = disc,
                            LineTotal = total,
                            EmpNo = empno,
                            TypeId = (int)typeid,
                            IsBackordered = backorder,

                            AddBy = Session["EmpNo"].ToString() + "|" + Session["EmpName"].ToString(),
                            ModBy = Session["EmpNo"].ToString() + "|" + Session["EmpName"].ToString(),
                            AddDate = DateTime.Now,
                            ModDate = DateTime.Now
                        };

                        db.SalesItems.Add(item);
                        db.SaveChanges();
                    }

                    var redirectUrl = "";
                    var currentUrl = Request.Url.AbsoluteUri;
                    int lastSlashIndex = currentUrl.LastIndexOf('/');
                    var baseUrl = currentUrl.Substring(0, lastSlashIndex);
                    lastSlashIndex = baseUrl.LastIndexOf('/');
                    baseUrl = baseUrl.Substring(0, lastSlashIndex);

                    if (int.TryParse(cusid, out int parsedCusId))
                    {
                        ViewBag.CusId = parsedCusId;
                        if (flag == "credit")
                        {
                            redirectUrl = $"/Sales/InvoiceByCredit/{parsedCusId}";
                            //return JavaScript($"window.location.href = '{baseUrl}{redirectUrl}';");
                            return JavaScript($"window.location.href = '/Sales/InvoiceByCredit/{parsedCusId}';");
                        }
                        if (flag == "backorder")
                        {
                            redirectUrl = $"/BackOrder/{path}/{parsedCusId}";
                            return JavaScript($"window.location.href = '/BackOrder/{path}/{parsedCusId}';");
                        }
                        else
                        {
                            redirectUrl = $"/Sales/{path}/{parsedCusId}";
                            //return JavaScript("window.location.reload();");
                            return JavaScript($"window.location.href = '{baseUrl}{redirectUrl}';");
                        }
                    }

                    // force to refresh page if path are the same
                    var currentPath = Request.UrlReferrer.AbsoluteUri + '#';
                    redirectUrl = $"/Sales/{path}#";
                    var newPath = $"{baseUrl}{redirectUrl}";
                    if (currentPath == newPath)
                    {
                        return JavaScript("window.location.reload(true);");
                    }

                    redirectUrl = $"/Sales/{path}#";
                    //return JavaScript("window.location.reload();");
                    return JavaScript($"window.location.href = '{baseUrl}{redirectUrl}';");
                    //   return new EmptyResult();
                    //return JavaScript("window.location.reload(true);");
                }

                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public ActionResult PreviewINV(int? id, string salesid, string from)
        {
            try
            {
                if (!string.IsNullOrEmpty(Session["EmpNo"] as string))
                {
                    ViewData["From"] = from ?? "";
                    SalesInvoiceViewModels newsales = new SalesInvoiceViewModels();
                    SalesInvoiceViewModels invoiceVM = new SalesInvoiceViewModels();

                    var salesVM = db.Saless.FirstOrDefault(x => x.SalesId == salesid);
                    var Cus = db.Customers.FirstOrDefault(x => x.CusId == salesVM.CusId);
                    ViewData["CustomerName"] = Cus.FullName;

                    if (!salesid.IsNullOrWhiteSpace())
                    {
                        var paymentType = db.Types.Find(salesVM.PaymentMethod).TypeName;

                        decimal lineTotal = 0;
                        int qtyTotal = 0;

                        if (salesVM.Exchange)
                        {
                            // Get Sales item
                            List<SalesItemUpdate> salesItem =
                               (from si in db.SalesItems
                                join emp in db.Employees on si.EmpNo equals emp.EmpNo into empGroup
                                from employee in empGroup.DefaultIfEmpty()
                                join pro in db.Products on si.ProductId equals pro.ProductId
                                where si.SalesId.Equals(salesid)
                                select new SalesItemUpdate
                                {
                                    SalesItemId = si.SalesItemId,
                                    SalesId = salesid,
                                    EmpName = employee.FullName ?? "",
                                    ProductName = pro.ProductCode + " - " + pro.ProductName,
                                    Quantity = si.Quantity,
                                    UnitPrice = si.UnitPrice,
                                    LineDiscAmt = si.LineDiscAmt,
                                    LineTotal = si.LineTotal,
                                    Remarks = si.Remarks,
                                }).ToList();

                            // Get Exchange item
                            List<SalesItemUpdate> exchangeItem =
                               (from si in db.Exchanges
                                join pro in db.Products on si.ProductId equals pro.ProductId
                                where si.SalesId.Equals(salesid)
                                select new SalesItemUpdate
                                {
                                    SalesId = salesid,
                                    ProductName = pro.ProductCode + " - " + pro.ProductName,
                                    Quantity = -si.Quantity ?? 0,
                                    UnitPrice = -si.UnitPrice ?? 0,
                                    LineTotal = -si.LineTotal ?? 0,
                                    //LineDiscAmt = si.LineDiscAmt ?? 0,
                                    Remarks = si.Remarks,
                                }).ToList();

                            // Sum the LineTotal values
                            lineTotal += salesItem.Sum(item => item.LineTotal);
                            lineTotal += exchangeItem.Sum(item => item.LineTotal);
                            qtyTotal += salesItem.Sum(item => item.Quantity);
                            qtyTotal += exchangeItem.Sum(item => item.Quantity);

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
                                SalesDetails = salesItem,
                                ExchangeDetails = exchangeItem,

                                subtotal = lineTotal,
                                totalDisc = salesItem.Sum(item => item.LineDiscAmt),
                                totalQty = qtyTotal,
                            };
                        }
                        else
                        {
                            ViewBag.Employees = db.Employees.Where(e => e.Active.Equals(false) && e.Status.Equals(false) && !e.FullName.Contains("Admin")).ToList();
                            ViewData["Employee"] = ViewBag.Employees;
                            ViewBag.Products = db.Products.Where(x => x.Status == false && x.Active == false).ToList();
                            ViewData["Products"] = ViewBag.Products;

                            List<SalesItemUpdate> salesitem =
                                (from si in db.SalesItems
                                 join emp in db.Employees on si.EmpNo equals emp.EmpNo into empGroup
                                 from employee in empGroup.DefaultIfEmpty()
                                 join pro in db.Products on si.ProductId equals pro.ProductId
                                 where si.SalesId.Equals(salesid)
                                 select new SalesItemUpdate
                                 {
                                     SalesItemId = si.SalesItemId,
                                     SalesId = salesid,
                                     EmpName = employee.FullName ?? "",
                                     ProductName = pro.ProductCode + " - " + pro.ProductName,
                                     Quantity = si.Quantity,
                                     UnitPrice = si.UnitPrice,
                                     LineTotal = si.LineTotal,
                                     LineDiscAmt = si.LineDiscAmt,
                                     Remarks = si.Remarks,
                                 }).ToList();

                            var salesitem2 =
                                (from si in db.SalesItems
                                 join emp in db.Employees on si.EmpNo equals emp.EmpNo into empGroup
                                 from employee in empGroup.DefaultIfEmpty()
                                 join pack in db.Packages on si.ProductId equals pack.ProductId
                                 where si.SalesId.Equals(salesid)
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
                                DiscAmt = salesVM.DiscAmt + salesVM.DiscPercentageAmt + salesitem.Sum(item => item.LineDiscAmt),
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

                                subtotal = salesitem.Sum(item => item.LineTotal),
                                totalDisc = salesitem.Sum(item => item.LineDiscAmt),
                                totalQty = salesitem.Sum(item => item.Quantity),
                            };
                        }

                        //return Json(new { SalesId = invoiceVM.SalesId }, JsonRequestBehavior.AllowGet);
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
                            salesitemlist =
                                (from si in db.SalesItems
                                 join emp in db.Employees on si.EmpNo equals emp.EmpNo into empGroup
                                 from employee in empGroup.DefaultIfEmpty()
                                 join pro in db.Products on si.ProductId equals pro.ProductId
                                 where si.SalesId.Equals(salesid)
                                 select new SalesItemUpdate
                                 {
                                     SalesItemId = si.SalesItemId,
                                     SalesId = si.SalesId,
                                     EmpName = employee.FullName ?? "",
                                     ProductName = pro.ProductCode + " - " + pro.ProductName,
                                     Quantity = si.Quantity,
                                     UnitPrice = si.UnitPrice,
                                     LineTotal = si.LineTotal,
                                     LineDiscAmt = si.LineDiscAmt,
                                     Remarks = si.Remarks,
                                 }).ToList();

                            var salesitemlist2 =
                                (from si in db.SalesItems
                                 join emp in db.Employees on si.EmpNo equals emp.EmpNo into empGroup
                                 from employee in empGroup.DefaultIfEmpty()
                                 join pack in db.Packages on si.ProductId equals pack.ProductId
                                 where si.SalesId.Equals(salesid)
                                 select new SalesItemUpdate
                                 {
                                     SalesItemId = si.SalesItemId,
                                     SalesId = si.SalesId,
                                     EmpName = employee.FullName ?? "",
                                     ProductName = pack.Code + " - " + pack.Remarks,
                                     Quantity = si.Quantity,
                                     UnitPrice = si.UnitPrice,
                                     LineTotal = si.LineTotal,
                                     LineDiscAmt = si.LineDiscAmt,
                                     Remarks = si.Remarks,
                                 }).ToList();

                            salesitemlist = salesitemlist.Concat(salesitemlist2).ToList();
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
        #endregion

        #region Invoice By Credit   
        [HttpGet]
        public ActionResult InvoiceByCredit(int? id, string salesid, string flag)
        {
            try
            {
                if (!string.IsNullOrEmpty(Session["EmpNo"] as string))
                {
                    SalesInvoiceViewModels newsales = new SalesInvoiceViewModels();
                    List<SalesItemUpdate> salesitemlist = new List<SalesItemUpdate>();

                    var type = db.Types.Where(x => x.Active == false && x.Status == false);
                    ViewData["Category"] = type.Where(x => x.Module == "PaymentType");
                    ViewBag.Products = GetProductList().Where(x => x.TypeName == "Service");// && x.CreditBuy == true);
                    ViewBag.Employees = db.Employees.Where(e => e.Active.Equals(false) && e.Status.Equals(false) && !e.FullName.Contains("Admin") && !e.FullName.Contains("Service")).ToList();

                    ViewBag.Cusmtomer = db.Customers.Where(e => e.Active.Equals(false) && e.Status.Equals(false) && !e.FullName.Contains("Admin")).ToList();
                    var cus = (from e in db.Customers
                               where e.Active == false && e.Status == false && e.CusId == id
                               select new Customer
                               {
                                   CusId = e.CusId,
                                   CardNo = e.CardNo,
                                   FullName = e.FullName + " (TEL: " + e.ContactNo + ")",//+ " / IC: " + e.IcNo,
                               }).ToList();

                    ViewData["Customer"] = cus;
                    ViewData["Employee"] = ViewBag.Employees;
                    ViewData["Products"] = ViewBag.Products;

                    if (!salesid.IsNullOrWhiteSpace())
                    {
                        var salesVM = db.Saless.FirstOrDefault(x => x.SalesId == salesid);

                        var CusDetail = db.Customers.FirstOrDefault(x => x.CusId == salesVM.CusId);
                        ViewData["CustomerName"] = CusDetail.FullName + " (TEL: " + CusDetail.ContactNo + ")";
                        salesitemlist = GetSalesItemList(salesid); // salesitemlist.Concat(salesitemlist2).ToList();

                        newsales = new SalesInvoiceViewModels()
                        {
                            SalesId = salesVM.SalesId,
                            CusId = salesVM.CusId,
                            CusName = CusDetail.FullName,
                            PaymentMethod = salesVM.PaymentMethod,
                            OrderDate = salesVM.OrderDate,
                            PaymentDate = salesVM.PaymentDate,
                            TotalAmt = salesVM.TotalAmt,
                            PaidAmt = salesVM.PaidAmt,
                            DiscAmt = salesVM.DiscAmt + salesVM.DiscPercentageAmt,
                            BalAmt = salesVM.BalAmt,
                            Remarks = salesVM.Remarks,
                            Active = salesVM.Active,
                            AddBy = salesVM.AddBy,
                            ModBy = salesVM.ModBy,
                            AddDate = salesVM.AddDate,
                            ModDate = salesVM.ModDate,
                            SalesDetails = salesitemlist,

                            subtotal = salesitemlist.Sum(item => item.LineTotal),
                            totalDisc = salesitemlist.Sum(item => item.LineDiscAmt),
                            totalQty = salesitemlist.Sum(item => item.Quantity),
                        };

                        return View(newsales);
                    }
                    else
                    {
                        string currentYear = DateTime.Now.Year.ToString();
                        var counter = db.Counters.Where(x => x.count_name == "cuscredit").First();
                        salesid = salesid == null ? counter.format + currentYear.Substring(2, 2) + String.Format("{0:D5}", counter.count_no + 1) : salesid;
                        ViewBag.SalesId = salesid;

                        if (!id.ToString().IsNullOrWhiteSpace())
                        {
                            var CusDetail = db.Customers.FirstOrDefault(x => x.CusId == id);
                            ViewData["CustomerName"] = CusDetail.FullName + " (TEL: " + CusDetail.ContactNo + ")";
                            ViewData["CreaditBalance"] = CusDetail.CreditBal;
                            ViewBag.CreaditBalance = CusDetail.CreditBal;
                        }

                        var salesitem = db.SalesItems.Where(x => x.SalesId == salesid).ToList();
                        if (salesitem.Any())
                        {
                            salesitemlist = GetSalesItemList(salesid);
                        }

                        if (flag == "new" && salesitemlist.Count > 0)
                        {
                            foreach (var item in salesitemlist)
                            {
                                var toDlt = salesitem.FirstOrDefault(x => x.SalesItemId == item.SalesItemId);

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
                            CreditBal = ViewBag.CreaditBalance,
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
        #endregion

        #region CRUD Sales 
        [HttpPost]
        public ActionResult UpdateSales(UpdateSales salesitem)
        {
            DBContext db = new DBContext();
            var item = db.SalesItems.Find(salesitem.SalesItemId);

            if (item == null)
            {
                // not found
            }

            // Body
            item.Quantity = salesitem.Quantity;
            item.UnitPrice = salesitem.UnitPrice;
            item.LineDiscAmt = salesitem.LineDiscAmt;
            item.LineTotal = salesitem.LineTotal;
            item.ModBy = Session["EmpNo"].ToString() + "|" + Session["EmpName"].ToString();
            item.ModDate = DateTime.Now;

            db.SalesItems.Attach(item);
            db.Entry(item).State = EntityState.Modified;
            db.SaveChanges();

            var sales = db.Saless.Find(item.SalesId);
            var allitem = db.SalesItems.Where(x => x.SalesId == sales.SalesId).ToList();
            decimal subtotal = 0;

            foreach (var record in allitem)
            {
                subtotal += record.LineTotal;
            }

            // Header
            sales.TotalAmt = subtotal;
            sales.ModBy = Session["EmpNo"].ToString() + "|" + Session["EmpName"].ToString();
            sales.ModDate = DateTime.Now;
            db.Saless.Attach(sales);
            db.Entry(sales).State = EntityState.Modified;
            db.SaveChanges();

            return new EmptyResult();
        }

        [HttpPost]
        public ActionResult DeleteSales(int salesitemid)
        {
            SalesItemViewModels item = (from i in db.SalesItems
                                        where i.SalesItemId == salesitemid
                                        select i).FirstOrDefault();

            db.SalesItems.Remove(item);
            db.SaveChanges();

            return new EmptyResult();
        }
        #endregion

        #region SalesItem
        public ActionResult SalesItem(string id)
        {
            try
            {
                if (!string.IsNullOrEmpty(Session["EmpNo"] as string))
                {
                    ViewData["Product"] = db.Products.Where(x => x.Status == false && x.Active == false).ToList();
                    ViewData["Employee"] = db.Employees.Where(e => e.Active.Equals(false) && e.Status.Equals(false) && !e.FullName.Contains("Admin")).ToList();

                    if (!id.IsNullOrWhiteSpace())
                    {
                        var salesitem = db.SalesItems.Where(x => x.SalesId == id && x.Status.Equals(false) && x.Active.Equals(false)).ToList();
                        ViewBag.SalesId = id;
                        //ViewBag.SalesId = salesitem.Count > 0 ? salesitem.First().SalesId : "";
                    }

                    return PartialView();
                }
                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public ActionResult Read_SalesItem([DataSourceRequest] DataSourceRequest request, string id)
        {
            try
            {
                return Json(db.SalesItems.Where(x => x.SalesId == id && x.Status.Equals(false) && x.Active.Equals(false)).ToDataSourceResult(request, o => new SalesItemViewModels()
                {
                    SalesItemId = o.SalesItemId,
                    SalesId = o.SalesId,
                    EmpNo = o.EmpNo,
                    ProductId = o.ProductId,
                    Quantity = o.Quantity,
                    UnitPrice = o.UnitPrice,
                    LineTotal = o.LineTotal,
                    LineDiscAmt = o.LineDiscAmt,
                    Remarks = o.Remarks,
                    Active = o.Active,
                    Status = o.Status,
                    AddBy = o.AddBy,
                    ModBy = o.ModBy,
                    AddDate = o.AddDate,
                    ModDate = o.ModDate,
                }), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public virtual ActionResult Create_SalesItem([DataSourceRequest] DataSourceRequest request, [Bind(Exclude = "SalesItemId")] SalesItemViewModels salesitem, string id)
        {
            if (salesitem != null && ModelState.IsValid)
            {
                var unitprice = db.Products.FirstOrDefault(x => x.ProductId == salesitem.ProductId).Price;
                var item = new SalesItemViewModels
                {
                    //SalesItemId = salesitem.SalesItemId,
                    SalesId = id,
                    EmpNo = salesitem.EmpNo,
                    ProductId = salesitem.ProductId,
                    Quantity = salesitem.Quantity,
                    UnitPrice = unitprice,
                    LineDiscAmt = salesitem.LineDiscAmt,
                    LineTotal = unitprice * salesitem.Quantity - salesitem.LineDiscAmt,
                    Remarks = salesitem.Remarks,

                    AddBy = Session["EmpNo"].ToString() + "|" + Session["EmpName"].ToString(),
                    ModBy = Session["EmpNo"].ToString() + "|" + Session["EmpName"].ToString(),
                    AddDate = DateTime.Now,
                    ModDate = DateTime.Now
                };

                // Add the entity.
                db.SalesItems.Add(item);

                // Insert the entity in the database.
                db.SaveChanges();
            }

            return Json(new[] { salesitem }.ToDataSourceResult(request, ModelState), JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public virtual ActionResult Update_SalesItem([DataSourceRequest] DataSourceRequest request, SalesItemViewModels salesitem)
        {
            if (salesitem != null && ModelState.IsValid)
            {
                var item = new SalesItemViewModels
                {
                    SalesItemId = salesitem.SalesItemId,
                    SalesId = salesitem.SalesId,
                    EmpNo = salesitem.EmpNo,
                    ProductId = salesitem.ProductId,
                    Quantity = salesitem.Quantity,
                    UnitPrice = salesitem.UnitPrice,
                    LineDiscAmt = salesitem.LineDiscAmt,
                    LineTotal = salesitem.UnitPrice * salesitem.Quantity - salesitem.LineDiscAmt,
                    Remarks = salesitem.Remarks,
                    Active = salesitem.Active,
                    Status = salesitem.Status,
                    AddBy = Session["EmpNo"].ToString() + "|" + Session["EmpName"].ToString(),
                    ModBy = Session["EmpNo"].ToString() + "|" + Session["EmpName"].ToString(),
                    AddDate = salesitem.AddDate,
                    ModDate = DateTime.Now
                };

                // Attach the entity
                db.SalesItems.Attach(item);

                // Change its state to Modified so Entity Framework can update the existing product instead of creating a new one.
                db.Entry(item).State = EntityState.Modified;

                // Update the entity in the database.
                db.SaveChanges();
            }

            return Json(new[] { salesitem }.ToDataSourceResult(request, ModelState), JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public virtual ActionResult Destroy_SalesItem([DataSourceRequest] DataSourceRequest request, SalesItemViewModels salesitem)
        {
            if (salesitem != null && ModelState.IsValid)
            {
                using (var db = new DBContext())
                {
                    var item = new SalesItemViewModels
                    {
                        SalesItemId = salesitem.SalesItemId,
                        SalesId = salesitem.SalesId,
                        EmpNo = salesitem.EmpNo,
                        ProductId = salesitem.ProductId,
                        Quantity = salesitem.Quantity,
                        UnitPrice = salesitem.UnitPrice,
                        LineTotal = salesitem.LineTotal,
                        Remarks = salesitem.Remarks,
                        Active = true,
                        Status = salesitem.Status,
                        AddBy = Session["EmpNo"].ToString() + " | " + Session["EmpName"].ToString(),
                        ModBy = Session["EmpNo"].ToString() + " | " + Session["EmpName"].ToString(),
                        AddDate = salesitem.AddDate,
                        ModDate = DateTime.Now
                    };

                    // Attach the entity
                    db.SalesItems.Attach(item);

                    // Change its state to Modified so Entity Framework can update the existing product instead of creating a new one.
                    db.Entry(item).State = EntityState.Modified;

                    // Update the entity in the database.
                    db.SaveChanges();
                }
            }

            return Json(new[] { salesitem }.ToDataSourceResult(request, ModelState), JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Payment
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

        public ActionResult GIROPopUp(string salesid, int cusid, decimal total, decimal payamt, decimal minamt = 0m)
        {
            if (!string.IsNullOrEmpty(Session["EmpNo"] as string))
            {
                ViewData["minPayAmount"] = "";
                List<TypeViewModels> PaymentType = db.Types.Where(x => x.Module == "PaymentType" && x.TypeName != "Credit Balance" && x.Active == false).ToList();
                ViewData["PaymentType"] = PaymentType;
                ViewBag.PaymentType = PaymentType;

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
                        payment.TotalAmt = sales.TotalAmt;
                        payment.PaidAmt = sales.TotalAmt;
                        payment.DiscAmt = sales.DiscAmt;
                    }
                    else
                    {
                        payment.TotalAmt = total;
                        payment.PaidAmt = payamt;
                        payment.BalAmt = payamt - total;
                    }
                }
                else
                {
                    // error
                }

                // double ensure the 1st payment at least 50%
                decimal minPercentToPay = 0.5m;
                if (minamt == 0 && payment.PaidAmt < payment.TotalAmt * minPercentToPay)
                {
                    var pay = payment.BalAmt * minPercentToPay;
                    ViewBag.minPayAmount = "1st GIRO payment must be at least 50% (RM" + pay + ").";
                }

                return PartialView(payment);
            }

            return RedirectToAction("Login", "Account");
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Update_Payment(PaymentViewModels payment)
        {
            if (!string.IsNullOrEmpty(Session["EmpNo"] as string))
            {
                if (ModelState.IsValid && payment != null)
                {
                    //string currentYear = DateTime.Now.Year.ToString();
                    //var current_year = int.Parse(currentYear.Substring(2, 2));
                    //var paymentTypeFormat = db.Types.FirstOrDefault(x => x.TypeId == payment.PaymentMethod)?.InvFormat;
                    //var invFormat = db.Counters.FirstOrDefault(x => x.format == paymentTypeFormat && x.year == current_year);
                    //var newSalesId = paymentTypeFormat + current_year + String.Format("{0:D5}", invFormat.count_no + 1);
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
                        var typeid = type.FirstOrDefault(e => e.TypeName == "Service")?.TypeId;
                        var topupid = type.FirstOrDefault(e => e.TypeName == "TopUp")?.TypeId;
                        var productid = type.FirstOrDefault(e => e.TypeName == "Product")?.TypeId;
                        var packageid = type.FirstOrDefault(e => e.TypeName == "Package")?.TypeId;

                        var allsalesitem = db.SalesItems.Where(x => x.SalesId == payment.SalesId && x.Active == false && x.Status == false).ToList();
                        var serviceitem = allsalesitem.Where(x => x.TypeId == typeid && x.Active == false && x.Status == false).ToList();
                        var topupitem = allsalesitem.Where(x => x.TypeId == topupid && x.Active == false && x.Status == false).ToList();
                        var productitem = allsalesitem.Where(x => x.TypeId == productid && x.Active == false && x.Status == false).ToList();
                        var product = db.Products.Where(x => x.Active == false && x.Status == false);

                        // Get sales package in 
                        var packageitem = allsalesitem.Where(x => x.TypeId == packageid).ToList();
                        if (packageitem.Any())
                        {
                            // all active Package
                            var main = db.Packages.Where(x => x.Active == false && x.Status == false).ToList();

                            // Clone Package Sold
                            foreach (var pack in packageitem)
                            {
                                var main_p = main.FirstOrDefault(x => x.ProductId == pack.ProductId);
                                var p_item = db.PackageDetails.Where(x => x.PackageId == main_p.Id && x.Active == false && x.Status == false).ToList();

                                db.PackageSolds.Add(new PackageSoldViewModels
                                {
                                    CusId = payment.CusId,
                                    PackageId = main_p.Id,
                                    PackageCode = main_p.Code,
                                    PackageType = main_p.ProductType,
                                    TotalCost = main_p.TotalCost,
                                    SellingPrice = main_p.SellingPrice,
                                    PackageRemarks = main_p.Remarks,
                                    ExpiryPeriod = main_p.ExpiryPeriod,
                                    //ExpiryDate = main_p.ExpiryPeriod == 0 ? null : DateTime.Now.AddMonths(main_p.ExpiryPeriod).AddDays(-1),
                                    ExpiryDate = main_p.ExpiryPeriod.HasValue && main_p.ExpiryPeriod.Value > 0
                                    ? DateTime.Now.AddMonths(main_p.ExpiryPeriod.Value).AddDays(-1)
                                    : (DateTime?)null,

                                    Active = false,
                                    Status = false,
                                    AddBy = Session["EmpNo"].ToString() + " | " + Session["EmpName"].ToString(),
                                    ModBy = Session["EmpNo"].ToString() + " | " + Session["EmpName"].ToString(),
                                    AddDate = DateTime.Now,
                                    ModDate = DateTime.Now
                                });

                                // Clone Package Sold Details
                                foreach (var details in p_item)
                                {
                                    db.PackageSoldDetails.Add(new PackageSoldDetailsViewModels
                                    {
                                        PackageSoldId = details.PackageId,
                                        ItemId = details.ItemId,
                                        ItemType = details.ItemType,
                                        Qty = details.Qty,
                                        Cost = details.Cost,
                                        TotalCost = details.TotalCost,
                                        Remarks = details.Remarks,
                                        Active = false,
                                        Status = false,
                                        AddBy = Session["EmpNo"].ToString() + " | " + Session["EmpName"].ToString(),
                                        ModBy = Session["EmpNo"].ToString() + " | " + Session["EmpName"].ToString(),
                                        AddDate = DateTime.Now,
                                        ModDate = DateTime.Now
                                    });
                                }

                                var p_serviceitem = p_item.Where(x => x.ItemType == typeid).ToList();
                                var p_topupitem = p_item.Where(x => x.ItemType == topupid).ToList();
                                var p_product = p_item.Where(x => x.ItemType == productid).ToList();

                                // Clone to Service table
                                foreach (var svc in p_serviceitem)
                                {
                                    var p_svc = new SalesItemViewModels
                                    {
                                        SalesItemId = pack.SalesItemId,
                                        ProductId = svc.ItemId,
                                        Quantity = svc.Qty * pack.Quantity,
                                        ExpiryPeriod = main_p.ExpiryPeriod,
                                        //ExpiryDate = DateTime.Now.AddMonths(main_p.ExpiryPeriod).AddDays(-1),
                                        ExpiryDate = main_p.ExpiryPeriod.HasValue && main_p.ExpiryPeriod.Value > 0
                                        ? DateTime.Now.AddMonths(main_p.ExpiryPeriod.Value).AddDays(-1)
                                        : (DateTime?)null,
                                    };

                                    serviceitem.Add(p_svc);
                                }

                                // Update customer Credit
                                foreach (var topup in p_topupitem)
                                {
                                    var p_topup = new SalesItemViewModels
                                    {
                                        ProductId = topup.ItemId,
                                        Quantity = topup.Qty,
                                    };

                                    topupitem.Add(p_topup);
                                }

                                // Deduct Stock
                                foreach (var prod in p_product)
                                {
                                    var p_prod = new SalesItemViewModels
                                    {
                                        ProductId = prod.ItemId,
                                        Quantity = prod.Qty * pack.Quantity,
                                    };

                                    productitem.Add(p_prod);
                                }
                            }
                        }

                        // update salesId for diff Payment Code (INV/E/CS)
                        if (allsalesitem.Any())
                        {
                            foreach (var item in allsalesitem)
                            {
                                item.SalesId = newSalesId;

                                db.SalesItems.Attach(item);
                                db.Entry(item).State = EntityState.Modified;
                                //db.SaveChanges();
                            }
                        }

                        // Add treatment
                        decimal? svDue = 0;
                        if (serviceitem.Any())
                        {
                            // service
                            foreach (var item in serviceitem)
                            {
                                var pro = product.FirstOrDefault(x => x.ProductId == item.ProductId);

                                Service service = new Service
                                {
                                    SalesItemId = item.SalesItemId,
                                    SalesId = newSalesId,
                                    CusId = sale.CusId,
                                    ServiceName = pro.ProductCode + " - " + pro.ProductName,
                                    Course = item.Quantity,     //pro.Unit,
                                    CourseBal = item.Quantity,  //pro.Unit,
                                    Status = false,
                                    PurchaseDate = DateTime.Now,
                                    ExpiryPeriod = item.ExpiryPeriod,
                                    ExpiryDate = item.ExpiryDate,
                                    Remarks = (dueBalance < 0 && topupitem.Count > 0) ? payment.Remarks + "Full payment required for free treatment." : payment.Remarks,
                                    DueFlag = (dueBalance < 0),
                                    FreeService = (dueBalance < 0 && topupitem.Count > 0),
                                };

                                if (service.DueFlag && topupitem.Count == 0)
                                {
                                    svDue = dueBalance;
                                }

                                for (int i = 0; i < item.Quantity; i++)
                                {
                                    db.Services.Add(service);
                                    //db.SaveChanges();
                                }
                            }
                        }

                        // update customer credit / Outstanding amt
                        if (topupitem.Any() || svDue < 0)
                        {
                            var cus = db.Customers.FirstOrDefault(x => x.CusId == payment.CusId);

                            if (topupitem.Count > 0 && cus.TPDueAmt < 0)
                            {
                                return Json(new { success = true, message = "Please clear current outstanding Top up amount." });
                            }

                            if (topupitem.Count > 0)
                            {
                                cus.TPDueAmt += dueBalance ?? 0;
                                sale.Remarks = payment.IsGIRO ? "GIRO TopUp" : null;
                            }
                            else if (svDue < 0 && topupitem.Count == 0)
                            {
                                cus.SVDueAmt += svDue ?? 0;
                            }

                            //if (dueBalance < 0)
                            //{
                                foreach (var item in topupitem)
                                {
                                    var pro = product.FirstOrDefault(x => x.ProductId == item.ProductId);
                                    cus.CreditBal += cus.TPDueAmt < 0 && dueBalance < 0 ? payment.PaidAmt ?? 0 : (decimal)(pro.Credit > 0 ? pro.Credit * item.Quantity : 0);
                                }
                           // }

                            db.Customers.Attach(cus);
                            db.Entry(cus).State = EntityState.Modified;
                            //db.SaveChanges();
                        }

                        // deduct stock
                        if (productitem.Any())
                        {
                            // stock
                            foreach (var i in productitem)
                            {
                                var stock = db.Stock.FirstOrDefault(x => x.ProductId == i.ProductId);

                                if (stock == null || stock.QtyAvailable < i.Quantity)
                                {
                                    var prodName = db.Products.FirstOrDefault(x => x.ProductId == i.ProductId)?.ProductName ?? "Unknown";
                                    return Json(new { success = false, message = $"Insufficient stock for product: {prodName}." });
                                }

                                stock.QtyAvailable -= i.Quantity;
                                stock.ModDate = DateTime.Now;
                                stock.ModBy = Session["EmpNo"].ToString() + "|" + Session["EmpName"].ToString();

                                db.Stock.Attach(stock);
                                db.Entry(stock).State = EntityState.Modified;
                                //db.SaveChanges();
                            }
                        }
                    }

                    payment.SalesId = newSalesId;
                }

                db.SaveChanges();
                return Json(new { payment.SalesId });
            }

            return RedirectToAction("Login", "Account");
        }

        public ActionResult PaymentByCredit(int CusId, string SalesId, decimal TotalAmt)
        {
            var type = db.Types.Where(e => e.Active.Equals(false) && e.Status.Equals(false) && e.Module == "Product");
            var typeid = type.FirstOrDefault(e => e.TypeName == "Service").TypeId;
            var productid = type.FirstOrDefault(e => e.TypeName == "Product").TypeId;
            var packageid = type.FirstOrDefault(e => e.TypeName == "Package").TypeId;

            var allsalesitem = db.SalesItems.Where(x => x.SalesId == SalesId).ToList();
            var product = db.Products.Where(x => x.Active == false && x.Status == false);

            // deduct customer credit 
            var cus = db.Customers.FirstOrDefault(x => x.CusId == CusId);
            cus.CreditBal -= TotalAmt;

            var serviceitem = allsalesitem.Where(x => x.SalesId == SalesId && x.TypeId == typeid).ToList();
            var productitem = allsalesitem.Where(x => x.SalesId == SalesId && x.TypeId == productid).ToList();

            var packageitem = allsalesitem.Where(x => x.TypeId == packageid).ToList();
            if (packageitem.Any())
            {
                var main = db.Packages.Where(x => x.Active == false).ToList();

                // Clone Package Sold
                foreach (var pack in packageitem)
                {
                    var main_p = main.FirstOrDefault(x => x.ProductId == pack.ProductId);
                    var p_item = db.PackageDetails.Where(x => x.PackageId == main_p.Id).ToList();

                    db.PackageSolds.Add(new PackageSoldViewModels
                    {
                        CusId = CusId,
                        PackageId = main_p.Id,
                        PackageCode = main_p.Code,
                        PackageType = main_p.ProductType,
                        TotalCost = main_p.TotalCost,
                        SellingPrice = main_p.SellingPrice,
                        PackageRemarks = main_p.Remarks,
                        ExpiryPeriod = main_p.ExpiryPeriod,
                        //ExpiryDate = DateTime.Now.AddMonths(main_p.ExpiryPeriod).AddDays(-1),
                        ExpiryDate = main_p.ExpiryPeriod.HasValue && main_p.ExpiryPeriod.Value > 0
                                        ? DateTime.Now.AddMonths(main_p.ExpiryPeriod.Value).AddDays(-1)
                                        : (DateTime?)null,

                        Active = false,
                        Status = false,
                        AddBy = Session["EmpNo"].ToString() + " | " + Session["EmpName"].ToString(),
                        ModBy = Session["EmpNo"].ToString() + " | " + Session["EmpName"].ToString(),
                        AddDate = DateTime.Now,
                        ModDate = DateTime.Now
                    });

                    // Clone Package Sold Details
                    foreach (var details in p_item)
                    {
                        db.PackageSoldDetails.Add(new PackageSoldDetailsViewModels
                        {
                            PackageSoldId = details.PackageId,
                            ItemId = details.ItemId,
                            ItemType = details.ItemType,
                            Qty = details.Qty,
                            Cost = details.Cost,
                            TotalCost = details.TotalCost,
                            Remarks = details.Remarks,
                            Active = false,
                            Status = false,
                            AddBy = Session["EmpNo"].ToString() + " | " + Session["EmpName"].ToString(),
                            ModBy = Session["EmpNo"].ToString() + " | " + Session["EmpName"].ToString(),
                            AddDate = DateTime.Now,
                            ModDate = DateTime.Now
                        });
                    }

                    var p_serviceitem = p_item.Where(x => x.ItemType == typeid).ToList();
                    var p_product = p_item.Where(x => x.ItemType == productid).ToList();

                    // Clone to Service table
                    foreach (var svc in p_serviceitem)
                    {
                        var p_svc = new SalesItemViewModels
                        {
                            SalesItemId = pack.SalesItemId,
                            ProductId = svc.ItemId,
                            Quantity = svc.Qty,
                            ExpiryPeriod = main_p.ExpiryPeriod,
                            //ExpiryDate = DateTime.Now.AddMonths(main_p.ExpiryPeriod).AddDays(-1),
                            ExpiryDate = main_p.ExpiryPeriod.HasValue && main_p.ExpiryPeriod.Value > 0
                                        ? DateTime.Now.AddMonths(main_p.ExpiryPeriod.Value).AddDays(-1)
                                        : (DateTime?)null,
                        };

                        serviceitem.Add(p_svc);
                    }

                    // Deduct Stock
                    foreach (var prod in p_product)
                    {
                        var p_prod = new SalesItemViewModels
                        {
                            ProductId = prod.ItemId,
                            Quantity = prod.Qty,
                        };

                        productitem.Add(p_prod);
                    }
                }
            }

            // Add treatment
            if (serviceitem.Any())
            {
                foreach (var item in serviceitem)
                {
                    var pro = product.FirstOrDefault(x => x.ProductId == item.ProductId);

                    Service service = new Service
                    {
                        SalesItemId = item.SalesItemId,
                        SalesId = SalesId,
                        CusId = CusId,
                        ServiceName = pro.ProductCode + " - " + pro.ProductName,
                        Course = item.Quantity, //pro.Unit,
                        CourseBal = item.Quantity, //pro.Unit,
                        Status = false,
                        PurchaseDate = DateTime.Now,
                        ExpiryPeriod = item.ExpiryPeriod,
                        ExpiryDate = item.ExpiryDate,
                        //Remarks = payment.Remarks,
                    };

                    for (int i = 0; i < item.Quantity; i++)
                    {
                        db.Services.Add(service);
                        //db.SaveChanges();
                    }
                }
            }

            // deduct stock
            if (productitem.Any())
            {
                // stock
                foreach (var i in productitem)
                {
                    var stock = db.Stock.FirstOrDefault(x => x.ProductId == i.ProductId);
                    stock.QtyAvailable -= i.Quantity;
                    stock.ModDate = DateTime.Now;
                    stock.ModBy = Session["EmpNo"].ToString() + "|" + Session["EmpName"].ToString();

                    db.Stock.Attach(stock);
                    db.Entry(stock).State = EntityState.Modified;
                    //db.SaveChanges();
                }
            }


            // header
            var PaymentTypeId = db.Types.FirstOrDefault(x => x.Module == "PaymentType" && x.TypeName == "Credit Balance").TypeId;

            var sale = new SalesViewModels
            {
                CusId = CusId,
                SalesId = SalesId,
                PaymentMethod = PaymentTypeId,
                TotalAmt = TotalAmt,
                //DiscAmt = 0.00,
                //PaidAmt = payment.PaidAmt,
                //BalAmt = payment.TotalAmt - payment.DiscAmt - payment.PaidAmt,
                OrderDate = DateTime.Now,
                PaymentDate = DateTime.Now,
                //Remarks = payment.Remarks,

                Active = false,
                Status = false,
                AddBy = Session["EmpNo"].ToString() + "|" + Session["EmpName"].ToString(),
                ModBy = Session["EmpNo"].ToString() + "|" + Session["EmpName"].ToString(),
                AddDate = DateTime.Now,
                ModDate = DateTime.Now
            };

            db.Saless.Add(sale);
            db.SaveChanges();
            return new EmptyResult();
        }
        #endregion

        #region Service
        public ActionResult ServiceLeft(int? cusid)
        {
            try
            {
                if (!string.IsNullOrEmpty(Session["EmpNo"] as string))
                {
                    ViewBag.CusId = cusid;
                    ViewBag.Employees = db.Employees.Where(e => e.Active.Equals(false) && e.Status.Equals(false) && !e.FullName.Contains("Admin") && !e.FullName.Contains("Service")).ToList();
                    return PartialView();
                }

                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public ActionResult Read_Service([DataSourceRequest] DataSourceRequest request, int cusid)
        {
            try
            {
                var query = db.Services
                        .Where(x => x.CusId == cusid
                        && x.Status == false
                        && x.CourseBal > 0
                        && (x.ExpiryDate > DateTime.Now || x.ExpiryDate == null))
                        .OrderBy(x => x.ExpiryDate ?? DateTime.MaxValue); // ⭐ Sort NULL last

                return Json(query.ToDataSourceResult(request, o => new Service()
                {
                    Id = o.Id,
                    SalesItemId = o.SalesItemId,
                    SalesId = o.SalesId,
                    CusId = cusid,
                    ServiceName = o.ServiceName,
                    Course = o.Course,
                    CourseBal = o.CourseBal,
                    ExpiryDate = o.ExpiryDate,
                    ExpiryPeriod = o.ExpiryPeriod,
                    Remarks = o.Remarks,
                    Status = o.Status,
                    PurchaseDate = o.PurchaseDate,
                    DueFlag = o.DueFlag,
                    FreeService = o.FreeService,
                }), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public ActionResult Update_Service(int id, string picid, string picid2, string picid3, string picname, string pic2name, string pic3name, string remarks)
        {
            try
            {
                if (!string.IsNullOrEmpty(Session["EmpNo"] as string))
                {
                    var service = db.Services.Find(id);
                    service.CourseBal = service.CourseBal - 1;

                    db.Services.Attach(service);
                    db.Entry(service).State = EntityState.Modified;
                    db.SaveChanges();

                    List<string> picIdsList = new List<string> { picid, picid2, picid3 };
                    List<string> picNamesList = new List<string> { picname, pic2name, pic3name };
                    string picIdString = string.Join(",", picIdsList.Where(s => !string.IsNullOrEmpty(s)));
                    string picNameString = string.Join(",", picNamesList.Where(s => !string.IsNullOrEmpty(s)));
                    picNameString = picNameString.Replace(", -- Select PIC -- ", "");
                    picNameString = picNameString.Replace("-- Select PIC -- ,", "");

                    ServiceHistory addHistory = new ServiceHistory()
                    {
                        SalesItemId = service.SalesItemId,
                        SalesId = service.SalesId,
                        CusId = service.CusId,
                        PICId = picIdString,
                        PICName = picNameString,
                        ServiceName = service.ServiceName,
                        Remarks = remarks,
                        ServiceDate = DateTime.Now,
                        Status = false,
                    };

                    db.ServiceHistories.Add(addHistory);

                    //if(picid2 != "")
                    //{
                    //    ServiceHistory addHistory2 = new ServiceHistory()
                    //    {
                    //        SalesItemId = service.SalesItemId,
                    //        SalesId = service.SalesId,
                    //        CusId = service.CusId,
                    //        PICId = picid2,
                    //        PICName = pic2name,
                    //        ServiceName = service.ServiceName,
                    //        Remarks = remarks,
                    //        ServiceDate = DateTime.Now,
                    //        Status = false,
                    //    };

                    //    db.ServiceHistories.Add(addHistory2);
                    //}

                    //if (picid3 != "")
                    //{
                    //    ServiceHistory addHistory3 = new ServiceHistory()
                    //    {
                    //        SalesItemId = service.SalesItemId,
                    //        SalesId = service.SalesId,
                    //        CusId = service.CusId,
                    //        PICId = picid3,
                    //        PICName = pic3name,
                    //        ServiceName = service.ServiceName,
                    //        Remarks = remarks,
                    //        ServiceDate = DateTime.Now,
                    //        Status = false,
                    //    };

                    //    db.ServiceHistories.Add(addHistory3);
                    //}

                    db.SaveChanges();

                    var history = db.ServiceHistories.OrderByDescending(x => x.ServiceDate).FirstOrDefault();
                    return Json(new { success = true, data = history });
                    //return new EmptyResult(history);
                }

                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public ActionResult ServiceHistory(int? cusid)
        {
            try
            {
                if (!string.IsNullOrEmpty(Session["EmpNo"] as string))
                {
                    ViewBag.CusId = cusid;
                    ViewBag.Employees = db.Employees.Where(e => e.Active.Equals(false) && e.Status.Equals(false) && !e.FullName.Contains("Admin")).ToList();
                    return PartialView();
                }

                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public ActionResult Read_ServiceHistory([DataSourceRequest] DataSourceRequest request, int cusid)
        {
            try
            {
                return Json(db.ServiceHistories.Where(x => x.CusId == cusid && x.Status.Equals(false)).ToDataSourceResult(request, o => new ServiceHistory()
                {
                    Id = o.Id,
                    SalesItemId = o.SalesItemId,
                    SalesId = o.SalesId,
                    CusId = cusid,
                    PICName = o.PICName,
                    ServiceName = o.ServiceName,
                    Remarks = o.Remarks,
                    ServiceDate = o.ServiceDate,
                    Status = o.Status,
                }), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public ActionResult ServiceExpired(int? cusid)
        {
            try
            {
                if (!string.IsNullOrEmpty(Session["EmpNo"] as string))
                {
                    ViewBag.CusId = cusid;
                    ViewBag.Employees = db.Employees.Where(e => e.Active.Equals(false) && e.Status.Equals(false) && !e.FullName.Contains("Admin")).ToList();
                    return PartialView();
                }

                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }
        public ActionResult Read_ServiceExpired([DataSourceRequest] DataSourceRequest request, int cusid)
        {
            try
            {
                return Json(db.Services.Where(x => x.CusId == cusid && x.Status.Equals(false) && x.CourseBal > 0 && (x.ExpiryDate < DateTime.Now)).ToDataSourceResult(request, o => new Service()
                {
                    Id = o.Id,
                    SalesItemId = o.SalesItemId,
                    SalesId = o.SalesId,
                    CusId = cusid,
                    ServiceName = o.ServiceName,
                    Course = o.Course,
                    CourseBal = o.CourseBal,
                    ExpiryDate = o.ExpiryDate,
                    ExpiryPeriod = o.ExpiryPeriod,
                    Remarks = o.Remarks,
                    Status = o.Status,
                    PurchaseDate = o.PurchaseDate,
                }), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

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
        #endregion


        #region -------------- Customer Product Purchased  --------------
        public ActionResult ProductHistory(int? id)
        {
            try
            {
                if (!string.IsNullOrEmpty(Session["EmpNo"] as string))
                {
                    ViewData["Product"] = db.Products.Where(x => x.Status == false && x.Active == false);
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

        public virtual ActionResult Read_ProductHistory([DataSourceRequest] DataSourceRequest request, int cusId)
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
                    .Where(o => !o.Status && o.TypeId == prodId && db.Saless.Any(s => s.CusId == cusId && s.SalesId == o.SalesId))
                     .AsEnumerable()
                     .Select(o => new SalesItemViewModels
                     {
                         SalesItemId = o.SalesItemId,
                         SalesId = o.SalesId,
                         EmpNo = o.EmpNo,
                         ProductId = o.ProductId,
                         Quantity = o.Quantity,
                         QtyBalance = o.QtyBalance,
                         UnitPrice = o.UnitPrice,
                         LineTotal = o.LineTotal,
                         LineDiscAmt = o.LineDiscAmt,
                         Exchange = o.Exchange,
                         Remarks = o.Remarks,
                         Active = o.Active,
                         Status = o.Status,
                         AddBy = o.AddBy,
                         ModBy = o.ModBy,
                         AddDate = o.AddDate,
                         ModDate = o.ModDate
                     }).ToList();

                var query2 = db.PackageSoldDetails
                   .Where(o => !o.Status && o.ItemType == prodId && db.PackageSolds.Any(s => s.CusId == cusId && s.Id == o.PackageSoldId && !o.Status && !o.Active))
                   .AsEnumerable()
                   .Select(o => new SalesItemViewModels
                   {
                       SalesItemId = o.Id,
                       SalesId = "",
                       EmpNo = "",
                       ProductId = o.ItemId,
                       Quantity = o.Qty,
                       QtyBalance = o.Qty,
                       UnitPrice = o.Cost,
                       LineTotal = o.TotalCost,
                       //LineDiscAmt = o.LineDiscAmt,
                       //Exchange = o.Exchange,
                       Remarks = o.Remarks,
                       Active = o.Active,
                       Status = o.Status,
                       AddBy = o.AddBy,
                       ModBy = o.ModBy,
                       AddDate = o.AddDate,
                       ModDate = o.ModDate
                   }).ToList();

                var combinedList = query.Concat(query2).ToList();
                return Json(combinedList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
    }
}