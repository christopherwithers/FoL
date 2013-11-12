using System.Collections.Generic;
using eMotive.Models.Objects.Search;

namespace eMotive.Models.Objects.Users
{
    public class UserSearch : BasicSearch
    {
        public UserSearch()
        {
            ItemType = "Users";
        }
        public IEnumerable<User> Users { get; set; }
       // public bool CanCreate { get; set; }
    }
}
