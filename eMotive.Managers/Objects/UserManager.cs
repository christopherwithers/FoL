using System;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using Omu.ValueInjecter;
using eMotive.Managers.Interfaces;
using eMotive.Managers.Objects.Search;
using eMotive.Models.Objects;
using eMotive.Models.Objects.Users;
using eMotive.Repository.Interfaces;
using repUsers = eMotive.Repository.Objects.Users;

namespace eMotive.Managers.Objects
{
    public class UserManager : IUserManager
    {
        private readonly IUserRepository userRep;
        private readonly IRoleRepository roleRep;
        private readonly ISiteSearchManager searchManager;

        public UserManager(IUserRepository _userRep, IRoleRepository _roleRep, ISiteSearchManager _searchManager)
        {
            userRep = _userRep;
            roleRep = _roleRep;
            searchManager = _searchManager;
        }

        public User New()
        {
            var repUser = userRep.New(); 

            var user = new User();

            user.InjectFrom(repUser);

            return user;
        }

        public User Fetch(int _id)
        {
            var repUser = userRep.Fetch(_id);

            if (repUser != null)
                repUser.Roles = roleRep.FetchUserRoles(_id);

            var user = new User();

            user.InjectFrom(repUser);

            return user;
        }

        //TODO: if we cache, then assign id to _user too and cache Models.User
        public CreateUser Create(User _user)
        {
            var repUser = new repUsers.User();
            repUser.InjectFrom(_user);

            var result = searchManager.DoSearch(new eMotive.Search.Objects.Search
                            {
                                CustomQuery = new Dictionary<string, string>
                                    {
                                        { "Username", _user.Username }, 
                                        { "Email", _user.Email },
                                        { "Type", "User"}
                                    }
                            });

            if (result.Items.HasContent())
            {
                var users = userRep.Fetch(result.Items.Select(n => n.ID).ToList());

                if(users.Any(n => n.Archived))
                    return CreateUser.Deletedaccount;

                if(users.Any(n => n.Username == _user.Username))
                    return CreateUser.DuplicateUsername;

                if(users.Any(n => n.Email == _user.Email))
                    return CreateUser.DuplicateEmail;

                throw new Exception("User exists");
            }

            int id;
            if (userRep.Create(repUser, out id))
            {
                repUser.ID = id;

                searchManager.Add(new UserSearchDocument(repUser));

                return CreateUser.Success;
            }

            return CreateUser.Error;
        }
    }
}
