using System.Collections.Generic;
using System.Data;
using ServiceStack.OrmLite;
using eMotive.Repository.Interfaces;
using eMotive.Repository.Objects.Users;

namespace eMotive.Repository.Objects
{//https://github.com/ServiceStack/ServiceStack.OrmLite
    //http://stackoverflow.com/questions/14480237/servicestack-ormlite-repository-pattern
    //https://groups.google.com/forum/#!msg/servicestack/1pA41E33QII/R-trWwzYgjEJ
    //http://xunitpatterns.com/Obscure%20Test.html#General
    //http://xunitpatterns.com/Obscure%20Test.html#General
    public class MySqlUserRepository : IUserRepository
    {

        public IDbConnectionFactory DbFactory { get; set; } //injected by IOC

        IDbConnection db;
        IDbConnection Db
        {
            get { return db ?? (db = DbFactory.Open()); }
        }
        //http://stackoverflow.com/questions/10127296/ormlite-createtableifnotexists-fails-if-table-has-index
        public User Fetch(int _id)
        {
            return Db.Id<User>(_id);
        }

        public IEnumerable<User> Fetch(IEnumerable<int> _ids)
        {
            return Db.Ids<User>(_ids);
        }

        public CreateUser Create(User _user)
        {
            var user = Db.QuerySingle<User>(new {_user.Username});

            if (user != null)
                return user.Archived ? CreateUser.Deletedaccount : CreateUser.DuplicateEmail;
            
            if (Db.QuerySingle<User>(new {_user.Email}) != null)
                return CreateUser.DuplicateUsername;
            
            db.Insert(_user);

            return db.GetLastInsertId() > 0 ? CreateUser.Success : CreateUser.Error;
        }

        public void Update(User _user)
        {
            db.Update(_user);
        }

        public void Delete(User _user)
        {
            _user.Enabled = false;
            _user.Surname = string.Empty;
            _user.Forename = string.Empty;
            _user.Archived = false;

            db.Update(_user);
        }

        public void Dispose()
        {
            if (db != null)
                db.Dispose();
        }
    }
}
