using System;
using System.Collections.Generic;
using eMotive.Repository.Objects.Signups;

namespace eMotive.Repository.Interfaces
{
    public interface ISignupRepository
    {
        IEnumerable<Group> FetchGroups();
        IEnumerable<Group> FetchGroups(IEnumerable<int> _ids);
        IEnumerable<Signup> FetchSignupsByGroup(IEnumerable<int> _groupIds);

        IEnumerable<Signup> FetchAll(); 

        bool AddUserToGroup(int _userId, int _id);
        bool AddUserToGroups(int _userId, IEnumerable<int> _ids);
        //todo: fetch signups for user!

        bool SignupToSlot(int _idSlot, int _idUser, DateTime _signupDate, out int _id);
        bool CancelSignupToSlot(int _idSlot, int _idUser);

        Signup FetchSignup(int _id);

        int GetSignupIdFromSlot(int _id);

        Group FetchSignupGroup(int _id);

        //TODO: HAVE A SIGNUPINFO CLASS FOR USERS WHO HAVE SIGNED UP?? COULD CONTAIN IDSIGNUP, IDSLOT ETC

        UserSignup FetchUserSignup(int _userId, int _groupId);
        UserSignup FetchUserSignup(int _userId, IEnumerable<int> _groupIds);
        
    }
}
