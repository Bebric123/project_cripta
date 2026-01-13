using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace project_cripta
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.MapMvcAttributeRoutes();

            routes.MapRoute(
                name: "Portfolio",
                url: "portfolio",
                defaults: new { controller = "Profile", action = "Portfolio" }
            );

            routes.MapRoute(
                name: "Balance",
                url: "balance",
                defaults: new { controller = "Balance", action = "Index" }
            );

            routes.MapRoute(
                name: "Profile",
                url: "profile",
                defaults: new { controller = "Profile", action = "Profile" }
            );

            routes.MapRoute(
                name: "Exchange",
                url: "exchange",
                defaults: new { controller = "Profile", action = "Exchange" }
            );

            routes.MapRoute(
                name: "Login",
                url: "login",
                defaults: new { controller = "Account", action = "Login" }
            );

            routes.MapRoute(
                name: "Register",
                url: "register",
                defaults: new { controller = "Account", action = "Register" }
            );

            routes.MapRoute(
                name: "Logout",
                url: "logout",
                defaults: new { controller = "Account", action = "Logout" }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
