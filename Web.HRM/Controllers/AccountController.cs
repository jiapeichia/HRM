using Meo.Web.DBContext;
using Meo.Web.Model;
using Meo.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using DocumentFormat.OpenXml.Spreadsheet;

namespace Web.HRM.Controllers
{
    public class AccountController : Controller
    {
        DBContext db = new DBContext();
        string userEmail = "";
        //string strDomainPath = "LDAP://DC=meo,DC=local";

        //GET: Account
        public ActionResult Login()
        {
            TempData["LoginMsg"] = "";
            return View();
        }

        public bool AuthenticateUser(string path, string user, string pass)
        {
            var de = new DirectoryEntry(path, user, pass, AuthenticationTypes.Secure);
            try
            {
                // run a search using those credentials.  
                // If it returns anything, then you're authenticated
                var ds = new DirectorySearcher(de);
                ds.Filter = "(SAMAccountName=" + user + ")";
                // ds.PropertiesToLoad.Add("givenName")
                // ds.PropertiesToLoad.Add("sn")
                ds.PropertiesToLoad.Add("mail");
                SearchResult result = ds.FindOne();
                userEmail = result.Properties["mail"][0].ToString();
                return true;
            }
            catch (Exception ex)
            {
                // otherwise, it will crash out so return false
                return false;
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(UserLoginModel accessright)
        {
            try
            {
                Session.Clear();
                //Session.Abandon();

                //if (accessright.email != "" && accessright.email != null && accessright.Password != "" && accessright.Password != null)
                if (accessright.username != "" && accessright.username != null && accessright.Password != "" && accessright.Password != null)
                {
                    DataTable dt = new DataTable();
                    ActionModel action = new ActionModel();

                    //string eMail = accessright.email; //UserPrincipal.Current.EmailAddress;

                    // *** Auto login as window credential 
                    var aspuser = db.AspUsers.FirstOrDefault(e => e.username == accessright.username.ToUpper() && e.status == false);
                    var emp_email = db.Employees.FirstOrDefault(e => e.EmpNo.Equals(aspuser.emp_no) && e.Status.Equals(false) && e.Active.Equals(false));
                    var access = db.PageAccesses.Where(x => x.Emp_no == aspuser.emp_no).ToList();
                    var page = db.Pages.Where(x => x.Status == false).ToList();

                    if (emp_email != null)
                    {
                        dt = UserLoginModel.LoadApsUserLogin(accessright.username, accessright.Password);

                        Session["EmpNo"] = dt.Rows[0]["EmpNo"].ToString();
                        Session["Username"] = dt.Rows[0]["username"].ToString();
                        Session["EmpName"] = dt.Rows[0]["emp_name"].ToString();
                        Session["DisplayName"] = dt.Rows[0]["DisplayName"].ToString();
                        //Session["Fullname"] = dt.Rows[0]["Fullname"].ToString();
                        Session["Email"] = dt.Rows[0]["Email"].ToString();
                        //Session["Gender"] = dt.Rows[0]["Gender"].ToString();
                        //Session["ImagePath"] = dt.Rows[0]["ImagePath"].ToString();
                        Session["RoleName"] = dt.Rows[0]["RoleName"].ToString();

                        if(access.Count > 0)
                        {
                            foreach (var item in access)
                            {
                                var name = page.Find(x => x.Id == item.PageId)?.Name.Replace(" ", "");
                                Session[name] = "true";
                            }
                        }

                        return RedirectToAction("Index", "Home");
                    }

                    //var check = db.ApsUsers.FirstOrDefault(e => e.email.Equals(accessright.email) && e.status.Equals(false));
                    //var emp_email = db.Employees.FirstOrDefault(e => e.OEmail.Equals(accessright.email) && e.Status.Equals(false) && e.Active.Equals(false));

                    if (aspuser != null && emp_email != null)
                    {
                        ModelState.AddModelError(string.Empty, "UserName/Password is Invalid! Please try again!");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Invalid email! Please contact your administator for more information.");
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Please fill up the required field.");
                }
                return View();
            }
            catch (Exception ex)
            {
                //ViewBag.errorMessage = ex.ToString();
                ModelState.AddModelError(string.Empty, "Connection Issue");
                return View();
                // throw new Exception(ex.ToString());
            }
        }

        #region POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("Login", "Account");
        }
        #endregion

        public ActionResult ForgetPassword()
        {
            TempData["LoginMsg"] = "";
            return View();
        }

        [HttpPost]
        public ActionResult ForgetPassword(UserLoginModel accessright)
        {
            try
            {
                DataTable dt = new DataTable();
                ActionModel action = new ActionModel();

                if (accessright.email != "" && accessright.email != null)
                {
                    var check = db.AspUsers.Where(e => e.email.Equals(accessright.email) && e.status.Equals(false)).ToList();
                    if (check.Count > 0)
                    {
                        var owner = db.Employees.Where(e => e.OEmail.Equals(accessright.email) && e.Status.Equals(false) && e.Active.Equals(false)).FirstOrDefault();
                        //EmailModel.SP_HR_Add_C_Notification(900, owner.User_id, accessright.email);

                        return RedirectToAction("Login", "Account");
                    }
                    else
                    {
                        ModelState.AddModelError("contact", "Invalid email. Please contact your administator for more information.");
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Please fill up the required field.");
                }
                return View();
            }
            catch (Exception)
            {
                //ModelState.AddModelError(string.Empty, ex.Message.ToString());
                ModelState.AddModelError(string.Empty, "Failed to send email. Please contact your administrator.");
                return View();
                //throw new Exception(ex.ToString());
            }
        }

        // GET: /Manage/ChangePassword
        public ActionResult ResetPassword()
        {
            Session["PageLoad"] = "";
            return View(new ChangePasswordViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(ChangePasswordViewModel changepw)
        {
            if (ModelState.IsValid)
            {
                using (var db = new DBContext())
                {
                    //var userid = Session["user_id"].ToString();
                    //var target = db.ApsUsers.Where(e => e.user_id.ToString().Equals(userid)).FirstOrDefault();

                    //if (target != null)
                    //{
                    //    target.Password = changepw.NewPassword;
                    //    target.update_by = Session["user_id"].ToString();
                    //    target.update_date = DateTime.Now;

                    //    db.ApsUsers.Attach(target);
                    //    db.Entry(target).State = EntityState.Modified;
                    //    db.SaveChanges();

                    //    return View("Re_Login");
                    //}

                    ModelState.AddModelError("", "Incorrect password! Please try again.");
                    return View();
                }
            }
            return View();
        }

        [HttpGet]
        public ActionResult ExtendSession()
        {
            System.Web.Security.FormsAuthentication.SetAuthCookie(User.Identity.Name, false);
            var data = new { IsSuccess = true };
            return Json(data, JsonRequestBehavior.AllowGet);
        }

    }
}