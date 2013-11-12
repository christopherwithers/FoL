using System;
using System.Linq;
using System.Web.Security;
using Extensions;
using Ninject;
using eMotive.Managers.Interfaces;

namespace eMotive.FoL.Common.Providers
{
    public class eMotiveRoleProvider : RoleProvider
    {
        [Inject]
        public IUserManager UserManager { get; set; }

        public override bool IsUserInRole(string _username, string _roleName)
        {
            var user = UserManager.Fetch(_username);

            if (user == null)
                return false;

            return user.Roles.Any(n => n.Name.ToLowerInvariant() == _roleName.ToLowerInvariant());
        }

        public override string[] GetRolesForUser(string _username)
        {
            var user = UserManager.Fetch(_username);

            if (user == null || !user.Roles.HasContent())
                return new string[0];

            return user.Roles.Select(n => n.Name).ToArray();

        }

        public override void CreateRole(string roleName)
        {
            throw new NotImplementedException();
        }

        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            throw new NotImplementedException();
        }

        public override bool RoleExists(string roleName)
        {
            throw new NotImplementedException();
        }

        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        public override string[] GetUsersInRole(string roleName)
        {
            throw new NotImplementedException();
        }

        public override string[] GetAllRoles()
        {
            throw new NotImplementedException();
        }

        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            throw new NotImplementedException();
        }

        public override string ApplicationName { get; set; }

    }
}