using Kendo.Mvc.Extensions;
using Meo.Web.DBContext;
using Meo.Web.ViewModels;
using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace Web.HRM.Controllers
{
    public class ExchangeController : Controller
    {
        DBContext db = new DBContext();

        #region Exchange
        public ActionResult Exchange(/*int cusId*/)
        {
            try
            {
                if (!string.IsNullOrEmpty(Session["EmpNo"] as string))
                {
                    // increase product quantity if any
                    ViewData["Customer"] = GetCustomerList();
                    ViewData["Products"] = GetExchangeList();
                    ViewData["ProductsIn"] = GetProductList();
                    //ViewData["SoldProducts"] = GetSoldProduct(cusId);
                    //ViewData["ServiceAvailable"] = GetServiceAvailable(cusId);

                    string currentYear = DateTime.Now.Year.ToString();
                    var counter = db.Counters.Where(x => x.count_name == "invoiceno").First();
                    ViewBag.SalesId = counter.format + currentYear.Substring(2, 2) + String.Format("{0:D5}", counter.count_no + 1);

                    var vm = new SalesInvoiceViewModels()
                    {
                        SalesId = ViewBag.SalesId,
                        OrderDate = DateTime.Now,
                    };

                    return View(vm);
                }

                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public IList<Customer> GetCustomerList()
        {
            var cus = (from e in db.Customers
                       where e.Active == false && e.Status == false
                       select new Customer
                       {
                           CusId = e.CusId,
                           CardNo = e.CardNo,
                           FullName = e.CardNo + " : " + e.FullName + " (TEL: " + e.ContactNo + ")",
                       }).ToList();

            return cus;
        }

        public IList<ProductGIRO> GetExchangeList()
        {
            var proList = (from p in db.Products
                           join t in db.Types on p.TypeId equals t.TypeId
                           join s in db.Stock on p.ProductId equals s.ProductId into proGroup
                           from stk in proGroup.DefaultIfEmpty()
                           where p.Active == false && p.Status == false && (stk == null || stk.Status == false) && (t.TypeName == "Product" || t.TypeName == "Service")
                           select new ProductGIRO
                           {
                               ProductId = p.ProductId,
                               ProductCode = p.ProductCode,
                               ProductName = p.ProductCode + " (" + p.ProductName + ") ",
                               TypeName = t.TypeName,
                               Cost = p.Cost,
                               Price = p.Price,
                               QtyAvailable = stk.QtyAvailable ?? 0,
                               // CreditBuy = p.CreditBuy,
                           }).ToList();

            // If package allowed
            //var packList = (from p in db.Packages
            //                join t in db.Types on p.ProductType equals t.TypeId
            //                where p.Active == false && p.Status == false
            //                select new Product
            //                {
            //                    ProductId = p.ProductId,
            //                    ProductCode = p.Code,
            //                    ProductName = p.Code + " (" + p.Remarks + ")",
            //                    TypeName = t.TypeName,
            //                    Cost = p.TotalCost,
            //                    Price = p.SellingPrice,
            //                    CreditBuy = p.CreditBuy,
            //                }).ToList();

            //IList<Product> combinedList = proList.Concat(packList).ToList();
            //return combinedList.OrderBy(p => p.ProductCode).ToList();
            return proList;
        }

        public IList<ProductGIRO> GetProductList()
        {
            var proList = (from p in db.Products
                           join t in db.Types on p.TypeId equals t.TypeId
                           join s in db.Stock on p.ProductId equals s.ProductId into proGroup
                           from stk in proGroup.DefaultIfEmpty()
                           where p.Active == false && p.Status == false && (stk == null || stk.Status == false) && t.TypeName == "Product"
                           select new ProductGIRO
                           {
                               ProductId = p.ProductId,
                               ProductCode = p.ProductCode,
                               ProductName = p.ProductCode + " (" + p.ProductName + ") ",
                               TypeName = t.TypeName,
                               Cost = p.Cost,
                               Price = p.Price,
                               QtyAvailable = stk.QtyAvailable ?? 0,
                               // CreditBuy = p.CreditBuy,
                           }).ToList();

            // If package allowed
            //var packList = (from p in db.Packages
            //                join t in db.Types on p.ProductType equals t.TypeId
            //                where p.Active == false && p.Status == false
            //                select new Product
            //                {
            //                    ProductId = p.ProductId,
            //                    ProductCode = p.Code,
            //                    ProductName = p.Code + " (" + p.Remarks + ")",
            //                    TypeName = t.TypeName,
            //                    Cost = p.TotalCost,
            //                    Price = p.SellingPrice,
            //                    CreditBuy = p.CreditBuy,
            //                }).ToList();

            //IList<Product> combinedList = proList.Concat(packList).ToList();
            //return combinedList.OrderBy(p => p.ProductCode).ToList();
            return proList;
        }

        // Product that purchase within a month
        //public JsonResult GetCustomerProduct(int customerId)
        //{
        //    try
        //    {
        //        //var oneMonthAgo = DateTime.Now.AddMonths(-1);
        //        var productList = new List<ProductGIRO>();
        //        var product = this.GetProductList();

        //        if (customerId > 0 && product.Count > 0)
        //        {
        //            // Query the data
        //            var salesItemsData =
        //                (from c in db.Saless
        //                 join i in db.SalesItems on c.SalesId equals i.SalesId
        //                 where c.Active == false && c.Status == false && i.Active == false
        //                 && i.Status == false && c.CusId == customerId && !i.Exchange
        //                 //&& c.PaymentDate >= oneMonthAgo
        //                 select new
        //                 {
        //                     ProductId = i.ProductId,
        //                     TypeId = i.TypeId,
        //                     Quantity = i.Quantity,
        //                     UnitPrice = i.UnitPrice,
        //                     LineTotal = i.LineTotal,
        //                     LineDiscAmt = i.LineDiscAmt
        //                 }).ToList();

        //            // Map the data to your view model
        //            var salesItems = salesItemsData.Select(item => new SalesItemViewModels
        //            {
        //                ProductId = item.ProductId,
        //                TypeId = item.TypeId,
        //                Quantity = item.Quantity,
        //                UnitPrice = item.UnitPrice,
        //                LineTotal = item.LineTotal,
        //                LineDiscAmt = item.LineDiscAmt
        //            }).ToList();

        //            var productQuantities = salesItemsData
        //                .GroupBy(si => si.ProductId)
        //                .ToDictionary(g => g.Key, g => g.Sum(si => si.Quantity));

        //            var salesitemIds = new HashSet<int>(salesItemsData.Select(p => p.ProductId));
        //            //var productIds = new HashSet<int>(product.Select(p => p.ProductId));
        //            productList = product.Where(si => salesitemIds.Contains(si.ProductId)).ToList();

        //            foreach (var prod in productList)
        //            {
        //                if (productQuantities.TryGetValue(prod.ProductId, out int quantity))
        //                {
        //                    prod.QtyAvailable = quantity;
        //                }
        //            }
        //        }

        //        return Json(productList, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.ToString());
        //    }
        //}

        public IList<Service> GetServiceAvailable(int cusId)
        {
            var salesItem = (from c in db.Services
                             where c.CourseBal > 0 == false && c.Status == false
                             select new Service
                             {
                                 Id = c.Id,
                                 SalesItemId = c.SalesItemId,
                                 SalesId = c.SalesId,
                                 CusId = cusId,
                                 ServiceName = c.ServiceName,
                                 Course = c.Course,
                                 CourseBal = c.CourseBal,
                                 Remarks = c.Remarks,
                                 Status = c.Status,
                                 PurchaseDate = c.PurchaseDate,
                             }).ToList();

            return salesItem;
        }

        public IList<ExchangableItem> GetExchangableItems(IList<SalesItemViewModels> itemSold, IList<Service> serviceAvai, IList<ProductGIRO> product)
        {
            var result = new List<ExchangableItem>();

            foreach (var i in itemSold)
            {
                var pro = product.FirstOrDefault(x => x.ProductId == i.ProductId);

                if (pro != null)
                {
                    result.Add(new ExchangableItem
                    {
                        Code = pro.ProductCode,
                        Name = pro.ProductCode + "(" + pro.ProductName + ")",
                    });
                }
            }

            foreach (var s in serviceAvai)
            {
                // Need to add service code in service table!!!
                //var pro = product.FirstOrDefault(x => x.ProductId == s.ProductId);

                //if (pro != null)
                //{
                //    result.Add(new ExchangableItem
                //    {
                //        Code = pro.ProductCode,
                //        Name = pro.ProductCode + "(" + pro.ProductName + ")",
                //    });
                //}
            }

            return result;
        }
        #endregion

        #region Exchange payment
        public ActionResult ExchangePaymentPopUp(string salesid, int cusid, decimal total)
        {
            if (!string.IsNullOrEmpty(Session["EmpNo"] as string))
            {
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
        #endregion

        [HttpPost]
        public ActionResult AddExchangeItem(ExchangeItemViewModel model)
        {
            if (ModelState.IsValid)
            {
                List<ExchangeIn> itemsToIn = model.itemsToIn;
                List<ExchangeOut> itemsToOut = model.itemsToOut;

                // exchange record which need to be removed 
                var salesIdExistInExchange = db.Exchanges.Where(x => x.SalesId == model.SalesId).ToList();
                if (salesIdExistInExchange.Count > 0)
                {
                    foreach (var itemToRemoved in salesIdExistInExchange)
                    {
                        var product = db.Products.FirstOrDefault(x => x.ProductCode == itemToRemoved.ProductCode);
                        if (product != null)
                        {
                            var stock = db.Stock.FirstOrDefault(x => x.ProductId == product.ProductId);
                            stock.QtyAvailable -= itemToRemoved.Quantity;

                            db.Stock.Attach(stock);
                            db.Entry(stock).State = EntityState.Modified;
                        }

                        db.Exchanges.Remove(itemToRemoved);
                        db.SaveChanges();
                    }
                }

                // sales item record which need to be removed 
                var salesIdExistInSalesItem = db.SalesItems.Where(x => x.SalesId == model.SalesId).ToList();
                if (salesIdExistInSalesItem.Count > 0)
                {
                    foreach (var itemToRemoved in salesIdExistInSalesItem)
                    {
                        var stock = db.Stock.FirstOrDefault(x => x.ProductId == itemToRemoved.ProductId);

                        if (stock != null)
                        {
                            stock.QtyAvailable += itemToRemoved.Quantity;

                            db.Stock.Attach(stock);
                            db.Entry(stock).State = EntityState.Modified;
                        }
                        else
                        {
                            var service = db.Services.FirstOrDefault(x => x.SalesId.Equals(itemToRemoved.SalesId));
                            service.Status = true;

                            db.Services.Attach(service);
                            db.Entry(service).State = EntityState.Modified;
                        }
                        

                        db.SalesItems.Remove(itemToRemoved);
                        db.SaveChanges();
                    }
                }

                // Process the incoming exchange items
                foreach (var itemIn in itemsToIn)
                {
                    // Save or update the exchange products (ItemsToIn)
                    SaveOrUpdateExchangeInProduct(itemIn, model.SalesId, model.CustomerId);
                }

                // Process the sales items
                foreach (var itemOut in itemsToOut)
                {
                    // Save or update the sales products (ItemsToOut)
                    SaveOrUpdateExchangeOutProduct(itemOut, model.SalesId, model.CustomerId);
                }

                // Return a success response with necessary data
                return Json(new { success = true, salesId = model.SalesId, cusId = model.CustomerId, total = model.Total });
            }

            // If there's an issue with the form data, return an error response
            return Json(new { success = false });
        }

        private void SaveOrUpdateExchangeInProduct(ExchangeIn exchangeIn, string salesId, int cusId)
        {
            // Insert or update all exchange product
            if (exchangeIn != null)
            {
                // if it's an product - Increase stock quantity
                var exchangeItem = GetProductList().FirstOrDefault(x => x.ProductCode == exchangeIn.InCode && x.TypeName == "Product");
                if (exchangeItem != null)
                {
                    //var type = db.Types.FirstOrDefault(x => x.TypeId == product.TypeId);
                    var stock = db.Stock.FirstOrDefault(x => x.ProductId == exchangeItem.ProductId);
                    stock.QtyAvailable += exchangeIn.InQty;

                    stock.ModBy = Session["EmpNo"].ToString() + "|" + Session["EmpName"].ToString();
                    stock.ModDate = DateTime.Now;

                    db.Stock.Attach(stock);
                    db.Entry(stock).State = EntityState.Modified;
                    //db.SaveChanges();
                }
                else
                {
                    //is it a service?
                    exchangeItem = GetProductList().FirstOrDefault(x => x.ProductCode == exchangeIn.InCode);
                }

                // Insert into exchange table 
                var item = new Exchange
                {
                    SalesId = salesId,
                    ProductId = exchangeItem.ProductId,
                    ProductCode = exchangeItem.ProductCode,
                    ProductName = exchangeItem.ProductName,
                    ProductType = exchangeItem.TypeName,
                    Quantity = exchangeIn.InQty,
                    UnitPrice = exchangeItem.Price,
                    LineTotal = exchangeItem.Price * exchangeIn.InQty,

                    Active = false,
                    Status = false,
                    AddBy = Session["EmpNo"].ToString() + "|" + Session["EmpName"].ToString(),
                    ModBy = Session["EmpNo"].ToString() + "|" + Session["EmpName"].ToString(),
                    AddDate = DateTime.Now,
                    ModDate = DateTime.Now
                };

                db.Exchanges.Add(item);
                db.SaveChanges();
            }
        }

        private void SaveOrUpdateExchangeOutProduct(ExchangeOut exchangeOut, string salesId, int cusId)
        {
            bool isService = false;
            if (exchangeOut != null)
            {
                var exchangeItem = GetProductList().FirstOrDefault(x => x.ProductCode == exchangeOut.OutCode && x.TypeName == "Product");
                var getTypeId = db.Products.FirstOrDefault(x => x.ProductCode == exchangeOut.OutCode);

                // Deduct stock & insert into exchange table
                if (exchangeItem != null)
                {
                    var stock = db.Stock.FirstOrDefault(x => x.ProductId == exchangeItem.ProductId);
                    stock.QtyAvailable -= exchangeOut.OutQty;
                    stock.ModBy = Session["EmpNo"].ToString() + "|" + Session["EmpName"].ToString();
                    stock.ModDate = DateTime.Now;

                    db.Stock.Attach(stock);
                    db.Entry(stock).State = EntityState.Modified;
                    //db.SaveChanges();
                }
                else
                {
                    // if Service, Add to treatment table
                    var list = GetExchangeList();
                    exchangeItem = list.FirstOrDefault(x => x.ProductCode == exchangeOut.OutCode);
                    isService = true;
                }

                // Insert into sales item
                var item = new SalesItemViewModels
                {
                    SalesId = salesId,
                    ProductId = exchangeItem.ProductId,
                    Quantity = exchangeOut.OutQty,
                    UnitPrice = exchangeItem.Price,
                    LineTotal = exchangeItem.Price * exchangeOut.OutQty,
                    EmpNo = null,   // person who sales
                    TypeId = getTypeId.TypeId,
                    Exchange = true,

                    AddBy = Session["EmpNo"].ToString() + "|" + Session["EmpName"].ToString(),
                    ModBy = Session["EmpNo"].ToString() + "|" + Session["EmpName"].ToString(),
                    AddDate = DateTime.Now,
                    ModDate = DateTime.Now
                };

                db.SalesItems.Add(item);
                db.SaveChanges();

                if (isService)
                {
                    Service service = new Service
                    {
                        SalesItemId = item.SalesItemId,
                        SalesId = salesId,
                        CusId = cusId,
                        ServiceName = exchangeItem.ProductCode + " - " + exchangeItem.ProductName,
                        Course = exchangeOut.OutQty,
                        CourseBal = exchangeOut.OutQty,
                        Status = false,
                        PurchaseDate = DateTime.Now,
                        Remarks = "Exchange",
                        DueFlag = false,
                        FreeService = false,
                    };

                    db.Services.Add(service);
                    db.SaveChanges();
                }
            }
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
                    string currentYear = DateTime.Now.Year.ToString();
                    var counter = db.Counters.Where(x => x.count_name == "invoiceno").First();
                    string newSalesId = counter.format + currentYear.Substring(2, 2) + String.Format("{0:D5}", counter.count_no + 1);

                    var sales = db.Saless.FirstOrDefault(x => x.SalesId == payment.SalesId && x.Status == false && x.Active == false);
                    if (sales == null)
                    {
                        // header
                        var sale = new SalesViewModels
                        {
                            CusId = payment.CusId,
                            SalesId = newSalesId,
                            PaymentMethod = payment.PaymentMethod,
                            TotalAmt = payment.TotalAmt,
                            DiscAmt = payment.DiscAmt,
                            PaidAmt = payment.PaidAmt,
                            BalAmt = payment.TotalAmt - payment.DiscAmt - payment.PaidAmt,
                            OrderDate = DateTime.Now,
                            PaymentDate = DateTime.Now,
                            Remarks = payment.Remarks,
                            Exchange = true,
                            Active = false,
                            Status = false,
                            AddBy = Session["EmpNo"].ToString() + "|" + Session["EmpName"].ToString(),
                            ModBy = Session["EmpNo"].ToString() + "|" + Session["EmpName"].ToString(),
                            AddDate = DateTime.Now,
                            ModDate = DateTime.Now
                        };

                        db.Saless.Add(sale);

                        // update salesId for diff Payment Code (INV/E/CS)
                        var allsalesitem = db.SalesItems.Where(x => x.SalesId == payment.SalesId && x.Active == false && x.Status == false).ToList();
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

                        // Exchange - update salesId for diff Payment Code (INV/E/CS)
                        var allexchange = db.Exchanges.Where(x => x.SalesId == payment.SalesId && x.Active == false && x.Status == false).ToList();
                        if (allexchange.Any())
                        {
                            foreach (var item in allexchange)
                            {
                                item.SalesId = newSalesId;

                                db.Exchanges.Attach(item);
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

            return RedirectToAction("Login", "Account");
        }
    }
}