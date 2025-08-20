using Meo.Web.DBContext;
using Meo.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;

namespace Web.HRM.Controllers
{
    public class CompanyProfileController : Controller
    {
        DBContext db = new DBContext();

        // GET: CompanyProfile
        public ActionResult Index()
        {
            return View();
        }

        #region Company Profile
        public ActionResult Preview()
        {
            try
            {
                if (!string.IsNullOrEmpty(Session["EmpNo"] as string))
                {
                    var company = db.CompanyProfile.FirstOrDefault();
                    return View(company);
                }
                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public ActionResult Update(CompanyProfile data, string action)
        {
            try
            {
                if (!string.IsNullOrEmpty(Session["EmpNo"] as string))
                {
                    if (action == "Update" && data.Id == 0)
                    {
                        var com = db.CompanyProfile.FirstOrDefault();
                        return View(com);
                    }

                    var company = db.CompanyProfile.FirstOrDefault(x => x.Id == data.Id);
                    company.Name = data.Name;
                    company.Address1 = data.Address1;
                    company.Address2 = data.Address2;
                    company.Address3 = data.Address3;
                    company.RegNo = data.RegNo;
                    company.Tel = data.Tel;
                    company.Currency = data.Currency;
                    company.Logo = data.Logo;

                    db.CompanyProfile.Attach(company);
                    db.Entry(company).State = EntityState.Modified;
                    db.SaveChanges();

                    return RedirectToAction("Preview", "CompanyProfile");
                }

                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }
        #endregion
    }
}