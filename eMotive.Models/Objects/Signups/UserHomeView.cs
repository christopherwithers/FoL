using System;
using System.Collections.Generic;
using eMotive.Models.Objects.Pages;
using eMotive.Models.Objects.Users;

namespace eMotive.Models.Objects.Signups
{
    public class UserHomeView
    {
        public UserHomeView()
        {
            PageSections = new Dictionary<string, PartialPage>();
        }
        public DateTime SignUpDate { get; set; }
        public string SignUpDetails { get; set; }
        public User User { get; set; }
        public bool HasSignedUp { get; set; }
        public int SignedUpSlotID { get; set; }
        public int SignupID { get; set; }

        public IDictionary<string, PartialPage> PageSections { get; set; }
    }
}
