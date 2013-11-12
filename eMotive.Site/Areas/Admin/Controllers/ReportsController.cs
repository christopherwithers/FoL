using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Extensions;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using eMotive.Managers.Interfaces;
using eMotive.Models.Objects.Signups;
using eMotive.Models.Objects.Users;
using eMotive.Services.Interfaces;

namespace eMotive.FoL.Areas.Admin.Controllers
{
    [Common.ActionFilters.Authorize(Roles = "Super Admin, Admin, Moderator")]
    public class ReportsController : Controller
    {
        private readonly IReportService reportService;
        private readonly IDocumentManagerService documentManager;
        private readonly ISignupManager signupManager;
        private readonly IUserManager userManager;
        private readonly INotificationService notificationService;

        private readonly string CONTENT_TYPE;

        public ReportsController(IReportService _reportService, IDocumentManagerService _documentManager, ISignupManager _signupManager, IUserManager _userManager, INotificationService _notificationService)
        {
            reportService = _reportService;
            documentManager = _documentManager;
            signupManager = _signupManager;
            userManager = _userManager;
            notificationService = _notificationService;

            CONTENT_TYPE = documentManager.FetchMimeTypeForExtension("xlxs").Type;
        }

        public ActionResult Index()
        {
            var signupAdminView = new AdminSignupView
            {
                Signups = signupManager.FetchAll()
            };

            return View(signupAdminView);
        }

        public FileStreamResult ApplicantsNotSignedUp()
        {
            var users = reportService.FetchUsersNotSignedUp();

            if (users.HasContent())
            {
                using (var xlPackage = new ExcelPackage())
                {
                    const string REPORT_NAME = "Inactive Applicant Report";
                    var worksheet = xlPackage.Workbook.Worksheets.Add(REPORT_NAME);

                    int x = 1;
                    using (var r = worksheet.Cells["A1:D1"])
                    {
                        r.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        r.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(171, 205, 250));
                        r.Style.Font.Bold = true;
                        r.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    }

                    worksheet.Cells[x, 1].Value = "Username";
                    worksheet.Cells[x, 2].Value = "Forename"; 
                    worksheet.Cells[x, 3].Value = "Surname"; 
                    worksheet.Cells[x, 4].Value = "Email"; 

                    x++;

                    foreach (var user in users)
                    {
                        if (!user.Enabled)
                        {
                            using (var r = worksheet.Cells[string.Concat("A", x, ":D", x)])
                            {
                                r.Style.Fill.PatternType = ExcelFillStyle.Solid;
                                r.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 203, 203));
                            }
                        }

                        worksheet.Cells[x, 1].Value = user.Username;
                        worksheet.Cells[x, 2].Value = user.Forename;
                        worksheet.Cells[x, 3].Value = user.Surname;
                        worksheet.Cells[x, 4].Value = user.Email;

                        x++;
                    }

