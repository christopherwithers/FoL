using System;
using System.Collections.Generic;

namespace eMotive.Models.Objects.Signups
{
    public class UserSlotView
    {
        public int SignupID { get; set; }
        public DateTime SignupDate { get; set; }
        public bool IsSignedup { get { return SignedUpSlotInformation != null; } }
        public UserSignupInformation SignedUpSlotInformation { get; set; }
        public bool DisabilitySignup { get; set; }
        public IEnumerable<SlotState> Slots { get; set; }

        public string HeaderText { get; set; }
        public string FooterText { get; set; }
    }
}
