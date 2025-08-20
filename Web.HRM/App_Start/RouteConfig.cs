using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Meo.Web
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            // Added to reset the first screen
            routes.MapRoute(
               name: "Default",
               url: "{controller}/{action}/{id}",
               defaults: new { controller = "Account", action = "Login", id = UrlParameter.Optional }
           );

            //routes.MapRoute(
            //    name: "GenerateReceipt",
            //    url: "Receipt/GenerateReceipt",
            //    defaults: new { controller = "Receipt", action = "GenerateReceipt" }
            //);

            // Default view for sample project (HRM) 
            //routes.MapRoute(
            //    name: "Default",
            //    url: "{controller}/{action}/{id}",
            //    defaults: new { controller = "Employee", action = "Create", id = UrlParameter.Optional }
            //);
        }
    }
}
