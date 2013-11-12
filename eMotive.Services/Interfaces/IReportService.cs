using System.Collections.Generic;
using eMotive.Models.Objects.Users;

namespace eMotive.Services.Interfaces
{
    public interface IReportService
    {
        IEnumerable<User> FetchUsersNotSignedUp();
    }
}
