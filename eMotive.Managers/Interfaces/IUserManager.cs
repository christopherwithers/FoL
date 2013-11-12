using System.Collections.Generic;
using eMotive.Models.Objects.Users;
using eMotive.Search.Interfaces;
using eMotive.Search.Objects;
using repUsers = eMotive.Repository.Objects.Users;
using modSearch = eMotive.Models.Objects.Search;

namespace eMotive.Managers.Interfaces
{
    public interface IUserManager : ISearchable
    {
        User New();
        User Fetch(int _id);
        User Fetch(string _username);
        IEnumerable<User> Fetch(IEnumerable<int> _ids);
        IEnumerable<User> Fetch(IEnumerable<string> _usernames);

        bool CreateApplicantAccounts(List<ApplicantUploadData> _applicantData, IEnumerable<int> _groupIds);

        IEnumerable<ApplicantData> FetchApplicantData(string _username);
        IDictionary<string, List<ApplicantData>> FetchApplicantData(IEnumerable<string> _username);

        bool Create(User _user, out int _id);
        bool Create(User _user, IEnumerable<int> _groupIds);
        bool Update(User _user);
        bool Delete(User _user);
        IEnumerable<User> FetchRecordsFromSearch(SearchResult _searchResult);

        Profile FetchProfile(string _username);

        bool SaveApplicantData(IEnumerable<ApplicantData> _applicantData);
    }
}
