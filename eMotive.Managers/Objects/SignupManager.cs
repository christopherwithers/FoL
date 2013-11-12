using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AutoMapper;
using Extensions;
using eMotive.Managers.Interfaces;
using eMotive.Models.Objects.Signups;
using eMotive.Repository.Interfaces;
using eMotive.Services.Interfaces;
using rep = eMotive.Repository.Objects.Signups;

namespace eMotive.Managers.Objects
{
    public class SignupManager : ISignupManager
    {
        private readonly ISignupRepository signupRepository;
        private readonly IUserManager userManager;
        private readonly INotificationService notificationService;
        private readonly IEmailService emailService;
        private readonly IeMotiveConfigurationService configurationService;

        readonly Dictionary<int, object> dictionary = new Dictionary<int, object>();
        readonly object dictionaryLock = new object();

        public SignupManager(ISignupRepository _signupRepository, IUserManager _userManager, IEmailService _emailService,
                             INotificationService _notificationService, IeMotiveConfigurationService _configurationService)
        {
            signupRepository = _signupRepository;
            userManager = _userManager;
            emailService = _emailService;
            notificationService = _notificationService;
            configurationService = _configurationService;

            AutoMapperManagerConfiguration.Configure();
        }

        private IEnumerable<Repository.Objects.Signups.Signup> FetchSignupsByGroup(IEnumerable<int> _groups)
        {
            return signupRepository.FetchSignupsByGroup(_groups);
        }

        public Signup Fetch(int _id)
        {
            var repSignup = signupRepository.FetchSignup(_id);


            if (repSignup == null)
            {
                notificationService.AddError("The requested signup could not be found.");
                return null;
            }

            var signup = Mapper.Map<rep.Signup, Signup>(repSignup);

            var usersDict = userManager.Fetch(repSignup.Slots.Where(n => n.UsersSignedUp.HasContent()).SelectMany(n => n.UsersSignedUp).Select(m => m.IdUser)).ToDictionary(k => k.ID, v => v);

            foreach (var repSlot in repSignup.Slots)
            {
                foreach (var slot in signup.Slots)
                {
                    if (repSlot.id != slot.ID) continue;

                    if (!repSlot.UsersSignedUp.HasContent()) continue;

                    slot.ApplicantsSignedUp = new Collection<UserSignup>();
                    foreach (var user in repSlot.UsersSignedUp)
                    {
                        slot.ApplicantsSignedUp.Add(new UserSignup { User = usersDict[user.IdUser], SignupDate = user.SignupDate, ID = user.ID});
                    }
                }
            }


            return signup;
        }

        //TODO: need a new signup admin obj which contains full user + signup date etc! Then map to it!
        public IEnumerable<Signup> FetchAll()
        {
            var signups = signupRepository.FetchAll();

            if (!signups.HasContent())
                return null;

            var signupModels = Mapper.Map<IEnumerable<rep.Signup>, IEnumerable<Signup>>(signups);

            var usersDict = userManager.Fetch(signups.SelectMany(u => u.Slots.Where(n => n.UsersSignedUp.HasContent()).SelectMany(n => n.UsersSignedUp).Select(m => m.IdUser))).ToDictionary(k => k.ID, v => v);

            foreach (var repSignup in signups)
            {
                foreach (var modSignup in signupModels)
                {
                    foreach (var repSlot in repSignup.Slots)
                    {
                        foreach (var slot in modSignup.Slots)
                        {
                            if (repSlot.id != slot.ID) continue;

                            if (!repSlot.UsersSignedUp.HasContent()) continue;

                            slot.ApplicantsSignedUp = new Collection<UserSignup>();
                            foreach (var user in repSlot.UsersSignedUp)
                            {
                                slot.ApplicantsSignedUp.Add(new UserSignup { User = usersDict[user.IdUser], SignupDate = user.SignupDate, ID = user.ID });
                            }
                        }
                    }
                }
            }
            
            return signupModels;
        }

        public IEnumerable<SessionDay> FetchAllBrief()
        {
            var groups = signupRepository.FetchGroups();

            if (!groups.HasContent())
            {
                notificationService.AddError("An error occurred. Groups could not be found.");
                return null;
            }

            var signups = FetchAll();

            return Mapper.Map < IEnumerable <Signup>, IEnumerable <SessionDay>> (signups);
        }

