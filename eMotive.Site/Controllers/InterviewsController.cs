using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Extensions;
using eMotive.FoL.Common;
using eMotive.FoL.Common.ActionFilters;
using eMotive.Managers.Interfaces;
using eMotive.Services.Interfaces;

namespace eMotive.FoL.Controllers
{
    [Common.ActionFilters.Authorize(Roles = "Applicant")]
    public class InterviewsController : Controller
    {
        private readonly IUserManager userManager;
        private readonly ISignupManager signupManager;
        private readonly INotificationService notificationService;
        private readonly IPartialPageManager pageManager;
        private readonly IeMotiveConfigurationService configurationService;

        public InterviewsController(IUserManager _userManager, ISignupManager _signupManager, IPartialPageManager _pageManager, 
            INotificationService _notificationService, IeMotiveConfigurationService _configurationService)
        {
            userManager = _userManager;
            signupManager = _signupManager;
            notificationService = _notificationService;
            pageManager = _pageManager;
            configurationService = _configurationService;


        }

        public ActionResult Disability()
        {
            var signups = signupManager.FetchSignupInformation(User.Identity.Name);

            var pageText = pageManager.FetchPartials(new[] { "Disability-Session-List-header", "Disability-Session-List-Footer" }).ToDictionary(k => k.Key, v => v.Text);
            signups.HeaderText = pageText["Disability-Session-List-header"];
            signups.FooterText = pageText["Disability-Session-List-Footer"];

       //     signups.HeaderText = pageManager.Fetch("Disability-Session-List-header").Text;
            
            return View(signups);
        }

        public ActionResult Signups()
        {
            var signups = signupManager.FetchSignupInformation(User.Identity.Name);

            var pageText = pageManager.FetchPartials(new[] {"Session-List-header", "Session-List-Footer"}).ToDictionary(k => k.Key, v => v.Text);
            signups.HeaderText = pageText["Session-List-header"];
            signups.FooterText = pageText["Session-List-Footer"];

            return View(signups);
        }

        public ActionResult Slots(int? id)
        {
            var slots = signupManager.FetchSlotInformation(id.HasValue ? id.Value : -1, User.Identity.Name);

            if (slots != null)
            {
                var replacements = new Dictionary<string, string>(4)
                {
                    {"#interviewdate#", slots.SignupDate.ToString("dddd d MMMM yyyy")}
                };
                //Disability-Interview-Date-Page
                var sb = new StringBuilder(slots.DisabilitySignup ? pageManager.Fetch("Disability-Interview-Date-Page").Text : pageManager.Fetch("Interview-Date-Page").Text);

                foreach (var replacment in replacements)
                {
                    sb.Replace(replacment.Key, replacment.Value);
                }

                slots.HeaderText = sb.ToString();
                slots.FooterText = pageManager.Fetch("Interview-Date-Page-Footer").Text;
            }

            return View(slots);
        }

        [AjaxOnly]
        public CustomJsonResult SignupToSlot(int idSignup, int idSlot)
        {
            if (signupManager.SignupToSlot(idSignup, idSlot, User.Identity.Name))
            {
                var signup = signupManager.Fetch(idSignup);
                var slot = signup.Slots.Single(n => n.ID == idSlot);

                ApplicantSignupPush(signup.ID, signup.Slots.Sum(n => n.TotalPlacesAvailable),
                    signup.Slots.Sum(n => n.ApplicantsSignedUp.HasContent() ? n.TotalPlacesAvailable - n.ApplicantsSignedUp.Count() : n.TotalPlacesAvailable));

                ApplicantSlotPush(slot.ID, slot.TotalPlacesAvailable,
                    slot.ApplicantsSignedUp.HasContent() ? slot.TotalPlacesAvailable - slot.ApplicantsSignedUp.Count() : slot.TotalPlacesAvailable);

                return new CustomJsonResult
                    {
                        Data = new {success = true, message = "successfully signed up."}
                    };
            }

            var issues = notificationService.FetchIssues();


                return new CustomJsonResult
                {
                    Data = new { success = false, message = issues }
                };
            
        }

        [AjaxOnly]
        public CustomJsonResult CancelSignupToSlot(int idSignup, int idSlot)
        {
            if (signupManager.CancelSignupToSlot(idSignup, idSlot, User.Identity.Name))
            {
                var signup = signupManager.Fetch(idSignup);
                var slot = signup.Slots.Single(n => n.ID == idSlot);

                ApplicantSignupPush(signup.ID, signup.Slots.Sum(n => n.TotalPlacesAvailable),
                    signup.Slots.Sum(n => n.ApplicantsSignedUp.HasContent() ? n.TotalPlacesAvailable - n.ApplicantsSignedUp.Count() : n.TotalPlacesAvailable));

                ApplicantSlotPush(slot.ID, slot.TotalPlacesAvailable,
                    slot.ApplicantsSignedUp.HasContent() ? slot.TotalPlacesAvailable - slot.ApplicantsSignedUp.Count() : slot.TotalPlacesAvailable);

                return new CustomJsonResult
                {
                    Data = new { success = true, message = "successfully cancelled appointment." }
                };
            }

            return new CustomJsonResult
            {
                Data = new { success = false, message = "An error occurred. The signup could not be cancelled." }
            };
        }

        private void ApplicantSignupPush(int _signupID, int _totalPlaces, int _remainingPlaces)
        {
            var pusher = new PusherServer.Pusher(configurationService.PusherID(), configurationService.PusherKey(), configurationService.PusherSecret());

            var result = pusher.Trigger("SignupSelection", "PlacesChanged",
                                                    new
                                                    {
                                                        SignUpId = _signupID,
                                                        TotalPlaces = _totalPlaces,
                                                        PlacesAvailable = _remainingPlaces
                                                    });
        }

        private void ApplicantSlotPush(int _slotID, int _totalPlaces, int _remainingPlaces)
        {
            var pusher = new PusherServer.Pusher(configurationService.PusherID(), configurationService.PusherKey(), configurationService.PusherSecret());

            var result = pusher.Trigger("SignupSelection", "SlotChanged",
                                                    new
                                                    {
                                                        SlotId = _slotID,
                                                        TotalPlaces = _totalPlaces,
                                                        PlacesAvailable = _remainingPlaces
                                                    });
        }

    }
}
