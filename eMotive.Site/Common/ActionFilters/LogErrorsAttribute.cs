using System.Web.Mvc;
using Ninject;
using eMotive.Services.Interfaces;

namespace eMotive.FoL.Common.ActionFilters
{
    public class LogErrorsAttribute : ActionFilterAttribute
    {
        [Inject]
        public INotificationService notifications { private get; set; }

        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            if(notifications != null) notifications.CommitDatabaseLog();
            base.OnResultExecuted(filterContext);
        }
    }
}