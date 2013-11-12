﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using eMotive.Models.Objects.Uploads;
using eMotive.Models.Objects.Users;
using Extensions;
using Newtonsoft.Json.Linq;
using eMotive.FoL.Infrastructure;
using eMotive.Managers.Interfaces;
using eMotive.Models.Objects.Signups;
using eMotive.Services.Interfaces;
using OfficeOpenXml;

namespace eMotive.FoL.Areas.Admin.Controllers
{
    public class SignupsController : Controller
    {
        private readonly ISignupManager signupManager;
        private readonly IDocumentManagerService documentManager;
        private readonly IUserManager userManager;
        private readonly INotificationService notificationService;

        public SignupsController(ISignupManager _signupManager, IDocumentManagerService _documentManager, IUserManager _userManager, INotificationService _notificationService)
        {
            signupManager = _signupManager;
            documentManager = _documentManager;
            userManager = _userManager;
            notificationService = _notificationService;
        }
        
        [Common.ActionFilters.Authorize(Roles = "Super Admin, Admin")]
        public ActionResult Index()
        {
            var signupAdminView = new AdminSignupView
                {
                    Signups = signupManager.FetchAll()
                };

            return View(signupAdminView);
        }

        [Common.ActionFilters.Authorize(Roles = "Super Admin, Admin")]
        public ActionResult SignupDetails(int id)
        {
            var signup = signupManager.Fetch(id);

            return View(signup);
        }


        public class FineUploaderResult : ActionResult
        {
            public const string ResponseContentType = "text/plain";

            private readonly bool _success;
            private readonly string _error;
            private readonly bool? _preventRetry;
            private readonly JObject _otherData;

            public FineUploaderResult(bool success, object otherData = null, string error = null, bool? preventRetry = null)
            {
                _success = success;
                _error = error;
                _preventRetry = preventRetry;

                if (otherData != null)
                    _otherData = JObject.FromObject(otherData);
            }

            public override void ExecuteResult(ControllerContext context)
            {
                var response = context.HttpContext.Response;
                response.ContentType = ResponseContentType;

                response.Write(BuildResponse());
            }

            public string BuildResponse()
            {
                var response = _otherData ?? new JObject();
                response["success"] = _success;

                if (!string.IsNullOrWhiteSpace(_error))
                    response["error"] = _error;

                if (_preventRetry.HasValue)
                    response["preventRetry"] = _preventRetry.Value;

                return response.ToString();
            }
        }

        [HttpGet]
        [Common.ActionFilters.Authorize(Roles = "Super Admin, Admin")]
        public ActionResult BoxiUpload()
        {
            var lastUpload = documentManager.FetchLastUploaded(UploadReference.BOXI);

            return View(lastUpload);
        }

