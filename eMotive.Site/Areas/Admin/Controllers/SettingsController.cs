using System.Web.Mvc;
using eMotive.FoL.Common;
using eMotive.FoL.Common.ActionFilters;
using eMotive.Managers.Interfaces;
using eMotive.Models.Objects.Search;
using eMotive.Search.Interfaces;
using eMotive.Services.Interfaces;

namespace eMotive.FoL.Areas.Admin.Controllers
{
    public class SettingsController : Controller
    {
        private readonly INewsManager newsManager;
        private readonly IUserManager userManager;
        private readonly IRoleManager roleManager;
        private readonly ISearchManager searchManager;
        private readonly IEmailService emailService;
        private readonly IPageManager pageManager;
        private readonly IPartialPageManager partialPageManager;

        public SettingsController(ISearchManager _searchManager, IUserManager _userManager, IRoleManager _rolemanager, 
                                  IPageManager _pageManager, IPartialPageManager _partialPageManager, INewsManager _newsManager, IEmailService _emailService)
        {
            newsManager = _newsManager;
            userManager = _userManager;
            roleManager = _rolemanager;
            searchManager = _searchManager;
            emailService = _emailService;
            pageManager = _pageManager;
            partialPageManager = _partialPageManager;
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Search()
        {
            var stats = new SearchStatistics {NumberOfDocuments = searchManager.NumberOfDocuments()};

            return View(stats);
        }

        [AjaxOnly]
        [Common.ActionFilters.Authorize(Roles="Super Admin, Admin")]
        public CustomJsonResult ReindexAllDocuments()
        {
            searchManager.DeleteAll();
            newsManager.ReindexSearchRecords();
            roleManager.ReindexSearchRecords();
            userManager.ReindexSearchRecords();
            emailService.ReindexSearchRecords();
            pageManager.ReindexSearchRecords();
            partialPageManager.ReindexSearchRecords();

            return new CustomJsonResult
            {
                Data = new { success = true }
            };
        }

    }
}