        public UserHomeView FetchHomeView(string _username)
        {
            //todo: fetch user and group
            var user = userManager.Fetch(_username);

            if (user == null)
            {
                //TODo: ERROR MESSAGE HERE!
                return null;
            }

            var profile = userManager.FetchProfile(_username);
            var signups = FetchSignupsByGroup(profile.Groups.Select(n => n.ID));

            if (signups == null)
                return null;

            var homeView = new UserHomeView
                {
                    HasSignedUp =
                        signups.Any(
                            h =>
                            h.Slots.Any(n => n.UsersSignedUp != null && n.UsersSignedUp.Any(m => m.IdUser == user.ID))),
                    User = user
                };

            if (homeView.HasSignedUp)
            {
                foreach (var signup in signups)
                {
                    if (!signup.Slots.HasContent())
                        continue;

                    foreach (var slot in signup.Slots)
                    {
                        if (!slot.UsersSignedUp.HasContent())
                            continue;

                        foreach (var userSignup in slot.UsersSignedUp)
                        {
                            if (userSignup.IdUser != user.ID) continue;

                            homeView.SignUpDate = signup.Date.AddHours(slot.Time.Hour).AddMinutes(slot.Time.Minute);
                            homeView.SignUpDetails = slot.Description;
                            homeView.SignedUpSlotID = slot.id;
                            homeView.SignupID = signup.id;
                        }
                    }
                }

            }
            return homeView;
        }

        //todo: need to fetch user Group!!
        public UserSignupView FetchSignupInformation(string _username)
        {
            var signupCollection = new Collection<SignupState>();
            var user = userManager.Fetch(_username);

            if (user == null)
            {//TODO: ERROR MESSAGE HERE!!
                return null;
            }

            //bool isApplicant = user.Roles.Any(n => n.Name == "Applicant");
            
            var profile = userManager.FetchProfile(_username);
            var userSignUp = FetchuserSignup(user.ID, profile.Groups.Select(n => n.ID));
            var signups = FetchSignupsByGroup(profile.Groups.Select(n => n.ID));

            bool signedup = false;
            int signupId = 0;

            if (signups.HasContent())
            {
                //signupCollection
                foreach (var item in signups)
                {
                    //Logic to deal with applicants and closed signups
                    //if a signup is closed, we hide it from applicants UNLESS they are signed up to a slot in that signup
                    if (/*isApplicant && */(!item.Closed || userSignUp != null && userSignUp.IdSignUp == item.id))
                    {
                        var signup = new SignupState
                            {
                                ID = item.id,
                                Date = item.Date,
                                SignedUp =
                                    item.Slots.Any(
                                        n =>
                                        n.UsersSignedUp.HasContent() &&
                                        n.UsersSignedUp.Any(m => m != null && m.IdUser == user.ID)),
                                TotalSlotsAvailable = item.Slots.Sum(n => n.PlacesAvailable),
                                SlotsAvailable =
                                    item.Slots.Sum(
                                        n =>
                                        n.UsersSignedUp.HasContent()
                                            ? n.PlacesAvailable - n.UsersSignedUp.Count()
                                            : n.PlacesAvailable),
                                DisabilitySignup = item.Group.DisabilitySignups,
                                Closed = item.Closed || item.CloseDate < DateTime.Now //############################ LOOK INTO THIS LOGIC
                            };

                        if (signup.SignedUp)
                        {
                            signedup = true;
                            signupId = signup.ID;
                        }

                        signupCollection.Add(signup);
                    }
                }
            }

            var signupView = new UserSignupView
            {
                SignupInformation = signupCollection,
                SignupID = signupId,
                SignedUp = signedup
            };

            return signupView;

        }

