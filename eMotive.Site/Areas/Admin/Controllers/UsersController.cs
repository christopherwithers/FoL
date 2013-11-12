using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Extensions;
using eMotive.FoL.Common;
using eMotive.FoL.Common.ActionFilters;
using eMotive.Managers.Interfaces;
using eMotive.Models.Objects.StatusPages;
using eMotive.Models.Objects.Users;
using eMotive.Services.Interfaces;
using Novacode;

namespace eMotive.FoL.Areas.Admin.Controllers
{
    //http://stackoverflow.com/questions/11461142/parse-json-string-into-an-array
    [Common.ActionFilters.Authorize(Roles = "Super Admin, Admin")]
    public class UsersController : Controller
    {
        private readonly IUserManager userManager;
        private readonly IRoleManager roleManager;
        private readonly ISignupManager signupManager;
        private readonly INotificationService notificationService;
        private readonly string[] searchType;
        public UsersController(IUserManager _userManager, IRoleManager _roleManager, ISignupManager _signupManager, INotificationService _notificationService)
        {
            userManager = _userManager;
            roleManager = _roleManager;
            signupManager = _signupManager;
            notificationService = _notificationService;
            searchType = new[] { "User" };
        }

        [Common.ActionFilters.Authorize(Roles = "Super Admin, Admin, Moderator")]
        public ActionResult Index(UserSearch userSearch)
        {
            userSearch.Type = searchType;

            var searchItem = userManager.DoSearch(userSearch);

            if (searchItem.Items.HasContent())
            {
                userSearch.NumberOfResults = searchItem.NumberOfResults;
                userSearch.Users = userManager.FetchRecordsFromSearch(searchItem);

                return View(userSearch);
            }
            return View(new UserSearch());
        }

        [HttpGet]
        public ActionResult Create()
        {
            var user = new UserWithRoles();
            ViewBag.Roles = new SelectList(roleManager.FetchAll(), "Id", "Name");

            return View(user);
        }

        [HttpPost]
        public ActionResult Create(UserWithRoles user)
        {
            var roles = roleManager.FetchAll();
            if (ModelState.IsValid)
            {
                var role = roles.Single(n => n.ID.ToString() == user.SelectedRole);
                user.Roles = new[] {role};

                int id;
                
                if (userManager.Create(user, out id))
                {
                    var successView = new SuccessView
                    {
                        Message = "The new User was successfully created.",
                        Links = new[]
                            {
                                new SuccessView.Link {Text = string.Format("Edit {0}", user.Username), URL = @Url.Action("Edit", "Users", new {username = user.Username})},
                                new SuccessView.Link {Text = "Return to Users Home", URL = "/Admin/Users"}
                            }
                    };

                    TempData["SuccessView"] = successView;

                    return RedirectToAction("Success", "Home", new { area = "Admin" });
                }

                var errors = notificationService.FetchIssues();
                if (errors.HasContent())
                {
                    foreach (var error in errors)
                    {
                        ModelState.AddModelError("error", error);
                    }
                }
            }

            ViewBag.Roles = new SelectList(roles, "Id", "Name");
            return View(user);
        }

        [HttpGet]
        public ActionResult Edit(string username)
        {
            var user = userManager.Fetch(username);

            var userWithRoles = new UserWithRoles
                {
                    Username = user.Username,
                    Archived = user.Archived,
                    Email = user.Email,
                    Enabled = user.Enabled,
                    Forename = user.Forename,
                    Surname = user.Surname,
                    ID = user.ID,
                    Roles = user.Roles,
                    SelectedRole = user.Roles.HasContent() ? user.Roles.FirstOrDefault().ID.ToString(CultureInfo.InvariantCulture) : "0"
                };

            ViewBag.Roles = new SelectList(roleManager.FetchAll(), "Id", "Name");//userWithRoles.AllRoles.Select(m => new SelectListItem {Text = m.Name, Value = m.ID.ToString()});

            return View(userWithRoles);
        }