                    return new FileStreamResult(new MemoryStream(xlPackage.GetAsByteArray()), CONTENT_TYPE) { FileDownloadName = string.Format("{0}.xlsx", REPORT_NAME) };

                }
            }

            return null;
        }

        public FileStreamResult InterviewReport(int id)
        {
            var signup = signupManager.Fetch(id);
            if (signup != null)
            {
                var userIDs = signup.Slots.Where(o => o.ApplicantsSignedUp != null).SelectMany(n => n.ApplicantsSignedUp.Select(m => m.User.Username));

                if(!userIDs.HasContent())
                    throw new HttpException(404, "File not found.");

                var userDict = userManager.FetchApplicantData(userIDs);

                if(!userDict.HasContent())
                    throw new HttpException(404, "File not found.");

                using (var xlPackage = new ExcelPackage())
                {
                    string REPORT_NAME = string.Format("{0} Report",signup.Date.ToString("dddd-d-MMMM-yyyy"));
                    var worksheet = xlPackage.Workbook.Worksheets.Add(REPORT_NAME);
                    
                    int x = 1;
                    using (var r = worksheet.Cells["A1:U1"])
                    {
                        r.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        r.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(171, 205, 250));
                        r.Style.Font.Bold = true;
                        r.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    }

                    worksheet.Cells[x, 1].Value = "Candidate No";
                    worksheet.Cells[x, 2].Value = "Photo ID Checked (Y or leave blank)";
                    worksheet.Cells[x, 3].Value = "Invite for Interview? (Y or leave blank)";
                    worksheet.Cells[x, 4].Value = "Non-UK Resident? (Y or leave blank)";
                    worksheet.Cells[x, 5].Value = "Date of Interview";
                    worksheet.Cells[x, 6].Value = "Registration Time";
                    worksheet.Cells[x, 7].Value = "Attended Interview? (DNA or leave blank)";
                    worksheet.Cells[x, 8].Value = "Term Code";
                    worksheet.Cells[x, 9].Value = "Personal ID";
                    worksheet.Cells[x, 10].Value = "Applicant Prefix";
                    worksheet.Cells[x, 11].Value = "Surname";
                    worksheet.Cells[x, 12].Value = "First Name";
                    worksheet.Cells[x, 13].Value = "Date of Birth";
                    worksheet.Cells[x, 14].Value = "Gender";
                    worksheet.Cells[x, 15].Value = "Disability Code";
                    worksheet.Cells[x, 16].Value = "Email Address";
                    worksheet.Cells[x, 17].Value = "Previous School Desc";
                    worksheet.Cells[x, 18].Value = "School Address City";
                    worksheet.Cells[x, 19].Value = "Interview Time Allocated";
                    worksheet.Cells[x, 20].Value = "Candidate Signature";

                    x++;

                    foreach (var slot in signup.Slots.OrderBy(n => n.Time))
                    {
                        if (slot.ApplicantsSignedUp.HasContent())
                        {
                            foreach (var user in slot.ApplicantsSignedUp)
                            {
                                List<ApplicantData> currentApplicantData;
                                bool hasData = userDict.TryGetValue(user.User.Username, out currentApplicantData);

                                worksheet.Cells[x, 1].Value = string.Empty;
                                worksheet.Cells[x, 2].Value = string.Empty;
                                worksheet.Cells[x, 3].Value = string.Empty;
                                worksheet.Cells[x, 4].Value = string.Empty;
                                worksheet.Cells[x, 5].Value = signup.Date.ToString("dd/MM/yyyy");
                                worksheet.Cells[x, 6].Value = slot.Description;
                                worksheet.Cells[x, 7].Value = string.Empty;
                                worksheet.Cells[x, 8].Value = hasData ? currentApplicantData.First().TermCode : string.Empty;
                                worksheet.Cells[x, 9].Value = hasData ? currentApplicantData.First().PersonalID : string.Empty;
                                worksheet.Cells[x, 10].Value = hasData ? currentApplicantData.First().ApplicantPrefix : string.Empty;
                                worksheet.Cells[x, 11].Value = hasData ? currentApplicantData.First().Surname : string.Empty;
                                worksheet.Cells[x, 12].Value = hasData ? currentApplicantData.First().Firstname : string.Empty;
                                worksheet.Cells[x, 13].Value = hasData ? currentApplicantData.First().DateOfBirth.ToString("dd/MM/yyyy") : string.Empty;
                                worksheet.Cells[x, 14].Value = hasData ? currentApplicantData.First().Gender : string.Empty;
                                worksheet.Cells[x, 15].Value = hasData ? currentApplicantData.First().DisabilityCode : string.Empty;
                                worksheet.Cells[x, 16].Value = hasData ? string.Join(",", currentApplicantData.Select(n => n.EmailAddress)) : string.Empty;
                                worksheet.Cells[x, 17].Value = hasData ? currentApplicantData.First().PreviousSchoolDesc : string.Empty; ;
                                worksheet.Cells[x, 18].Value = hasData ? currentApplicantData.First().SchoolAddressCity : string.Empty; ;
                                worksheet.Cells[x, 19].Value = string.Empty;
                                worksheet.Cells[x, 20].Value = string.Empty;

                                x++;
                            }
                        }
                    }
                    worksheet.Cells.AutoFitColumns();

                    return new FileStreamResult(new MemoryStream(xlPackage.GetAsByteArray()), CONTENT_TYPE)
                        {
                            FileDownloadName = string.Format("{0}.xlsx", REPORT_NAME)
                        };
                }
            }
            
            throw new HttpException(404, "File not found.");
        }
    }
}
