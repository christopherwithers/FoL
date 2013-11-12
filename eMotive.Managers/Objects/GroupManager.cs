using System.Collections.Generic;
using AutoMapper;
using eMotive.Managers.Interfaces;
using eMotive.Models.Objects.Signups;
using eMotive.Repository.Interfaces;
using eMotive.Services.Interfaces;

namespace eMotive.Managers.Objects
{
    //TODO add search stuff??
    public class GroupManager : IGroupManager
    {
        private readonly ISignupRepository signupRepository;
        private readonly INotificationService notificationService;

        public GroupManager(ISignupRepository _signupRepository, INotificationService _notificationService)
        {
            signupRepository = _signupRepository;
            notificationService = _notificationService;
        }

        public bool AddUserToGroup(int _userId, int _id)
        {
            return signupRepository.AddUserToGroup(_userId, _id);
        }

        public IEnumerable<Group> FetchGroups()
        {
            return Mapper.Map <IEnumerable<Repository.Objects.Signups.Group>, IEnumerable<Group>>(signupRepository.FetchGroups());
        }

        public bool AddUserToGroups(int _userId, IEnumerable<int> _ids)
        {
            return signupRepository.AddUserToGroups(_userId, _ids);
        }
    }
}