        [HttpPost]
        public ActionResult Edit(UserWithRoles user)
        {
            var roles = roleManager.FetchAll();
            if (ModelState.IsValid)
            {
                var role = roles.Single(n => n.ID.ToString() == user.SelectedRole);
                user.Roles = new[] { role };

                if (userManager.Update(user))
                {
                    var successView = new SuccessView
                    {
                        Message = "The User was successfully updated.",
                        Links = new[]
                            {
                                new SuccessView.Link {Text = string.Format("Edit {0}", user.Username), URL = @Url.Action("Edit", "Users", new {username = user.Username})},
                                new SuccessView.Link {Text = "Return to Users Home", URL = "/Admin/Users"}
                            }
                    };

                    TempData["SuccessView"] = successView;

                    return RedirectToAction("Success", "Home", new { area = "Admin" });
                }

                var errors = notificationService.FetchIssues();
                if (errors.HasContent())
                {
                    foreach (var error in errors)
                    {
                        ModelState.AddModelError("error", error);
                    }
                }
            }

            return View(user);
        }

        [AjaxOnly]
        public CustomJsonResult FetchApplicantData(string username)
        {
            var data = userManager.FetchApplicantData(username);
            var success = data.HasContent();
            var errors = !success ? notificationService.FetchIssues() : new string[] { };

            return new CustomJsonResult
            {
                Data = new { success = success, message = errors, results = data }
            };
        }

        [AjaxOnly]
        public CustomJsonResult FetchApplicantSignups(string username)
        {
            var data = signupManager.FetchHomeView(username);
            var success = data != null;
            var errors = !success ? notificationService.FetchIssues() : new string[] { };

            return new CustomJsonResult
            {
                Data = new { success = success, message = errors, results = data }
            };
        }

        [AjaxOnly]
        public CustomJsonResult DeleteUser(string username)
        {
            var user = userManager.Fetch(username);

            var success = userManager.Delete(user);

            var errors = !success ? notificationService.FetchIssues() : new string[] { };

            return new CustomJsonResult
            {
                Data = new { success = success, message = errors, results = string.Empty }
            };
        }

        public FileStreamResult GenerateApplicantLetter(string username)
        {
            var user = userManager.Fetch(username);

            if(user == null)
                throw new HttpException(404, "The requested letter could not be generated.");

            var docName = string.Format("{0}_{1}_letter.docx", user.Forename, user.Surname);

            using (var document = DocX.Create(docName))
            {

                var p = document.InsertParagraph();
                p.Append(DateTime.Now.ToString("dddd d MMMM yyyy")).FontSize(8).AppendLine().AppendLine()
                    .Append(string.Format("Dear {0} {1}", user.Forename, user.Surname)).AppendLine().AppendLine()
                    .Append("INVITATION FOR MEDICINE INTERVIEW").Bold().FontSize(14).AppendLine().AppendLine()
                    .Append("Thank you for your application to study Medicine and Surgery at the University of Birmingham. I am pleased to invite you for an interview.").AppendLine().AppendLine()
                    .Append("Please log in to our web based interview sign up system using the link and details below. Once you have selected your chosen date and time you will receive further information by email.").AppendLine().AppendLine()
                    .Append("Username:").AppendLine().AppendLine()
                    .Append("Password:").AppendLine().AppendLine()
                    .Append("http://mymds.bham.ac.uk/MMIApplicants").UnderlineStyle(UnderlineStyle.singleLine).AppendLine().AppendLine()
                    .Append("Yours sincerely").AppendLine();
                
                    var img = document.AddImage(Server.MapPath("~/Content/images/Signature.jpg"));
                    var pic = img.CreatePicture();
                    pic.Width = 100;
                pic.Height = 35;
                    p = document.InsertParagraph();
                    p.InsertPicture(pic);
                    p.AppendLine().AppendLine().Append("Dr Austen Spruce").AppendLine()
                    .Append("Medicine Admissions Tutor").AppendLine()
                    .Append("Telephone: 0121 414 9044 / 4046").FontSize(8).AppendLine()
                    .Append("Email: medicineinterviews@contacts.bham.ac.uk").FontSize(8);


                var ms = new MemoryStream();
                document.SaveAs(ms);
                ms.Position = 0;

                var file = new FileStreamResult(ms, "application/vnd.openxmlformats-officedocument.wordprocessingml.document")
                {
                    FileDownloadName = docName
                };

                return file;

            }
        }

    }
}
