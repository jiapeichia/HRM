using System;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.ComponentModel;

namespace Meo.Web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            //// For datetime conversion 
            //CultureInfo info = new CultureInfo("en-GB");
            //info.DateTimeFormat.ShortDatePattern = "dd/MM/yyyy";
            //info.DateTimeFormat.LongDatePattern = "dd/MM/yyyy HH:mm";
            //info.DateTimeFormat.DateSeparator = "/";
            //info.NumberFormat.NumberDecimalDigits = 2;
            //Thread.CurrentThread.CurrentCulture = info;
            //Thread.CurrentThread.CurrentUICulture = info;
        }
    }
}
