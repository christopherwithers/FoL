using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using Extensions;
using eMotive.FoL.Common;
using eMotive.FoL.Common.ActionFilters;
using eMotive.Managers.Interfaces;
using eMotive.Models.Objects.Account;
using eMotive.Models.Objects.StatusPages;
using eMotive.Services.Interfaces;

namespace eMotive.FoL.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAccountManager accountManager;
        private readonly INotificationService notificationSerivce;
        private readonly IEmailService emailService;
        private readonly IUserManager userManager;

        public AccountController(IAccountManager _accountManager, IUserManager _userManager, IEmailService _emailService, INotificationService _notificationSerivce)
        {
            accountManager = _accountManager;
            userManager = _userManager;
            emailService = _emailService;
            notificationSerivce = _notificationSerivce;
        }

        [HttpGet]
        public ActionResult Login()
        {
            return View(new Login());
        }

        [HttpPost]
        public ActionResult Login(Login login)
        {
            if (ModelState.IsValid)
            {
                if (accountManager.ValidateUser(login.UserName, login.Password))
                {
                    var user = userManager.Fetch(login.UserName);

                    FormsAuthentication.SetAuthCookie(login.UserName, login.RememberMe);

                    if (user.Roles.Any(n => n.Name == "Applicant"))
                    {
                        return RedirectToAction("Index", "Home", new { area = "" });
                    }

                    return RedirectToAction("Index", "Home", new { area = "Admin" });
                }

                var errors = notificationSerivce.FetchIssues();
                if (errors.HasContent())
                {
                    foreach (var error in errors)
                    {
                        ModelState.AddModelError("error", error);
                    }
                }
            }
            return View(login);
        }

        [HttpGet]
        public ActionResult UsernameReminder()
        {
            return View(new AccountReminder());
        }

        [HttpPost]
        public ActionResult UsernameReminder(AccountReminder accountReminder)
        {
            if (ModelState.IsValid)
            {

                if (accountManager.ResendUsername(accountReminder.EmailAddress))
                {
                    var successView = new SuccessView
                    {
                        Message = "Your username has been emailed to your registered email address.",
                        Links = new[]
                            {
                                new SuccessView.Link {Text = "Return to Login", URL = "/MMIAdmin/Account/Login"}

                            }
                    };

                    TempData["SuccessView"] = successView;

                    return RedirectToAction("Success", "Home", new { area = "" });
                }


                var errors = notificationSerivce.FetchIssues();

                if (errors.HasContent())
                {
                    foreach (var error in errors)
                    {
                        ModelState.AddModelError("error", error);
                    }
                }
            }
            return View(new AccountReminder());
        }

        [HttpGet]
        public ActionResult ResendPassword()
        {
            return View(new AccountReminder());
        }

        [HttpPost]
        public ActionResult ResendPassword(AccountReminder accountReminder)
        {
            if (ModelState.IsValid)
            {
                if (accountManager.ResendPassword(accountReminder.EmailAddress))
                {
                    var successView = new SuccessView
                    {
                        Message = "A new password has been emailed to your registered email address.",
                        Links = new[]
                            {
                                new SuccessView.Link {Text = "Return to Login", URL = "/MMIAdmin/Account/Login"}

                            }
                    };

                    TempData["SuccessView"] = successView;

                    return RedirectToAction("Success", "Home", new { area = "" });
                }

                var errors = notificationSerivce.FetchIssues();

                if (errors.HasContent())
                {
                    foreach (var error in errors)
                    {
                        ModelState.AddModelError("error", error);
                    }
                }
            }
            return View(new AccountReminder());
        }

        [HttpGet]
        [System.Web.Mvc.Authorize]
        public ActionResult Details()
        {
            return View(new ChangePassword());
        }

        [HttpPost]
        [System.Web.Mvc.Authorize]//todo: look into using this https://github.com/colinangusmackay/Xander.PasswordValidator
        public ActionResult Details(ChangePassword changePassword)
        {
            if (ModelState.IsValid)
            {
                changePassword.Username = User.Identity.Name;

                if (accountManager.ChangePassword(changePassword))
                {
                    var successView = new SuccessView
                    {
                        Message = "Your password has been updated.",
                        Links = new[]
                            {
                                new SuccessView.Link {Text = "Return to Details page", URL = "/MMIAdmin/Account/Details"}
                            }
                    };

                    TempData["SuccessView"] = successView;

                    return RedirectToAction("Success", "Home", new { area = "" });
                }

                var errors = notificationSerivce.FetchIssues();

                if (errors.HasContent())
                {
                    foreach (var error in errors)
                    {
                        ModelState.AddModelError("error", error);
                    }
                }
            }

            return View(changePassword);
        }

        public ActionResult AccessDenied()
        {
            return View();
        }

        [System.Web.Mvc.Authorize]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Response.Cache.SetCacheability(System.Web.HttpCacheability.NoCache);
            Response.Cache.SetNoStore();

            Session.Abandon();

            return RedirectToAction("Login");
        }

        [AjaxOnly]
        public CustomJsonResult FetchEmailSentLog(string username)
        {
            var emailsSentInformation = emailService.FetchEmailLog(username);
            var success = emailsSentInformation.HasContent();
            return new CustomJsonResult
            {
                Data = new { success = success, message = success ? string.Empty : string.Format("No emails have been sent for {0}.", username), results = emailsSentInformation }
            };

        }

        [AjaxOnly]
        public CustomJsonResult ResendAccountCreationEmail(string username)
        {
            var success = accountManager.ResendAccountCreationEmail(username);

            //TODO: pull errors here and pass in Json obj?

            return new CustomJsonResult
                {
                    Data = new {success = success, message = string.Empty, results = string.Empty}
                };
        }

    }
}
