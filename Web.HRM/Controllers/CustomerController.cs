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
using Meo.Web.Model;
using Microsoft.Ajax.Utilities;
using NPOI.SS.Formula.Functions;

namespace Web.HRM.Controllers
{
    public class CustomerController : AuthController
    {
        DBContext db = new DBContext();

        // GET: Customer
        public ActionResult Index()
        {
            try
            {
                if (!string.IsNullOrEmpty(Session["EmpNo"] as string))
                {
                    ViewData["Customer"] = db.Customers.Where(e => e.Active.Equals(false)).ToList();

                    return View();

                }
                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }
        public ActionResult _SearchCustomer(string searchContent)
        {
            ViewBag.searchContent = searchContent;

            // Retrieve the viewmodel for the view here, depending on your data structure.
            return PartialView();
        }
        public ActionResult GetSearchData(string searchContent, [DataSourceRequest] DataSourceRequest request)
        {
            List<CustomerDetailsViewModel> employee = new List<CustomerDetailsViewModel>();
            if (!searchContent.IsNullOrWhiteSpace())
            {
                searchContent = searchContent.Trim();

                employee = (from cus in db.Customers
                            where (cus.IcNo.Contains(searchContent) || cus.CardNo.Contains(searchContent) || 
                            cus.ContactNo.Contains(searchContent) || cus.FullName.Replace(" ", "").ToLower().Contains(searchContent))
                            && cus.Active.Equals(false) && cus.Status.Equals(false)
                            select new CustomerDetailsViewModel
                            {
                                CusId = cus.CusId,
                                CardNo = cus.CardNo,
                                ImagePath = cus.ImagePath,
                                FullName = cus.FullName,
                                IcNo = cus.IcNo,
                                ContactNo = cus.ContactNo,
                                CreditBal = cus.CreditBal,
                                TPDueAmt = cus.TPDueAmt,
                                SVDueAmt = cus.SVDueAmt,
                                Active = cus.Active,
                                Status = cus.Status,
                                AddBy = cus.AddBy,
                                ModBy = cus.ModBy,
                                AddDate = cus.AddDate,
                                ModDate = cus.ModDate,
                            }).ToList();
            }
            else
            {
                employee = (from cus in db.Customers
                            where cus.Active.Equals(false) && cus.Status.Equals(false)
                            select new CustomerDetailsViewModel
                            {
                                CusId = cus.CusId,
                                CardNo = cus.CardNo,
                                ImagePath = cus.ImagePath,
                                FullName = cus.FullName,
                                IcNo = cus.IcNo,
                                ContactNo = cus.ContactNo,
                                CreditBal = cus.CreditBal,
                                TPDueAmt = cus.TPDueAmt,
                                SVDueAmt = cus.SVDueAmt,
                                Active = cus.Active,
                                Status = cus.Status,
                                AddBy = cus.AddBy,
                                ModBy = cus.ModBy,
                                AddDate = cus.AddDate,
                                ModDate = cus.ModDate,
                            }).ToList();
            }

            return Json(employee.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        #region -- CRUD customer 
        public ActionResult Preview(int id)
        {
            try
            {
                if (!string.IsNullOrEmpty(Session["EmpNo"] as string))
                {
                    var cus = db.Customers.Find(id);
                    return View(cus);
                }
                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        [HttpGet]
        public ActionResult NewCustomer()
        {
            try
            {
                if (!string.IsNullOrEmpty(Session["EmpNo"] as string))
                {
                    return View(new CustomerViewModels());
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
        public ActionResult NewCustomer(HttpPostedFileBase file, CustomerViewModels cus)
        {
            try
            {
                if (!string.IsNullOrEmpty(Session["EmpNo"] as string))
                {
                    if (cus != null && ModelState.IsValid)
                    {
                        CustomerViewModels customer = new CustomerViewModels();

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

                        // Create a new Customer entity and set its properties from the posted CustomerModel.
                        customer = new CustomerViewModels
                        {
                            CusId = (db.Counters.FirstOrDefault(x => x.count_name == "cusid").count_no) + 1,
                            CardNo = cus.CardNo,
                            FullName = cus.FullName,
                            IcNo = cus.IcNo,
                            ImagePath = fileName,
                            ContactNo = cus.ContactNo,
                            Gender = cus.Gender,
                            Remarks = cus.Remarks,
                            CreditBal = 0,
                            TPDueAmt = 0,
                            SVDueAmt = 0,

                            Active = false,
                            Status = false,
                            AddBy = Session["EmpNo"].ToString() + "|" + Session["EmpName"].ToString(),
                            ModBy = Session["EmpNo"].ToString() + "|" + Session["EmpName"].ToString(),
                            AddDate = DateTime.Now,
                            ModDate = DateTime.Now
                        };

                        var icInvalid = db.Customers.FirstOrDefault(x => x.IcNo == cus.IcNo.Trim());
                        var cardnoInvalid = db.Customers.FirstOrDefault(x => x.CardNo == cus.CardNo.Trim());
                        
                        if(icInvalid != null)
                        {
                            ModelState.AddModelError("IcNo", "Duplicate Ic No found, please check!");
                            return View(cus);
                        }
                        if (cardnoInvalid != null)
                        {
                            ModelState.AddModelError("CardNo", "Duplicate Card No found, please check!");
                            return View(cus);
                        }
                        else
                        {
                            db.Customers.Add(customer);
                            db.SaveChanges();
                        }

                        return Redirect("Index");
                    }

                    ViewData["Customer"] = db.Customers.Where(e => e.Status.Equals(false)).ToList();
                    return View(cus);
                }
                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        [HttpGet]
        public ActionResult UpdateCustomer(int id)
        {
            if (!string.IsNullOrEmpty(Session["EmpNo"] as string))
            {
                try
                {
                    ViewData["Active"] = _LoadActivate();

                    var detail = (from cus in db.Customers
                                  where cus.CusId.Equals(id)
                                  select new CustomerDetailsViewModel()
                                  {
                                      CusId = cus.CusId,
                                      CardNo = cus.CardNo,
                                      ImagePath = cus.ImagePath,
                                      FullName = cus.FullName,
                                      IcNo = cus.IcNo,
                                      ContactNo = cus.ContactNo,
                                      Gender = cus.Gender,
                                      Remarks = cus.Remarks,
                                      Active = cus.Active,
                                      Status = cus.Status,
                                      AddBy = cus.AddBy,
                                      ModBy = cus.ModBy,
                                      AddDate = cus.AddDate,
                                      ModDate = cus.ModDate
                                  }).FirstOrDefault();

                    return View(detail);
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.ToString());
                }
            }
            return RedirectToAction("Login", "Account");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateCustomer(HttpPostedFileBase file, CustomerDetailsViewModel cus)
        {
            try
            {
                if (!string.IsNullOrEmpty(Session["EmpNo"] as string))
                {
                    if (cus != null && !cus.CusId.Equals("") && ModelState.IsValid)
                    {
                        // Get Back exists records 
                        var target = db.Customers.Where(e => e.CusId.Equals(cus.CusId)).FirstOrDefault();

                        // pre-set default image 
                        var fileName = target.ImagePath;

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
                            fileName = DateTime.Now.Year.ToString() + "/" + fileName;
                            //file.SaveAs(path + fileName);
                            //ViewBag.Message += string.Format(Path.GetFileName(file.FileName));
                        }

                        // -- update employee info --
                        target.ImagePath = fileName;
                        target.FullName = cus.FullName;
                        target.IcNo = cus.IcNo;
                        target.ContactNo = cus.ContactNo;
                        target.Gender = cus.Gender;
                        target.Remarks = cus.Remarks;
                        target.Active = cus.Active;
                        target.Status = cus.Status;
                        target.ModBy = Session["EmpNo"].ToString() + "|" + Session["EmpName"].ToString();
                        target.ModDate = DateTime.Now;

                        db.SaveChanges();

                        return RedirectToAction("Index");
                        //return Redirect(@Url.Action("Index", "Customer"));
                        //return Redirect(Request.UrlReferrer.ToString());
                    }

                    ViewData["Active"] = _LoadActivate();
                    return View(cus);
                }
                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Destroy([DataSourceRequest] DataSourceRequest request, CustomerViewModels cus)
        {
            var target = db.Customers.Find(cus.CusId);

            target.Active = true;
            target.Status = true;
            target.ModBy = Session["EmpNo"].ToString() + "|" + Session["EmpName"].ToString();
            target.ModDate = DateTime.Now;

            // Attach the entity
            db.Customers.Attach(target);

            // Change its state to Modified so Entity Framework can update the existing product instead of creating a new one.
            db.Entry(target).State = EntityState.Modified;

            // Update the entity in the database.
            db.SaveChanges();

            //return Redirect(Request.UrlReferrer.ToString());
            return Json(new[] { cus }.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
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


        #region -------------- Expiring treatment / service -------------- 
        public ActionResult ExpiringList()
        {
            try
            {
                if (!string.IsNullOrEmpty(Session["EmpNo"] as string))
                {
                    ViewData["Customer"] = db.Customers.Where(e => e.Active.Equals(false)).ToList();
                    return View();
                }

                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }
        public ActionResult _SearchExpiringList(string searchContent)
        {
            ViewBag.searchContent = searchContent;
            return PartialView();
        }
        public ActionResult GetExpiringList(string searchContent, [DataSourceRequest] DataSourceRequest request)
        {
            List<ExpiringListModel> service = new List<ExpiringListModel>();
            if (!searchContent.IsNullOrWhiteSpace())
            {
                searchContent = searchContent.Trim();

                service = (from cus in db.Customers
                           join sv in db.Services on cus.CusId equals sv.CusId
                            where (cus.IcNo.Contains(searchContent) || cus.CardNo.Contains(searchContent) ||
                            cus.ContactNo.Contains(searchContent) || cus.FullName.Replace(" ", "").ToLower().Contains(searchContent))
                            && cus.Active.Equals(false) && cus.Status.Equals(false) && sv.ExpiryDate != null && sv.CourseBal > 0
                            select new ExpiringListModel
                            {
                                CusId = cus.CusId,
                                CardNo = cus.CardNo,
                                ImagePath = cus.ImagePath,
                                FullName = cus.FullName,
                                SalesId = sv.SalesId,
                                Service = sv.ServiceName,
                                CourseBal = sv.CourseBal ?? 0,
                                ExpiryDate = sv.ExpiryDate,
                                PurchaseDate = sv.PurchaseDate,
                            }).ToList();
            }
            else
            {
                service = (from cus in db.Customers
                           join sv in db.Services on cus.CusId equals sv.CusId
                           where cus.Active.Equals(false) && cus.Status.Equals(false)
                           && sv.ExpiryDate != null && sv.CourseBal > 0
                           select new ExpiringListModel
                            {
                                CusId = cus.CusId,
                                CardNo = cus.CardNo,
                                ImagePath = cus.ImagePath,
                                FullName = cus.FullName,
                                SalesId = sv.SalesId,
                                Service = sv.ServiceName,
                                CourseBal = sv.CourseBal ?? 0,
                                ExpiryDate = sv.ExpiryDate,
                               PurchaseDate = sv.PurchaseDate,
                           }).ToList();
            }

            return Json(service.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}