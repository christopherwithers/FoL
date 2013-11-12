using System.Web.Mvc;
using eMotive.Models.Objects.StatusPages;

namespace eMotive.FoL.Areas.Admin.Controllers
{
    
    public class HomeController : Controller
    {
        //
        // GET: /Admin/Home/
        [Common.ActionFilters.Authorize(Roles = "Super Admin, Admin, Moderator")]
        public ActionResult Index()
        {


            return View();
        }

        public ActionResult Error()
        {
            ErrorView errorView;
            if (TempData["CriticalErrors"] != null)
            {
                errorView = TempData["CriticalErrors"] as ErrorView;
                TempData["CriticalErrors"] = TempData["CriticalErrors"];
            }
            else
            {
                errorView = new ErrorView
                {
                    ControllerName = "Home",
                    Errors = new[] { "An error occurred." }
                };
            }


            return View(errorView);
        }

        public ActionResult Success()
        {
            if (TempData["SuccessView"] == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var successView = TempData["SuccessView"] as SuccessView;
            TempData["SuccessView"] = TempData["SuccessView"];

            return View(successView);

        }

    }
}