        public UserSlotView FetchSlotInformation(int _signup, string _username)
        {
            var user = userManager.Fetch(_username);

            //TODO need id of slot!
            var signup = signupRepository.FetchSignup(_signup);

            var signupGroup = signupRepository.FetchSignupGroup(_signup);
            var userProfile = userManager.FetchProfile(_username);

            if (signupGroup == null || userProfile == null || !userProfile.Groups.HasContent())
            {
                notificationService.AddError("An error occurred. The selected interview date could not be loaded.");
                
                if(signupGroup == null)
                    notificationService.Log(string.Format("SignupManager: FetchSlotInformation: The signupGroup was null for signup: {0}.", _signup));

                if (userProfile == null)
                    notificationService.Log(string.Format("SignupManager: FetchSlotInformation: The userProfile was null for username: {0}", _username));

                return null;
            }//why isn't this working now?!?

            var hasAccess = userProfile.Groups.Any(@group => signupGroup.ID == @group.ID);

            if (!hasAccess)//signupGroup.ID != 
            {
                notificationService.AddError("You do not have permission to view the requested interview.");
                return null;
            }

            if (signup == null)
            {
                notificationService.AddError("The requested interview date could not be found.");
                return null;
            }

            var userSignup = FetchuserSignup(user.ID, userProfile.Groups.Select(n => n.ID));

            if (signup.Closed && (userSignup == null || userSignup.IdSignUp != signup.id))
            {
                notificationService.AddError("The requested interview date is clsoed.");
                return null;
            }

            var slotCollection = new Collection<SlotState>();

            //TODO: COULD HAVE A BOOL HERE TO CHECK FOR 1 SIGNUP AGAINST ALL GROUPS OR A SIGNUP PER GROUP?
            var userSignUp = signupRepository.FetchUserSignup(user.ID, userProfile.Groups.Select(n => n.ID));

            var slotView = new UserSlotView
            {
                SignupID = _signup,
                SignupDate = signup.Date
            };

            foreach (var sl in signup.Slots)
            {
                if(!sl.UsersSignedUp.HasContent())
                    continue;

                foreach (var applicant in sl.UsersSignedUp)
                {
                    if (applicant.IdUser != user.ID) continue;

                    slotView.SignedUpSlotInformation = new UserSignupInformation
                        {
                            SignupID = signup.id,
                            Date = signup.Date,
                            SlotID = sl.id,
                            Description = sl.Description
                        };
                    break;
                }
            }

            if (slotView.SignedUpSlotInformation == null && userSignUp != null)
            {
                slotView.SignedUpSlotInformation = new UserSignupInformation
                    {
                        SignupID = userSignUp.IdSignUp,
                        Date = userSignUp.SignupDate,
                        SlotID = userSignUp.IdSlot,
                        Description = userSignUp.Description

                    };
            }

            foreach (var item in signup.Slots)
            {
                var slot = new SlotState
                {
                    ID = item.id,
                    Description = item.Description,
                    Enabled = item.Enabled,
                    PlacesAvailable = item.UsersSignedUp.HasContent() ? item.PlacesAvailable - item.UsersSignedUp.Count() : item.PlacesAvailable,
                    TotalPlacesAvailable = item.PlacesAvailable,
                    Status = GenerateSlotStatus(item, slotView, userSignUp)
                };

                slotCollection.Add(slot);
            }

            slotView.Slots = slotCollection;
            slotView.DisabilitySignup = signup.Group.DisabilitySignups;

            return slotView;
        }

