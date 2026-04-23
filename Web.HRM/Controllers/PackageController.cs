using Antlr.Runtime;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Meo.Web.DBContext;
using Meo.Web.ViewModels;
using Microsoft.Ajax.Utilities;
using NPOI.POIFS.Properties;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO.Packaging;
using System.Linq;
using System.Web.Mvc;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

namespace Web.HRM.Controllers
{
    public class PackageController : AuthController
    {
        DBContext db = new DBContext();

        #region ------ Package ------
        public ActionResult AllPackageIndex()
        {
            try
            {
                if (!string.IsNullOrEmpty(Session["EmpNo"] as string))
                {
                    ViewData["Employee"] = db.Employees.ToList();
                    return View();

                }
                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public ActionResult AllPackage(string searchContent)
        {
            ViewBag.searchContent = searchContent;
            return PartialView();
        }

        public virtual ActionResult Read_Package(string searchContent, [DataSourceRequest] DataSourceRequest request)
        {
            var result = db.Packages.Where(o => o.Status.Equals(false)).ToList();
            //.ToDataSourceResult(request, o => new PackageViewModels()
            //{
            //    Id = o.Id,
            //    ProductId = o.ProductId,
            //    Code = o.Code,
            //    TotalCost = o.TotalCost,
            //    SellingPrice = o.SellingPrice,
            //    ProductType = o.ProductType,
            //    Description = o.Description,
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
                result = result.Where(x => x.Code.ToLower().Contains(searchContent)).ToList();
            }

            return Json(result.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public virtual ActionResult Create_Package([DataSourceRequest] DataSourceRequest request, PackageViewModels package)
        {
            var typeId = db.Types.FirstOrDefault(x => x.TypeName == "Package")?.TypeId;
            var dt = db.Counters.Where(e => e.count_name.Equals("productid")).FirstOrDefault();

            if (dt != null && package != null && ModelState.IsValid)
            {
                try
                {
                    var pack = new PackageViewModels
                    {
                        Code = package.Code,
                        ProductId = dt.count_no + 1,
                        TotalCost = package.TotalCost,
                        SellingPrice = package.SellingPrice,
                        Description = package.Description,
                        Remarks = package.Remarks,
                        ProductType = (int)typeId,
                        Active = false,
                        Status = false,
                        AddBy = Session["EmpNo"].ToString() + " | " + Session["EmpName"].ToString(),
                        ModBy = Session["EmpNo"].ToString() + " | " + Session["EmpName"].ToString(),
                        AddDate = DateTime.Now,
                        ModDate = DateTime.Now,
                        //CreditBuy = true,
                        FirstPayAmt = 0,
                        SpecialPay = false,
                        ExpiryPeriod = package.ExpiryPeriod ?? 0,
                    };

                    db.Packages.Add(pack);
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.ToString());
                }
            }

            return Json(new[] { package }.ToDataSourceResult(request, ModelState), JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Update_Package([DataSourceRequest] DataSourceRequest request, PackageViewModels package)
        {
            if (package != null && ModelState.IsValid)
            {
                using (var db = new DBContext())
                {
                    var pack = db.Packages.Find(package.Id);
                    if (pack != null)
                    {
                        pack.Code = package.Code;
                        pack.TotalCost = package.TotalCost;
                        pack.SellingPrice = package.SellingPrice;
                        pack.Description = package.Description;
                        pack.Remarks = package.Remarks;
                        pack.ModBy = Session["EmpNo"].ToString() + " | " + Session["EmpName"].ToString();
                        pack.ModDate = DateTime.Now;
                       // pack.CreditBuy = package.CreditBuy;
                        pack.ExpiryPeriod = package.ExpiryPeriod ?? 0;
                        pack.GIROBuy = package.GIROBuy;
                        pack.FirstPayAmt = package.FirstPayAmt;
                        pack.SpecialPay = package.SpecialPay;
                        db.Entry(pack).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    else
                    {
                        ModelState.AddModelError("", "Package not found.");
                    }
                }
            }
            else
            {
                ModelState.AddModelError("", "There was an error updating the package.");
                return RedirectToAction("UpdateDetails", new { packageId = package.Id });
            }

            return RedirectToAction("AllPackageIndex", "Package");
            //return Json(new[] { package }.ToDataSourceResult(request, ModelState), JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public virtual ActionResult Destroy_Package([DataSourceRequest] DataSourceRequest request, PackageViewModels package)
        {
            if (package != null && ModelState.IsValid)
            {
                using (var db = new DBContext())
                {
                    var pack = db.Packages.Find(package.Id);
                    if (pack != null)
                    {
                        pack.Active = true;
                        pack.ModBy = Session["EmpNo"].ToString() + " | " + Session["EmpName"].ToString();
                        pack.ModDate = DateTime.Now;

                        db.Packages.Attach(pack);
                        db.Entry(pack).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    else
                    {
                        // Handle case where the package does not exist
                        ModelState.AddModelError("", "Package not found.");
                    }
                }
            }

            // Return result with validation information
            return Json(new[] { package }.ToDataSourceResult(request, ModelState), JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region ------ Package Details ------
        public ActionResult UpdateDetails(int packageId)
        {
            try
            {
                if (!string.IsNullOrEmpty(Session["EmpNo"] as string))
                {
                    var package = db.Packages.Find(packageId);
                    ViewBag.PackageId = packageId;

                    return View(package);
                }
                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public ActionResult _PackageDetails(int packageId)
        {
            ViewBag.packageId = packageId;
            ViewBag.PackageId = packageId;
            ViewData["PackageId"] = packageId;
            //ViewData["Product"] = db.Products.Where(x => x.Active == false).ToList();
            ViewData["Product"] = GetProductList();
            ViewData["Type"] = db.Types.Where(x => x.Active == false && x.Module == "Product").ToList();

            return PartialView();
        }

        public ActionResult AllPackageDetails()
        {
            try
            {
                if (!string.IsNullOrEmpty(Session["EmpNo"] as string))
                {
                    ViewData["Employee"] = db.Employees.ToList();
                    return View();
                }
                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public virtual ActionResult Read_PackageDetails([DataSourceRequest] DataSourceRequest request, int packageId)
        {
            return Json(db.PackageDetails.Where(o => o.Status.Equals(false) && o.PackageId == packageId).ToDataSourceResult(request, o => new PackageDetailsViewModels()
            {
                Id = o.Id,
                PackageId = o.PackageId,
                ItemId = o.ItemId,
                ItemType = o.ItemType,
                Qty = o.Qty,
                Cost = o.Cost,
                TotalCost = o.TotalCost,
                Remarks = o.Remarks,
                Active = o.Active,
                Status = o.Status,
                AddBy = o.AddBy,
                ModBy = o.ModBy,
                AddDate = o.AddDate,
                ModDate = o.ModDate
            }), JsonRequestBehavior.AllowGet);
        }

        public ActionResult CreateDetails(int packageId)
        {
            PackageDetailsViewModels details = new PackageDetailsViewModels();
            details.PackageId = packageId;
            details.Qty = 1;
            //ViewBag.packageId = packageId;
            ViewData["Product"] = GetProductList();// db.Products.Where(x => x.Active == false).ToList();
            return View(details);
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
                           //    CreditBuy = p.CreditBuy,
                           }).ToList();

            return proList;
        }

        //[AcceptVerbs(HttpVerbs.Post)]
        //public virtual ActionResult Create_PackageDetails([DataSourceRequest] DataSourceRequest request, PackageDetailsViewModels details, int packageid)
        //{
        //    try
        //    {
        //        var product = db.Products.Find(details.ItemId);
        //        var pack = new PackageDetailsViewModels
        //        {
        //            PackageId = packageid,
        //            ItemId = details.ItemId,
        //            ItemType = product.TypeId,
        //            Qty = details.Qty,
        //            Cost = product.Cost,
        //            TotalCost = details.Qty * product.Cost,
        //            Remarks = details.Remarks,
        //            Active = false,
        //            Status = false,
        //            AddBy = Session["EmpNo"].ToString() + " | " + Session["EmpName"].ToString(),
        //            ModBy = Session["EmpNo"].ToString() + " | " + Session["EmpName"].ToString(),
        //            AddDate = DateTime.Now,
        //            ModDate = DateTime.Now
        //        };

        //        db.PackageDetails.Add(pack);
        //        db.SaveChanges();
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.ToString());
        //    }

        //    return RedirectToAction("UpdateDetails", "Package", new { packageId = packageid });
        //    //return Json(new[] { details }.ToDataSourceResult(request, ModelState), JsonRequestBehavior.AllowGet);
        //}

        //[HttpPost]
        //public ActionResult Create_PackageDetails(int packageId, PackageDetailsViewModels model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var product = db.Products.Find(model.ItemId);

        //        // Use the packageId from the model to create the new record.
        //        db.PackageDetails.Add(new PackageDetailsViewModels
        //        {
        //            PackageId = model.PackageId,
        //            ItemId = model.ItemId,
        //            ItemType = product.TypeId,
        //            Qty = model.Qty,
        //            Cost = product.Cost,
        //            TotalCost = model.Qty * product.Cost,
        //            Remarks = model.Remarks,
        //            Active = false,
        //            Status = false,
        //            AddBy = Session["EmpNo"].ToString() + " | " + Session["EmpName"].ToString(),
        //            ModBy = Session["EmpNo"].ToString() + " | " + Session["EmpName"].ToString(),
        //            AddDate = DateTime.Now,
        //            ModDate = DateTime.Now
        //        });

        //        db.SaveChanges();
        //        return Json(new { success = true });
        //    }

        //    return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors) });
        //}
        [AcceptVerbs(HttpVerbs.Post)]
        public virtual ActionResult Create_PackageDetails([DataSourceRequest] DataSourceRequest request, PackageDetailsViewModels details, int packageId, int PackageId)
        {
            if (packageId == 0)
            {
                throw new Exception("PackageId is missing from the request!");
            }

            try
            {
                var product = db.Products.Find(details.ItemId);
                var pack = new PackageDetailsViewModels
                {
                    PackageId = packageId,
                    ItemId = details.ItemId,
                    ItemType = product.TypeId,
                    Qty = details.Qty,
                    Cost = product.Cost,
                    TotalCost = details.Qty * product.Cost,
                    Remarks = details.Remarks,
                    Active = false,
                    Status = false,
                    AddBy = Session["EmpNo"].ToString() + " | " + Session["EmpName"].ToString(),
                    ModBy = Session["EmpNo"].ToString() + " | " + Session["EmpName"].ToString(),
                    AddDate = DateTime.Now,
                    ModDate = DateTime.Now
                };

                db.PackageDetails.Add(pack);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }

            //return RedirectToAction("UpdateDetails", "Package", new { packageId = details.PackageId });
            return Json(new[] { details }.ToDataSourceResult(request, ModelState), JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public virtual ActionResult Update_PackageDetails([DataSourceRequest] DataSourceRequest request, PackageDetailsViewModels details)
        {
            if (details != null && ModelState.IsValid)
            {
                using (var db = new DBContext())
                {
                    var pack = db.PackageDetails.Find(details.Id);
                    var itemDetails = db.Products.Find(details.ItemId);
                    var existItem = db.PackageDetails.Where(x => x.PackageId == details.PackageId && x.ItemId == details.ItemId && x.Id != details.Id).ToList();

                    if (existItem.Any())
                    {
                        ModelState.AddModelError("", "This item already exists in the package.");
                    }
                    else if (pack != null)
                    {
                        pack.PackageId = details.PackageId;
                        pack.ItemId = details.ItemId;
                        pack.ItemType = itemDetails.TypeId;
                        pack.Qty = details.Qty;
                        pack.Cost = itemDetails.Cost == 0.00m ? itemDetails.Price : itemDetails.Cost;
                        pack.TotalCost = pack.Cost * details.Qty;
                        pack.Remarks = details.Remarks;
                        pack.ModBy = Session["EmpNo"].ToString() + " | " + Session["EmpName"].ToString();
                        pack.ModDate = DateTime.Now;

                        // Sum details Cost
                        var mainPackage = db.PackageDetails.Where(x => x.PackageId == details.PackageId).ToList();
                        var totalCost = mainPackage.Sum(x => x.TotalCost);

                        // Update Package Cost
                        var main = db.Packages.Find(details.PackageId);
                        main.TotalCost = totalCost;

                        db.PackageDetails.Attach(pack);
                        db.Entry(pack).State = EntityState.Modified;

                        db.Packages.Attach(main);
                        db.Entry(main).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    else
                    {
                        ModelState.AddModelError("", "PackageDetails not found.");
                    }
                }
            }

            return Json(new[] { details }.ToDataSourceResult(request, ModelState), JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Destroy_PackageDetails([DataSourceRequest] DataSourceRequest request, PackageDetailsViewModels details)
        {
            if (details != null && ModelState.IsValid)
            {
                using (var db = new DBContext())
                {
                    var pack = db.PackageDetails.Find(details.Id);
                    if (pack != null)
                    {
                        pack.Active = true;
                        pack.Status = true;
                        pack.ModBy = Session["EmpNo"].ToString() + " | " + Session["EmpName"].ToString();
                        pack.ModDate = DateTime.Now;

                        db.SaveChanges();
                    }
                    else
                    {
                        ModelState.AddModelError("", "PackageDetails not found.");
                    }
                }
            }

            return Json(new[] { details }.ToDataSourceResult(request, ModelState), JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region ------ Package Sold ------
        public ActionResult AllPackageSold()
        {
            try
            {
                if (!string.IsNullOrEmpty(Session["EmpNo"] as string))
                {
                    ViewData["Employee"] = db.Employees.ToList();
                    var cus = (from e in db.Customers
                               where e.Active == false && e.Status == false
                               select new Customer
                               {
                                   CusId = e.CusId,
                                   CardNo = e.CardNo,
                                   FullName = e.CardNo + " : " + e.FullName,
                               }).ToList();

                    ViewData["Customer"] = cus;
                    //ViewData["Customer"] = db.Customers.Where(e => e.Status.Equals(false)).ToList();
                    
                    return View();
                }
                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public ActionResult _SearchPackageSold(string searchContent)
        {
            ViewBag.searchContent = searchContent;
            var cus = (from e in db.Customers
                       where e.Active == false && e.Status == false
                       select new Customer
                       {
                           CusId = e.CusId,
                           CardNo = e.CardNo,
                           FullName = e.CardNo + " : " + e.FullName,
                       }).ToList();

            ViewData["Customer"] = cus;
            return PartialView();
        }

        public virtual ActionResult Read_PackageSold([DataSourceRequest] DataSourceRequest request, string searchContent)
        {
            var query = db.PackageSolds.Where(o => o.Status.Equals(false)).ToList();

            if (!string.IsNullOrWhiteSpace(searchContent))
            {
                var cusQuery = db.Customers.Where(o => o.Status.Equals(false) && (o.FullName.Contains(searchContent) || o.CardNo.Contains(searchContent))).Select(o => o.CusId).ToList();
                query = query.Where(o => cusQuery.Contains(o.CusId)).ToList();
            }

            var result = query.ToDataSourceResult(request, o => new PackageSoldViewModels()
            {
                Id = o.Id,
                CusId = o.CusId,
                PackageId = o.PackageId,
                PackageCode = o.PackageCode,
                PackageType = o.PackageType,
                TotalCost = o.TotalCost,
                SellingPrice = o.SellingPrice,
                PackageRemarks = o.PackageRemarks,
                Active = o.Active,
                Status = o.Status,
                AddBy = o.AddBy,
                ModBy = o.ModBy,
                AddDate = o.AddDate,
                ModDate = o.ModDate,
            });

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        //[AcceptVerbs(HttpVerbs.Post)]
        //public virtual ActionResult Create_Package([DataSourceRequest] DataSourceRequest request, PackageViewModels package)
        //{
        //    if (package != null && ModelState.IsValid)
        //    {
        //        try
        //        {
        //            var pack = new PackageViewModels
        //            {
        //                Code = package.Code,
        //                TotalCost = package.TotalCost,
        //                Remarks = package.Remarks,
        //                Active = false,
        //                Status = false,
        //                AddBy = Session["EmpNo"].ToString() + " | " + Session["EmpName"].ToString(),
        //                ModBy = Session["EmpNo"].ToString() + " | " + Session["EmpName"].ToString(),
        //                AddDate = DateTime.Now,
        //                ModDate = DateTime.Now
        //            };

        //            // Add the entity.
        //            db.Packages.Add(pack);

        //            // Insert the entity in the database.
        //            db.SaveChanges();
        //        }
        //        catch (Exception ex)
        //        {
        //            throw new Exception(ex.ToString());
        //        }
        //    }
        //    // Return the inserted title control. The grid needs the generated Id. Also return any validation errors.
        //    return Json(new[] { package }.ToDataSourceResult(request, ModelState), JsonRequestBehavior.AllowGet);
        //}

        //[AcceptVerbs(HttpVerbs.Post)]
        //public virtual ActionResult Update_Package([DataSourceRequest] DataSourceRequest request, PackageViewModels package)
        //{
        //    if (package != null && ModelState.IsValid)
        //    {
        //        using (var db = new DBContext())
        //        {
        //            var pack = db.Packages.Find(package.Code);
        //            if (pack != null)
        //            {
        //                // Update fields
        //                pack.TotalCost = pack.TotalCost;
        //                pack.Remarks = pack.Remarks;
        //                pack.Active = pack.Active;
        //                pack.ModBy = Session["EmpNo"].ToString() + " | " + Session["EmpName"].ToString();
        //                pack.ModDate = DateTime.Now;

        //                // Save changes
        //                db.SaveChanges();
        //            }
        //            else
        //            {
        //                // Handle case where the package does not exist
        //                ModelState.AddModelError("", "Package not found.");
        //            }
        //        }
        //    }
        //    return Json(new[] { package }.ToDataSourceResult(request, ModelState), JsonRequestBehavior.AllowGet);
        //}

        //[AcceptVerbs(HttpVerbs.Post)]
        //public ActionResult Destroy_Package([DataSourceRequest] DataSourceRequest request, PackageViewModels package)
        //{
        //    if (package != null && ModelState.IsValid)
        //    {
        //        using (var db = new DBContext())
        //        {
        //            var pack = db.Packages.Find(package.Id);
        //            if (package != null)
        //            {
        //                package.Active = false;
        //                package.ModBy = Session["EmpNo"].ToString() + " | " + Session["EmpName"].ToString();
        //                package.ModDate = DateTime.Now;

        //                // Save changes
        //                db.SaveChanges();
        //            }
        //            else
        //            {
        //                // Handle case where the package does not exist
        //                ModelState.AddModelError("", "Package not found.");
        //            }
        //        }
        //    }

        //    // Return result with validation information
        //    return Json(new[] { package }.ToDataSourceResult(request, ModelState), JsonRequestBehavior.AllowGet);
        //}
        #endregion
    }
}