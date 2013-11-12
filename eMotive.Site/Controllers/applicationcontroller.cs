using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace eMotive.FoL.Controllers
{
    public class ApplicationController : Controller
    {
        /// <summary> 
        /// Renders out javascript 
        /// </summary> 
        /// <returns></returns> 
        [OutputCache(CacheProfile = "Cache1Hour")]
        [ActionName("js")]
        public ContentResult RenderJavascript()
        {
            return new ContentResult()
            {
                Content = GetLookupTables(),
                ContentType = "application/x-javascript"
            };
        }

        [NonAction]
        private string GetLookupTables()
        {
            var js = new StringBuilder();

            // list of keys that correspond to route URLS 
            var urls = new[] 
            {
                new { key = "FetchAllRoles", url = Url.RouteUrl("Admin_default", new { action = "GetAllRoles", controller = "Roles"}) },
                new { key = "CreateRole", url = Url.RouteUrl("Admin_default", new { action = "CreateRole", controller = "Roles"}) },
                new { key = "UpdateRole", url = Url.RouteUrl("Admin_default", new { action = "UpdateRole", controller = "Roles"}) },
                new { key = "DeleteRole", url = Url.RouteUrl("Admin_default", new { action = "DeleteRole", controller = "Roles"}) },
                /*
                new { key = "FetchUsers", url = Url.RouteUrl("Admin_default", new { action = "GetUsers", controller = "Users"}) },
                new { key = "CreateUser", url = Url.RouteUrl("Admin_default", new { action = "CreateUser", controller = "Users"}) },
                new { key = "UpdateUser", url = Url.RouteUrl("Admin_default", new { action = "UpdateUser", controller = "Users"}) },
                new { key = "DeleteUser", url = Url.RouteUrl("Admin_default", new { action = "DeleteUser", controller = "Users"}) },*/

                new { key = "SignupToSlot", url = Url.RouteUrl("default", new { action = "SignupToSlot", controller = "Interviews" }) },
                new { key = "CancelSignup", url = Url.RouteUrl("default", new { action = "CancelSignupToSlot", controller = "Interviews" }) },

                new { key = "FetchEmailSentLog", url = Url.RouteUrl("default", new { action = "FetchEmailSentLog", controller = "Account"}) },
                new { key = "ResendAccountCreationEmail", url = Url.RouteUrl("default", new { action = "ResendAccountCreationEmail", controller = "Account"}) },
                new { key = "DeleteUser", url = Url.RouteUrl("Admin_default", new { action = "DeleteUser", controller = "Users"}) },
                new { key = "FetchApplicantSignups", url = Url.RouteUrl("Admin_default", new { action = "FetchApplicantSignups", controller = "Users"}) },
                new { key = "FetchApplicantData", url = Url.RouteUrl("Admin_default", new { action = "FetchApplicantData", controller = "Users"}) },
                
                new { key = "ReindexAllDocuments", url = Url.RouteUrl("Admin_default", new { action = "ReindexAllDocuments", controller = "Settings"}) }
            };

            js.AppendLine("var Routes = (function(){");

            js.AppendLine("// URL Lookuptable");
            js.AppendLine("var urls=function(url) {");
            js.AppendLine("var lookupTable = " + new JavaScriptSerializer().Serialize(urls.ToDictionary(x => x.key, x => x.url)) + ";");
            js.AppendLine("return lookupTable[url];");
            js.AppendLine("};");

            js.AppendLine("return {");
            js.AppendLine("URL: urls");
            js.AppendLine("};");

            js.AppendLine("})();");

            return js.ToString();

        }




    }
}
