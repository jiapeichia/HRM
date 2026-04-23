using DocumentFormat.OpenXml.Math;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Wordprocessing;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Meo.Web.DBContext;
using Meo.Web.ViewModels;
using Microsoft.Office.Interop.Excel;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web.Mvc;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Web.HRM.Controllers
{
    public class AdminController : AuthController
    {
        DBContext db = new DBContext();

        // Get Admin Settings button screen
        public ActionResult AdminSettings()
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

        #region ------ All Access ------ 
        public ActionResult Index(string pageid)
        {
            try
            {
                if (!string.IsNullOrEmpty(Session["EmpNo"] as string))
                {
                    Session["Edit"] = "";
                    Session["View"] = "";

                    return View(pageid);
                }
                return RedirectToAction("Login", "Account");
            }

            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }
        public ActionResult AllAccess()
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
        public ActionResult _SearchAccess(string displayName)
        {
            try
            {
                if (!string.IsNullOrEmpty(Session["EmpNo"] as string))
                {
                    ViewData["Employee"] = db.Employees.ToList();
                    ViewData["Role"] = db.Roles.ToList();

                    ViewBag.displayName = displayName;
                    return PartialView();
                }
                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
}
        public virtual ActionResult Read_UserRole(string displayName, [DataSourceRequest] DataSourceRequest request)
        {
            List<EmployeeDetailsViewModel> employee = new List<EmployeeDetailsViewModel>();

            employee = (from a in db.AspUsers 
                        join e in db.Employees on a.emp_no equals e.EmpNo
                        where (displayName == "" || displayName == null || e.DisplayName.Replace(" ", "").Contains(displayName) || e.FullName.Replace(" ", "").Contains(displayName))
                         && e.Status.Equals(false) && (e.ResignationDate.Equals(null) || e.ResignationDate > DateTime.Now)
                        select new EmployeeDetailsViewModel
                        {
                            Emp_id = e.Emp_id,
                            RoleId = e.RoleId,
                            ImagePath = e.ImagePath,
                            DisplayName = e.DisplayName,
                            EmpNo = e.EmpNo,
                            FullName = e.FullName,
                            HiredDate = e.HiredDate,
                            OEmail = e.OEmail,
                            Gender = e.Gender,
                            ConfirmationDate = e.ConfirmationDate,
                            Active = e.Active,
                            Status = e.Status,
                            AddBy = e.AddBy,
                            ModBy = e.ModBy,
                            AddDate = e.AddDate,
                            ModDate = e.ModDate,
                        }).ToList();

            return Json(employee.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Update_UserRole([DataSourceRequest] DataSourceRequest request, EmployeeDetailsViewModel emp)
        {
            try
            {
                if (emp != null && ModelState.IsValid)
                {
                    using (var db = new DBContext())
                    {
                        var employee = db.Employees.Find(emp.Emp_id);

                        // Update Employee entity and set its properties from the posted Employee Model.

                        employee.Emp_id = emp.Emp_id;
                        employee.RoleId = emp.RoleId;

                        employee.Status = emp.Status;
                        employee.ModBy = Session["EmpId"].ToString();
                        employee.ModDate = DateTime.Now;

                        db.Employees.Attach(employee);
                        db.Entry(employee).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                }
                return Json(new[] { emp }.ToDataSourceResult(request, ModelState), JsonRequestBehavior.AllowGet);
            }
            catch (DbEntityValidationException e)
            {
                foreach (var eve in e.EntityValidationErrors)
                {
                    Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                        eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    foreach (var ve in eve.ValidationErrors)
                    {
                        Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                            ve.PropertyName, ve.ErrorMessage);
                    }
                }
                throw;
            }
            //return Redirect(Request.UrlReferrer.ToString());
            //return Json(new[] { emp }.ToDataSourceResult(request, ModelState), JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region ------ Page Access ------ 
        public ActionResult PageAccess(string id)
        {
            try
            {
                if (!string.IsNullOrEmpty(Session["EmpNo"] as string))
                {
                    // To return user display name on grid table
                    //ViewData["Employee"] = db.Employees.ToList();
                    //ViewData["Page"] = db.Pages.Where(e => e.Status == false).ToList();

                    var userinfo = db.Employees.FirstOrDefault(x => x.EmpNo == id);
                    Session["EmpName_pageaccess"] = userinfo.DisplayName;
                    Session["Emp_id_pageaccess"] = userinfo.EmpNo;
                    ViewData["Roles"] = db.Roles.AsNoTracking().Where(x => x.Status == false).ToList();

                    return View(userinfo);
                }
                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        [HttpGet]
        public ActionResult Read_PageAccess(string id)
        {
            var result = (from p in db.Pages
                          join pa in db.PageAccesses on new { p.Id, EmpNo = id } equals new { Id = pa.PageId, EmpNo = pa.Emp_no } into pageAccessGroup
                          from pag in pageAccessGroup.DefaultIfEmpty()
                          where p.Status == false
                          select new
                          {
                              PageId = p.Id,
                              PageName = p.Name, 
                              Active = pag != null && pag.Active
                          }).OrderBy(x => x.PageName).ToList();

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]

        public ActionResult Update_PageAccess(PageAccessUpdateModel data)
        {
            var currentEmpNo = Session["Emp_id_pageaccess"]?.ToString();

            if(data.SelectedRoleId < 1)
            {
                return Json(new { error = true, message = "Role is mandatory." }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var empRole = db.Employees.FirstOrDefault(x => x.EmpNo == currentEmpNo && x.Status == false);
                empRole.RoleId = data.SelectedRoleId;

                // Attach the entity if it is not being tracked
                if (db.Entry(empRole).State == EntityState.Detached)
                {
                    db.Employees.Attach(empRole);
                }
            }

            if (data.PageAccess != null && currentEmpNo != null)
            {
                foreach (var access in data.PageAccess)
                {
                    // Find the existing entity in the context
                    var entity = db.PageAccesses.FirstOrDefault(p => p.PageId == access.PageId && p.Emp_no == currentEmpNo);

                    if (access.Active)
                    {
                        // Update the entity if it exists
                        if (entity != null && !entity.Active)
                        {
                            entity.Active = true;
                            entity.ModBy = Session["EmpNo"].ToString() + " | " + Session["EmpName"].ToString();
                            entity.ModDate = DateTime.Now;

                            // Attach the entity if it is not being tracked
                            if (db.Entry(entity).State == EntityState.Detached)
                            {
                                db.PageAccesses.Attach(entity);
                            }
                        }
                        // Create a new entry if it does not exist
                        else if (entity == null)
                        {
                            var newAccess = new PageAccessViewModels
                            {
                                Active = true,
                                Emp_no = currentEmpNo,
                                PageId = access.PageId,
                                Status = false,
                                AddBy = Session["EmpNo"].ToString() + " | " + Session["EmpName"].ToString(),
                                ModBy = Session["EmpNo"].ToString() + " | " + Session["EmpName"].ToString(),
                                AddDate = DateTime.Now,
                                ModDate = DateTime.Now
                            };

                            db.PageAccesses.Add(newAccess);
                        }
                    }
                    else
                    {
                        if (entity != null)
                        {
                            db.PageAccesses.Remove(entity);
                        }
                    }
                }

                db.SaveChanges();
            }

            return Json(new { success = true, message = "Page accesses updated successfully." });
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public virtual ActionResult Destroy_PageAccess([DataSourceRequest] DataSourceRequest request, PageAccessViewModels pageaccess)
        {
            if (pageaccess != null && ModelState.IsValid)
            {
                using (var db = new DBContext())
                {
                    var access = db.PageAccesses.Find(pageaccess.PageAccess_id);
                    //access.ReadRecord = "N";
                    //access.EditRecord = "N";
                    //access.DeleteRecord = "N";
                    access.Status = true;
                    access.ModBy = Session["EmpNo"].ToString() + "|" + Session["EmpName"].ToString();
                    access.AddDate = pageaccess.AddDate;
                    access.ModDate = DateTime.Now;


                    //var access = new PageAccessViewModels
                    //{
                    //    PageAccess_id = pageaccess.PageAccess_id,
                    //    Emp_id = pageaccess.Emp_id,
                    //    Id = pageaccess.Id,
                    //    ReadRecord = read,
                    //    EditRecord = edit,
                    //    DeleteRecord = delete,
                    //    Status = true,

                    //    AddBy = pageaccess.AddBy,  
                    //    ModBy = Session["EmpId"].ToString(),   
                    //    AddDate = pageaccess.AddDate,  
                    //    ModDate = DateTime.Now  
                    //};

                    // Attach the entity
                    db.PageAccesses.Attach(access);

                    // Change its state to Modified so Entity Framework can update the existing product instead of creating a new one.
                    db.Entry(access).State = EntityState.Modified;

                    // Update the entity in the database.
                    db.SaveChanges();
                }
            }
            return Json(new[] { pageaccess }.ToDataSourceResult(request, ModelState), JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region ------ Page Control ------
        public ActionResult AllPage()
        {
            try
            {
                if (!string.IsNullOrEmpty(Session["EmpNo"] as string))
                {
                    // To return user display name on grid table
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

        public virtual ActionResult Read_Page([DataSourceRequest] DataSourceRequest request)
        {
            return Json(db.Pages.Where(o => o.Status.Equals(false)).ToDataSourceResult(request, o => new PageViewModels()
            {
                Id = o.Id,
                Name = o.Name,

                Status = o.Status,
                AddBy = o.AddBy,
                ModBy = o.ModBy,
                AddDate = o.AddDate,
                ModDate = o.ModDate
            }), JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public virtual ActionResult Create_Page([DataSourceRequest] DataSourceRequest request, PageViewModels page)
        {
            if (page != null && ModelState.IsValid)
            {
                try
                {
                    var pages = new PageViewModels
                    {
                        Name = page.Name,
                        Status = false,
                        AddBy = Session["EmpNo"].ToString() + " | " + Session["EmpName"].ToString(),
                        ModBy = Session["EmpNo"].ToString() + " | " + Session["EmpName"].ToString(),
                        AddDate = DateTime.Now,
                        ModDate = DateTime.Now
                    };

                    // Add the entity.
                    db.Pages.Add(pages);

                    // Insert the entity in the database.
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.ToString());
                }
            }
            // Return the inserted title control. The grid needs the generated Id. Also return any validation errors.
            return Json(new[] { page }.ToDataSourceResult(request, ModelState), JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public virtual ActionResult Update_Page([DataSourceRequest] DataSourceRequest request, PageViewModels page)
        {
            if (page != null && ModelState.IsValid)
            {
                using (var db = new DBContext())
                {
                    // Create a new Department entity and set its properties from the posted Department Model.
                    var pages = new PageViewModels
                    {
                        Id = page.Id,
                        Name = page.Name,

                        Status = page.Status,
                        AddBy = page.AddBy,  
                        ModBy = Session["EmpNo"].ToString() + " | " + Session["EmpName"].ToString(),
                        AddDate = page.AddDate,  
                        ModDate = DateTime.Now  
                    };

                    // Attach the entity
                    db.Pages.Attach(pages);

                    // Change its state to Modified so Entity Framework can update the existing product instead of creating a new one.
                    db.Entry(pages).State = EntityState.Modified;

                    // Update the entity in the database.
                    db.SaveChanges();
                }
            }
            return Json(new[] { page }.ToDataSourceResult(request, ModelState), JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Destroy_Page([DataSourceRequest] DataSourceRequest request, PageViewModels page)
        {
            if (page != null && ModelState.IsValid)
            {
                using (var db = new DBContext())
                {
                    var pages = new PageViewModels
                    {
                        Id = page.Id,
                        Name = page.Name,

                        Status = false,
                        AddBy = page.AddBy,  
                        ModBy = Session["EmpNo"].ToString() + " | " + Session["EmpName"].ToString(),
                        AddDate = page.AddDate,  
                        ModDate = page.ModDate,
                    };

                    // Attach the entity
                    db.Pages.Attach(pages);

                    // Change its state to Modified so Entity Framework can update the existing product instead of creating a new one.
                    db.Entry(pages).State = EntityState.Modified;

                    // Update the entity in the database.
                    db.SaveChanges();
                }
            }

            return Json(new[] { page }.ToDataSourceResult(request, ModelState), JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region ------ Role ------
        public ActionResult AllRole()
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

        public virtual ActionResult Read_Role([DataSourceRequest] DataSourceRequest request)
        {
            return Json(db.Roles.Where(o => o.Status.Equals(false)).ToDataSourceResult(request, o => new RoleViewModels()
            {
                RoleId = o.RoleId,
                RoleName = o.RoleName,

                Status = o.Status,
                AddBy = o.AddBy,
                ModBy = o.ModBy,
                AddDate = o.AddDate,
                ModDate = o.ModDate
            }), JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public virtual ActionResult Create_Role([DataSourceRequest] DataSourceRequest request, RoleViewModels ro)
        {
            if (ro != null && ModelState.IsValid)
            {
                try
                {
                    var dt = db.Counters.Where(e => e.count_name.Equals("role_id")).FirstOrDefault();

                    if (dt != null)
                    {
                        var dbroles = new RoleViewModels
                        {
                            RoleId = dt.count_no + 1,
                            RoleName = ro.RoleName,

                            Status = false,
                            AddBy = Session["EmpNo"].ToString() + " | " + Session["EmpName"].ToString(),  
                            ModBy = Session["EmpNo"].ToString() + " | " + Session["EmpName"].ToString(),
                            AddDate = DateTime.Now,  
                            ModDate = DateTime.Now  
                    };

                    // Add the entity.
                    db.Roles.Add(dbroles);

                    // Insert the entity in the database.
                    db.SaveChanges();
                }
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.ToString());
                }
            }
            // Return the inserted title control. The grid needs the generated Id. Also return any validation errors.
            return Json(new[] { ro }.ToDataSourceResult(request, ModelState), JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public virtual ActionResult Update_Role([DataSourceRequest] DataSourceRequest request, RoleViewModels ro)
        {
            if (ro != null && ModelState.IsValid)
            {
                using (var db = new DBContext())
                {
                    // Create a new Department entity and set its properties from the posted Department Model.
                    var roles = new RoleViewModels
                    {
                        RoleId = ro.RoleId,
                        RoleName = ro.RoleName,

                        Status = ro.Status,
                        AddBy = ro.AddBy,  
                        ModBy = Session["EmpId"].ToString(),   
                        AddDate = ro.AddDate,  
                        ModDate = DateTime.Now  
                    };

                    // Attach the entity
                    db.Roles.Attach(roles);

                    // Change its state to Modified so Entity Framework can update the existing product instead of creating a new one.
                    db.Entry(roles).State = EntityState.Modified;

                    // Update the entity in the database.
                    db.SaveChanges();
                }
            }
            return Json(new[] { ro }.ToDataSourceResult(request, ModelState), JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Destroy_Role([DataSourceRequest] DataSourceRequest request, RoleViewModels ro)
        {
            if (ro != null && ModelState.IsValid)
            {
                using (var db = new DBContext())
                {
                    var roles = new RoleViewModels
                    {
                        RoleId = ro.RoleId,
                        RoleName = ro.RoleName,

                        Status = true,
                        AddBy = ro.AddBy,
                        ModBy = ro.ModBy,
                        AddDate = ro.AddDate,
                        ModDate = ro.ModDate,
                    };

                    // Attach the entity
                    db.Roles.Attach(roles);

                    // Change its state to Modified so Entity Framework can update the existing product instead of creating a new one.
                    db.Entry(roles).State = EntityState.Modified;

                    // Update the entity in the database.
                    db.SaveChanges();
                }
            }
            return Json(new[] { ro }.ToDataSourceResult(request, ModelState), JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region ------ Type master ------
        public ActionResult AllType()
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

        #region READ 
        public virtual ActionResult Read_Type([DataSourceRequest] DataSourceRequest request)
        {
            return Json(db.Types.Where(o => o.Status.Equals(false)).ToDataSourceResult(request, o => new TypeViewModels()
            {
                TypeId = o.TypeId,
                TypeName = o.TypeName,
                Module = o.Module,
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

        #region Create
        [AcceptVerbs(HttpVerbs.Post)]
        public virtual ActionResult Create_Type([DataSourceRequest] DataSourceRequest request, TypeViewModels type)
        {
            if (type != null && ModelState.IsValid)
            {
                try
                {
                    var dt = db.Counters.Where(e => e.count_name.Equals("typeid")).FirstOrDefault();

                    if (dt != null)
                    {
                        var item = new TypeViewModels
                        {
                            TypeId = dt.count_no + 1,
                            TypeName = type.TypeName,
                            Module = type.Module,
                            Remarks = type.Remarks,

                            AddBy = Session["EmpNo"].ToString() + "|" + Session["EmpName"].ToString(),
                            ModBy = Session["EmpNo"].ToString() + "|" + Session["EmpName"].ToString(),
                            AddDate = DateTime.Now,
                            ModDate = DateTime.Now
                        };

                        // Add the entity.
                        db.Types.Add(item);

                        // Insert the entity in the database.
                        db.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.ToString());
                }
            }
            return Json(new[] { type }.ToDataSourceResult(request, ModelState), JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Update
        [AcceptVerbs(HttpVerbs.Post)]
        public virtual ActionResult Update_Type([DataSourceRequest] DataSourceRequest request, TypeViewModels type)
        {
            if (type != null && ModelState.IsValid)
            {
                using (var db = new DBContext())
                {
                    var item = new TypeViewModels
                    {
                        TypeId = type.TypeId,
                        TypeName = type.TypeName,
                        Module = type.Module,
                        Remarks = type.Remarks,
                        AddBy = type.AddBy,
                        ModBy = Session["EmpNo"].ToString() + "|" + Session["EmpName"].ToString(),
                        AddDate = type.AddDate,
                        ModDate = DateTime.Now
                    };

                    // Attach the entity
                    db.Types.Attach(item);

                    // Change its state to Modified so Entity Framework can update the existing product instead of creating a new one.
                    db.Entry(item).State = EntityState.Modified;

                    // Update the entity in the database.
                    db.SaveChanges();
                }
            }
            return Json(new[] { type }.ToDataSourceResult(request, ModelState), JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region ------ Payment Type master ------
        public ActionResult AllPaymentType()
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

        #region READ 
        public virtual ActionResult Read_PaymentType([DataSourceRequest] DataSourceRequest request)
        {
            return Json(db.Types.Where(o => o.Status.Equals(false) && o.Active.Equals(false) && o.Module == "PaymentType").ToDataSourceResult(request, o => new TypeViewModels()
            {
                TypeId = o.TypeId,
                TypeName = o.TypeName,
                Module = o.Module,
                Remarks = o.Remarks,
                Active = o.Active,
                Status = o.Status,
                AddBy = o.AddBy,
                ModBy = o.ModBy,
                AddDate = o.AddDate,
                ModDate = o.ModDate,
                InvFormat = o.InvFormat,
            }), JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Create
        [AcceptVerbs(HttpVerbs.Post)]
        public virtual ActionResult Create_PaymentType([DataSourceRequest] DataSourceRequest request, TypeViewModels type)
        {
            if (type != null && ModelState.IsValid)
            {
                try
                {
                    var exist = db.Types.Any(e => e.TypeName.ToLower() == type.TypeName.ToLower() && e.Active == false);
                    if (exist)
                    {
                        ModelState.AddModelError("PaymentType", "Payment Type already exists.");
                    }
                    else
                    {
                        var dt = db.Counters.Where(e => e.count_name.Equals("typeid")).FirstOrDefault();

                        if (dt != null)
                        {
                            var item = new TypeViewModels
                            {
                                TypeId = dt.count_no + 1,
                                TypeName = type.TypeName,
                                Module = "PaymentType",
                                Remarks = type.Remarks,
                                InvFormat = type.InvFormat,

                                AddBy = Session["EmpNo"].ToString() + "|" + Session["EmpName"].ToString(),
                                ModBy = Session["EmpNo"].ToString() + "|" + Session["EmpName"].ToString(),
                                AddDate = DateTime.Now,
                                ModDate = DateTime.Now,
                            };

                            // Add the entity.
                            db.Types.Add(item);

                            // Insert the entity in the database.
                            db.SaveChanges();
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.ToString());
                }
            }
            return Json(new[] { type }.ToDataSourceResult(request, ModelState), JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Update
        [AcceptVerbs(HttpVerbs.Post)]
        public virtual ActionResult Update_PaymentType([DataSourceRequest] DataSourceRequest request, TypeViewModels type)
        {
            if (type != null && ModelState.IsValid)
            {
                using (var db = new DBContext())
                {
                    var paymenttype = db.Types.Find(type.TypeId);

                    // update counter table format
                    if (paymenttype.InvFormat != type.InvFormat)
                    {
                        var counter = db.Counters.FirstOrDefault(x => x.format == paymenttype.InvFormat);
                        counter.format = type.InvFormat;
                        db.Counters.Attach(counter);
                        db.Entry(counter).State = EntityState.Modified;
                    }

                    // update payment type
                    if (paymenttype != null)
                    {
                        paymenttype.TypeName = type.TypeName;
                        paymenttype.Remarks = type.Remarks;
                        paymenttype.InvFormat = type.InvFormat;
                        paymenttype.ModBy = Session["EmpNo"].ToString() + "|" + Session["EmpName"].ToString();
                        paymenttype.ModDate = DateTime.Now;
                    }

                    // Attach the entity
                    db.Types.Attach(paymenttype);

                    // Change its state to Modified so Entity Framework can update the existing product instead of creating a new one.
                    db.Entry(paymenttype).State = EntityState.Modified;

                    // Update the entity in the database.
                    db.SaveChanges();
                }
            }
            return Json(new[] { type }.ToDataSourceResult(request, ModelState), JsonRequestBehavior.AllowGet);
        }
        #endregion

        [AcceptVerbs(HttpVerbs.Post)]
        public virtual ActionResult Destroy_PaymentType([DataSourceRequest] DataSourceRequest request, TypeViewModels type)
        {
            if (type != null && ModelState.IsValid)
            {
                using (var db = new DBContext())
                {
                    var paymenttype = db.Types.Find(type.TypeId);
                    if (paymenttype != null)
                    {
                        paymenttype.Active = true;
                        paymenttype.ModBy = Session["EmpNo"].ToString() + "|" + Session["EmpName"].ToString();
                        paymenttype.ModDate = DateTime.Now;
                    }

                    // Attach the entity
                    db.Types.Attach(paymenttype);

                    // Change its state to Modified so Entity Framework can update the existing product instead of creating a new one.
                    db.Entry(paymenttype).State = EntityState.Modified;

                    // Update the entity in the database.
                    db.SaveChanges();
                }
            }
            return Json(new[] { type }.ToDataSourceResult(request, ModelState), JsonRequestBehavior.AllowGet);
        }
        #endregion

        //#region Delete
        //[AcceptVerbs(HttpVerbs.Post)]
        //public ActionResult Destroy_Type([DataSourceRequest] DataSourceRequest request, TypeViewModels type)
        //{
        //    if (type != null && ModelState.IsValid)
        //    {
        //        using (var db = new DBContext())
        //        {
        //            var categories = new TypeViewModels
        //            {
        //                GoalType_id = type.GoalType_id,
        //                Type_name = type.Type_name,

        //                Status = "C",
        //                Create_by = type.Create_by,
        //                Update_by = Session["Emp_id"].ToString(),
        //                Create_date = type.Create_date,
        //                Update_date = DateTime.Now
        //            };

        //            // Attach the entity
        //            db.Categories.Attach(categories);

        //            // Change its state to Modified so Entity Framework can update the existing product instead of creating a new one.
        //            db.Entry(type).State = EntityState.Modified;

        //            // Or use ObjectStateManager if using a previous version of Entity Framework.
        //            // northwind.ObjectStateManager.ChangeObjectState(entity, EntityState.Modified);

        //            // Update the entity in the database.
        //            db.SaveChanges();
        //        }
        //    }

        //    return Json(new[] { type }.ToDataSourceResult(request, ModelState), JsonRequestBehavior.AllowGet);
        //}
        //#endregion
        #endregion

        //#region Counter
        //public ActionResult AllCounter(/*string pageid*/)
        //{
        //    try
        //    {
        //        if (!string.IsNullOrEmpty(Session["EmpNo"] as string))
        //        {
        //            Session["Edit"] = "";
        //            Session["Read"] = "";
        //            Session["Delete"] = "";
        //            Session["PageLoad"] = "";

        //            ViewData["Employee"] = db.Employees.ToList();

        //            return View();
        //        }
        //        return RedirectToAction("Login", "Account");
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.ToString());
        //    }
        //}

        //public virtual ActionResult Read_Counter([DataSourceRequest] DataSourceRequest request)
        //{
        //    return Json(db.Counters.ToDataSourceResult(request, o => new CounterViewModels()
        //    {
        //        count_id = o.count_id,
        //        count_name = o.count_name,
        //        count_no = o.count_no,

        //        //Status = o.Status,
        //        //AddBy = o.AddBy,
        //        //ModBy = o.ModBy,
        //        //AddDate = o.AddDate,
        //        //ModDate = o.ModDate
        //    }), JsonRequestBehavior.AllowGet);
        //}

        //[AcceptVerbs(HttpVerbs.Post)]
        //public virtual ActionResult Create_Counter([DataSourceRequest] DataSourceRequest request, CounterViewModels c)
        //{
        //    if (c != null && ModelState.IsValid)
        //    {
        //        try
        //        {
        //            var dt = db.Counters.ToList();

        //            if (dt != null)
        //            {
        //                var count = new CounterViewModels
        //                {
        //                    count_id = dt.Count + 1,
        //                    count_name = c.count_name,
        //                    count_no = c.count_no,
        //                };

        //                // Add the entity.
        //                db.Counters.Add(count);

        //                // Insert the entity in the database.
        //                db.SaveChanges();
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            throw new Exception(ex.ToString());
        //        }
        //    }
        //    return Json(new[] { c }.ToDataSourceResult(request, ModelState), JsonRequestBehavior.AllowGet);

        //}

        //[AcceptVerbs(HttpVerbs.Post)]
        //public virtual ActionResult Update_Counter([DataSourceRequest] DataSourceRequest request, CounterViewModels count)
        //{
        //    if (count != null && ModelState.IsValid)
        //    {
        //        using (var db = new DBContext())
        //        {
        //            // Create a new Department entity and set its properties from the posted Department Model.
        //            var counter = new CounterViewModels
        //            {
        //                count_id = count.count_id,
        //                count_name = count.count_name,
        //                count_no = count.count_no,

        //                //Status = o.Status,
        //                //AddBy = o.AddBy,
        //                //ModBy = o.ModBy,
        //                //AddDate = o.AddDate,
        //                //ModDate = o.ModDate
        //            };

        //            // Attach the entity
        //            db.Counters.Attach(counter);

        //            // Change its state to Modified so Entity Framework can update the existing product instead of creating a new one.
        //            db.Entry(counter).State = EntityState.Modified;

        //            // Update the entity in the database.
        //            db.SaveChanges();
        //        }
        //    }
        //    return Json(new[] { count }.ToDataSourceResult(request, ModelState), JsonRequestBehavior.AllowGet);
        //}

        //[AcceptVerbs(HttpVerbs.Post)]
        //public ActionResult Destroy_Counter([DataSourceRequest] DataSourceRequest request, CounterViewModels count)
        //{
        //    if (count != null && ModelState.IsValid)
        //    {
        //        using (var db = new DBContext())
        //        {
        //            var counter = new CounterViewModels
        //            {
        //                count_id = count.count_id,
        //                count_name = count.count_name,
        //                count_no = count.count_no,

        //                //Status = o.Status,
        //                //AddBy = o.AddBy,
        //                //ModBy = o.ModBy,
        //                //AddDate = o.AddDate,
        //                //ModDate = o.ModDate
        //            };

        //            // Attach the entity
        //            db.Counters.Attach(counter);

        //            // Change its state to Modified so Entity Framework can update the existing product instead of creating a new one.
        //            db.Entry(counter).State = EntityState.Modified;

        //            // Update the entity in the database.
        //            db.SaveChanges();
        //        }
        //    }
        //    return Json(new[] { count }.ToDataSourceResult(request, ModelState), JsonRequestBehavior.AllowGet);
        //}
        //#endregion
    }
}