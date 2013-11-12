using System;

namespace eMotive.Models.Objects.Signups
{
    public class SignupState
    {
        public int ID { get; set; }
        public DateTime Date { get; set; }
        public int SlotsAvailable { get; set; }
        public bool SignedUp { get; set; }
        public int TotalSlotsAvailable { get; set; }
        public bool DisabilitySignup { get; set; }
        public bool Closed { get; set; }

        public string SlotsAvailableString()
        {
            return Closed ? "Sign up closed" : string.Format("{0} Places Available", SlotsAvailable);
        }
    }
}
