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
    public class HomeController : Controller
    {
        DBContext db = new DBContext();

        public ActionResult Index()
        {
            try
            {
                if (!string.IsNullOrEmpty(Session["EmpNo"] as string))
                {
                    var empno = Session["EmpNo"].ToString();
                    
                    return View();
                }
                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public ActionResult Paymentdone()
        {
            return View();
        }
    }
}