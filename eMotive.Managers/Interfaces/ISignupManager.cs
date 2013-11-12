using System.Collections.Generic;
using eMotive.Models.Objects.Signups;

namespace eMotive.Managers.Interfaces
{
    public interface ISignupManager
    {
        Signup Fetch(int _id);

        IEnumerable<Signup> FetchAll();

        IEnumerable<SessionDay> FetchAllBrief();
        UserHomeView FetchHomeView(string _username);
        UserSignupView FetchSignupInformation(string _username);
        UserSlotView FetchSlotInformation(int _signup, string _username);

        bool SignupToSlot(int _signupID, int _slotId, string _username);
        bool CancelSignupToSlot(int _signupID, int _slotId, string _username);
    }
}
