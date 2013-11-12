using System;
using System.Collections.Generic;
using System.Linq;
using eMotive.Repository.Objects.Users;

namespace eMotive.Repository.Objects.Signups
{
    public class Slot
    {
        public int id { get; set; }
        public string Description { get; set; }
        public int PlacesAvailable { get; set; }
        public bool Enabled { get; set; }
        public int IdSignUp { get; set; }
        public DateTime Time { get; set; }
        public ICollection<UserSignup> UsersSignedUp { get; set; }

        public bool AddUserToSlot(int _id, User _user, DateTime _date, out string _error)
        {
            if (UsersSignedUp.Any(n => n.IdUser == _user.ID))
            {
                _error = "You have already signed up to this slot.";

                return false;
            }

            if (UsersSignedUp.Count() >= PlacesAvailable)
            {
                _error = "The selected slot is now full.";

                return false;
            }

            UsersSignedUp.Add(new UserSignup
            {
                IdUser = _user.ID,
                ID = _id,
                SignupDate = _date
            });


            _error = string.Empty;

            return true;
        }
    }
}
