using System.Web.Mvc;
using Extensions;
using Ninject;
using eMotive.Models.Objects.StatusPages;
using eMotive.Services.Interfaces;

namespace eMotive.FoL.Common.ActionFilters
{
    public class CriticalErrorAttribute : ActionFilterAttribute
    {
        [Inject]
        public INotificationService NotificationService { get; set; }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            //if the request is an ajax request, we don't want a redirect to happen
            //the controller dealing with the ajax request can fetch the critical
            //errors and pass them back to the user for display
            if (!filterContext.HttpContext.Request.IsAjaxRequest())
            {
                var criticalErrors = NotificationService.FetchErrors();

                if (criticalErrors.HasContent())
                {
                    var helper = new UrlHelper(filterContext.RequestContext);

                    

                    var controller = filterContext.RouteData.DataTokens["controller"] ?? string.Empty;
                    var action = filterContext.RouteData.DataTokens["action"] ?? string.Empty;
                    var area = filterContext.RouteData.DataTokens["area"] ?? string.Empty;

                    var url = helper.Action("Error", "Home", new { area = (!string.IsNullOrEmpty(area.ToString()) && area.ToString() == "Admin") ? "Admin" : "" });

                    var errorView = new ErrorView
                    {
                        Referrer = string.Format("/{0}/{1}", area, controller),
                        ControllerName = controller.ToString(),
                        Errors = criticalErrors
                    };

                    filterContext.Controller.TempData["CriticalErrors"] = errorView;

                    filterContext.Result = new RedirectResult(url);
                }
            }
            base.OnActionExecuted(filterContext);
        }

    }
}