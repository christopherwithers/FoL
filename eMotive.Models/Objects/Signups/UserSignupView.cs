using System.Collections.Generic;

namespace eMotive.Models.Objects.Signups
{
    public class UserSignupView
    {
        public string HeaderText { get; set; }
        public string FooterText { get; set; }
        public bool SignedUp { get; set; }
        public int SignupID { get; set; }
        public ICollection<SignupState> SignupInformation { get; set; }
    }
}
