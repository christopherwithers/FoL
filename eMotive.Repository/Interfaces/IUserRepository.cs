using System.Collections.Generic;
using eMotive.Repository.Objects.Users;

namespace eMotive.Repository.Interfaces
{
    public interface IUserRepository
    {
        User New();
        User Fetch(int _id);

        IEnumerable<User> Fetch(IEnumerable<int> _ids);

       // CreateUser Create(User _user);

       /* void AddUserToRoles(int _id, IEnumerable<int> _ids);
        void RemoveUserFromRoles(int _userId, IEnumerable<int> _ids);
        IEnumerable<int> FindUsersInRole(int _userId, int _id);*/
        
        bool Create(User _user, out int _id);
        bool Update(User _user);
        bool Delete(User _user);
    }
}
