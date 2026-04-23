using System.Web.Mvc;

namespace Web.HRM.Controllers
{
    /// <summary>
    /// Base controller that enforces session authentication for all derived controllers.
    /// Any action that does NOT opt out via [AllowAnonymous] will redirect to Login
    /// if Session["EmpNo"] is absent.
    /// </summary>
    public abstract class AuthController : Controller
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            // Skip auth for actions explicitly decorated with [AllowAnonymous]
            var allowAnon = filterContext.ActionDescriptor
                .GetCustomAttributes(typeof(AllowAnonymousAttribute), inherit: true);
            if (allowAnon.Length > 0)
                return;

            // Also skip for controller-level [AllowAnonymous]
            var controllerAllowAnon = filterContext.ActionDescriptor.ControllerDescriptor
                .GetCustomAttributes(typeof(AllowAnonymousAttribute), inherit: true);
            if (controllerAllowAnon.Length > 0)
                return;

            if (string.IsNullOrEmpty(Session["EmpNo"] as string))
            {
                filterContext.Result = new RedirectResult("~/Account/Login");
            }
        }

        /// <summary>
        /// Convenience property — never null inside an authenticated action.
        /// </summary>
        protected string CurrentEmpNo => Session["EmpNo"]?.ToString();

        protected string CurrentEmpName => Session["EmpName"]?.ToString();

        protected string CurrentRoleName => Session["RoleName"]?.ToString();
    }
}
