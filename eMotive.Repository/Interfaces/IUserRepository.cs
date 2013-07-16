using System.Collections.Generic;
using eMotive.Repository.Objects.Users;

namespace eMotive.Repository.Interfaces
{
    public interface IUserRepository
    {
        User Fetch(int _id);
        IEnumerable<User> Fetch(IEnumerable<int> _ids);

        CreateUser Create(User _user);
        void Update(User _user);
        void Delete(User _user);
    }
}