        [HttpPost]
        [Common.ActionFilters.Authorize(Roles = "Super Admin, Admin")]
        public FineUploaderResult BoxiUpload(FineUpload upload)
        {
            // asp.net mvc will set extraParam1 and extraParam2 from the params object passed by Fine-Uploader

            var uploadLocation = documentManager.FetchUploadLocation(UploadReference.BOXI);
            var dir = uploadLocation.Directory;

            var filename = Path.GetFileName(upload.Filename);
            var modifiedFilename = string.Format("{0}_{1}", DateTime.Now.ToString("hhmmss"), filename);
            var path = Server.MapPath(dir);

            var filePath = Path.Combine(path, modifiedFilename);
            try
            {
                upload.SaveAs(filePath);
            }
            catch (Exception ex)
            {
                return new FineUploaderResult(false, error: ex.Message);
            }

            documentManager.SaveDocumentInformation(new UploadedDocument
            {
                Name = filename,
                DateUploaded = DateTime.Now,
                Extension = Path.GetExtension(upload.Filename),
                Location = path,
                ModifiedName = modifiedFilename,
                Reference = UploadReference.BOXI,
                UploadedByUsername = User.Identity.Name

            });

            var workBook = new FileInfo(filePath);

            using (var xlPackage = new ExcelPackage(workBook))
            {
                ExcelWorksheet ws
                    = xlPackage.Workbook.Worksheets[1];

                var applicants = new List<ApplicantData>();

                for (var rowNum = 2; rowNum <= ws.Dimension.End.Row; rowNum++)
                {
                    applicants.Add(new ApplicantData
                    {
                        TermCode = ws.Cells[rowNum, 1].GetValue<string>(),
                        ApplicationDate = DateTime.FromOADate(ws.Cells[rowNum, 2].GetValue<double>()),
                        ApplicantID = ws.Cells[rowNum, 3].GetValue<string>(),
                        PersonalID = ws.Cells[rowNum, 4].GetValue<string>(),
                        ApplicantPrefix = ws.Cells[rowNum, 5].GetValue<string>(),
                        Surname = ws.Cells[rowNum, 6].GetValue<string>(),
                        Firstname = ws.Cells[rowNum, 7].GetValue<string>(),
                        DateOfBirth = DateTime.FromOADate(ws.Cells[rowNum, 8].GetValue<double>()),
                        Age = ws.Cells[rowNum, 9].GetValue<int>(),
                        Gender = ws.Cells[rowNum, 10].GetValue<string>(),
                        DisabilityCode = ws.Cells[rowNum, 11].GetValue<string>(),
                        DisabilityDesc = ws.Cells[rowNum, 12].GetValue<string>(),
                        ResidenceCode = ws.Cells[rowNum, 13].GetValue<string>(),
                        NationalityDesc = ws.Cells[rowNum, 14].GetValue<string>(),
                        CorrespondenceAddr1 = ws.Cells[rowNum, 15].GetValue<string>(),
                        CorrespondenceAddr2 = ws.Cells[rowNum, 16].GetValue<string>(),
                        CorrespondenceAddr3 = ws.Cells[rowNum, 17].GetValue<string>(),
                        CorrespondenceCity = ws.Cells[rowNum, 18].GetValue<string>(),
                        CorrespondenceNationDesc = ws.Cells[rowNum, 19].GetValue<string>(),
                        CorrespondencePostcode = ws.Cells[rowNum, 20].GetValue<string>(),
                        EmailAddress = ws.Cells[rowNum, 21].GetValue<string>(),
                        PreviousSchoolDesc = ws.Cells[rowNum, 22].GetValue<string>(),
                        SchoolAddressCity = ws.Cells[rowNum, 23].GetValue<string>(),
                        SchoolLEADescription = ws.Cells[rowNum, 24].GetValue<string>()
                    });


                }

                if (!applicants.HasContent())
                    return new FineUploaderResult(true, error: "An error occurred. The applicant data could not be saved.");


                userManager.SaveApplicantData(applicants);


                return new FineUploaderResult(true, new { extraInformation = 12345 });
            }
        }

        [HttpGet]
        [Common.ActionFilters.Authorize(Roles = "Super Admin, Admin")]
        public ActionResult A100ApplicantUpload()
        {
            var applicantUploadView = new ApplicantUploadView
            {
                Groups = null,
                LastUploadedDocument = documentManager.FetchLastUploaded(UploadReference.A100Applicants)
            };


            return View(applicantUploadView);
        }

        [HttpPost]
        [Common.ActionFilters.Authorize(Roles = "Super Admin, Admin")]
        public FineUploaderResult A100ApplicantUpload(FineUpload upload)
        {
            // asp.net mvc will set extraParam1 and extraParam2 from the params object passed by Fine-Uploader

            var uploadLocation = documentManager.FetchUploadLocation(UploadReference.A100Applicants);
            var dir = uploadLocation.Directory;

            var filename = Path.GetFileName(upload.Filename);
            var modifiedFilename = string.Format("{0}_{1}", DateTime.Now.ToString("hhmmss"), filename);
            var path = Server.MapPath(dir);

            var filePath = Path.Combine(path, modifiedFilename);
            try
            {
                upload.SaveAs(filePath);
            }
            catch (Exception ex)
            {
                return new FineUploaderResult(false, error: ex.Message);
            }

            documentManager.SaveDocumentInformation(new UploadedDocument
            {
                Name = filename,
                DateUploaded = DateTime.Now,
                Extension = Path.GetExtension(upload.Filename),
                Location = path,
                ModifiedName = modifiedFilename,
                Reference = UploadReference.A100Applicants,
                UploadedByUsername = User.Identity.Name

            });

            var workBook = new FileInfo(filePath);

            using (var xlPackage = new ExcelPackage(workBook))
            {
                ExcelWorksheet ws
                    = xlPackage.Workbook.Worksheets[1];

                var applicants = new List<ApplicantUploadData>();

                for (var rowNum = 2; rowNum <= ws.Dimension.End.Row; rowNum++)
                {
                    if (!string.IsNullOrEmpty(ws.Cells[rowNum, 3].GetValue<string>()))
                    {
                        applicants.Add(new ApplicantUploadData
                        {
                            InviteForInterview = ws.Cells[rowNum, 3].GetValue<string>(),
                            NonUkResident = ws.Cells[rowNum, 4].GetValue<string>(),
                            PersonID = ws.Cells[rowNum, 12].GetValue<string>(),

                        });

                    }
                }

                if (!applicants.HasContent())
                    return new FineUploaderResult(true, error: "An error occurred. The applicant data could not be saved.");

                var success = userManager.CreateApplicantAccounts(applicants, new[] { 1, 2 });


                //  userManager.SaveApplicantData(applicants);


                return new FineUploaderResult(true, new { extraInformation = 12345 });
            }

            // the anonymous object in the result below will be convert to json and set back to the browser
         //   return new FineUploaderResult(true, new { extraInformation = 12345 });
        }