        public bool SignupToSlot(int _signupID, int _slotId, string _username)
        {
            var signup = Fetch(_signupID);

            var slot = signup.Slots.SingleOrDefault(n => n.ID == _slotId);

            if (slot == null)
            {
                notificationService.AddError(string.Format("The requested slot ({0}) could not be found for signup {1}.", _slotId, _signupID));
                return false;
            }

            var user = userManager.Fetch(_username);
            object bodyLock;
            lock (dictionaryLock)
            {
                
                if (!dictionary.TryGetValue(_slotId, out bodyLock))
                {
                    bodyLock = new object();
                    dictionary[_slotId] = bodyLock;
                }
            }

            if(signup.Closed)
            {
                notificationService.AddIssue("You cannot sign up to this slot. The sign up is closed.");

                return false;
            }

            if (DateTime.Now > signup.CloseDate)
            {
                notificationService.AddIssue(string.Format("You cannot sign up to this slot. The sign up closed on {0}.", signup.CloseDate.ToString("dddd d MMMM yyyy")));

                return false;
            }

            lock (bodyLock)
            {
                var signupDate = DateTime.Now;

              //  string error;

                if (slot.ApplicantsSignedUp.HasContent())
                {
                    if (slot.ApplicantsSignedUp.Any(n => n.User.Username == user.Username))
                    {
                        notificationService.AddIssue("You have already signed up to this slot.");
                        return false;
                    }

                    if (slot.ApplicantsSignedUp.Count() >= slot.TotalPlacesAvailable)
                    {
                        notificationService.AddIssue("The selected slot is now full.");
                        return false;
                    }
                }

                int id;
                if (signupRepository.SignupToSlot(_slotId, user.ID, signupDate, out id))
                {
                   // slot.ApplicantsSignedUp.Single(n => n.ID == 0).ID = id;
                    var replacements = new Dictionary<string, string>(4)
                    {
                        {"#forename#", user.Forename},
                        {"#surname#", user.Surname},
                        {"#SignupDate#", signup.Date.ToString("dddd d MMMM yyyy")},
                        {"#SlotDescription#", slot.Description},
                        {"#username#", user.Username},
                        {"#sitename#", configurationService.SiteName()},
                        {"#siteurl#", configurationService.SiteURL()}
                    };

                    const string key = "UserSessionSignup";

                    if (emailService.SendMail(key, user.Email, replacements))
                    {
                        emailService.SendEmailLog(key, user.Username);
                        return true;
                    }
                    return true;
                }

                notificationService.AddError("An error occured. ");
                return false;

            }


        }

        public bool CancelSignupToSlot(int _signupID, int _slotId, string _username)
        {
            var signup = Fetch(_signupID);

            var slot = signup.Slots.SingleOrDefault(n => n.ID == _slotId);

            if (slot == null)
            {
                notificationService.AddError(string.Format("The requested slot ({0}) could not be found for signup {1}.", _slotId, _signupID));
                return false;
            }

            var user = userManager.Fetch(_username);
            object bodyLock;
            lock (dictionaryLock)
            {
                if (!dictionary.TryGetValue(_slotId, out bodyLock))
                {
                    bodyLock = new object();
                    dictionary[_slotId] = bodyLock;
                }
            }

            lock (bodyLock)
            {
                if (signupRepository.CancelSignupToSlot(_slotId, user.ID))
                {
                    /*                    var userSignup = signup.Slots.SingleOrDefault(n => n.ID == _slotId).ApplicantsSignedUp.SingleOrDefault(n => n.Applicant.Username == _username);

                    signup.Slots.SingleOrDefault(n => n.ID == _slotId).ApplicantsSignedUp.Remove(userSignup);
*/
                    var replacements = new Dictionary<string, string>(4)
                    {
                        {"#forename#", user.Forename},
                        {"#surname#", user.Surname},
                        {"#SignupDate#", signup.Date.ToString("dddd d MMMM yyyy")},
                        {"#SlotDescription#", slot.Description},
                        {"#username#", user.Username},
                        {"#sitename#", configurationService.SiteName()},
                        {"#siteurl#", configurationService.SiteURL()}
                    };

                    const string key = "UserSessionCancel";

                    if (emailService.SendMail(key, user.Email, replacements))
                    {
                        emailService.SendEmailLog(key, user.Username);
                        return true;
                    }
                    notificationService.AddError("An error occured. ");
                    return true;
                }

                return false;
            }
        }

        public bool AddUserToGroup(int _userId, int _id)
        {
            return signupRepository.AddUserToGroup(_userId, _id);
        }

        //cache this!
        private rep.UserSignup FetchuserSignup(int _iduser, IEnumerable<int> _groupIds)
        {
            return signupRepository.FetchUserSignup(_iduser, _groupIds);
        }

        private SlotStatus GenerateSlotStatus(rep.Slot _slot, UserSlotView _slotView, Repository.Objects.Signups.UserSignup _userSignup)
        {

            if (!_slot.Enabled)
                return SlotStatus.Closed;

            if (_slotView.IsSignedup)
            {
                if (_slot.id == _slotView.SignedUpSlotInformation.SlotID)
                    return SlotStatus.AlreadySignedUp;

                return SlotStatus.Clash;
            }

            if(_userSignup != null)
                return SlotStatus.Clash;

            if (_slot.UsersSignedUp.HasContent() && _slot.UsersSignedUp.Count() >= _slot.PlacesAvailable)
                return SlotStatus.Full;

            return SlotStatus.Signup;
        }
    }
}
