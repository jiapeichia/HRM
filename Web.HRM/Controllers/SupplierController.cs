using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Meo.Web.DBContext;
using Meo.Web.ViewModels;
using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace Web.HRM.Controllers
{
    public class SupplierController : Controller
    {
        DBContext db = new DBContext();

        // Get Suppliers button screen
        public ActionResult Supplier()
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

        #region Start -- Supplier master 
        public ActionResult Index()
        {
            try
            {
                if (!string.IsNullOrEmpty(Session["EmpNo"] as string))
                {
                    //ViewBag.typeList = db.Types.Where(e => e.Active.Equals(false) && e.Status.Equals(false) && e.Module == "Supplier").ToList();
                    //ViewData["Type"] = ViewBag.typeList;
                    return View();
                }
                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public ActionResult _SearchSupplier(string searchContent)
        {
            ViewBag.searchContent = searchContent;
            return PartialView();
        }
        public ActionResult GetSearchData(string searchContent, [DataSourceRequest] DataSourceRequest request)
        {
            List<SupplierViewModels> supplier = new List<SupplierViewModels>();
            if (!searchContent.IsNullOrWhiteSpace())
            {
                searchContent = searchContent.Trim();

                supplier = (from sup in db.Suppliers
                            where (sup.Name.Contains(searchContent) || sup.SupplierId.Contains(searchContent) ||
                            sup.ContactNo.Contains(searchContent) || sup.Name.Replace(" ", "").ToLower().Contains(searchContent))
                            && sup.Active.Equals(false) && sup.Status.Equals(false)
                            select new SupplierViewModels
                            {
                                SupplierId = sup.SupplierId,
                                Name = sup.Name,
                                ContactNo = sup.ContactNo,
                                ContactNo2 = sup.ContactNo2,
                                Address1 = sup.Address1,
                                Address2 = sup.Address2,
                                Active = sup.Active,
                                Status = sup.Status,
                                Remarks = sup.Remarks,
                                AddBy = sup.AddBy,
                                ModBy = sup.ModBy,
                                AddDate = sup.AddDate,
                                ModDate = sup.ModDate,
                            }).ToList();
            }
            else
            {
                supplier = db.Suppliers.Where(x => x.Active == false && x.Status == false).ToList();
                //supplier = (from sup in db.Suppliers
                //            where sup.Active.Equals(false) && sup.Status.Equals(false)
                //            select new SupplierViewModels
                //            {
                //                SupplierId = sup.SupplierId,
                //                Name = sup.Name,
                //                ContactNo = sup.ContactNo,
                //                ContactNo2 = sup.ContactNo2,
                //                Address1 = sup.Address1,
                //                Address2 = sup.Address2,
                //                Active = sup.Active,
                //                Status = sup.Status,
                //                Remarks = sup.Remarks,
                //                AddBy = sup.AddBy,
                //                ModBy = sup.ModBy,
                //                AddDate = sup.AddDate,
                //                ModDate = sup.ModDate,
                //            }).ToList();
            }

            return Json(supplier.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }


        public virtual ActionResult Read_Supplier([DataSourceRequest] DataSourceRequest request)
        {
            return Json(db.Suppliers.Where(o => o.Status.Equals(false) && o.Active.Equals(false)).ToDataSourceResult(request, o => new SupplierViewModels()
            {
                SupplierId = o.SupplierId,
                Name = o.Name,
                ContactNo = o.ContactNo,
                ContactNo2 = o.ContactNo2,
                Address1 = o.Address1,
                Address2 = o.Address2,
                Remarks = o.Remarks,
                Active = o.Active,
                Status = o.Status,
                AddBy = o.AddBy,
                ModBy = o.ModBy,
                AddDate = o.AddDate,
                ModDate = o.ModDate,
            }), JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public virtual ActionResult Create_Supplier([DataSourceRequest] DataSourceRequest request, SupplierViewModels supplier)
        {
            if (supplier != null && ModelState.IsValid)
            {
                try
                {
                    // Check for duplicate SupplierCode
                    bool isDuplicate = db.Suppliers.Any(p => p.SupplierId == supplier.SupplierId && p.Active.Equals(false));
                    if (isDuplicate)
                    {
                        ModelState.AddModelError("SupplierId", "Supplier Id already exists. Please use a different Id.");
                    }
                    else
                    {
                        var item = new SupplierViewModels
                        {
                            SupplierId = supplier.SupplierId,
                            Name = supplier.Name,
                            ContactNo = supplier.ContactNo,
                            ContactNo2 = supplier.ContactNo2,
                            Address1 = supplier.Address1,
                            Address2 = supplier.Address2,
                            Remarks = supplier.Remarks,
                            Active = false,
                            Status = false,
                            AddBy = Session["EmpNo"].ToString() + "|" + Session["EmpName"].ToString(),
                            ModBy = Session["EmpNo"].ToString() + "|" + Session["EmpName"].ToString(),
                            AddDate = DateTime.Now,
                            ModDate = DateTime.Now,
                        };

                        // Add the entity.
                        db.Suppliers.Add(item);

                        // Insert the entity in the database.
                        db.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.ToString());
                }
            }
            return Json(new[] { supplier }.ToDataSourceResult(request, ModelState), JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public virtual ActionResult Update_Supplier([DataSourceRequest] DataSourceRequest request, SupplierViewModels supplier)
        {
            if (supplier != null && ModelState.IsValid)
            {
                using (var db = new DBContext())
                {
                    var item = new SupplierViewModels
                    {
                        Id = supplier.Id,
                        SupplierId = supplier.SupplierId,
                        Name = supplier.Name,
                        ContactNo = supplier.ContactNo,
                        ContactNo2 = supplier.ContactNo2,
                        Address1 = supplier.Address1,
                        Address2 = supplier.Address2,
                        Remarks = supplier.Remarks,
                        Active = supplier.Active,
                        Status = supplier.Status,
                        AddBy = supplier.AddBy,
                        ModBy = Session["EmpNo"].ToString() + "|" + Session["EmpName"].ToString(),
                        AddDate = supplier.AddDate,
                        ModDate = DateTime.Now
                    };

                    // Attach the entity
                    db.Suppliers.Attach(item);

                    // Change its state to Modified so Entity Framework can update the existing Supplier instead of creating a new one.
                    db.Entry(item).State = EntityState.Modified;

                    // Update the entity in the database.
                    db.SaveChanges();
                }
            }
            return Json(new[] { supplier }.ToDataSourceResult(request, ModelState), JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Destroy_Supplier([DataSourceRequest] DataSourceRequest request, SupplierViewModels supplier)
        {
            if (supplier != null && ModelState.IsValid)
            {
                using (var db = new DBContext())
                {
                    var item = db.Suppliers.Find(supplier.Id);
                    item.Active = true;

                    // Attach the entity
                    db.Suppliers.Attach(item);

                    // Change its state to Modified so Entity Framework can update the existing Supplier instead of creating a new one.
                    db.Entry(item).State = EntityState.Modified;

                    // Update the entity in the database.
                    db.SaveChanges();
                }
            }

            return Json(new[] { supplier }.ToDataSourceResult(request, ModelState), JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}