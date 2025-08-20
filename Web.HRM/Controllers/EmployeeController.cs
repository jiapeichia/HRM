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

namespace Web.HRM.Controllers
{
    public class EmployeeController : Controller
    {
        DBContext db = new DBContext();

        // GET: Employee
        public ActionResult Index()
        {
            try
            {
                if (!string.IsNullOrEmpty(Session["EmpNo"] as string))
                {
                    ViewData["Employee"] = db.Employees.Where(e => e.Active.Equals(false) && e.Status.Equals(false)).ToList();


                    return View();

                }
                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }
        public ActionResult _SearchEmployee(string searchContent)
        {
            ViewBag.searchContent = searchContent;

            // Retrieve the viewmodel for the view here, depending on your data structure.
            return PartialView();
        }
        public ActionResult GetSearchData(string searchContent, [DataSourceRequest] DataSourceRequest request)
        {
            List<EmployeeDetailsViewModel> employee = new List<EmployeeDetailsViewModel>();
            if (!searchContent.IsNullOrWhiteSpace())
            {
                searchContent = searchContent.Trim();

                employee = (from emp in db.Employees
                            where (emp.IcNo.Contains(searchContent) || emp.DisplayName.Replace(" ", "").Contains(searchContent) || emp.FullName.Replace(" ", "").Contains(searchContent))
                               && emp.Active.Equals(false) && emp.Status.Equals(false) && !emp.DisplayName.Contains("system") && !emp.DisplayName.Contains("admin")
                               && !emp.DisplayName.Contains("service") && !emp.DisplayName.Contains("pic")
                            select new EmployeeDetailsViewModel
                            {
                                Emp_id = emp.Emp_id,
                                EmpNo = emp.EmpNo,
                                ImagePath = emp.ImagePath,
                                DisplayName = emp.DisplayName,
                                FullName = emp.FullName,
                                IcNo = emp.IcNo,
                                OEmail = emp.OEmail,
                                Status = emp.Status,
                                AddBy = emp.AddBy,
                                ModBy = emp.ModBy,
                                AddDate = emp.AddDate,
                                ModDate = emp.ModDate,
                                Active = emp.Active,
                            }).ToList();
            }
            else
            {
                employee = (from emp in db.Employees
                            where emp.Active.Equals(false) && emp.Status.Equals(false) && !emp.DisplayName.Contains("System") && !emp.DisplayName.Contains("Admin")
                            && !emp.DisplayName.Contains("service") && !emp.DisplayName.Contains("pic")
                            select new EmployeeDetailsViewModel
                            {
                                Emp_id = emp.Emp_id,
                                EmpNo = emp.EmpNo,
                                ImagePath = emp.ImagePath,
                                DisplayName = emp.DisplayName,
                                FullName = emp.FullName,
                                IcNo = emp.IcNo,
                                OEmail = emp.OEmail,
                                Status = emp.Status,
                                AddBy = emp.AddBy,
                                ModBy = emp.ModBy,
                                AddDate = emp.AddDate,
                                ModDate = emp.ModDate,
                                Active = emp.Active,
                            }).ToList();
            }

            return Json(employee.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        #region -- CRUD employee information
        // Get Employee Profile
        public ActionResult Preview(Guid id)
        {
            try
            {
                if (!string.IsNullOrEmpty(Session["EmpNo"] as string))
                {
                    var emp = db.Employees.Find(id);
                    return View(emp);
                }
                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        [HttpGet]
        public ActionResult NewEmployee()
        {
            try
            {
                if (!string.IsNullOrEmpty(Session["EmpNo"] as string))
                {
                    var counter = db.Counters.FirstOrDefault(x => x.count_name == "empno");
                    return View(new EmployeeViewModels() { Gender = "Female", EmpNo = (counter.format ?? "S") + String.Format("{0:D4}", counter.count_no + 1) });
                }
                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult NewEmployee(HttpPostedFileBase file, EmployeeViewModels emp)
        {
            try
            {
                if (!string.IsNullOrEmpty(Session["EmpNo"] as string))
                {
                    if (emp != null && ModelState.IsValid)
                    {
                        // pre-set default image 
                        var fileName = "user.png";

                        if (file != null)
                        {
                            string path = Server.MapPath("~/Images/Customer/" + DateTime.Now.Year.ToString() + "/");
                            fileName = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Day.ToString() + "_" + DateTime.Now.Hour.ToString() + DateTime.Now.Millisecond.ToString() + "_" + Path.GetFileName(file.FileName);

                            if (!Directory.Exists(path))
                            {
                                Directory.CreateDirectory(path);
                            }

                            file.SaveAs(path + fileName);
                            ViewBag.Message += string.Format(Path.GetFileName(file.FileName));
                            fileName = DateTime.Now.Year.ToString() + "/" + fileName;
                        }

                        // get user_id from dbo.userlogin
                        //var id = db.ApsUsers.Where(e => e.email.Equals(emp.OEmail)).FirstOrDefault();

                        try
                        {
                            // Create a new Employee entity and set its properties from the posted EmployeeModel.
                            var employee = new EmployeeViewModels
                            {
                                //Role_id = 2,
                                //User_id = id.user_id ?? "",
                                Emp_id = emp.Emp_id,
                                EmpNo = emp.EmpNo,
                                DisplayName = emp.DisplayName,
                                FullName = emp.FullName.ToUpper(),
                                IcNo = emp.IcNo,
                                ImagePath = fileName,
                                Gender = emp.Gender,
                                OEmail = emp.OEmail,
                                BirthDate = emp.BirthDate,
                                HiredDate = emp.HiredDate?.AddHours(12),

                                Active = false,
                                Status = false,
                                AddBy = Session["EmpNo"].ToString() + "|" + Session["EmpName"].ToString(),
                                ModBy = Session["EmpNo"].ToString() + "|" + Session["EmpName"].ToString(),
                                AddDate = DateTime.Now,
                                ModDate = DateTime.Now
                            };

                            db.Employees.Add(employee);
                            db.SaveChanges();

                            emp.Emp_id = employee.Emp_id;

                        }
                        catch (Exception ex)
                        {
                            throw new Exception(ex.ToString());
                        }

                        return Redirect("Index");
                    }

                    ViewData["Employee"] = db.Employees.Where(e => e.Active.Equals(false) && e.Status.Equals(false)).ToList();
                    return View(emp);
                }
                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }
        [HttpGet]
        public ActionResult EmployeeEdited(Guid id)
        {
            try
            {
                if (!string.IsNullOrEmpty(Session["EmpNo"] as string))
                {
                    ViewData["Employee_All"] = db.Employees.Where(e => e.Status.Equals(false)).ToList();
                    ViewData["Employee"] = db.Employees.Where(e => e.Active.Equals(false) && e.Status.Equals(false)).ToList();
                    ViewData["Active"] = _LoadActivate();

                    var detail = (from emp in db.Employees
                                  where emp.Emp_id.Equals(id)
                                  select new EmployeeDetailsViewModel()
                                  {
                                      Emp_id = emp.Emp_id,
                                      RoleId = emp.RoleId,
                                      ImagePath = emp.ImagePath,
                                      DisplayName = emp.DisplayName,
                                      EmpNo = emp.EmpNo,
                                      FullName = emp.FullName,
                                      IcNo = emp.IcNo,
                                      OEmail = emp.OEmail,
                                      Gender = emp.Gender,
                                      BirthDate = emp.BirthDate,
                                      HiredDate = emp.HiredDate,
                                      ConfirmationDate = emp.ConfirmationDate,
                                      ResignationDate = emp.ResignationDate,

                                      Active = emp.Active,
                                      Status = emp.Status,
                                      AddBy = emp.AddBy,
                                      ModBy = emp.ModBy,
                                      AddDate = emp.AddDate,
                                      ModDate = emp.ModDate
                                  }).FirstOrDefault();

                    return View(detail);
                }
                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EmployeeEdited(HttpPostedFileBase file, EmployeeDetailsViewModel emp)
        {
            if (!string.IsNullOrEmpty(Session["EmpNo"] as string))
            {
                try
                {
                    if (emp != null && !emp.Emp_id.Equals("") && ModelState.IsValid)
                    {
                        // Get Back exists records 
                        var target = db.Employees.Where(e => e.Emp_id.Equals(emp.Emp_id)).FirstOrDefault();
                       
                        var fileName = "";
                        if (file != null)
                        {
                            // -- upload user image to /Images/ --
                            string path = Server.MapPath("~/Images/Customer/" + DateTime.Now.Year.ToString() + "/");
                            fileName = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Day.ToString() + "_" + DateTime.Now.Hour.ToString() + DateTime.Now.Millisecond.ToString() + "_" + Path.GetFileName(file.FileName);
                            if (!Directory.Exists(path))
                            {
                                Directory.CreateDirectory(path);
                            }

                            file.SaveAs(path + fileName);
                            ViewBag.Message += string.Format(Path.GetFileName(file.FileName));

                            // To remove existing image
                            if (target.ImagePath != "user.png")
                            {
                                var physicalPath = Path.Combine(Server.MapPath("~/Images/"), target.ImagePath);

                                // TODO: Verify user permissions
                                if (System.IO.File.Exists(physicalPath))
                                {
                                    // The files are not actually removed in this demo
                                    System.IO.File.Delete(physicalPath);
                                }
                            }
                        }

                        // -- update employee info --
                        target.ImagePath = fileName.Equals("") ? target.ImagePath : fileName.Equals(null) ? target.ImagePath : DateTime.Now.Year.ToString() + "/" + fileName;
                        target.DisplayName = emp.DisplayName;
                        target.EmpNo = emp.EmpNo;
                        target.FullName = emp.FullName.ToUpper();
                        target.IcNo = emp.IcNo;
                        target.OEmail = emp.OEmail;
                        target.Gender = emp.Gender;
                        target.BirthDate = emp.BirthDate;
                        target.HiredDate = emp.HiredDate != null ? (DateTime?)emp.HiredDate.Value.AddHours(12) : null;
                        target.ConfirmationDate = emp.ConfirmationDate != null ? (DateTime?)emp.ConfirmationDate.Value.AddHours(12) : null;
                        target.ResignationDate = emp.ResignationDate;
                        target.ModBy = Session["EmpNo"].ToString() + "|" + Session["EmpName"].ToString();
                        target.ModDate = DateTime.Now;

                        db.SaveChanges();

                        return RedirectToAction("Preview", "Employee", new { id = emp.Emp_id });
                        //return Redirect(Request.UrlReferrer.ToString());
                    }

                    ViewData["Employee"] = db.Employees.ToList();
                    ViewData["Employee_List"] = db.Employees.Where(e => e.Status.Equals(false)).ToList();
                    ViewData["Active"] = _LoadActivate();
                    return View(emp);
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.ToString());
                }
            }
            return RedirectToAction("Login", "Account");
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Destroy([DataSourceRequest] DataSourceRequest request, EmployeeViewModels emp)
        {
            var target = db.Employees.Find(emp.Emp_id);

            target.Active = true;
            target.Status = true;
            target.ModBy = Session["EmpNo"].ToString();
            target.ModDate = DateTime.Now;

            // Attach the entity
            db.Employees.Attach(target);

            // Change its state to Modified so Entity Framework can update the existing product instead of creating a new one.
            db.Entry(target).State = EntityState.Modified;

            // Update the entity in the database.
            db.SaveChanges();

            return Json(new[] { emp }.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }
        #endregion

        public IList<SelectListItem> _LoadActivate()
        {
            var items = new List<SelectListItem>
            {
                new SelectListItem { Text = "Active", Value = "false" },
                new SelectListItem { Text = "Inactive", Value = "true" }
            };

            return items;
        }
    }
}