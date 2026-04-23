using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Meo.Web.DBContext;
using Meo.Web.Model;
using Meo.Web.ViewModels;
using System;
using System.Data;
using System.Linq;
using System.Web.Mvc;

namespace Meo.Web.Controllers
{
    public class PageController : AuthController
    {
        public ActionResult PageValid(string empno, string pageid)
        {
            try
            {
                if (!string.IsNullOrEmpty(Session["EmpNo"] as string))
                {
                    DataTable dt = new DataTable();
                    ActionModel action = new ActionModel();
                    empno = Session["EmpNo"].ToString();
                    dt = PageModel.CheckPageAccess(empno, pageid);

                    if (dt.Rows.Count > 0)
                    {
                        //Session["View"] = dt.Rows[0]["View"].ToString();
                        //Session["Edit"] = dt.Rows[0]["Edit"].ToString();
                        Session["PageLoad"] = "Loaded";
                        return Content("");
                    }
                    //Session["View"] = false;
                    //Session["Edit"] = false;
                    Session["PageLoad"] = "Loaded";
                    return Content("");
                }
                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }
    }
}