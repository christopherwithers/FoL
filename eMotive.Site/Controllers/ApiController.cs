using System.Web.Mvc;
using GoogleAnalyticsTracker.Web.Mvc;

namespace eMotive.FoL.Controllers
{
    [ActionTracking("UA-45423815-1", "MMI")]
    public class AnalyticsController : Controller
    {
        public JsonResult Create()
        {
            return Json(true, JsonRequestBehavior.AllowGet);
        }
    }
}
