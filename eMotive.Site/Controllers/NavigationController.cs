using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using eMotive.Managers.Interfaces;
using eMotive.Models.Objects.Menu;
using eMotive.Models.Objects.Users;

namespace eMotive.FoL.Controllers
{
    public class NavigationController : Controller
    {
       // private readonly INavigationServices navigationService;
        private readonly IUserManager userManager;

        private User user;


        public NavigationController(IUserManager _userManager)
        {
            userManager = _userManager;

        }

        public string UserWelcome()
        {
            if (user == null)
                return "<p>Welcome <b>";
            user = userManager.Fetch(User.Identity.Name);
            return string.Concat("<p>Welcome <b>", user.Forename, " ", user.Surname, "</b></p><p>", DateTime.Now.ToString("dddd d MMMM yyyy"), "</p>");
        }

        public PartialViewResult MainMenu()
        {
            return PartialView("_mainMenu", User.Identity.IsAuthenticated ? FetchMainMenu(true) : FetchMainMenu(false));
        }

        private static Menu FetchMainMenu(bool _loggedIn)
        {

            if (!_loggedIn)
                return BuildLoggedOutMenu();

            //applicant menu
            return BuildApplicantMenu();
        }

        private static Menu BuildLoggedOutMenu()
        {
            var menu = new Menu
            {
                ID = 1,
                Title = "LoggedOutMenu",
                MenuItems = new[]
                            {
                                 new MenuItem
                                    {
                                        ID = 1,
                                        Name = "Admission Interviews",
                                        URL = "",
                                        Title = "Admission Interviews Public Homepage"
                                    }
                            }
            };

            return menu;
        }

        private static Menu BuildApplicantMenu()
        {
            var menu = new Menu
            {
                ID = 1,
                Title = "ApplicantMenu",
                MenuItems = new[]
                            {
                                 new MenuItem
                                    {
                                        ID = 1,
                                        Name = "Admission Interviews Home",
                                        URL = "/Home/",
                                        Title = "Admission Interviews Home"
                                    },
                                 new MenuItem
                                    {
                                        ID = 2,
                                        Name = "Interview Dates",
                                        URL = "/Interviews/Signups",
                                        Title = "View Interview Slots"
                                    },
                                    new MenuItem
                                    {
                                        ID = 2,
                                        Name = "Applicants with Disabilities",
                                        URL = "/Interviews/Disability",
                                        Title = "View Disability Interview Slots"
                                    },
                                 new MenuItem
                                    {
                                        ID = 2,
                                        Name = "Contact Us",
                                        URL = "/Home/ContactUs",
                                        Title = "Our Contact Details"
                                    },
                                 new MenuItem
                                    {
                                        ID = 2,
                                        Name = "Change Password",
                                        URL = "/Account/Details",
                                        Title = "Change Password"
                                    },
                                 new MenuItem
                                    {
                                        ID = 2,
                                        Name = "Logout",
                                        URL = "/Account/Logout",
                                        Title = "Logout Of The Admission Interviews System"
                                    }

                            }
            };

            return menu;
        }

    }
}
