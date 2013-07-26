using eMotive.Models.Objects;
using eMotive.Models.Objects.Users;
using repUsers = eMotive.Repository.Objects.Users;

namespace eMotive.Managers.Interfaces
{
    public interface IUserManager
    {
        /*        User New();
        User Fetch(int _id);
        IEnumerable<User> Fetch(IEnumerable<int> _ids);

       // CreateUser Create(User _user);

       /* void AddUserToRoles(int _id, IEnumerable<int> _ids);
        void RemoveUserFromRoles(int _userId, IEnumerable<int> _ids);
        IEnumerable<int> FindUsersInRole(int _userId, int _id);*/
        /*
        bool Create(User _user);
        bool Update(User _user);
        bool Delete(User _user);*/
        User New();
        User Fetch(int _id);
        CreateUser Create(User _user);
    }
}
