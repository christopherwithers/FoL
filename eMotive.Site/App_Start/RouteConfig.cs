using System.Web.Mvc;
using System.Web.Routing;

namespace eMotive.FoL
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
           // routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.MapRoute(
                "Default",
                "{controller}/{action}/{id}",
                new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                new[] { "eMotive.FoL.Controllers" }
            );
            /*    routes.MapRoute(
                    "Default",
                    "{controller}/{action}/{id}",
                    new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                    new[] { "eMotive.FoL.Controllers" }
                );*/
            /*
        routes.MapRoute(
"AccountDefault",
"{controller}/{action}/{id}",
new { controller = "Account", action = "Login", id = UrlParameter.Optional },
new[] { "eMotive.FoL.Controllers" }
);
        routes.MapRoute(
            "PageSection", // Route name
            "{section}/{page}", // URL with parameters
            new { controller = "Page", action = "Index" }, // Parameter defaults
            new { section = @"^[a-zA-Z0-9\-]+$", page = @"^[a-zA-Z0-9\-]+$" },
            new[] { "eMotive.FoL.Controllers" }// Parameter defaults
        );

        routes.MapRoute(
            "Page", // Route name
            "{section}", // URL with parameters
            new { controller = "Page", action = "Index" }, // Parameter defaults
            new { section = @"^[a-zA-Z0-9\-]+$" },
            new[] { "eMotive.FoL.Controllers" }// Parameter defaults
        );
        */
            //home page - custom news items etc!


        }
    }
}