using DocumentFormat.OpenXml.Bibliography;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Meo.Web.DBContext;
using Meo.Web.ViewModels;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Web.Services.Description;

namespace Web.HRM.Controllers
{
    public class SettingController : AuthController
    {
        DBContext db = new DBContext();

        // Get Settings button screen
        public ActionResult Setting()
        {
            try
            {
                if (!string.IsNullOrEmpty(Session["EmpNo"] as string))
                {
                    return PartialView();
                }

                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        #region Start -- Product master 
        public ActionResult ProductIndex()
        {
            try
            {
                if (!string.IsNullOrEmpty(Session["EmpNo"] as string))
                {
                    return View();

                }
                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }
        public ActionResult Product(string searchContent)
        {
            ViewBag.searchContent = searchContent;
            ViewBag.typeList = db.Types.Where(e => e.Active.Equals(false) && e.Status.Equals(false) && e.Module == "Product").ToList();
            ViewData["Type"] = ViewBag.typeList;
            return PartialView();
        }

        public virtual ActionResult Read_Product(string searchContent, [DataSourceRequest] DataSourceRequest request)
        {
            ViewBag.typeList = db.Types.Where(e => e.Active.Equals(false) && e.Status.Equals(false) && e.Module == "Product").ToList();
            var type = db.Types.FirstOrDefault(e => e.Active == false && e.Status == false && e.Module == "Product" && e.TypeName == "Product");
            var typeid = type.TypeId;

            var result =
                (from pro in db.Products
                 join s in db.Stock on pro.ProductId equals s.ProductId into stockGroup
                 from stock in stockGroup.DefaultIfEmpty()
                 where pro.Status == false && (stock == null || stock.Status == false) && pro.Active == false && pro.TypeId == typeid
                 select new
                 {
                     pro.ProductId,
                     pro.ProductCode,
                     pro.ProductName,
                     pro.TypeId,
                     pro.Cost,
                     pro.Price,
                     Unit = stock.QtyAvailable ?? 0,
                     pro.Remarks,
                     pro.Active,
                     pro.Status,
                     pro.AddBy,
                     pro.ModBy,
                     pro.AddDate,
                     pro.ModDate,
                     //pro.CreditBuy,
                 }).ToList()
                 .Select(p => new ProductViewModels
                 {
                     ProductId = p.ProductId,
                     ProductCode = p.ProductCode,
                     ProductName = p.ProductName,
                     TypeId = p.TypeId,
                     Cost = p.Cost,
                     Price = p.Price,
                     Unit = p.Unit,
                     Remarks = p.Remarks,
                     Active = p.Active,
                     Status = p.Status,
                     AddBy = p.AddBy,
                     ModBy = p.ModBy,
                     AddDate = p.AddDate,
                     ModDate = p.ModDate,
                     //CreditBuy = p.CreditBuy,
                 }).ToList();

            if(!string.IsNullOrEmpty(searchContent))
            {
                searchContent = searchContent.ToLower();
                result = result.Where(x => x.ProductCode.ToLower().Contains(searchContent) || x.ProductName.ToLower().Contains(searchContent)).ToList();
            }

            return Json(result.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public virtual ActionResult Create_Product([DataSourceRequest] DataSourceRequest request, ProductViewModels product)
        {
            if (product != null && ModelState.IsValid)
            {
                try
                {
                    // Check for duplicate ProductCode
                    bool isDuplicate = db.Products.Any(p => p.ProductCode == product.ProductCode && p.Active.Equals(false));
                    if (isDuplicate)
                    {
                        ModelState.AddModelError("ProductCode", "Code already exists. Please use a different Code.");
                    }
                    else
                    {
                        var typeid = db.Types.FirstOrDefault(e => e.Active.Equals(false) && e.Status.Equals(false) && e.Module == "Product" && e.TypeName == "Product").TypeId;

                        //ViewBag.typeList = db.Types.Where(e => e.Active.Equals(false) && e.Status.Equals(false) && e.Module == "Product").ToList();
                        var dt = db.Counters.Where(e => e.count_name.Equals("productid")).FirstOrDefault();

                        if (dt != null)
                        {
                            var item = new ProductViewModels
                            {
                                ProductId = dt.count_no + 1,
                                ProductCode = product.ProductCode,
                                ProductName = product.ProductName,
                                TypeId = typeid,
                                Cost = product.Cost,
                                Price = product.Price,
                                Unit = product.Unit,
                                Remarks = product.Remarks,
                                AddBy = Session["EmpNo"].ToString() + "|" + Session["EmpName"].ToString(),
                                ModBy = Session["EmpNo"].ToString() + "|" + Session["EmpName"].ToString(),
                                AddDate = DateTime.Now,
                                ModDate = DateTime.Now,
                                //CreditBuy = false,
                            };

                            db.Products.Add(item);
                            db.SaveChanges();
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.ToString());
                }
            }
            return Json(new[] { product }.ToDataSourceResult(request, ModelState), JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public virtual ActionResult Update_Product([DataSourceRequest] DataSourceRequest request, ProductViewModels product)
        {
            if (product != null && ModelState.IsValid)
            {
                using (var db = new DBContext())
                {
                    var item = new ProductViewModels
                    {
                        ProductId = product.ProductId,
                        ProductCode = product.ProductCode,
                        ProductName = product.ProductName,
                        TypeId = product.TypeId,
                        Cost = product.Cost,
                        Price = product.Price,
                        Unit = product.Unit,
                        Remarks = product.Remarks,
                        Active = product.Active,
                        Status = product.Status,
                        AddBy = product.AddBy,
                        ModBy = Session["EmpNo"].ToString() + "|" + Session["EmpName"].ToString(),
                        AddDate = product.AddDate,
                        ModDate = DateTime.Now,
                        //CreditBuy = product.CreditBuy,
                    };

                    db.Products.Attach(item);
                    db.Entry(item).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            return Json(new[] { product }.ToDataSourceResult(request, ModelState), JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Destroy_Product([DataSourceRequest] DataSourceRequest request, ProductViewModels product)
        {
            if (product != null && ModelState.IsValid)
            {
                using (var db = new DBContext())
                {
                    var item = db.Products.Find(product.ProductId);
                    item.Active = true;

                    db.Products.Attach(item);
                    db.Entry(item).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }

            return Json(new[] { product }.ToDataSourceResult(request, ModelState), JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Start -- Service master 
        public ActionResult ServiceIndex()
        {
            try
            {
                if (!string.IsNullOrEmpty(Session["EmpNo"] as string))
                {
                    return View();

                }
                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }
        public ActionResult Service(string searchContent)
        {
            ViewBag.searchContent = searchContent;
            return PartialView();
        }

        public virtual ActionResult Read_Service(string searchContent, [DataSourceRequest] DataSourceRequest request)
        {
            var typeid = db.Types.FirstOrDefault(e => e.Active.Equals(false) && e.Status.Equals(false) && e.Module == "Product" && e.TypeName == "Service").TypeId;

            var result = db.Products.Where(o => o.Status.Equals(false) && o.Active.Equals(false) && o.TypeId == typeid).ToList();
            //{
            //    ProductId = o.ProductId,
            //    ProductCode = o.ProductCode,
            //    ProductName = o.ProductName,
            //    TypeId = o.TypeId,
            //    Cost = o.Cost,
            //    Price = o.Price,
            //    Unit = o.Unit,
            //    Remarks = o.Remarks,
            //    Active = o.Active,
            //    Status = o.Status,
            //    AddBy = o.AddBy,
            //    ModBy = o.ModBy,
            //    AddDate = o.AddDate,
            //    ModDate = o.ModDate,
            //    CreditBuy = o.CreditBuy,
            //}), JsonRequestBehavior.AllowGet);

            if (!string.IsNullOrEmpty(searchContent))
            {
                searchContent = searchContent.ToLower();
                result = result.Where(x => x.ProductCode.ToLower().Contains(searchContent) || x.ProductName.ToLower().Contains(searchContent)).ToList();
            }

            return Json(result.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public virtual ActionResult Create_Service([DataSourceRequest] DataSourceRequest request, ServiceVM service)
        {
            if (service != null && ModelState.IsValid)
            {
                try
                {
                    // Check for duplicate ProductCode
                    bool isDuplicate = db.Products.Any(p => p.ProductCode == service.ProductCode && p.Active.Equals(false));
                    if (isDuplicate)
                    {
                        ModelState.AddModelError("ProductCode", "Code already exists. Please use a different Code.");
                    }
                    else
                    {
                        var typeid = db.Types.FirstOrDefault(e => e.Active.Equals(false) && e.Status.Equals(false) && e.Module == "Product" && e.TypeName == "Service").TypeId;
                        var dt = db.Counters.Where(e => e.count_name.Equals("productid")).FirstOrDefault();

                        if (dt != null)
                        {
                            var item = new ProductViewModels
                            {
                                ProductId = dt.count_no + 1,
                                ProductCode = service.ProductCode,
                                ProductName = service.ProductName,
                                TypeId = typeid,
                                Cost = service.Cost,
                                Price = service.Price,
                                Unit = service.Unit,
                                Remarks = service.Remarks,
                                //Active = service.Active,
                                //Status = service.Status,
                                AddBy = Session["EmpNo"].ToString() + "|" + Session["EmpName"].ToString(),
                                ModBy = Session["EmpNo"].ToString() + "|" + Session["EmpName"].ToString(),
                                AddDate = DateTime.Now,
                                ModDate = DateTime.Now,
                               // CreditBuy = false,
                            };

                            db.Products.Add(item);
                            db.SaveChanges();
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.ToString());
                }
            }
            return Json(new[] { service }.ToDataSourceResult(request, ModelState), JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public virtual ActionResult Update_Service([DataSourceRequest] DataSourceRequest request, ServiceVM service)
        {
            if (service != null && ModelState.IsValid)
            {
                using (var db = new DBContext())
                {
                    var item = new ProductViewModels
                    {
                        ProductId = service.ProductId,
                        ProductCode = service.ProductCode,
                        ProductName = service.ProductName,
                        TypeId = service.TypeId,
                        Cost = service.Cost,
                        Price = service.Price,
                        Unit = service.Unit,
                        Remarks = service.Remarks,
                        Active = service.Active,
                        Status = service.Status,
                        AddBy = service.AddBy,
                        ModBy = Session["EmpNo"].ToString() + "|" + Session["EmpName"].ToString(),
                        AddDate = service.AddDate,
                        ModDate = DateTime.Now,
                        //CreditBuy = service.CreditBuy,
                    };

                    db.Products.Attach(item);
                    db.Entry(item).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            return Json(new[] { service }.ToDataSourceResult(request, ModelState), JsonRequestBehavior.AllowGet);
        }

        #region Delete
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Destroy_Service([DataSourceRequest] DataSourceRequest request, ServiceVM service)
        {
            if (service != null && ModelState.IsValid)
            {
                using (var db = new DBContext())
                {
                    var item = db.Products.Find(service.ProductId);
                    item.Active = true;

                    db.Products.Attach(item);
                    db.Entry(item).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }

            return Json(new[] { service }.ToDataSourceResult(request, ModelState), JsonRequestBehavior.AllowGet);
        }
        #endregion
        #endregion

        #region Start -- TopUp master 
        public ActionResult TopupIndex()
        {
            try
            {
                if (!string.IsNullOrEmpty(Session["EmpNo"] as string))
                {
                    return View();

                }
                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public ActionResult TopUp(string searchContent)
        {
            ViewBag.searchContent = searchContent;
            return PartialView();
        }

        public virtual ActionResult Read_TopUp(string searchContent, [DataSourceRequest] DataSourceRequest request)
        {
            var typeid = db.Types.FirstOrDefault(e => e.Active.Equals(false) && e.Status.Equals(false) && e.Module == "Product" && e.TypeName == "TopUp").TypeId;

            var result = db.Products.Where(o => o.Status.Equals(false) && o.Active.Equals(false) && o.TypeId == typeid).ToList();
            //.ToDataSourceResult(request, o => new TopUp()
            //{
            //    ProductId = o.ProductId,
            //    ProductCode = o.ProductCode,
            //    ProductName = o.ProductName,
            //    TypeId = o.TypeId,
            //    Cost = o.Cost,
            //    Price = o.Price,
            //    Unit = o.Unit,
            //    Credit = o.Credit,
            //    Remarks = o.Remarks,
            //    Active = o.Active,
            //    Status = o.Status,
            //    AddBy = o.AddBy,
            //    ModBy = o.ModBy,
            //    AddDate = o.AddDate,
            //    ModDate = o.ModDate,
            //    CreditBuy = o.CreditBuy,
            //});


            if (!string.IsNullOrEmpty(searchContent))
            {
                searchContent = searchContent.ToLower();
                result = result.Where(x => x.ProductCode.ToLower().Contains(searchContent) || x.ProductName.ToLower().Contains(searchContent)).ToList();
            }

            return Json(result.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public virtual ActionResult Create_TopUp([DataSourceRequest] DataSourceRequest request, TopUp topup)
        {
            if (topup != null && ModelState.IsValid)
            {
                try
                {
                    // Check for duplicate ProductCode
                    bool isDuplicate = db.Products.Any(p => p.ProductCode == topup.ProductCode && p.Active.Equals(false));
                    if (isDuplicate)
                    {
                        ModelState.AddModelError("ProductCode", "Code already exists. Please use a different Code.");
                    }
                    else
                    {
                        var typeid = db.Types.FirstOrDefault(e => e.Active.Equals(false) && e.Status.Equals(false) && e.Module == "Product" && e.TypeName == "TopUp").TypeId;
                        var dt = db.Counters.Where(e => e.count_name.Equals("productid")).FirstOrDefault();

                        if (dt != null)
                        {
                            var item = new ProductViewModels
                            {
                                ProductId = dt.count_no + 1,
                                ProductCode = topup.ProductCode,
                                ProductName = topup.ProductName,
                                TypeId = typeid,
                                Cost = 0,
                                Price = topup.Price,
                                Credit = topup.Credit,
                                Unit = 1,
                                Remarks = topup.Remarks,
                                Active = false,
                                Status = false,
                                AddBy = Session["EmpNo"].ToString() + "|" + Session["EmpName"].ToString(),
                                ModBy = Session["EmpNo"].ToString() + "|" + Session["EmpName"].ToString(),
                                AddDate = DateTime.Now,
                                ModDate = DateTime.Now,
                              //  CreditBuy = false,
                            };

                            db.Products.Add(item);
                            db.SaveChanges();
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.ToString());
                }
            }
            return Json(new[] { topup }.ToDataSourceResult(request, ModelState), JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public virtual ActionResult Update_TopUp([DataSourceRequest] DataSourceRequest request, TopUp topup)
        {
            if (topup != null && ModelState.IsValid)
            {
                using (var db = new DBContext())
                {
                    var item = db.Products.Find(topup.ProductId);
                    item.ProductName = topup.ProductName;
                    item.Price = topup.Price;
                    item.Credit = topup.Credit;
                    item.Remarks = topup.Remarks;
                    item.ModBy = Session["EmpNo"].ToString() + "|" + Session["EmpName"].ToString();
                    item.ModDate = DateTime.Now;
                    //item.CreditBuy = topup.CreditBuy;

                    db.Products.Attach(item);
                    db.Entry(item).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            return Json(new[] { topup }.ToDataSourceResult(request, ModelState), JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Destroy_TopUp([DataSourceRequest] DataSourceRequest request, TopUp topup)
        {
            if (topup != null && ModelState.IsValid)
            {
                using (var db = new DBContext())
                {
                    var item = db.Products.Find(topup.ProductId);
                    item.Active = true;
                    item.ModBy = Session["EmpNo"].ToString() + "|" + Session["EmpName"].ToString();
                    item.ModDate = DateTime.Now;

                    db.Products.Attach(item);
                    db.Entry(item).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }

            return Json(new[] { topup }.ToDataSourceResult(request, ModelState), JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Punch Card Report
        public ActionResult PunchCardPopUp()
        {
            if (!string.IsNullOrEmpty(Session["EmpNo"] as string))
            {
                ViewData["Employee"] = db.Employees.Where(x => x.Active == false && x.Status == false && !x.FullName.Contains("Admin")).ToList();

                return PartialView(new PunchCardViewModels() { Date = DateTime.Now.Date, ClockIn = DateTime.Now });
            }

            return RedirectToAction("Login", "Account");
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Update_PunchCard(PunchCardViewModels punchcard)
        {
            if (!string.IsNullOrEmpty(Session["EmpNo"] as string))
            {
                var card = db.PunchCard.FirstOrDefault(x => x.EmpNo == punchcard.EmpNo && (!x.ClockIn.HasValue || !x.ClockOut.HasValue || !x.ClockIn2.HasValue || !x.ClockOut2.HasValue || !x.ClockIn3.HasValue || !x.ClockOut3.HasValue));

                if (card != null)
                {
                    if (!card.ClockOut.HasValue)
                        card.ClockOut = DateTime.Now;
                    else if (!card.ClockIn2.HasValue)
                        card.ClockIn2 = DateTime.Now;
                    else if (!card.ClockOut2.HasValue)
                        card.ClockOut2 = DateTime.Now;
                    else if (!card.ClockIn3.HasValue)
                        card.ClockIn3 = DateTime.Now;
                    else if (!card.ClockOut3.HasValue)
                        card.ClockOut3 = DateTime.Now;

                    db.PunchCard.Attach(card);

                    // Change its state to Modified so Entity Framework can update the existing product instead of creating a new one.
                    db.Entry(card).State = EntityState.Modified;
                }
                else
                {
                    //create
                    punchcard.ClockIn = DateTime.Now;

                    // Add the entity.
                    db.PunchCard.Add(punchcard);
                }

                // Insert the entity in the database.
                db.SaveChanges();

                // Return JavaScript to close the window
                string closeScript = "<script>window.close();</script>";
                return Content(closeScript, "text/html");
            }

            return RedirectToAction("Login", "Account");
        }

        // GET: Report
        public ActionResult PunchCardReport()
        {
            try
            {
                if (!string.IsNullOrEmpty(Session["EmpNo"] as string))
                {
                    List<EmployeeViewModels> EmpList = db.Employees.Where(e => e.Active.Equals(false) && e.Status.Equals(false) && !e.FullName.Contains("Admin")).ToList();
                    ViewData["Employee"] = EmpList;
                    ViewBag.EmpList = EmpList;

                    return View();
                }
                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }
        public ActionResult _SearchPunchCard(string searchContent, DateTime? dateContent)
        {
            ViewBag.searchContent = searchContent;
            ViewData["Employee"] = db.Employees.Where(e => e.Active.Equals(false) && e.Status.Equals(false)).ToList();
            ViewBag.dateContent = dateContent;

            // Retrieve the viewmodel for the view here, depending on your data structure.
            return PartialView();
        }
        public ActionResult GetSearchData(string searchContent, DateTime? dateContent, [DataSourceRequest] DataSourceRequest request)
        {
            List<PunchCardReportViewModels> report = new List<PunchCardReportViewModels>();
            List<PunchCardViewModels> allCard = new List<PunchCardViewModels>();
            //var defaultDate = dateContent == null ? DateTime.Now.Date : dateContent;

            if (searchContent.HasValue() && dateContent.HasValue)
            {
                allCard = db.PunchCard.Where(x => x.Date == dateContent && x.EmpNo == searchContent).ToList();
            }
            else if (searchContent.HasValue())
            {
                allCard = db.PunchCard.Where(x => x.EmpNo == searchContent).ToList();
            }
            else if (dateContent.HasValue)
            {
                allCard = db.PunchCard.Where(x => x.Date == dateContent).ToList();
            }
            else
            {
                allCard = db.PunchCard.ToList();
            }

            foreach (var item in allCard)
            {
                PunchCardReportViewModels list = new PunchCardReportViewModels
                {
                    EmpNo = item.EmpNo,
                    ClockIn = item.ClockIn,
                    ClockOut = item.ClockOut,
                    ClockIn2 = item.ClockIn2,
                    ClockOut2 = item.ClockOut2,
                    ClockIn3 = item.ClockIn3,
                    ClockOut3 = item.ClockOut3,
                };

                report.Add(list);
            }

            return Json(report.OrderByDescending(x => x.Date).ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Start -- BackUp Path 
        public ActionResult DBBackup()
        {
            try
            {
                if (!string.IsNullOrEmpty(Session["EmpNo"] as string))
                {
                    return View();
                }
                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public virtual ActionResult Read_BackupPath([DataSourceRequest] DataSourceRequest request)
        {
            return Json(db.DBBackup.Where(o => o.Status.Equals(false)).ToDataSourceResult(request, o => new DBBackupViewModels()
            {
                Id = o.Id,
                Path = o.Path,
                FileName = o.FileName,
                Status = o.Status,
            }), JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public virtual ActionResult Create_BackupPath([DataSourceRequest] DataSourceRequest request, DBBackupViewModels data)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var item = new DBBackupViewModels
                    {
                        Path = data.Path,
                        FileName = "BeYou",
                        Status = false,
                    };

                    // Add the entity.
                    db.DBBackup.Add(item);

                    // Insert the entity in the database.
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.ToString());
                }
            }
            return Json(new[] { data }.ToDataSourceResult(request, ModelState), JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public virtual ActionResult Update_BackupPath([DataSourceRequest] DataSourceRequest request, DBBackupViewModels data)
        {
            if (ModelState.IsValid)
            {
                using (var db = new DBContext())
                {
                    var item = db.DBBackup.Find(data.Id);
                    item.Path = data.Path;

                    // Attach the entity
                    db.DBBackup.Attach(item);

                    // Change its state to Modified so Entity Framework can update the existing product instead of creating a new one.
                    db.Entry(item).State = EntityState.Modified;

                    // Update the entity in the database.
                    db.SaveChanges();
                }
            }
            return Json(new[] { data }.ToDataSourceResult(request, ModelState), JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Destroy_BackupPath([DataSourceRequest] DataSourceRequest request, DBBackupViewModels data)
        {
            if (ModelState.IsValid)
            {
                using (var db = new DBContext())
                {
                    var item = db.DBBackup.Find(data.Id);
                    item.Status = true;

                    // Attach the entity
                    db.DBBackup.Attach(item);

                    // Change its state to Modified so Entity Framework can update the existing product instead of creating a new one.
                    db.Entry(item).State = EntityState.Modified;

                    // Update the entity in the database.
                    db.SaveChanges();
                }
            }

            return Json(new[] { data }.ToDataSourceResult(request, ModelState), JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}