        [HttpGet]
        [Common.ActionFilters.Authorize(Roles = "Super Admin, Admin")]
        public ActionResult A101ApplicantUpload()
        {
            var applicantUploadView = new ApplicantUploadView
            {
                Groups = null,
                LastUploadedDocument = documentManager.FetchLastUploaded(UploadReference.A101Applicants)
            };


            return View(applicantUploadView);
        }

        [HttpPost]
        [Common.ActionFilters.Authorize(Roles = "Super Admin, Admin")]
        public FineUploaderResult A101ApplicantUpload(FineUpload upload)
        {
            // asp.net mvc will set extraParam1 and extraParam2 from the params object passed by Fine-Uploader

            var uploadLocation = documentManager.FetchUploadLocation(UploadReference.A101Applicants);
            var dir = uploadLocation.Directory;

            var filename = Path.GetFileName(upload.Filename);
            var modifiedFilename = string.Format("{0}_{1}", DateTime.Now.ToString("hhmmss"), filename);
            var path = Server.MapPath(dir);

            var filePath = Path.Combine(path, modifiedFilename);
            try
            {
                upload.SaveAs(filePath);
            }
            catch (Exception ex)
            {
                return new FineUploaderResult(false, error: ex.Message);
            }

            documentManager.SaveDocumentInformation(new UploadedDocument
            {
                Name = filename,
                DateUploaded = DateTime.Now,
                Extension = Path.GetExtension(upload.Filename),
                Location = path,
                ModifiedName = modifiedFilename,
                Reference = UploadReference.A101Applicants,
                UploadedByUsername = User.Identity.Name

            });

            var workBook = new FileInfo(filePath);

            using (var xlPackage = new ExcelPackage(workBook))
            {
                ExcelWorksheet ws
                    = xlPackage.Workbook.Worksheets[1];

                var applicants = new List<ApplicantUploadData>();

                for (var rowNum = 2; rowNum <= ws.Dimension.End.Row; rowNum++)
                {
                    if (!string.IsNullOrEmpty(ws.Cells[rowNum, 3].GetValue<string>()))
                    {
                        applicants.Add(new ApplicantUploadData
                        {
                            InviteForInterview = ws.Cells[rowNum, 3].GetValue<string>(),
                            NonUkResident = ws.Cells[rowNum, 4].GetValue<string>(),
                            PersonID = ws.Cells[rowNum, 12].GetValue<string>(),

                        });

                    }
                }

                if (!applicants.HasContent())
                    return new FineUploaderResult(true, error: "An error occurred. The applicant data could not be saved.");

                var success = userManager.CreateApplicantAccounts(applicants, new[] { 2, 3 });


                //  userManager.SaveApplicantData(applicants);


                return new FineUploaderResult(true, new { extraInformation = 12345 });
            }

            // the anonymous object in the result below will be convert to json and set back to the browser
            //return new FineUploaderResult(true, new { extraInformation = 12345 });
        }


        [Common.ActionFilters.Authorize(Roles = "Super Admin, Admin")]
        public ActionResult Groups()
        {
            return View();
        }

        [Common.ActionFilters.Authorize(Roles = "Super Admin, Admin")]
        public ActionResult OpenClose()
        {
            var signups = signupManager.FetchAllBrief();

            var signupDict = signups.GroupBy(
        m =>
        string.Format("{1} {0}", m.Date.Year,
                      System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(
                          m.Date.Month))).ToDictionary(k => k.Key, g => g.OrderBy(n => n.Date).ToList());

            return View(signupDict);
        }

    }
}
